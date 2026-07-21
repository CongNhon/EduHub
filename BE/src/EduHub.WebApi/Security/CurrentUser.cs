using System.Security.Claims;
using EduHub.Application.Interfaces.Authentication;
using EduHub.Domain.Enums;

namespace EduHub.WebApi.Security;

/// <summary>
/// Ghi chú: CurrentUser đại diện cho người dùng hiện tại đọc từ HttpContext/JWT trong hệ thống EduHub.
/// </summary>
public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public Guid? UserId =>
        Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;

    public UserRole? Role =>
        Enum.TryParse<UserRole>(Principal?.FindFirstValue(ClaimTypes.Role), out var role) ? role : null;

    public string? SecurityStamp => Principal?.FindFirstValue("security_stamp");
}
