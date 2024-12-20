namespace AutoArchiveLog.Models;

public class JobInfo
{
    public Type Type { get; set; }
    public string JobName { get; set; }
    public string TriggerName { get; set; }
    public string CronExpression { get; set; }

}