using EduHub.Application.Interfaces.Services.Reports;
using EduHub.Infrastructure.Services.Reports;
using Hangfire;

namespace EduHub.Infrastructure.Services.Jobs;

/// <summary>
/// Ghi chú: HangfireReportJobScheduler enqueue job sinh PDF report bằng Hangfire.
/// </summary>
public sealed class HangfireReportJobScheduler(IBackgroundJobClient backgroundJobClient) : IReportJobScheduler
{
    /// <summary>
    /// Ghi chú: EnqueueReportJob enqueue SimplePdfReportGenerator với payload chỉ chứa report job id.
    /// </summary>
    public string EnqueueReportJob(Guid reportJobId) =>
        backgroundJobClient.Enqueue<SimplePdfReportGenerator>(
            generator => generator.GenerateReportCardAsync(reportJobId, CancellationToken.None));
}
