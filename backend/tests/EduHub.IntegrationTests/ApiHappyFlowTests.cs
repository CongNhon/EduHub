using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using EduHub.Infrastructure.Persistence;
using EduHub.Infrastructure.Seed;
using EduHub.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EduHub.IntegrationTests;

/// <summary>
/// Ghi chú: ApiHappyFlowTests kiểm tra API thật với Postgres, Redis, Mongo, Hangfire và fake Ministry server.
/// </summary>
[Collection(EduHubIntegrationSuite.Name)]
public sealed class ApiHappyFlowTests(EduHubIntegrationFixture fixture)
{
    /// <summary>
    /// Ghi chú: SwaggerHappyFlowAsync đăng nhập, nhập điểm, submit, publish và phụ huynh đọc điểm đã công bố.
    /// </summary>
    [Fact]
    public async Task SwaggerHappyFlowAsync()
    {
        using var client = fixture.Factory.CreateClient();
        var seeded = await GetSeededIdsAsync();
        await ResetGradebookAsync(seeded.AssignmentId);

        var teacherToken = await LoginAsync(client, DevelopmentAcademicSeeder.TeacherEmail);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", teacherToken);
        var bulkResponse = await client.PutAsJsonAsync($"/api/v1/assignments/{seeded.AssignmentId}/grades/bulk", new
        {
            atomic = true,
            items = seeded.ComponentIds.Select((componentId, index) => new
            {
                studentId = seeded.StudentId,
                componentId,
                score = index == 0 ? 8.25m : 8.75m,
                version = (int?)null,
                reason = "Integration test"
            }).ToArray()
        });
        Assert.True(
            bulkResponse.StatusCode == HttpStatusCode.OK,
            await bulkResponse.Content.ReadAsStringAsync());

        var submitResponse = await client.PostAsync($"/api/v1/assignments/{seeded.AssignmentId}/grades/submit", null);
        Assert.Equal(HttpStatusCode.OK, submitResponse.StatusCode);

        var academicToken = await LoginAsync(client, DevelopmentAcademicSeeder.AcademicEmail);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", academicToken);
        var publishResponse = await client.PostAsync($"/api/v1/assignments/{seeded.AssignmentId}/grades/publish", null);
        Assert.Equal(HttpStatusCode.OK, publishResponse.StatusCode);

        var parentToken = await LoginAsync(client, DevelopmentAcademicSeeder.ParentEmail);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentToken);
        var publishedResponse = await client.GetAsync($"/api/v1/assignments/{seeded.AssignmentId}/students/{seeded.StudentId}/grades/published");
        Assert.Equal(HttpStatusCode.OK, publishedResponse.StatusCode);

        var body = JsonNode.Parse(await publishedResponse.Content.ReadAsStringAsync())!;
        Assert.Equal(2, body["data"]!["grades"]!.AsArray().Count);
    }

    /// <summary>
    /// Ghi chú: HealthReadyIncludesDependenciesAsync kiểm tra /health/ready trả trạng thái từng dependency chính.
    /// </summary>
    [Fact]
    public async Task HealthReadyIncludesDependenciesAsync()
    {
        using var client = fixture.Factory.CreateClient();
        var adminToken = await LoginAsync(client, EduHubIntegrationFixture.SystemAdminEmail);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var response = await client.GetAsync("/health/ready");
        var content = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, content);

        var body = JsonNode.Parse(content)!;
        Assert.NotNull(body["checks"]!["postgres"]);
        Assert.NotNull(body["checks"]!["redis"]);
        Assert.NotNull(body["checks"]!["mongo"]);
        Assert.NotNull(body["checks"]!["ministry"]);
    }

    /// <summary>
    /// Ghi chú: HangfireUsesPostgresStorageAsync kiểm tra Hangfire đã tạo schema storage trên PostgreSQL thật.
    /// </summary>
    [Fact]
    public async Task HangfireUsesPostgresStorageAsync()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tableCount = await dbContext.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS \"Value\" FROM information_schema.tables WHERE table_schema = 'hangfire'")
            .SingleAsync();

        Assert.True(tableCount > 0);
    }

    /// <summary>
    /// Ghi chú: GetSeededIdsAsync lấy id dữ liệu seed để happy-flow gọi API đúng assignment/student/component.
    /// </summary>
    private async Task<SeededIds> GetSeededIdsAsync()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var student = await dbContext.Students.SingleAsync(student => student.NormalizedStudentCode == DevelopmentAcademicSeeder.StudentCode);
        var assignment = await dbContext.TeachingAssignments.SingleAsync();
        var components = await dbContext.GradeComponents
            .Where(component => component.SubjectId == assignment.SubjectId && component.SemesterId == assignment.SemesterId && component.IsActive)
            .OrderBy(component => component.DisplayOrder)
            .Select(component => component.Id)
            .ToListAsync();

        return new SeededIds(student.Id, assignment.Id, components);
    }

    /// <summary>
    /// Ghi chú: ResetGradebookAsync xóa dữ liệu điểm/sync/notification cũ của assignment để test happy-flow bắt đầu sạch.
    /// </summary>
    private async Task ResetGradebookAsync(Guid assignmentId)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.GradeChangeHistories
            .Where(history => dbContext.GradeEntries.Any(entry => entry.Id == history.GradeEntryId && entry.AssignmentId == assignmentId))
            .ExecuteDeleteAsync();
        await dbContext.GradeEntries
            .Where(entry => entry.AssignmentId == assignmentId)
            .ExecuteDeleteAsync();
        await dbContext.ExternalSyncRecords
            .Where(record => record.AggregateId == assignmentId)
            .ExecuteDeleteAsync();
        await dbContext.Notifications
            .Where(notification => notification.AssignmentId == assignmentId)
            .ExecuteDeleteAsync();
        await dbContext.OutboxMessages.ExecuteDeleteAsync();
    }

    /// <summary>
    /// Ghi chú: LoginAsync đăng nhập tài khoản seed và trả access token cho request API tiếp theo.
    /// </summary>
    private static async Task<string> LoginAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email,
            password = DevelopmentAcademicSeeder.SeedPassword,
            deviceId = "integration-tests"
        });
        response.EnsureSuccessStatusCode();

        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
        return body["data"]!["accessToken"]!.GetValue<string>();
    }

    private sealed record SeededIds(Guid StudentId, Guid AssignmentId, IReadOnlyList<Guid> ComponentIds);
}
