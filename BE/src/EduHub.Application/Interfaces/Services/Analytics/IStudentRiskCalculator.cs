using EduHub.Application.Contracts.Analytics;

namespace EduHub.Application.Interfaces.Services.Analytics;

/// <summary>
/// Ghi chú: IStudentRiskCalculator định nghĩa phương thức tính toán rủi ro cho học sinh.
/// </summary>
public interface IStudentRiskCalculator
{
    StudentRiskItem Calculate(StudentRiskInput input);
}
