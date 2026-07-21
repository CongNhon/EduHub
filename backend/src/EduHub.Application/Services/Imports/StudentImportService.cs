using System.Net.Mail;
using System.Security.Cryptography;
using EduHub.Application.Common.Models;
using EduHub.Application.Contracts.StudentImports;
using EduHub.Application.Features.StudentImports.Common;
using EduHub.Application.Features.Students.Common;
using EduHub.Application.Interfaces.Authentication;
using EduHub.Application.Interfaces.Data;
using EduHub.Application.Interfaces.Repositories.StudentImports;
using EduHub.Application.Interfaces.Services.StudentImports;
using EduHub.Domain.Entities.Academics;
using EduHub.Domain.Entities.Identity;
using EduHub.Domain.Entities.Students;
using EduHub.Domain.Enums;

namespace EduHub.Application.Services.StudentImports;

/// <summary>
/// Ghi chú: StudentImportService tạo học sinh, tài khoản Student/Parent, liên kết phụ huynh và enrollment từ Excel.
/// </summary>
public sealed class StudentImportService(
    IStudentImportRepository repository,
    IStudentImportWorkbookReader workbookReader,
    IPasswordHashService passwordHashService,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : IStudentImportService
{
    /// <summary>
    /// Ghi chú: ImportAsync kiểm tra workbook và xử lý độc lập từng dòng để trả lỗi chính xác cho quản trị học vụ.
    /// </summary>
    public async Task<Result<StudentImportResponse>> ImportAsync(ImportStudentsWorkbookCommand request, CancellationToken cancellationToken)
    {
        if (!IsAcademicAdministrator()) return Result.Failure<StudentImportResponse>(ImportErrors.AdminRequired);
        if (!request.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) || request.Content.Length is 0 or > 10 * 1024 * 1024)
        {
            return Result.Failure<StudentImportResponse>(ImportErrors.FileInvalid);
        }

        var parsed = workbookReader.Read(request.Content);
        if (parsed.Errors.Count > 0)
        {
            return Result.Failure<StudentImportResponse>(new Error(
                ImportErrors.WorkbookInvalid.Code,
                ImportErrors.WorkbookInvalid.Message,
                ImportErrors.WorkbookInvalid.Type,
                new Dictionary<string, string[]> { ["workbook"] = parsed.Errors.ToArray() }));
        }

        var results = new List<StudentImportRowResponse>(parsed.Rows.Count);
        var credentials = new List<StudentImportCredentialResponse>();
        var seenStudentCodes = new HashSet<string>(StringComparer.Ordinal);
        var usedStudentEmails = new HashSet<string>(StringComparer.Ordinal);
        var importedUsers = new Dictionary<string, User>(StringComparer.Ordinal);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var row in parsed.Rows)
        {
            var normalizedCode = StudentNormalization.Code(row.StudentCode);
            var studentEmail = row.StudentEmail.Trim();
            var normalizedStudentEmail = studentEmail.ToUpperInvariant();
            var parentEmail = row.ParentEmail.Trim();
            var normalizedParentEmail = parentEmail.ToUpperInvariant();
            var rowValidationError = ValidateRow(row, normalizedStudentEmail, normalizedParentEmail, timeProvider.GetUtcNow());
            if (rowValidationError is not null)
            {
                results.Add(Failure(row, "Import.RowInvalid", rowValidationError));
                continue;
            }

            if (!seenStudentCodes.Add(normalizedCode) || !usedStudentEmails.Add(normalizedStudentEmail))
            {
                results.Add(Failure(row, "Import.DuplicateWorkbookValue", "StudentCode hoặc StudentEmail bị trùng trong workbook."));
                continue;
            }

            if (!MailAddress.TryCreate(studentEmail, out _) || !MailAddress.TryCreate(parentEmail, out _))
            {
                results.Add(Failure(row, "Import.InvalidEmail", "StudentEmail hoặc ParentEmail không hợp lệ."));
                continue;
            }

            if (await repository.GetStudentByNormalizedCodeAsync(normalizedCode, cancellationToken) is not null)
            {
                results.Add(Failure(row, "Import.StudentCodeExists", "Mã học sinh đã tồn tại."));
                continue;
            }

            var classRoom = await repository.GetClassRoomByNormalizedCodeAsync(row.ClassCode.Trim().ToUpperInvariant(), cancellationToken);
            if (classRoom is null)
            {
                results.Add(Failure(row, "Import.ClassNotFound", "Không tìm thấy lớp đang hoạt động."));
                continue;
            }

            var semester = await repository.GetSemesterAsync(classRoom.AcademicYearId, row.SemesterName.Trim().ToUpperInvariant(), cancellationToken);
            if (semester is null)
            {
                results.Add(Failure(row, "Import.SemesterNotFound", "Không tìm thấy học kỳ của năm học chứa lớp."));
                continue;
            }

            var studentUser = importedUsers.GetValueOrDefault(normalizedStudentEmail) ??
                await repository.GetUserByNormalizedEmailAsync(normalizedStudentEmail, cancellationToken);
            if (studentUser is not null && (studentUser.Role != UserRole.Student || await repository.IsStudentUserLinkedAsync(studentUser.Id, cancellationToken)))
            {
                results.Add(Failure(row, "Import.StudentAccountConflict", "StudentEmail đã thuộc tài khoản hoặc hồ sơ khác."));
                continue;
            }

            var parentUser = importedUsers.GetValueOrDefault(normalizedParentEmail) ??
                await repository.GetUserByNormalizedEmailAsync(normalizedParentEmail, cancellationToken);
            if (parentUser is not null && parentUser.Role != UserRole.Parent)
            {
                results.Add(Failure(row, "Import.ParentAccountConflict", "ParentEmail đã thuộc role khác Parent."));
                continue;
            }

            if (!await repository.TryIncrementClassEnrollmentCountAsync(classRoom.Id, cancellationToken))
            {
                results.Add(Failure(row, "Import.ClassCapacityExceeded", "Lớp đã đủ sĩ số."));
                continue;
            }

            if (studentUser is null)
            {
                var password = GenerateTemporaryPassword();
                studentUser = new User(studentEmail, normalizedStudentEmail, passwordHashService.HashPassword(password), UserRole.Student, row.FullName, row.StudentCode);
                repository.AddUser(studentUser);
                importedUsers[normalizedStudentEmail] = studentUser;
                credentials.Add(new StudentImportCredentialResponse(studentEmail, UserRole.Student.ToString(), password));
            }

            if (parentUser is null)
            {
                var password = GenerateTemporaryPassword();
                parentUser = new User(parentEmail, normalizedParentEmail, passwordHashService.HashPassword(password), UserRole.Parent, row.ParentFullName, null, row.ParentPhone);
                repository.AddUser(parentUser);
                importedUsers[normalizedParentEmail] = parentUser;
                credentials.Add(new StudentImportCredentialResponse(parentEmail, UserRole.Parent.ToString(), password));
            }

            var student = new Student(
                row.StudentCode.Trim(),
                normalizedCode,
                row.FullName.Trim(),
                StudentNormalization.SearchText(row.FullName),
                row.DateOfBirth,
                row.Gender,
                null,
                row.Address);
            student.LinkUserAccount(studentUser.Id, now);
            repository.AddStudent(student);
            repository.AddParentLink(new ParentStudent(parentUser.Id, student.Id, row.Relationship, now));
            repository.AddEnrollment(new Enrollment(student.Id, classRoom.Id, semester.Id, now));
            results.Add(new StudentImportRowResponse(row.RowNumber, row.StudentCode, true, null, null));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        var successCount = results.Count(result => result.Success);
        return Result.Success(new StudentImportResponse(
            parsed.Rows.Count,
            successCount,
            results.Count - successCount,
            results,
            credentials));
    }

    /// <summary>
    /// Ghi chú: DownloadTemplateAsync trả file XLSX mẫu có cột đúng schema import hiện tại.
    /// </summary>
    public Task<Result<StudentImportTemplateResponse>> DownloadTemplateAsync(DownloadStudentImportTemplateQuery request, CancellationToken cancellationToken)
    {
        if (!IsAcademicAdministrator()) return Task.FromResult(Result.Failure<StudentImportTemplateResponse>(ImportErrors.AdminRequired));
        return Task.FromResult(Result.Success(new StudentImportTemplateResponse(
            "EduHub_Student_Parent_Import_Template.xlsx",
            workbookReader.CreateTemplate(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")));
    }

    private bool IsAcademicAdministrator() => currentUser.Role is UserRole.AcademicAdmin or UserRole.SystemAdmin;

    private static StudentImportRowResponse Failure(StudentImportRow row, string code, string message) =>
        new(row.RowNumber, row.StudentCode, false, code, message);

    /// <summary>
    /// Ghi chú: ValidateRow kiểm tra giới hạn cột và xung đột email của một học sinh-phụ huynh trước khi ghi database.
    /// </summary>
    private static string? ValidateRow(StudentImportRow row, string normalizedStudentEmail, string normalizedParentEmail, DateTimeOffset utcNow)
    {
        if (normalizedStudentEmail == normalizedParentEmail) return "StudentEmail và ParentEmail phải là hai tài khoản khác nhau.";
        if (row.StudentCode.Trim().Length > 64) return "StudentCode tối đa 64 ký tự.";
        if (row.FullName.Trim().Length > 256 || row.ParentFullName.Trim().Length > 256) return "Họ tên tối đa 256 ký tự.";
        if (row.Gender?.Trim().Length > 32) return "Gender tối đa 32 ký tự.";
        if (row.Address?.Trim().Length > 500) return "Address tối đa 500 ký tự.";
        if (row.ParentPhone?.Trim().Length > 32) return "ParentPhone tối đa 32 ký tự.";
        if (row.Relationship.Trim().Length > 64) return "Relationship tối đa 64 ký tự.";
        if (row.StudentEmail.Trim().Length > 320 || row.ParentEmail.Trim().Length > 320) return "Email tối đa 320 ký tự.";
        if (row.DateOfBirth > DateOnly.FromDateTime(utcNow.UtcDateTime)) return "DateOfBirth không được ở tương lai.";
        return null;
    }

    /// <summary>
    /// Ghi chú: GenerateTemporaryPassword tạo mật khẩu tạm ngẫu nhiên đủ chữ hoa, chữ thường, số và ký tự đặc biệt.
    /// </summary>
    private static string GenerateTemporaryPassword() => $"Eh!{Convert.ToHexString(RandomNumberGenerator.GetBytes(6))}a1";
}
