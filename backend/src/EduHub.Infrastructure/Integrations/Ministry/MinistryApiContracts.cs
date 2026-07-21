namespace EduHub.Infrastructure.Integrations.Ministry;

/// <summary>
/// Ghi chú: MinistryGradebookRequest là payload versioned gửi sổ điểm đã publish/lock sang Ministry API.
/// </summary>
public sealed record MinistryGradebookRequest(
    string ContractVersion,
    Guid AssignmentId,
    int PublicationVersion,
    IReadOnlyList<MinistryGradeItem> Grades);

/// <summary>
/// Ghi chú: MinistryGradeItem là một dòng điểm của học sinh trong payload gửi Ministry API.
/// </summary>
public sealed record MinistryGradeItem(Guid StudentId, Guid ComponentId, decimal Score);

/// <summary>
/// Ghi chú: MinistryGradebookResponse là response Ministry API trả về sau khi nhận sổ điểm.
/// </summary>
public sealed record MinistryGradebookResponse(string ExternalId, string ExternalVersion);
