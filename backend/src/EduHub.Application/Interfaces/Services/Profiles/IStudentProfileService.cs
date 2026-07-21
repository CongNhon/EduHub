using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Profiles;

namespace EduHub.Application.Interfaces.Services.Profiles;

/// <summary>
/// Ghi chú: IStudentProfileService là interface hồ sơ self-service, ảnh bằng chứng và luồng học vụ duyệt thay đổi.
/// </summary>
public interface IStudentProfileService
{
    Task<Result<StudentSelfProfileResponse>> GetMyProfileAsync(GetMyStudentProfileQuery request, CancellationToken cancellationToken);
    Task<Result<EvidenceUploadGrantResponse>> CreateEvidenceUploadGrantAsync(CreateEvidenceUploadGrantCommand request, CancellationToken cancellationToken);
    Task<Result> StoreLocalEvidenceAsync(StoreLocalProfileEvidenceCommand request, CancellationToken cancellationToken);
    Task<Result<ProfileEvidenceContentResponse>> GetLocalEvidenceAsync(GetLocalProfileEvidenceQuery request, CancellationToken cancellationToken);
    Task<Result<StudentProfileChangeRequestResponse>> CreateRequestAsync(CreateStudentProfileChangeRequestCommand request, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<StudentProfileChangeRequestResponse>>> ListMyRequestsAsync(ListMyStudentProfileChangeRequestsQuery request, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<StudentProfileChangeRequestResponse>>> ListRequestsAsync(ListStudentProfileChangeRequestsQuery request, CancellationToken cancellationToken);
    Task<Result<StudentProfileChangeRequestResponse>> ReviewRequestAsync(ReviewStudentProfileChangeRequestCommand request, CancellationToken cancellationToken);
}
