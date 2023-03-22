using Quartz;

namespace AutoArchiveLog.Service.Jobs;
[DisallowConcurrentExecution]
public class HelloJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await Console.Out.WriteLineAsync("Executing background job"); await Task.CompletedTask;
    }
}