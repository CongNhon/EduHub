using EduHub.Application.Contracts.School;
using EduHub.WebApi.Dtos.School;

namespace EduHub.WebApi.Mappings;

/// <summary>
/// Ghi chú: SchoolMappings chuyển hồ sơ trường Application sang DTO API.
/// </summary>
public static class SchoolMappings
{
    public static SchoolProfileDto ToDto(this SchoolProfileResponse response) =>
        new(response.Code, response.Name, response.LogoUrl, response.Address, response.Email, response.PhoneNumber);
}
