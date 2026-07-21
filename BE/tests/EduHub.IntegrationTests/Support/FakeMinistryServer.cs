using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EduHub.IntegrationTests.Support;

/// <summary>
/// Ghi chú: FakeMinistryServer chạy HTTP server giả lập Ministry API để kiểm tra contract sync gradebook.
/// </summary>
public sealed class FakeMinistryServer : IAsyncDisposable
{
    private readonly WebApplication app;
    private int receivedGradebooks;

    private FakeMinistryServer(WebApplication app, string baseUrl)
    {
        this.app = app;
        BaseUrl = baseUrl;
    }

    public string BaseUrl { get; }

    public int ReceivedGradebooks => receivedGradebooks;

    /// <summary>
    /// Ghi chú: StartAsync khởi động fake Ministry API với endpoint /health và POST /api/v1/gradebooks.
    /// </summary>
    public static async Task<FakeMinistryServer> StartAsync()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseSetting("urls", "http://127.0.0.1:0");
        var counter = new FakeMinistryCounter();
        builder.Services.AddSingleton(counter);
        var app = builder.Build();

        app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
        app.MapPost("/api/v1/gradebooks", async (HttpRequest request) =>
        {
            if (request.Headers["X-Api-Key"] != "test-ministry-key")
            {
                return Results.Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(request.Headers["Idempotency-Key"]))
            {
                return Results.BadRequest(new { error = "Idempotency-Key is required." });
            }

            using var document = await System.Text.Json.JsonDocument.ParseAsync(request.Body);
            if (!document.RootElement.TryGetProperty("contractVersion", out var contractVersion) ||
                contractVersion.GetString() != "ministry-gradebook-v1")
            {
                return Results.BadRequest(new { error = "Invalid contractVersion." });
            }

            var server = request.HttpContext.RequestServices.GetRequiredService<FakeMinistryCounter>();
            server.Increment();
            return Results.Ok(new { externalId = Guid.NewGuid().ToString("N"), externalVersion = "v1" });
        });

        await app.StartAsync();
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        var fake = new FakeMinistryServer(app, address);
        counter.Attach(fake);
        return fake;
    }

    /// <summary>
    /// Ghi chú: Increment tăng số request POST gradebook mà fake Ministry đã nhận.
    /// </summary>
    private void Increment() => Interlocked.Increment(ref receivedGradebooks);

    /// <summary>
    /// Ghi chú: DisposeAsync tắt fake Ministry server sau integration test.
    /// </summary>
    public async ValueTask DisposeAsync() => await app.DisposeAsync();

    private sealed class FakeMinistryCounter
    {
        private FakeMinistryServer? server;

        public void Attach(FakeMinistryServer fakeServer) => server = fakeServer;

        public void Increment() => server?.Increment();
    }
}
