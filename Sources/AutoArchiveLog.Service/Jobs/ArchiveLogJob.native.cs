using AutoArchiveLog.Services;
using Fanex.Logging;
using Quartz;

namespace AutoArchiveLog.Service.Jobs;

[DisallowConcurrentExecution]
public class ArchiveLogJob : IJob
{
    private readonly ArchiveLogService archiveLogService;

    private readonly ArchiveReportService archiveReportService;

    private readonly IMaintenanceService maintenanceService;

    public ArchiveLogJob(
        ArchiveLogService archiveLogService,
        ArchiveReportService archiveReportService,
        IMaintenanceService maintenanceService)
    {
        this.archiveLogService = archiveLogService;
        this.archiveReportService = archiveReportService;
        this.maintenanceService = maintenanceService;
    }

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