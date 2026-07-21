using Carter;
using EduHub.WebApi.Dtos.Auth;
using EduHub.WebApi.Extensions;
using EduHub.WebApi.Mappings;
using EduHub.WebApi.Security;
using MediatR;

namespace EduHub.WebApi.Modules.Auth;

/// <summary>
/// Ghi chú: AuthModule đăng ký các endpoint API cho auth.
/// </summary>
public sealed class AuthModule : ICarterModule
{
    /// <summary>
    /// Ghi chú: AddRoutes đăng ký route HTTP cho auth.
    /// </summary>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth").WithTags("Auth");

        group.MapPost("/login", async (LoginRequest request, ISender sender, CancellationToken cancellationToken) =>
            (await sender.Send(request.ToCommand(), cancellationToken)).ToHttpResult(AuthMappings.ToDto))
            .AllowAnonymous()
            .RequireRateLimiting(AuthRateLimitPolicies.Login)
            .WithName("Login");

        group.MapPost("/refresh", async (RefreshTokenRequest request, ISender sender, CancellationToken cancellationToken) =>
            (await sender.Send(request.ToCommand(), cancellationToken)).ToHttpResult(AuthMappings.ToDto))
            .AllowAnonymous()
            .WithName("RefreshToken");

        group.MapPost("/logout", async (LogoutRequest request, ISender sender, CancellationToken cancellationToken) =>
            (await sender.Send(request.ToCommand(), cancellationToken)).ToHttpResult())
            .AllowAnonymous()
            .RequireRateLimiting(AuthRateLimitPolicies.Login)
            .WithName("Logout");

        group.MapGet("/me", async (ISender sender, CancellationToken cancellationToken) =>
            (await sender.Send(new CurrentUserRequest().ToQuery(), cancellationToken)).ToHttpResult(AuthMappings.ToDto))
            .WithName("GetCurrentUser");
    }
}
