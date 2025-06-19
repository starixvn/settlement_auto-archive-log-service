using Fanex.Data.Repository;

namespace AutoArchiveLog.Features.SyncProvider;

public class InsProviderResultCriteria : NonQueryCommand
{
    public int ErrCode { get; set; }

    public string ErrMsg { get; set; }

    public string TableName { get; set; } = string.Empty;

    public byte[] JsonInfo { get; set; }
    public override string GetSettingKey() => "NAP_Set_Service_ArchiveData_Ins";

    public override bool IsValid() => true;
}
