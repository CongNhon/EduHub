using EduHub.Application.Common.Models;

namespace EduHub.Application.Features.Profiles.Common;

/// <summary>
/// Ghi chú: ProfileErrors tập trung lỗi hồ sơ học sinh, ảnh bằng chứng và trạng thái yêu cầu duyệt.
/// </summary>
public static class ProfileErrors
{
    public static readonly Error StudentAccountRequired = new("Profile.StudentAccountRequired", "A linked student account is required.", ErrorType.Forbidden);
    public static readonly Error StudentNotFound = new("Profile.StudentNotFound", "Student profile was not found.", ErrorType.NotFound);
    public static readonly Error PendingRequestExists = new("Profile.PendingRequestExists", "The student already has a pending profile change request.", ErrorType.Conflict);
    public static readonly Error EvidenceInvalid = new("Profile.EvidenceInvalid", "Evidence must be a JPG, PNG or WebP image no larger than 5 MB.", ErrorType.Validation);
    public static readonly Error RequestNotFound = new("Profile.RequestNotFound", "Profile change request was not found.", ErrorType.NotFound);
    public static readonly Error ReviewForbidden = new("Profile.ReviewForbidden", "Academic administrator role is required to review profile changes.", ErrorType.Forbidden);
}
