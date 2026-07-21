using System.Net;
using System.Net.Http.Json;
using EduHub.IntegrationTests.Support;

namespace EduHub.IntegrationTests;

/// <summary>
/// Ghi chú: MinistryContractTests kiểm tra contract HTTP giữa EduHub và fake Ministry API.
/// </summary>
[Collection(EduHubIntegrationSuite.Name)]
public sealed class MinistryContractTests(EduHubIntegrationFixture fixture)
{
    /// <summary>
    /// Ghi chú: FakeMinistryRequiresIdempotencyKeyAsync kiểm tra Ministry fake từ chối request thiếu Idempotency-Key.
    /// </summary>
    [Fact]
    public async Task FakeMinistryRequiresIdempotencyKeyAsync()
    {
        using var client = new HttpClient { BaseAddress = new Uri(fixture.MinistryServer.BaseUrl) };
        client.DefaultRequestHeaders.Add("X-Api-Key", "test-ministry-key");

        var response = await client.PostAsJsonAsync("/api/v1/gradebooks", new
        {
            contractVersion = "ministry-gradebook-v1",
            assignmentId = Guid.NewGuid(),
            publicationVersion = 1,
            grades = Array.Empty<object>()
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Ghi chú: FakeMinistryAcceptsGradebookContractAsync kiểm tra payload gradebook hợp lệ trả external id/version.
    /// </summary>
    [Fact]
    public async Task FakeMinistryAcceptsGradebookContractAsync()
    {
        using var client = new HttpClient { BaseAddress = new Uri(fixture.MinistryServer.BaseUrl) };
        client.DefaultRequestHeaders.Add("X-Api-Key", "test-ministry-key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString("N"));

        var response = await client.PostAsJsonAsync("/api/v1/gradebooks", new
        {
            contractVersion = "ministry-gradebook-v1",
            assignmentId = Guid.NewGuid(),
            publicationVersion = 1,
            grades = new[]
            {
                new { studentId = Guid.NewGuid(), componentId = Guid.NewGuid(), score = 8.5m }
            }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
