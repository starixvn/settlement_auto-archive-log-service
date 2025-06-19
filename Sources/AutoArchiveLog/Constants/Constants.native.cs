namespace AutoArchiveLog.Constants;

public static class Constants
{
    public static readonly string[] TableNames = ["Single_Calculate_Queue", "Parlay_Calculate_Queue"];
}

public static class CronExpressionJob
{
    public const string AutoArchiveLog = "0 0 {0} * * ?"; //Every day at 1am ref:https://www.freeformatter.com/cron-expression-generator-quartz.html

    // every 4 seconds
    public const string AutoArchiveLogEvery4Seconds = "0/4 * * * * ?"; //Every 4 seconds ref:https://www.freeformatter.com/cron-expression-generator-quartz.html

}