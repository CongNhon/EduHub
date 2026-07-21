using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.School;

namespace EduHub.Application.Interfaces.Services.School;

/// <summary>
/// Ghi chú: ISchoolProfileService cung cấp hồ sơ trường cho các feature cần ngữ cảnh single-school.
/// </summary>
public interface ISchoolProfileService
{
    Result<SchoolProfileResponse> GetProfile();
}
