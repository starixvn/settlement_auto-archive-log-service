using AutoArchiveLog.Services;
using Fanex.Logging;
using Quartz;

namespace AutoArchiveLog.Service.Jobs;

[DisallowConcurrentExecution]
public class ArchiveLogJob : IJob
{
    private readonly ArchiveLogService archiveLogService;

    private readonly IMaintenanceService maintenanceService;

    public ArchiveLogJob(ArchiveLogService archiveLogService, IMaintenanceService maintenanceService)
    {
        this.archiveLogService = archiveLogService;
        this.maintenanceService = maintenanceService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            if (!maintenanceService.IsMaintenance)
            {
                await archiveLogService.HandleArchiveLogData();
            }
        }
        catch (Exception ex)
        {
            await LogService.ErrorAsync("Fanex.AutoArchiveLog.Service:", ex);
            throw;
        }
    }
}