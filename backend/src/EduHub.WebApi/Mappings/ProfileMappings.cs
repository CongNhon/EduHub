using EduHub.Application.Contracts.Profiles;
using EduHub.WebApi.Dtos.Profiles;

namespace EduHub.WebApi.Mappings;

/// <summary>
/// Ghi chú: ProfileMappings chuyển DTO API với command/query/response của luồng sửa hồ sơ học sinh.
/// </summary>
public static class ProfileMappings
{
    public static CreateEvidenceUploadGrantCommand ToCommand(this CreateEvidenceUploadGrantRequest request) => new(request.FileName, request.ContentType, request.FileSize);
    public static StoreLocalProfileEvidenceCommand ToCommand(this StoreLocalEvidenceRequest request) => new(request.ObjectKey, request.ContentType, request.Content);
    public static GetLocalProfileEvidenceQuery ToQuery(this GetLocalEvidenceRequest request) => new(request.ObjectKey);
    public static CreateStudentProfileChangeRequestCommand ToCommand(this CreateStudentProfileChangeRequest request) => new(request.FullName, request.DateOfBirth, request.Gender, request.PhoneNumber, request.Address, request.Reason, request.EvidenceObjectKey);
    public static ListStudentProfileChangeRequestsQuery ToQuery(this ListStudentProfileChangeRequestsRequest request) => new(request.Status);
    public static ReviewStudentProfileChangeRequestCommand ToCommand(this ReviewStudentProfileChangeRequest request, Guid requestId) => new(requestId, request.Approve, request.ReviewNote);
    public static StudentSelfProfileDto ToDto(StudentSelfProfileResponse response) => new(
        response.StudentId,
        response.StudentCode,
        response.FullName,
        response.DateOfBirth,
        response.Gender,
        response.PhoneNumber,
        response.Address,
        response.Status,
        response.Version,
        response.CurrentClassId,
        response.CurrentClassName,
        response.CurrentSemesterId,
        response.CurrentSemesterName);
    public static EvidenceUploadGrantDto ToDto(EvidenceUploadGrantResponse response) => new(response.ObjectKey, response.UploadUrl, response.ExpiresAtUtc, response.UsesDirectCloudUpload);
    public static StudentProfileChangeRequestDto ToDto(StudentProfileChangeRequestResponse response) => new(
        response.Id,
        response.StudentId,
        response.StudentCode,
        response.CurrentFullName,
        response.CurrentDateOfBirth,
        response.CurrentGender,
        response.CurrentPhoneNumber,
        response.CurrentAddress,
        response.RequestedFullName,
        response.RequestedDateOfBirth,
        response.RequestedGender,
        response.RequestedPhoneNumber,
        response.RequestedAddress,
        response.Reason,
        response.EvidenceUrl,
        response.Status,
        response.RequesterName,
        response.ReviewerName,
        response.ReviewNote,
        response.RequestedAtUtc,
        response.ReviewedAtUtc);
}
