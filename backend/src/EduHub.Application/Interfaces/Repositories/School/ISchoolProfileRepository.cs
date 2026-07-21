using EduHub.Application.Contracts.School;

namespace EduHub.Application.Interfaces.Repositories.School;

/// <summary>
/// Ghi chú: ISchoolProfileRepository đọc cấu hình trường duy nhất từ nguồn cấu hình backend.
/// </summary>
public interface ISchoolProfileRepository
{
    /// <summary>
    /// Ghi chú: GetProfile trả hồ sơ trường single-school từ nguồn cấu hình backend.
    /// </summary>
    SchoolProfileResponse GetProfile();
}
