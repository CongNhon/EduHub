using EduHub.Application.Contracts.School;
using EduHub.Application.Interfaces.Repositories.School;
using EduHub.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace EduHub.Infrastructure.Repositories.School;

/// <summary>
/// Ghi chú: ConfigurationSchoolProfileRepository đọc hồ sơ trường single-school từ cấu hình runtime.
/// </summary>
public sealed class ConfigurationSchoolProfileRepository(IOptions<SchoolProfileOptions> options) : ISchoolProfileRepository
{
    /// <summary>
    /// Ghi chú: GetProfile ánh xạ cấu hình runtime thành hồ sơ trường trả cho Application.
    /// </summary>
    public SchoolProfileResponse GetProfile()
    {
        var value = options.Value;
        return new(value.Code, value.Name, value.LogoUrl, value.Address, value.Email, value.PhoneNumber);
    }
}
