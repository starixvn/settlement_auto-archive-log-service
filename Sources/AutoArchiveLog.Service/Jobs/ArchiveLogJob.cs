using AutoArchiveLog.Services;
using Fanex.Logging;
using Quartz;

namespace AutoArchiveLog.Service.Jobs;

[DisallowConcurrentExecution]
public class ArchiveLogJob : IJob
{
    private readonly ArchiveLogService archiveLogService;

    public ArchiveLogJob(ArchiveLogService archiveLogService)
    {
        this.archiveLogService = archiveLogService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine($"Execute Job {DateTime.Now}");

        try
        {
            await archiveLogService.HandleArchiveLogData();
        }
        catch (Exception ex)
        {
            LogService.Error("Fanex.AutoArchiveLog.Service:", ex);
        }
    }
}
