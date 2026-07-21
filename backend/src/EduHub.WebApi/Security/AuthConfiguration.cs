using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using EduHub.Application.Interfaces.Authentication;
using EduHub.Application.Interfaces.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EduHub.WebApi.Security;

/// <summary>
/// Ghi chú: AuthConfiguration cấu hình mapping/constraint/index cho auth.
/// </summary>
public static class AuthConfiguration
{
    /// <summary>
    /// Ghi chú: AddEduHubSecurity thực hiện phần xử lý của auth.
    /// </summary>
    public static IServiceCollection AddEduHubSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddOptions<JwtOptions>()
            .Configure(options =>
            {
                options.Issuer = configuration["Auth:Jwt:Issuer"] ?? string.Empty;
                options.Audience = configuration["Auth:Jwt:Audience"] ?? string.Empty;
                options.Secret = configuration["Auth:Jwt:Secret"] ?? string.Empty;
                options.AccessTokenMinutes = int.TryParse(configuration["Auth:Jwt:AccessTokenMinutes"], out var accessMinutes)
                    ? accessMinutes
                    : 15;
                options.RefreshTokenDays = int.TryParse(configuration["Auth:Jwt:RefreshTokenDays"], out var refreshDays)
                    ? refreshDays
                    : 7;
            })
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "JWT issuer is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "JWT audience is required.")
            .Validate(options => options.Secret.Length >= 32, "JWT secret must be at least 32 characters.")
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwtOptions = new JwtOptions
                {
                    Issuer = configuration["Auth:Jwt:Issuer"] ?? string.Empty,
                    Audience = configuration["Auth:Jwt:Audience"] ?? string.Empty,
                    Secret = configuration["Auth:Jwt:Secret"] ?? string.Empty,
                    AccessTokenMinutes = int.TryParse(configuration["Auth:Jwt:AccessTokenMinutes"], out var accessMinutes)
                        ? accessMinutes
                        : 15,
                    RefreshTokenDays = int.TryParse(configuration["Auth:Jwt:RefreshTokenDays"], out var refreshDays)
                        ? refreshDays
                        : 7
                };
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrWhiteSpace(accessToken) &&
                            path.StartsWithSegments("/hubs/notifications"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        var userIdValue = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                        var securityStamp = context.Principal?.FindFirstValue("security_stamp");
                        if (!Guid.TryParse(userIdValue, out var userId) || string.IsNullOrWhiteSpace(securityStamp))
                        {
                            context.Fail("Invalid token identity.");
                            return;
                        }

                        var dbContext = context.HttpContext.RequestServices.GetRequiredService<IApplicationDbContext>();
                        var user = await dbContext.Users
                            .AsNoTracking()
                            .SingleOrDefaultAsync(candidate => candidate.Id == userId, context.HttpContext.RequestAborted);

                        if (user is null || !user.IsActive || user.SecurityStamp.ToString() != securityStamp)
                        {
                            context.Fail("Invalid token identity.");
                        }
                    }
                };
            });

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
            .AddPolicy(AuthPolicies.SystemAdmin, policy => policy.RequireRole(AuthPolicies.SystemAdmin))
            .AddPolicy(AuthPolicies.AcademicAdmin, policy => policy.RequireRole(AuthPolicies.AcademicAdmin, AuthPolicies.SystemAdmin))
            .AddPolicy(AuthPolicies.Teacher, policy => policy.RequireRole(AuthPolicies.Teacher))
            .AddPolicy(AuthPolicies.Parent, policy => policy.RequireRole(AuthPolicies.Parent))
            .AddPolicy(AuthPolicies.Student, policy => policy.RequireRole(AuthPolicies.Student))
            .AddPolicy(AuthPolicies.IntegrationService, policy => policy.RequireRole(AuthPolicies.IntegrationService));

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(AuthRateLimitPolicies.Login, context =>
            {
                var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                });
            });
        });

        return services;
    }
}
