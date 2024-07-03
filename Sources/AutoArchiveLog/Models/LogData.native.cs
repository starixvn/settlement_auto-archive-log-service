namespace AutoArchiveLog.Models;

public class LogData
{
    public string DbServer { get; set; } = string.Empty;

    public string TableName { get; set; } = string.Empty;

    public byte[] JsonInfo { get; set; }
}