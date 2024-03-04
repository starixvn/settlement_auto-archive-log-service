using AutoArchiveLog.Services;
using Fanex.Logging;
using Quartz;

namespace AutoArchiveLog.Service.Jobs;

[DisallowConcurrentExecution]
public class ArchiveLogJob(
    ArchiveLogService archiveLogService,
    ArchiveReportService archiveReportService,
    IMaintenanceService maintenanceService)
    : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            if (!maintenanceService.IsMaintenance)
            {
                await archiveLogService.HandleArchiveLogData();
                await archiveReportService.HandleArchiveReportData();
            }
        }
        catch (Exception ex)
        {
            await LogService.ErrorAsync("Error from AutoArchiveLog Service:", ex);
            throw;
        }
    }
}