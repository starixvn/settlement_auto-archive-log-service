using AutoArchiveLog.Constants;
using Fanex.Data;
using Fanex.Data.Repository;

namespace AutoArchiveLog.Criteria;

public class GetArchiveSettingCriteria : CriteriaBase
{
    [SpParam(Ignored = true)]
    public string DbServer { get; set; } = string.Empty;

    [SpParam(Ignored = true)]
    public string SpName { get; set; } = "ArchiveService_Setting_Sel";

    public override bool IsValid() => true;

    public override string GetSettingKey() => DbServer switch
    {
        DatabaseName.DBARC => $"{SpName}_{DatabaseName.DBARC}",
        _ => SpName,
    };
}