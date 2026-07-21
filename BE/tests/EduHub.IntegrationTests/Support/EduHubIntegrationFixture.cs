using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using EduHub.Application.Interfaces.Authentication;
using EduHub.Infrastructure.Persistence;
using EduHub.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Sockets;
using Testcontainers.PostgreSql;

namespace EduHub.IntegrationTests.Support;

/// <summary>
/// Ghi chú: EduHubIntegrationFixture khởi tạo Postgres, Redis, Mongo, fake Ministry và seed dữ liệu học vụ cho API test.
/// </summary>
public sealed class EduHubIntegrationFixture : IAsyncLifetime
{
    public const string SystemAdminEmail = "admin@eduhub.local";

    private readonly PostgreSqlContainer postgres = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("eduhub_tests")
        .WithUsername("eduhub")
        .WithPassword("eduhub")
        .Build();

    private readonly IContainer redis = new ContainerBuilder("redis:7-alpine")
        .WithPortBinding(6379, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(6379))
        .Build();

    private readonly IContainer mongo = new ContainerBuilder("mongo:8")
        .WithPortBinding(27017, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(27017))
        .Build();

    public EduHubWebApplicationFactory Factory { get; private set; } = null!;

    public FakeMinistryServer MinistryServer { get; private set; } = null!;

    public string PostgresConnectionString => postgres.GetConnectionString();

    /// <summary>
    /// Ghi chú: InitializeAsync chạy container, start API test host, migrate database và seed dữ liệu happy-flow.
    /// </summary>
    public async Task InitializeAsync()
    {
        await postgres.StartAsync();
        await redis.StartAsync();
        await mongo.StartAsync();
        var redisPort = redis.GetMappedPublicPort(6379);
        await WaitForTcpPortAsync(redis.Hostname, redisPort, CancellationToken.None);
        MinistryServer = await FakeMinistryServer.StartAsync();

        var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(postgres.GetConnectionString())
            .Options;
        await using (var migrationContext = new ApplicationDbContext(dbOptions))
        {
            await migrationContext.Database.MigrateAsync();
        }

        Factory = new EduHubWebApplicationFactory(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Postgres"] = postgres.GetConnectionString(),
            ["ConnectionStrings:Redis"] = $"{redis.Hostname}:{redisPort},abortConnect=false,connectRetry=5",
            ["ConnectionStrings:Mongo"] = $"mongodb://{mongo.Hostname}:{mongo.GetMappedPublicPort(27017)}/",
            ["Auth:Jwt:Issuer"] = "EduHub.Tests",
            ["Auth:Jwt:Audience"] = "EduHub.Tests",
            ["Auth:Jwt:Secret"] = "test-only-eduhub-jwt-secret-change-before-production",
            ["Auth:DevelopmentSeed:Enabled"] = "true",
            ["Auth:DevelopmentSeed:Email"] = SystemAdminEmail,
            ["Auth:DevelopmentSeed:Password"] = DevelopmentAcademicSeeder.SeedPassword,
            ["Auth:DevelopmentSeed:Role"] = "SystemAdmin",
            ["Audit:Mongo:Enabled"] = "true",
            ["Audit:Mongo:DatabaseName"] = "eduhub_audit_tests",
            ["Audit:Mongo:CollectionName"] = "audit_logs",
            ["Audit:Mongo:ServerSelectionTimeoutMilliseconds"] = "1000",
            ["Email:Smtp:Enabled"] = "false",
            ["MinistryApi:BaseUrl"] = MinistryServer.BaseUrl,
            ["MinistryApi:ApiKey"] = "test-ministry-key",
            ["MinistryApi:TimeoutSeconds"] = "3",
            ["MinistryApi:CircuitBreakerFailures"] = "5",
            ["MinistryApi:CircuitBreakerSeconds"] = "10"
        });

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHashService = scope.ServiceProvider.GetRequiredService<IPasswordHashService>();
        await Factory.Services.SeedDevelopmentIdentityAsync();
        await DevelopmentAcademicSeeder.SeedDevelopmentAcademicDataAsync(dbContext, passwordHashService, CancellationToken.None);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await dbContext.Semesters.ExecuteUpdateAsync(setters => setters
            .SetProperty(semester => semester.GradeEntryFrom, today.AddDays(-1))
            .SetProperty(semester => semester.GradeEntryTo, today.AddDays(1)));
    }

    /// <summary>
    /// Ghi chú: DisposeAsync dọn API test host và các container sau khi chạy integration test.
    /// </summary>
    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await MinistryServer.DisposeAsync();
        await mongo.DisposeAsync();
        await redis.DisposeAsync();
        await postgres.DisposeAsync();
    }

    /// <summary>
    /// Ghi chu: Doi Redis mo host port that su truoc khi API tao Redis client de health check khong bi loi khoi dong som.
    /// </summary>
    private static async Task WaitForTcpPortAsync(string host, int port, CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow.AddSeconds(30);
        Exception? lastException = null;

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(host, port, cancellationToken);
                return;
            }
            catch (Exception exception) when (exception is SocketException or TimeoutException)
            {
                lastException = exception;
                await Task.Delay(250, cancellationToken);
            }
        }

        throw new InvalidOperationException($"Redis test container is not reachable at {host}:{port}.", lastException);
    }
}

/// <summary>
/// Ghi chú: EduHubIntegrationCollection gom các integration test dùng chung fixture container EduHub.
/// </summary>
[CollectionDefinition(Name)]
public sealed class EduHubIntegrationSuite : ICollectionFixture<EduHubIntegrationFixture>
{
    public const string Name = "EduHub integration collection";
}
