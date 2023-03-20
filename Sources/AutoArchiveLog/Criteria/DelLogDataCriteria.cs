using Fanex.Data;
using Fanex.Data.Repository;

namespace AutoArchiveLog.Criteria;

public class DelLogDataCriteria : NonQueryCommand
{
    [SpParam(Ignored = true)]
    public string DbServer { get; set; } = string.Empty;

    public string TableName { get; set; } = string.Empty;
    public byte[] JsonInfo { get; set; }

    public override string GetSettingKey() => $"NAP_Set_ArchiveDataDel_{DbServer}";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(DbServer);
}