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
                "AutoArchiveLogJobTrigger",CronExpressionJob.AutoArchiveLog)];

        return jobs;
    }
}

public record class JobConfiguration(Type JobType, string JobName, string TriggerName, string CronExpression);