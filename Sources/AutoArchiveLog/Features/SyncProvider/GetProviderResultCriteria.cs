using Fanex.Data.Repository;

namespace AutoArchiveLog.Features.SyncProvider;

public class GetProviderResultCriteria : NonQueryCommand
{
    public DateTime Date { get; set; }
    public int BatchNo { get; set; } = 2000;
    public string TableName { get; set; } = string.Empty;
    public string LastValue { get; set; } = string.Empty;

    public byte[] JsonInfo { get; set; }
    public override string GetSettingKey() => "NAP_Set_Service_ArchiveData_Sel";

    public override bool IsValid() => true;
}
