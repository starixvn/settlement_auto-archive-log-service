using Fanex.Data;
using Fanex.Data.Repository;

namespace AutoArchiveLog.Criteria;

public class UpdateLogDataCriteria : NonQueryCommand
{
    [SpParam(Ignored = true)]
    public string DbServer { get; init; } = string.Empty;

    public string TableName { get; set; } = string.Empty;
    public byte[] JsonInfo { get; set; }

    public override string GetSettingKey() => $"NAP_Set_ArchiveDataSystemSettings_Upd_{DbServer}";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(DbServer);
}