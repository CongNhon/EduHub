using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EduHub.Infrastructure.Persistence;

/// <summary>
/// Ghi chú: ApplicationDbContextFactory đại diện cho factory tạo DbContext cho EF migration design-time trong hệ thống EduHub.
/// </summary>
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    /// <summary>
    /// Ghi chú: CreateDbContext thực hiện phần xử lý của factory tạo DbContext cho EF migration design-time.
    /// </summary>
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("EDUHUB_POSTGRES_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Set EDUHUB_POSTGRES_CONNECTION before using EF Core design-time commands.");
        }

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ApplicationDbContext(options);
    }
}
