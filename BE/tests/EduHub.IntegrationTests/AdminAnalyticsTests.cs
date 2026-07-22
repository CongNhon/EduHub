using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using EduHub.Infrastructure.Seed;
using EduHub.Infrastructure.Persistence;
using EduHub.Infrastructure.Services.Jobs;
using EduHub.IntegrationTests.Support;
using EduHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EduHub.IntegrationTests;

/// <summary>
/// Ghi chú: AdminAnalyticsTests kiểm tra authorization, dataset, monitoring và DevExpress export trên API thật.
/// </summary>
[Collection(EduHubIntegrationSuite.Name)]
public sealed class AdminAnalyticsTests(EduHubIntegrationFixture fixture)
{
    /// <summary>
    /// Ghi chú: AnonymousCannotReadAnalyticsAsync xác nhận analytics không trả dữ liệu cho request chưa đăng nhập.
    /// </summary>
    [Fact]
    public async Task AnonymousCannotReadAnalyticsAsync()
    {
        using var client = fixture.Factory.CreateClient();
        var response = await client.GetAsync("/api/v1/admin/analytics/overview");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Ghi chú: AcademicAdminCannotReadSystemAnalyticsAsync xác nhận role học vụ không được xem dashboard SystemAdmin.
    /// </summary>
    [Fact]
    public async Task AcademicAdminCannotReadSystemAnalyticsAsync()
    {
        using var client = fixture.Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client, DevelopmentAcademicSeeder.AcademicEmail));
        var response = await client.GetAsync("/api/v1/admin/analytics/overview");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Ghi chú: SystemAdminReadsAllAnalyticsDatasetsAsync xác nhận ba API trả cùng học kỳ và các collection cho chart/grid.
    /// </summary>
    [Fact]
    public async Task SystemAdminReadsAllAnalyticsDatasetsAsync()
    {
        using var client = fixture.Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client, EduHubIntegrationFixture.SystemAdminEmail));

        var overview = await ReadJsonAsync(client, "/api/v1/admin/analytics/overview");
        var semesterId = Guid.Parse(overview["data"]!["semester"]!["id"]!.GetValue<string>());
        var academic = await ReadJsonAsync(client, $"/api/v1/admin/analytics/academic?semesterId={semesterId}");
        var quality = await ReadJsonAsync(client, $"/api/v1/admin/analytics/data-quality?semesterId={semesterId}");

        Assert.NotEmpty(overview["data"]!["availableSemesters"]!.AsArray());
        Assert.Equal(semesterId, Guid.Parse(academic["data"]!["semester"]!["id"]!.GetValue<string>()));
        Assert.Equal(semesterId, Guid.Parse(quality["data"]!["semester"]!["id"]!.GetValue<string>()));
        Assert.Equal(5, academic["data"]!["gradeDistribution"]!.AsArray().Count);
        Assert.Equal(8, quality["data"]!["issues"]!.AsArray().Count);
    }

    /// <summary>
    /// Ghi chú: SystemAdminReadsOperationalMonitoringAsync xác nhận monitoring trả cache, Hangfire và queue metrics.
    /// </summary>
    [Fact]
    public async Task SystemAdminReadsOperationalMonitoringAsync()
    {
        using var client = fixture.Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client, EduHubIntegrationFixture.SystemAdminEmail));
        var body = await ReadJsonAsync(client, "/api/v1/admin/monitoring");

        Assert.NotNull(body["data"]!["cache"]);
        Assert.NotNull(body["data"]!["hangfire"]);
        Assert.NotNull(body["data"]!["outbox"]);
        Assert.NotEmpty(body["data"]!["externalSyncs"]!.AsArray());
    }

    /// <summary>
    /// Ghi chú: SystemAdminExportsDevExpressPdfAsync xác nhận report endpoint trả file PDF do XtraReport tạo.
    /// </summary>
    [Fact]
    public async Task SystemAdminExportsDevExpressPdfAsync()
    {
        using var client = fixture.Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client, EduHubIntegrationFixture.SystemAdminEmail));
        var response = await client.GetAsync("/api/v1/admin/analytics/report/export?format=pdf");
        var content = await response.Content.ReadAsByteArrayAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.True(content.Length > 4);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(content, 0, 4));
    }

    /// <summary>
    /// Ghi chú: WeeklyAdminDigestIsIdempotentAsync xác nhận chạy lại Hangfire job không tạo delivery trùng cùng tuần.
    /// </summary>
    [Fact]
    public async Task WeeklyAdminDigestIsIdempotentAsync()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<WeeklyAdminAnalyticsDigestJob>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await job.SendAsync();
        await job.SendAsync();

        var expectedRecipients = await dbContext.Users.CountAsync(user => user.Role == UserRole.SystemAdmin && user.IsActive);
        var deliveries = await dbContext.EmailDigestDeliveries.CountAsync(delivery => delivery.TemplateVersion == "admin-analytics-weekly-v1");
        Assert.Equal(expectedRecipients, deliveries);
    }

    /// <summary>
    /// Ghi chú: ReadJsonAsync gọi endpoint và parse JSON sau khi xác nhận status thành công.
    /// </summary>
    private static async Task<JsonNode> ReadJsonAsync(HttpClient client, string path)
    {
        var response = await client.GetAsync(path);
        var content = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, content);
        return JsonNode.Parse(content)!;
    }

    /// <summary>
    /// Ghi chú: LoginAsync đăng nhập tài khoản seed và trả access token cho analytics test.
    /// </summary>
    private static async Task<string> LoginAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email,
            password = DevelopmentAcademicSeeder.SeedPassword,
            deviceId = "admin-analytics-tests"
        });
        response.EnsureSuccessStatusCode();
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
        return body["data"]!["accessToken"]!.GetValue<string>();
    }
}
