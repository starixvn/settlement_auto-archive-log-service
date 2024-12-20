using AutoArchiveLog.Constants;

namespace AutoArchiveLog.Worker.Jobs;

public static class JobConfigurations
{
    public static JobConfiguration[] GetJobs(IConfiguration configuration)
    {
        JobConfiguration[] jobs = [
            new JobConfiguration(
                typeof(ArchiveLogJob),
                "AutoArchiveLogJob",
                "AutoArchiveLogJobTrigger",
                string.Format(CronExpressionJob.AutoArchiveLog, configuration.GetSection("HoursRunning").Value))
            ];

        return jobs;
    }
}

public record class JobConfiguration(Type JobType, string JobName, string TriggerName, string CronExpression);