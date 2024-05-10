using AutoArchiveLog.Services;
using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.Database;
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
        catch (Exception exception)
        {
            var logMessage = new LogMessage(exception.Message)
                .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(exception))
                .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
                .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(exception))
                .ToString();
            await LogService.ErrorAsync(logMessage);
            throw;
        }
    }
}