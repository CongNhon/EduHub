using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.School;
using EduHub.Application.Interfaces.Repositories.School;
using EduHub.Application.Interfaces.Services.School;

namespace EduHub.Application.Services.School;

/// <summary>
/// Ghi chú: SchoolProfileService trả hồ sơ trường duy nhất từ repository cấu hình.
/// </summary>
public sealed class SchoolProfileService(ISchoolProfileRepository repository) : ISchoolProfileService
{
    /// <summary>
    /// Ghi chú: GetProfile trả hồ sơ trường single-school cho query API hiện tại.
    /// </summary>
    public Result<SchoolProfileResponse> GetProfile() => Result.Success(repository.GetProfile());
}
