namespace AutoArchiveLog.Models;

public class AppSettings
{
    public string[] SiteDbServer { get; set; } = { };
    public double HoursRunning { get; set; }
    public double HoursInterval { get; set; }
}