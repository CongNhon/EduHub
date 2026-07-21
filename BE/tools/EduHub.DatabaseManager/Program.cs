using EduHub.Application.Interfaces.Authentication;
using EduHub.Domain.Enums;
using EduHub.Infrastructure.Options;
using EduHub.Infrastructure.Persistence;
using EduHub.Infrastructure.Seed;
using EduHub.Infrastructure.Services.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

const string migrateCommand = "migrate";
const string seedCommand = "seed";

if (args.Length != 1 || args[0] is not (migrateCommand or seedCommand))
{
    Console.Error.WriteLine("Usage: dotnet EduHub.DatabaseManager.dll <migrate|seed>");
    return 64;
}

var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres");
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine("ConnectionStrings__Postgres is required.");
    return 78;
}

using var cancellationSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationSource.Cancel();
};

try
{
    var services = new ServiceCollection();
    services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
    services.AddSingleton<IPasswordHashService, PasswordHashService>();
    services.Configure<DevelopmentSeedOptions>(options =>
    {
        options.Enabled = true;
        options.Email = Environment.GetEnvironmentVariable("Auth__DevelopmentSeed__Email") ?? "admin@eduhub.local";
        options.Password = Environment.GetEnvironmentVariable("Auth__DevelopmentSeed__Password") ?? string.Empty;
        options.Role = Enum.TryParse<UserRole>(Environment.GetEnvironmentVariable("Auth__DevelopmentSeed__Role"), out var role)
            ? role
            : UserRole.SystemAdmin;
    });

    await using var provider = services.BuildServiceProvider();
    await using (var scope = provider.CreateAsyncScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync(cancellationSource.Token);
    }

    if (args[0] == seedCommand)
    {
        var password = Environment.GetEnvironmentVariable("Auth__DevelopmentSeed__Password") ?? string.Empty;
        if (password.Length < 12)
        {
            Console.Error.WriteLine("Auth__DevelopmentSeed__Password must contain at least 12 characters.");
            return 78;
        }

        await provider.SeedDevelopmentIdentityAsync();
        await provider.SeedDevelopmentAcademicDataAsync();
        await provider.SeedDevelopmentSchoolAsync();
    }

    Console.WriteLine(args[0] == seedCommand
        ? "EduHub migrations and development seed completed."
        : "EduHub migrations completed.");
    return 0;
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("Database operation was cancelled.");
    return 130;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Database operation failed: {exception.Message}");
    return 1;
}
