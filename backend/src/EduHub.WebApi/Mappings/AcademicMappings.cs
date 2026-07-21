using EduHub.Application.Contracts.Academics;
using EduHub.WebApi.Dtos.Academics;

namespace EduHub.WebApi.Mappings;

/// <summary>
/// Ghi chú: AcademicMappings chứa mapping giữa Academic DTO của API và command/query/response của Application.
/// </summary>
public static class AcademicMappings
{
    /// <summary>
    /// Ghi chú: ToCommand chuyển CreateAcademicYearRequest API thành CreateAcademicYearCommand application.
    /// </summary>
    public static CreateAcademicYearCommand ToCommand(this CreateAcademicYearRequest request) =>
        new(request.Name, request.StartDate, request.EndDate);

    /// <summary>
    /// Ghi chú: ToQuery chuyển ListAcademicYearsRequest API thành ListAcademicYearsQuery application.
    /// </summary>
    public static ListAcademicYearsQuery ToQuery(this ListAcademicYearsRequest request) =>
        new(request.Page ?? 1, request.PageSize ?? 20, request.Search);

    /// <summary>
    /// Ghi chú: ToDto chuyển AcademicYearResponse application thành AcademicYearDto API.
    /// </summary>
    public static AcademicYearDto ToDto(this AcademicYearResponse response) =>
        new(response.Id, response.Name, response.StartDate, response.EndDate, response.Status);

    /// <summary>
    /// Ghi chú: ToCommand chuyển CreateSemesterRequest API thành CreateSemesterCommand application.
    /// </summary>
    public static CreateSemesterCommand ToCommand(this CreateSemesterRequest request) =>
        new(
            request.AcademicYearId,
            request.Name,
            request.StartDate,
            request.EndDate,
            request.GradeEntryFrom,
            request.GradeEntryTo);

    /// <summary>
    /// Ghi chú: ToQuery chuyển ListSemestersRequest API thành ListSemestersQuery application.
    /// </summary>
    public static ListSemestersQuery ToQuery(this ListSemestersRequest request) =>
        new(request.AcademicYearId, request.Page ?? 1, request.PageSize ?? 20, request.Search);

    /// <summary>
    /// Ghi chú: ToDto chuyển SemesterResponse application thành SemesterDto API.
    /// </summary>
    public static SemesterDto ToDto(this SemesterResponse response) =>
        new(
            response.Id,
            response.AcademicYearId,
            response.Name,
            response.StartDate,
            response.EndDate,
            response.GradeEntryFrom,
            response.GradeEntryTo,
            response.Status);

    /// <summary>
    /// Ghi chú: ToCommand chuyển CreateSubjectRequest API thành CreateSubjectCommand application.
    /// </summary>
    public static CreateSubjectCommand ToCommand(this CreateSubjectRequest request) =>
        new(request.SubjectCode, request.Name, request.Credits, request.MaxScore);

    /// <summary>
    /// Ghi chú: ToQuery chuyển ListSubjectsRequest API thành ListSubjectsQuery application.
    /// </summary>
    public static ListSubjectsQuery ToQuery(this ListSubjectsRequest request) =>
        new(request.IsActive, request.Page ?? 1, request.PageSize ?? 20, request.Search);

    /// <summary>
    /// Ghi chú: ToCommand chuyển UpdateSubjectRequest API thành UpdateSubjectCommand application.
    /// </summary>
    public static UpdateSubjectCommand ToCommand(this UpdateSubjectRequest request, Guid id) =>
        new(id, request.Name, request.Credits, request.MaxScore);

    /// <summary>
    /// Ghi chú: ToCommand chuyển DisableSubjectRequest API thành DisableSubjectCommand application.
    /// </summary>
    public static DisableSubjectCommand ToCommand(this DisableSubjectRequest request) =>
        new(request.Id);

    /// <summary>
    /// Ghi chú: ToDto chuyển SubjectResponse application thành SubjectDto API.
    /// </summary>
    public static SubjectDto ToDto(this SubjectResponse response) =>
        new(response.Id, response.SubjectCode, response.Name, response.Credits, response.MaxScore, response.IsActive);
}
