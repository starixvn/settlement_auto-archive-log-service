using Fanex.Data.Repository;

namespace AutoArchiveLog.Criteria;

public class InsArchiveDataCriteria : NonQueryCommand
{
    public string TableName { get; set; } = string.Empty;

    public byte[] JsonInfo { get; set; }

    public override string GetSettingKey() => "ArchiveService_Ins";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(TableName);
}