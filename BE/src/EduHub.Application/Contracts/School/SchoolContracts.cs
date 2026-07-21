using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;

namespace EduHub.Application.Contracts.School;

/// <summary>
/// Ghi chú: SchoolProfileResponse chứa nhận diện trường duy nhất đang vận hành hệ thống EduHub.
/// </summary>
public sealed record SchoolProfileResponse(string Code, string Name, string? LogoUrl, string? Address, string? Email, string? PhoneNumber);

/// <summary>
/// Ghi chú: GetSchoolProfileQuery đọc hồ sơ trường để hiển thị nhất quán trên portal, điểm và báo cáo.
/// </summary>
public sealed record GetSchoolProfileQuery : IQuery<Result<SchoolProfileResponse>>;
