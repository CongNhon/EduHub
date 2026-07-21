namespace EduHub.Application.Interfaces.Services.Reports;

/// <summary>
/// Ghi chú: IReportJobScheduler enqueue background job sinh PDF bằng provider như Hangfire.
/// </summary>
public interface IReportJobScheduler
{
    /// <summary>
    /// Ghi chú: EnqueueReportJob enqueue job sinh PDF bằng report job id.
    /// </summary>
    string EnqueueReportJob(Guid reportJobId);
}
