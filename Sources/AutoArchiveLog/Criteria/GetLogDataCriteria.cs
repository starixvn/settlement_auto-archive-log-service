using Fanex.Data;
using Fanex.Data.Repository;

namespace AutoArchiveLog.Criteria;
public class GetLogDataCriteria : NonQueryCommand
{

    public int BatchNo { get; set; } = 1;
    public string TableName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    [SpParam(Ignored = true)]
    public string DbServer { get; set; } = string.Empty;

    public byte[] JsonInfo { get; set; }

    public override bool IsValid() => true;
    public override string GetSettingKey() => $"NAP_Set_ArchiveDataSel_{DbServer}";

}
