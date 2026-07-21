namespace EduHub.WebApi.Dtos.Grades;

/// <summary>
/// Ghi chú: CreateGradeComponentItemRequest là DTO API cho một thành phần điểm trong cấu hình mới.
/// </summary>
public sealed record CreateGradeComponentItemRequest(
    string Name,
    decimal Weight,
    decimal MaxScore,
    int DisplayOrder,
    bool IsRequired,
    bool IncludeInGpa);

/// <summary>
/// Ghi chú: CreateGradeConfigurationRequest là DTO API dùng để tạo version cấu hình điểm cho subject-semester.
/// </summary>
public sealed record CreateGradeConfigurationRequest(
    Guid SubjectId,
    Guid SemesterId,
    IReadOnlyList<CreateGradeComponentItemRequest> Components);

/// <summary>
/// Ghi chú: ListGradeConfigurationsRequest là DTO API dùng để lọc và phân trang cấu hình điểm.
/// </summary>
public sealed record ListGradeConfigurationsRequest(Guid? SubjectId, Guid? SemesterId, bool? IsActive, int? Page, int? PageSize);

/// <summary>
/// Ghi chú: GradeComponentDto là DTO API chứa thông tin một thành phần điểm.
/// </summary>
public sealed record GradeComponentDto(
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
/// Ghi chú: GradeConfigurationDto là DTO API chứa version cấu hình điểm và danh sách component.
/// </summary>
public sealed record GradeConfigurationDto(
    Guid SubjectId,
    Guid SemesterId,
    int Version,
    bool IsActive,
    decimal TotalWeight,
    IReadOnlyList<GradeComponentDto> Components);

/// <summary>
/// Ghi chú: UpdateGradeRequest là DTO API dùng để tạo hoặc sửa điểm của một học sinh.
/// </summary>
public sealed record UpdateGradeRequest(Guid StudentId, Guid AssignmentId, Guid ComponentId, decimal Score, int? Version, string? Reason);

/// <summary>
/// Ghi chú: BulkUpdateGradeItemRequest là DTO API cho một dòng nhập điểm hàng loạt.
/// </summary>
public sealed record BulkUpdateGradeItemRequest(Guid StudentId, Guid ComponentId, decimal Score, int? Version, string? Reason);

/// <summary>
/// Ghi chú: BulkUpdateGradesRequest là DTO API dùng để nhập điểm hàng loạt cho assignment.
/// </summary>
public sealed record BulkUpdateGradesRequest(bool Atomic, IReadOnlyList<BulkUpdateGradeItemRequest> Items);

/// <summary>
/// Ghi chú: ReopenGradebookRequest là DTO API dùng để mở lại sổ điểm với lý do.
/// </summary>
public sealed record ReopenGradebookRequest(string Reason);

/// <summary>
/// Ghi chú: GradeEntryDto là DTO API chứa điểm của học sinh theo assignment-component.
/// </summary>
public sealed record GradeEntryDto(
    Guid Id,
    Guid StudentId,
    Guid AssignmentId,
    Guid ComponentId,
    decimal Score,
    string Status,
    int Version,
    int PublicationVersion);

/// <summary>
/// Ghi chú: BulkUpdateGradeItemDto là DTO API chứa kết quả một dòng nhập điểm hàng loạt.
/// </summary>
public sealed record BulkUpdateGradeItemDto(
    Guid StudentId,
    Guid ComponentId,
    bool Success,
    GradeEntryDto? Grade,
    string? ErrorCode,
    string? ErrorMessage);

/// <summary>
/// Ghi chú: BulkUpdateGradesDto là DTO API chứa tổng kết nhập điểm hàng loạt.
/// </summary>
public sealed record BulkUpdateGradesDto(IReadOnlyList<BulkUpdateGradeItemDto> Items, int SuccessCount, int ErrorCount);

/// <summary>
/// Ghi chú: GradebookStateDto là DTO API chứa trạng thái sổ điểm sau submit/publish/reopen/lock.
/// </summary>
public sealed record GradebookStateDto(Guid AssignmentId, string Status, int AffectedCount, int PublicationVersion);

/// <summary>
/// Ghi chú: GradebookComponentDto mô tả cột điểm trong editor sổ điểm.
/// </summary>
public sealed record GradebookComponentDto(Guid Id, string Name, decimal Weight, decimal MaxScore, int DisplayOrder, bool IsRequired);

/// <summary>
/// Ghi chú: GradebookCellDto trả một ô điểm cùng version để cập nhật an toàn.
/// </summary>
public sealed record GradebookCellDto(Guid ComponentId, decimal? Score, string Status, int? Version, int PublicationVersion);

/// <summary>
/// Ghi chú: GradebookStudentDto trả một dòng học sinh, điểm và nhận xét trong sổ điểm.
/// </summary>
public sealed record GradebookStudentDto(Guid StudentId, string StudentCode, string FullName, string? Remark, int? RemarkVersion, IReadOnlyList<GradebookCellDto> Grades);

/// <summary>
/// Ghi chú: GradebookDto trả bounded read model đầy đủ cho editor giáo viên.
/// </summary>
public sealed record GradebookDto(Guid AssignmentId, Guid ClassRoomId, string ClassCode, string ClassName, Guid SubjectId, string SubjectCode, string SubjectName, Guid SemesterId, string SemesterName, Guid TeacherId, string TeacherName, string Status, IReadOnlyList<GradebookComponentDto> Components, IReadOnlyList<GradebookStudentDto> Students);

/// <summary>
/// Ghi chú: UpdateStudentRemarkRequest chứa nội dung và version nhận xét giáo viên cần lưu.
/// </summary>
public sealed record UpdateStudentRemarkRequest(string Content, int? Version);

/// <summary>
/// Ghi chú: StudentRemarkDto trả nhận xét môn học sau khi giáo viên lưu.
/// </summary>
public sealed record StudentRemarkDto(Guid Id, Guid AssignmentId, Guid StudentId, string Content, int Version, bool IsPublished);

/// <summary>
/// Ghi chú: GetPublishedGradesRequest là DTO route để phụ huynh đọc điểm đã công bố của học sinh trong assignment.
/// </summary>
public sealed record GetPublishedGradesRequest(Guid StudentId, Guid AssignmentId);

/// <summary>
/// Ghi chú: PublishedGradeEntryDto là DTO điểm đã công bố của một component cho phụ huynh xem.
/// </summary>
public sealed record PublishedGradeEntryDto(Guid ComponentId, string ComponentName, decimal Weight, decimal MaxScore, decimal Score, int PublicationVersion);

/// <summary>
/// Ghi chú: PublishedGradebookDto là DTO danh sách điểm đã công bố của học sinh trong assignment.
/// </summary>
public sealed record PublishedGradebookDto(Guid StudentId, string StudentCode, string StudentName, Guid AssignmentId, string ClassCode, string ClassName, string SubjectCode, string SubjectName, string SemesterName, string TeacherName, DateTime? PublishedAtUtc, string? Remark, IReadOnlyList<PublishedGradeEntryDto> Grades);
