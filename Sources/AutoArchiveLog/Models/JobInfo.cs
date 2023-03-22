namespace AutoArchiveLog.Models;
public class JobInfo
{
    public Type Type { get; set; }
    public string CronExpression { get; set; }

    public JobInfo(Type type, string cronExpression)
    {
        Type = type;
        CronExpression = cronExpression;
    }
}