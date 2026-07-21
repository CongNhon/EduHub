using EduHub.Application.Interfaces.Authentication;
using EduHub.Domain.Entities.Identity;
using EduHub.Infrastructure.Options;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EduHub.Infrastructure.Seed;

/// <summary>
/// Ghi chú: DevelopmentIdentitySeeder đại diện cho seed tài khoản admin development trong hệ thống EduHub.
/// </summary>
public static class DevelopmentIdentitySeeder
{
    /// <summary>
    /// Ghi chú: SeedDevelopmentIdentityAsync thực hiện phần xử lý của seed tài khoản admin development.
    /// </summary>
    public static async Task SeedDevelopmentIdentityAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<DevelopmentSeedOptions>>().Value;
        if (!options.Enabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(options.Email) || options.Password.Length < 12)
        {
            throw new InvalidOperationException(
                "Development seed requires Auth:DevelopmentSeed:Email and a password of at least 12 characters.");
        }

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHashService = scope.ServiceProvider.GetRequiredService<IPasswordHashService>();
        var normalizedEmail = options.Email.Trim().ToUpperInvariant();
        var exists = await dbContext.Users.AnyAsync(user => user.NormalizedEmail == normalizedEmail);
        if (exists)
        {
            return;
        }

        dbContext.Users.Add(new User(
            options.Email.Trim(),
            normalizedEmail,
            passwordHashService.HashPassword(options.Password),
            options.Role));

        await dbContext.SaveChangesAsync();
    }
}
