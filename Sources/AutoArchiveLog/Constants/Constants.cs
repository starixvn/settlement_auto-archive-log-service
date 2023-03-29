namespace AutoArchiveLog.Constants;

public static class Constants
{
    public static readonly string[] TableNames = { "Single_Calculate_Queue", "Parlay_Calculate_Queue" };

    public const string DbLog = "DBLOG";

    public const string DbLogStandby = "DBLOG_STANDBY";
}

public static class CronExpressionJob
{
    //public const string AutoArchiveLog = "0 */{0} * ? * *"; //Every 15 minutes ref:https://www.freeformatter.com/cron-expression-generator-quartz.html
    public const string AutoArchiveLog = "0 0 {0} * * ?"; //Every day at 1am ref:https://www.freeformatter.com/cron-expression-generator-quartz.html
}