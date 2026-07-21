using EduHub.Domain.Enums;

namespace EduHub.WebApi.Dtos.Profiles;

/// <summary>
/// Ghi chú: StudentSelfProfileDto trả hồ sơ cá nhân cho tài khoản học sinh.
/// </summary>
public sealed record StudentSelfProfileDto(
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
/// Ghi chú: CreateEvidenceUploadGrantRequest là DTO xin URL upload ảnh bằng chứng.
/// </summary>
public sealed record CreateEvidenceUploadGrantRequest(string FileName, string ContentType, long FileSize);

/// <summary>
/// Ghi chú: EvidenceUploadGrantDto trả object key và URL upload ảnh có thời hạn.
/// </summary>
public sealed record EvidenceUploadGrantDto(string ObjectKey, string UploadUrl, DateTime ExpiresAtUtc, bool UsesDirectCloudUpload);

/// <summary>
/// Ghi chú: StoreLocalEvidenceRequest chứa byte ảnh fallback local đã đọc từ HTTP request.
/// </summary>
public sealed record StoreLocalEvidenceRequest(string ObjectKey, string ContentType, byte[] Content);

/// <summary>
/// Ghi chú: GetLocalEvidenceRequest chứa object key ảnh local cần đọc.
/// </summary>
public sealed record GetLocalEvidenceRequest(string ObjectKey);

/// <summary>
/// Ghi chú: CreateStudentProfileChangeRequest là DTO gửi thông tin mới và ảnh bằng chứng để học vụ duyệt.
/// </summary>
public sealed record CreateStudentProfileChangeRequest(string FullName, DateOnly DateOfBirth, string? Gender, string? PhoneNumber, string? Address, string Reason, string EvidenceObjectKey);

/// <summary>
/// Ghi chú: ListStudentProfileChangeRequestsRequest là bộ lọc queue yêu cầu theo trạng thái.
/// </summary>
public sealed record ListStudentProfileChangeRequestsRequest(ProfileChangeRequestStatus? Status);

/// <summary>
/// Ghi chú: ReviewStudentProfileChangeRequest là DTO duyệt/từ chối và ghi chú của học vụ.
/// </summary>
public sealed record ReviewStudentProfileChangeRequest(bool Approve, string? ReviewNote);

/// <summary>
/// Ghi chú: StudentProfileChangeRequestDto trả dữ liệu hiện tại, dữ liệu đề nghị, ảnh và kết quả duyệt.
/// </summary>
public sealed record StudentProfileChangeRequestDto(
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
