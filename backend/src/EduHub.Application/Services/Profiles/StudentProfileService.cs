using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Profiles;
using EduHub.Application.Features.Profiles.Common;
using EduHub.Application.Features.Students.Common;
using EduHub.Application.Interfaces.Authentication;
using EduHub.Application.Interfaces.Data;
using EduHub.Application.Interfaces.Repositories.Profiles;
using EduHub.Application.Interfaces.Services.Profiles;
using EduHub.Domain.Entities.Students;
using EduHub.Domain.Enums;

namespace EduHub.Application.Services.Profiles;

/// <summary>
/// Ghi chú: StudentProfileService xử lý hồ sơ self-service, ảnh bằng chứng và duyệt thay đổi của học vụ.
/// </summary>
public sealed class StudentProfileService(
    IStudentProfileRepository repository,
    IProfileEvidenceStorage evidenceStorage,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : IStudentProfileService
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    /// <summary>
    /// Ghi chú: GetMyProfileAsync trả hồ sơ Student liên kết với tài khoản hiện tại, không nhận student id từ client.
    /// </summary>
    public async Task<Result<StudentSelfProfileResponse>> GetMyProfileAsync(GetMyStudentProfileQuery request, CancellationToken cancellationToken)
    {
        var student = await GetCurrentStudentAsync(cancellationToken);
        if (student is null) return Result.Failure<StudentSelfProfileResponse>(ProfileErrors.StudentAccountRequired);
        var enrollment = await repository.GetCurrentEnrollmentAsync(student.Id, cancellationToken);
        return Result.Success(ToSelfProfile(student, enrollment));
    }

    /// <summary>
    /// Ghi chú: CreateEvidenceUploadGrantAsync kiểm tra loại/kích thước ảnh và tạo URL upload riêng cho học sinh.
    /// </summary>
    public async Task<Result<EvidenceUploadGrantResponse>> CreateEvidenceUploadGrantAsync(CreateEvidenceUploadGrantCommand request, CancellationToken cancellationToken)
    {
        if (await GetCurrentStudentAsync(cancellationToken) is null || currentUser.UserId is null)
        {
            return Result.Failure<EvidenceUploadGrantResponse>(ProfileErrors.StudentAccountRequired);
        }

        if (!IsValidEvidence(request.ContentType, request.FileSize, request.FileName))
        {
            return Result.Failure<EvidenceUploadGrantResponse>(ProfileErrors.EvidenceInvalid);
        }

        var grant = await evidenceStorage.CreateUploadGrantAsync(currentUser.UserId.Value, request.FileName, request.ContentType, cancellationToken);
        return Result.Success(new EvidenceUploadGrantResponse(grant.ObjectKey, grant.UploadUrl, grant.ExpiresAtUtc, grant.UsesDirectCloudUpload));
    }

    /// <summary>
    /// Ghi chú: StoreLocalEvidenceAsync lưu ảnh fallback và chỉ chấp nhận object key thuộc tài khoản Student hiện tại.
    /// </summary>
    public async Task<Result> StoreLocalEvidenceAsync(StoreLocalProfileEvidenceCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null || await GetCurrentStudentAsync(cancellationToken) is null || !OwnsObjectKey(currentUser.UserId.Value, request.ObjectKey))
        {
            return Result.Failure(ProfileErrors.StudentAccountRequired);
        }

        if (!IsValidEvidence(request.ContentType, request.Content.LongLength, request.ObjectKey))
        {
            return Result.Failure(ProfileErrors.EvidenceInvalid);
        }

        await evidenceStorage.StoreLocalAsync(request.ObjectKey, request.ContentType, request.Content, cancellationToken);
        return Result.Success();
    }

    /// <summary>
    /// Ghi chú: GetLocalEvidenceAsync trả ảnh local cho chính học sinh sở hữu hoặc quản trị học vụ đang duyệt.
    /// </summary>
    public async Task<Result<ProfileEvidenceContentResponse>> GetLocalEvidenceAsync(GetLocalProfileEvidenceQuery request, CancellationToken cancellationToken)
    {
        var allowed = IsAcademicAdministrator() || currentUser.UserId.HasValue && OwnsObjectKey(currentUser.UserId.Value, request.ObjectKey);
        if (!allowed) return Result.Failure<ProfileEvidenceContentResponse>(ProfileErrors.ReviewForbidden);
        try
        {
            var content = await evidenceStorage.ReadLocalAsync(request.ObjectKey, cancellationToken);
            return Result.Success(new ProfileEvidenceContentResponse(content.Content, content.ContentType));
        }
        catch (FileNotFoundException)
        {
            return Result.Failure<ProfileEvidenceContentResponse>(ProfileErrors.EvidenceInvalid);
        }
    }

    /// <summary>
    /// Ghi chú: CreateRequestAsync tạo yêu cầu chờ duyệt và chặn spam nhiều yêu cầu mở của cùng học sinh.
    /// </summary>
    public async Task<Result<StudentProfileChangeRequestResponse>> CreateRequestAsync(CreateStudentProfileChangeRequestCommand request, CancellationToken cancellationToken)
    {
        var student = await GetCurrentStudentAsync(cancellationToken);
        if (student is null || currentUser.UserId is null)
        {
            return Result.Failure<StudentProfileChangeRequestResponse>(ProfileErrors.StudentAccountRequired);
        }

        if (!OwnsObjectKey(currentUser.UserId.Value, request.EvidenceObjectKey))
        {
            return Result.Failure<StudentProfileChangeRequestResponse>(ProfileErrors.EvidenceInvalid);
        }

        if (await repository.HasPendingRequestAsync(student.Id, cancellationToken))
        {
            return Result.Failure<StudentProfileChangeRequestResponse>(ProfileErrors.PendingRequestExists);
        }

        var profileRequest = new StudentProfileChangeRequest(
            student.Id,
            currentUser.UserId.Value,
            request.FullName,
            request.DateOfBirth,
            request.Gender,
            request.PhoneNumber,
            request.Address,
            request.Reason,
            request.EvidenceObjectKey);
        repository.AddRequest(profileRequest);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        var saved = await repository.GetRequestAsync(profileRequest.Id, cancellationToken);
        return Result.Success(await ToResponseAsync(saved!, cancellationToken));
    }

    /// <summary>
    /// Ghi chú: ListMyRequestsAsync trả lịch sử yêu cầu của đúng học sinh đang đăng nhập.
    /// </summary>
    public async Task<Result<IReadOnlyList<StudentProfileChangeRequestResponse>>> ListMyRequestsAsync(ListMyStudentProfileChangeRequestsQuery request, CancellationToken cancellationToken)
    {
        var student = await GetCurrentStudentAsync(cancellationToken);
        if (student is null) return Result.Failure<IReadOnlyList<StudentProfileChangeRequestResponse>>(ProfileErrors.StudentAccountRequired);
        return Result.Success(await ToResponsesAsync(await repository.ListRequestsAsync(student.Id, null, cancellationToken), cancellationToken));
    }

    /// <summary>
    /// Ghi chú: ListRequestsAsync cho quản trị học vụ xem queue yêu cầu và ảnh bằng chứng.
    /// </summary>
    public async Task<Result<IReadOnlyList<StudentProfileChangeRequestResponse>>> ListRequestsAsync(ListStudentProfileChangeRequestsQuery request, CancellationToken cancellationToken)
    {
        if (!IsAcademicAdministrator()) return Result.Failure<IReadOnlyList<StudentProfileChangeRequestResponse>>(ProfileErrors.ReviewForbidden);
        return Result.Success(await ToResponsesAsync(await repository.ListRequestsAsync(null, request.Status, cancellationToken), cancellationToken));
    }

    /// <summary>
    /// Ghi chú: ReviewRequestAsync áp dụng dữ liệu mới khi duyệt hoặc chỉ lưu lý do khi từ chối.
    /// </summary>
    public async Task<Result<StudentProfileChangeRequestResponse>> ReviewRequestAsync(ReviewStudentProfileChangeRequestCommand request, CancellationToken cancellationToken)
    {
        if (!IsAcademicAdministrator() || currentUser.UserId is null)
        {
            return Result.Failure<StudentProfileChangeRequestResponse>(ProfileErrors.ReviewForbidden);
        }

        var profileRequest = await repository.GetRequestAsync(request.RequestId, cancellationToken);
        if (profileRequest is null) return Result.Failure<StudentProfileChangeRequestResponse>(ProfileErrors.RequestNotFound);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (request.Approve)
        {
            profileRequest.Student.ApplyApprovedProfile(
                profileRequest.RequestedFullName,
                StudentNormalization.SearchText(profileRequest.RequestedFullName),
                profileRequest.RequestedDateOfBirth,
                profileRequest.RequestedGender,
                profileRequest.RequestedPhoneNumber,
                profileRequest.RequestedAddress,
                now);
            if (profileRequest.Student.User is not null)
            {
                var user = profileRequest.Student.User;
                user.UpdateProfile(
                    profileRequest.RequestedFullName,
                    user.ReferenceCode,
                    profileRequest.RequestedPhoneNumber,
                    user.Role,
                    user.IsActive,
                    now);
            }

            profileRequest.Approve(currentUser.UserId.Value, request.ReviewNote, now);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.ReviewNote))
            {
                return Result.Failure<StudentProfileChangeRequestResponse>(new Error("Profile.ReviewNoteRequired", "A rejection note is required.", ErrorType.Validation));
            }

            profileRequest.Reject(currentUser.UserId.Value, request.ReviewNote, now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        var saved = await repository.GetRequestAsync(profileRequest.Id, cancellationToken);
        return Result.Success(await ToResponseAsync(saved!, cancellationToken));
    }

    private async Task<Student?> GetCurrentStudentAsync(CancellationToken cancellationToken) =>
        currentUser.Role == UserRole.Student && currentUser.UserId.HasValue
            ? await repository.GetStudentByUserIdAsync(currentUser.UserId.Value, cancellationToken)
            : null;

    private bool IsAcademicAdministrator() => currentUser.Role is UserRole.AcademicAdmin or UserRole.SystemAdmin;

    private static bool OwnsObjectKey(Guid userId, string objectKey) =>
        objectKey.StartsWith($"profile-evidence/{userId:N}/", StringComparison.Ordinal);

    private static bool IsValidEvidence(string contentType, long fileSize, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedContentTypes.Contains(contentType) && fileSize is > 0 and <= 5 * 1024 * 1024 && extension is ".jpg" or ".jpeg" or ".png" or ".webp";
    }

    private static StudentSelfProfileResponse ToSelfProfile(Student student, IStudentProfileRepository.StudentCurrentEnrollment? enrollment) =>
        new(
            student.Id,
            student.StudentCode,
            student.FullName,
            student.DateOfBirth,
            student.Gender,
            student.PhoneNumber,
            student.Address,
            student.Status.ToString(),
            student.Version,
            enrollment?.ClassRoomId,
            enrollment?.ClassName,
            enrollment?.SemesterId,
            enrollment?.SemesterName);

    /// <summary>
    /// Ghi chú: ToResponsesAsync chuyển danh sách entity yêu cầu thành response có URL ảnh đọc tạm thời.
    /// </summary>
    private async Task<IReadOnlyList<StudentProfileChangeRequestResponse>> ToResponsesAsync(IReadOnlyList<StudentProfileChangeRequest> requests, CancellationToken cancellationToken)
    {
        var result = new List<StudentProfileChangeRequestResponse>(requests.Count);
        foreach (var request in requests) result.Add(await ToResponseAsync(request, cancellationToken));
        return result;
    }

    /// <summary>
    /// Ghi chú: ToResponseAsync tạo response so sánh dữ liệu hiện tại, dữ liệu đề nghị và URL bằng chứng.
    /// </summary>
    private async Task<StudentProfileChangeRequestResponse> ToResponseAsync(StudentProfileChangeRequest request, CancellationToken cancellationToken) =>
        new(
            request.Id,
            request.StudentId,
            request.Student.StudentCode,
            request.Student.FullName,
            request.Student.DateOfBirth,
            request.Student.Gender,
            request.Student.PhoneNumber,
            request.Student.Address,
            request.RequestedFullName,
            request.RequestedDateOfBirth,
            request.RequestedGender,
            request.RequestedPhoneNumber,
            request.RequestedAddress,
            request.Reason,
            await evidenceStorage.CreateReadUrlAsync(request.EvidenceObjectKey, cancellationToken),
            request.Status.ToString(),
            request.RequesterUser.FullName,
            request.ReviewerUser?.FullName,
            request.ReviewNote,
            request.CreatedAtUtc,
            request.ReviewedAtUtc);
}
