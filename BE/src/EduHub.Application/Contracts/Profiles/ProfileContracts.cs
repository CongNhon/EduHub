using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;
using EduHub.Domain.Enums;

namespace EduHub.Application.Contracts.Profiles;

/// <summary>
/// Ghi chú: StudentSelfProfileResponse trả hồ sơ cá nhân của học sinh đang đăng nhập.
/// </summary>
public sealed record StudentSelfProfileResponse(
    Guid StudentId,
    string StudentCode,
    string FullName,
    DateOnly DateOfBirth,
    string? Gender,
    string? PhoneNumber,
    string? Address,
    string Status,
    int Version,
    Guid? CurrentClassId,
    string? CurrentClassName,
    Guid? CurrentSemesterId,
    string? CurrentSemesterName);

/// <summary>
/// Ghi chú: GetMyStudentProfileQuery đọc hồ sơ liên kết với tài khoản Student hiện tại.
/// </summary>
public sealed record GetMyStudentProfileQuery : IQuery<Result<StudentSelfProfileResponse>>;

/// <summary>
/// Ghi chú: EvidenceUploadGrantResponse trả object key và URL upload ảnh bằng chứng có thời hạn.
/// </summary>
public sealed record EvidenceUploadGrantResponse(string ObjectKey, string UploadUrl, DateTime ExpiresAtUtc, bool UsesDirectCloudUpload);

/// <summary>
/// Ghi chú: CreateEvidenceUploadGrantCommand xin URL upload ảnh bằng chứng tối đa 5 MB cho học sinh hiện tại.
/// </summary>
public sealed record CreateEvidenceUploadGrantCommand(string FileName, string ContentType, long FileSize)
    : ICommand<Result<EvidenceUploadGrantResponse>>;

/// <summary>
/// Ghi chú: StoreLocalProfileEvidenceCommand lưu ảnh vào storage local khi môi trường chưa cấu hình Cloudflare R2.
/// </summary>
public sealed record StoreLocalProfileEvidenceCommand(string ObjectKey, string ContentType, byte[] Content)
    : ICommand<Result>;

/// <summary>
/// Ghi chú: ProfileEvidenceContentResponse trả byte ảnh local và content type cho endpoint tải bằng chứng.
/// </summary>
public sealed record ProfileEvidenceContentResponse(byte[] Content, string ContentType);

/// <summary>
/// Ghi chú: GetLocalProfileEvidenceQuery đọc ảnh local sau khi kiểm tra quyền học sinh hoặc học vụ.
/// </summary>
public sealed record GetLocalProfileEvidenceQuery(string ObjectKey)
    : IQuery<Result<ProfileEvidenceContentResponse>>;

/// <summary>
/// Ghi chú: StudentProfileChangeRequestResponse trả thông tin cũ, thông tin đề nghị và kết quả duyệt hồ sơ.
/// </summary>
public sealed record StudentProfileChangeRequestResponse(
    Guid Id,
    Guid StudentId,
    string StudentCode,
    string CurrentFullName,
    DateOnly CurrentDateOfBirth,
    string? CurrentGender,
    string? CurrentPhoneNumber,
    string? CurrentAddress,
    string RequestedFullName,
    DateOnly RequestedDateOfBirth,
    string? RequestedGender,
    string? RequestedPhoneNumber,
    string? RequestedAddress,
    string Reason,
    string EvidenceUrl,
    string Status,
    string RequesterName,
    string? ReviewerName,
    string? ReviewNote,
    DateTime RequestedAtUtc,
    DateTime? ReviewedAtUtc);

/// <summary>
/// Ghi chú: CreateStudentProfileChangeRequestCommand gửi thông tin mới và object key ảnh bằng chứng cho học vụ duyệt.
/// </summary>
public sealed record CreateStudentProfileChangeRequestCommand(
    string FullName,
    DateOnly DateOfBirth,
    string? Gender,
    string? PhoneNumber,
    string? Address,
    string Reason,
    string EvidenceObjectKey)
    : ICommand<Result<StudentProfileChangeRequestResponse>>;

/// <summary>
/// Ghi chú: ListMyStudentProfileChangeRequestsQuery đọc lịch sử yêu cầu của học sinh hiện tại.
/// </summary>
public sealed record ListMyStudentProfileChangeRequestsQuery : IQuery<Result<IReadOnlyList<StudentProfileChangeRequestResponse>>>;

/// <summary>
/// Ghi chú: ListStudentProfileChangeRequestsQuery cho học vụ lọc yêu cầu theo trạng thái duyệt.
/// </summary>
public sealed record ListStudentProfileChangeRequestsQuery(ProfileChangeRequestStatus? Status)
    : IQuery<Result<IReadOnlyList<StudentProfileChangeRequestResponse>>>;

/// <summary>
/// Ghi chú: ReviewStudentProfileChangeRequestCommand duyệt hoặc từ chối yêu cầu sửa hồ sơ học sinh.
/// </summary>
public sealed record ReviewStudentProfileChangeRequestCommand(Guid RequestId, bool Approve, string? ReviewNote)
    : ICommand<Result<StudentProfileChangeRequestResponse>>;
