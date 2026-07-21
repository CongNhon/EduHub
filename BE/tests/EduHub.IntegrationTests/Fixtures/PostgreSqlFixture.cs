using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EduHub.IntegrationTests.Fixtures;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("eduhub_integration")
        .WithUsername("eduhub_test")
        .WithPassword("eduhub_test_password")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    private ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new ApplicationDbContext(options);
    }
}
