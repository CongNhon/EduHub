using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.Reports;
using EduHub.Domain.Entities.Integration;
using EduHub.Domain.Entities.Reports;
using EduHub.Domain.Enums;

namespace EduHub.Application.Interfaces.Repositories.Reports;

/// <summary>
/// Ghi chú: IReportJobRepository truy cập report job và quyền parent-student cho PDF.
/// </summary>
public interface IReportJobRepository
{
    /// <summary>
    /// Ghi chú: ParentCanReadStudentAsync kiểm tra requester parent có quyền tạo/tải report của học sinh không.
    /// </summary>
    Task<bool> ParentCanReadStudentAsync(Guid parentUserId, Guid studentId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: StudentExistsAsync kiểm tra học sinh tồn tại trước khi tạo report.
    /// </summary>
    Task<bool> StudentExistsAsync(Guid studentId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: SemesterExistsAsync kiểm tra học kỳ tồn tại trước khi tạo report.
    /// </summary>
    Task<bool> SemesterExistsAsync(Guid semesterId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: StudentWasEnrolledAsync kiểm tra học sinh từng được ghi danh trong học kỳ yêu cầu báo cáo.
    /// </summary>
    Task<bool> StudentWasEnrolledAsync(Guid studentId, Guid semesterId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: StudentHasPublishedGradesAsync kiểm tra học sinh có điểm Published/Locked trong học kỳ báo cáo.
    /// </summary>
    Task<bool> StudentHasPublishedGradesAsync(Guid studentId, Guid semesterId, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetByRequesterAndIdempotencyKeyAsync lấy report job idempotent theo requester/key.
    /// </summary>
    Task<ReportJob?> GetByRequesterAndIdempotencyKeyAsync(
        Guid requesterUserId,
        string idempotencyKey,
        CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: GetAsync lấy report job theo id.
    /// </summary>
    Task<ReportJob?> GetAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Ghi chú: Add thêm report job mới vào DbContext.
    /// </summary>
    void Add(ReportJob reportJob);

    /// <summary>
    /// Ghi chú: HasOpenRequestAsync kiểm tra phụ huynh đã có yêu cầu chưa kết thúc cho học sinh-học kỳ hay chưa.
    /// </summary>
    Task<bool> HasOpenRequestAsync(Guid requesterUserId, Guid studentId, Guid semesterId, CancellationToken cancellationToken);
    Task<ReportRequest?> GetRequestAsync(Guid id, CancellationToken cancellationToken);
    Task<ReportRequestResponse?> GetRequestResponseAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<ReportRequestResponse>> ListRequestsAsync(Guid currentUserId, UserRole role, ReportRequestStatus? status, PageRequest pageRequest, CancellationToken cancellationToken);
    void AddRequest(ReportRequest request);
    void AddOutboxMessage(OutboxMessage message);
}
