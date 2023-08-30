using Fanex.Data.Repository;

namespace AutoArchiveLog.Criteria;

public class InsLogDataCriteria : NonQueryCommand
{
    public int SiteDbGroupId { get; set; }

    public string TableName { get; set; } = string.Empty;

    public byte[] JsonInfo { get; set; }

    public override string GetSettingKey() => "NAP_Set_ArchiveDataIns";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(TableName);
}