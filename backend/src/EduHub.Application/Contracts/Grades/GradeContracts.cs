using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;

namespace EduHub.Application.Contracts.Grades;

/// <summary>
/// Ghi chú: CreateGradeComponentItem là dữ liệu một thành phần điểm trong request tạo cấu hình điểm.
/// </summary>
public sealed record CreateGradeComponentItem(
    string Name,
    decimal Weight,
    decimal MaxScore,
    int DisplayOrder,
    bool IsRequired,
    bool IncludeInGpa);

/// <summary>
/// Ghi chú: GradeComponentResponse là dữ liệu trả về cho một thành phần điểm.
/// </summary>
public sealed record GradeComponentResponse(
    Guid Id,
    Guid SubjectId,
    Guid SemesterId,
    string Name,
    decimal Weight,
    decimal MaxScore,
    int DisplayOrder,
    bool IsRequired,
    bool IncludeInGpa,
    int Version,
    bool IsActive);

/// <summary>
/// Ghi chú: GradeConfigurationResponse là dữ liệu trả về cho một version cấu hình điểm của subject-semester.
/// </summary>
public sealed record GradeConfigurationResponse(
    Guid SubjectId,
    Guid SemesterId,
    int Version,
    bool IsActive,
    decimal TotalWeight,
    IReadOnlyList<GradeComponentResponse> Components);

/// <summary>
/// Ghi chú: CreateGradeConfigurationCommand là command tạo version cấu hình thành phần điểm cho subject-semester.
/// </summary>
public sealed record CreateGradeConfigurationCommand(
    Guid SubjectId,
    Guid SemesterId,
    IReadOnlyList<CreateGradeComponentItem> Components) : ICommand<Result<GradeConfigurationResponse>>;

/// <summary>
/// Ghi chú: ListGradeConfigurationsQuery là query đọc cấu hình thành phần điểm theo subject-semester.
/// </summary>
public sealed record ListGradeConfigurationsQuery(
    Guid? SubjectId = null,
    Guid? SemesterId = null,
    bool? IsActive = null,
    int Page = PageRequest.DefaultPage,
    int PageSize = PageRequest.DefaultPageSize) : IQuery<Result<PagedResult<GradeConfigurationResponse>>>;

/// <summary>
/// Ghi chú: GradeComponentScoreInput là điểm của một thành phần dùng để tính trung bình môn.
/// </summary>
public sealed record GradeComponentScoreInput(decimal? Score, decimal Weight, bool IsRequired, bool IncludeInGpa = true);

/// <summary>
/// Ghi chú: SubjectAverageResult là kết quả tính trung bình môn, có thể unavailable nếu cấu hình/điểm thiếu.
/// </summary>
public sealed record SubjectAverageResult(bool IsAvailable, decimal? Average, string? ErrorCode);

/// <summary>
/// Ghi chú: SubjectGradeForGpaInput là điểm trung bình môn và tín chỉ dùng để tính GPA học kỳ.
/// </summary>
public sealed record SubjectGradeForGpaInput(decimal? SubjectAverage, int Credits, bool IncludeInGpa = true);

/// <summary>
/// Ghi chú: SemesterGpaResult là kết quả tính GPA học kỳ, có thể unavailable nếu thiếu điểm hoặc không có tín chỉ hợp lệ.
/// </summary>
public sealed record SemesterGpaResult(bool IsAvailable, decimal? Gpa, string? ErrorCode);

/// <summary>
/// Ghi chú: ClassificationThreshold là ngưỡng xếp loại học lực theo policy version.
/// </summary>
public sealed record ClassificationThreshold(string Name, decimal MinimumGpa);

/// <summary>
/// Ghi chú: ClassificationPolicy là cấu hình version/effective date dùng để xếp loại học lực.
/// </summary>
public sealed record ClassificationPolicy(
    string Version,
    DateOnly EffectiveFrom,
    IReadOnlyList<ClassificationThreshold> Thresholds);

/// <summary>
/// Ghi chú: ClassificationResult là kết quả xếp loại GPA theo policy version đã dùng.
/// </summary>
public sealed record ClassificationResult(string Name, string PolicyVersion, DateOnly PolicyEffectiveFrom);

/// <summary>
/// Ghi chú: GradeEntryResponse là dữ liệu trả về cho điểm của một học sinh theo assignment-component.
/// </summary>
public sealed record GradeEntryResponse(
    Guid Id,
    Guid StudentId,
    Guid AssignmentId,
    Guid ComponentId,
    decimal Score,
    string Status,
    int Version,
    int PublicationVersion);

/// <summary>
/// Ghi chú: UpdateGradeCommand là command tạo hoặc sửa điểm Draft cho một học sinh.
/// </summary>
public sealed record UpdateGradeCommand(
    Guid StudentId,
    Guid AssignmentId,
    Guid ComponentId,
    decimal Score,
    int? Version,
    string? Reason) : ICommand<Result<GradeEntryResponse>>;

/// <summary>
/// Ghi chú: BulkUpdateGradeItem là một dòng điểm trong request nhập điểm hàng loạt.
/// </summary>
public sealed record BulkUpdateGradeItem(Guid StudentId, Guid ComponentId, decimal Score, int? Version, string? Reason);

/// <summary>
/// Ghi chú: BulkUpdateGradesCommand là command nhập điểm hàng loạt cho một assignment.
/// </summary>
public sealed record BulkUpdateGradesCommand(
    Guid AssignmentId,
    bool Atomic,
    IReadOnlyList<BulkUpdateGradeItem> Items) : ICommand<Result<BulkUpdateGradesResponse>>;

/// <summary>
/// Ghi chú: BulkUpdateGradeItemResponse là kết quả từng dòng khi nhập điểm hàng loạt.
/// </summary>
public sealed record BulkUpdateGradeItemResponse(
    Guid StudentId,
    Guid ComponentId,
    bool Success,
    GradeEntryResponse? Grade,
    string? ErrorCode,
    string? ErrorMessage);

/// <summary>
/// Ghi chú: BulkUpdateGradesResponse là tổng kết nhập điểm hàng loạt.
/// </summary>
public sealed record BulkUpdateGradesResponse(
    IReadOnlyList<BulkUpdateGradeItemResponse> Items,
    int SuccessCount,
    int ErrorCount);

/// <summary>
/// Ghi chú: SubmitGradebookCommand là command giáo viên nộp sổ điểm của assignment.
/// </summary>
public sealed record SubmitGradebookCommand(Guid AssignmentId) : ICommand<Result<GradebookStateResponse>>;

/// <summary>
/// Ghi chú: PublishGradebookCommand là command học vụ công bố sổ điểm của assignment.
/// </summary>
public sealed record PublishGradebookCommand(Guid AssignmentId) : ICommand<Result<GradebookStateResponse>>;

/// <summary>
/// Ghi chú: ReopenGradebookCommand là command học vụ mở lại sổ điểm để chỉnh sửa.
/// </summary>
public sealed record ReopenGradebookCommand(Guid AssignmentId, string Reason) : ICommand<Result<GradebookStateResponse>>;

/// <summary>
/// Ghi chú: LockGradebookCommand là command khóa sổ điểm đã published.
/// </summary>
public sealed record LockGradebookCommand(Guid AssignmentId) : ICommand<Result<GradebookStateResponse>>;

/// <summary>
/// Ghi chú: GradebookStateResponse là dữ liệu trả về sau submit/publish/reopen/lock assignment gradebook.
/// </summary>
public sealed record GradebookStateResponse(
    Guid AssignmentId,
    string Status,
    int AffectedCount,
    int PublicationVersion);

/// <summary>
/// Ghi chú: GradebookComponentResponse mô tả một cột điểm trong editor sổ điểm giáo viên.
/// </summary>
public sealed record GradebookComponentResponse(Guid Id, string Name, decimal Weight, decimal MaxScore, int DisplayOrder, bool IsRequired);

/// <summary>
/// Ghi chú: GradebookCellResponse chứa điểm và version của một học sinh tại một thành phần điểm.
/// </summary>
public sealed record GradebookCellResponse(Guid ComponentId, decimal? Score, string Status, int? Version, int PublicationVersion);

/// <summary>
/// Ghi chú: GradebookStudentResponse chứa một dòng học sinh, các ô điểm và nhận xét của giáo viên.
/// </summary>
public sealed record GradebookStudentResponse(Guid StudentId, string StudentCode, string FullName, string? Remark, int? RemarkVersion, IReadOnlyList<GradebookCellResponse> Grades);

/// <summary>
/// Ghi chú: GradebookResponse chứa đầy đủ context lớp-môn-học kỳ, components và roster cho editor sổ điểm.
/// </summary>
public sealed record GradebookResponse(Guid AssignmentId, Guid ClassRoomId, string ClassCode, string ClassName, Guid SubjectId, string SubjectCode, string SubjectName, Guid SemesterId, string SemesterName, Guid TeacherId, string TeacherName, string Status, IReadOnlyList<GradebookComponentResponse> Components, IReadOnlyList<GradebookStudentResponse> Students);

/// <summary>
/// Ghi chú: GetGradebookQuery đọc sổ điểm nếu user là giáo viên được phân công hoặc quản trị học vụ.
/// </summary>
public sealed record GetGradebookQuery(Guid AssignmentId) : IQuery<Result<GradebookResponse>>;

/// <summary>
/// Ghi chú: UpdateStudentRemarkCommand tạo hoặc sửa nhận xét môn học cho một học sinh trong assignment.
/// </summary>
public sealed record UpdateStudentRemarkCommand(Guid AssignmentId, Guid StudentId, string Content, int? Version) : ICommand<Result<StudentRemarkResponse>>;

/// <summary>
/// Ghi chú: StudentRemarkResponse trả nội dung, version và trạng thái công bố của nhận xét học sinh.
/// </summary>
public sealed record StudentRemarkResponse(Guid Id, Guid AssignmentId, Guid StudentId, string Content, int Version, bool IsPublished);

/// <summary>
/// Ghi chú: PublishedGradeEntryResponse là dữ liệu điểm đã công bố của một component cho phụ huynh xem.
/// </summary>
public sealed record PublishedGradeEntryResponse(
    Guid ComponentId,
    string ComponentName,
    decimal Weight,
    decimal MaxScore,
    decimal Score,
    int PublicationVersion);

/// <summary>
/// Ghi chú: PublishedGradebookResponse là danh sách điểm đã công bố của một học sinh trong assignment.
/// </summary>
public sealed record PublishedGradebookResponse(
    Guid StudentId,
    string StudentCode,
    string StudentName,
    Guid AssignmentId,
    string ClassCode,
    string ClassName,
    string SubjectCode,
    string SubjectName,
    string SemesterName,
    string TeacherName,
    DateTime? PublishedAtUtc,
    string? Remark,
    IReadOnlyList<PublishedGradeEntryResponse> Grades);

/// <summary>
/// Ghi chú: GetPublishedGradesForParentQuery là query phụ huynh đọc điểm đã published/locked của con trong assignment.
/// </summary>
public sealed record GetPublishedGradesForParentQuery(Guid StudentId, Guid AssignmentId)
    : IQuery<Result<PublishedGradebookResponse>>;
