using Fanex.Data;
using Fanex.Data.Repository;

namespace AutoArchiveLog.Criteria;

public class GetArchiveSettingCriteria : CriteriaBase
{
    [SpParam(Ignored = true)]
    public string DbServer { get; set; } = string.Empty;

    public override bool IsValid() => true;

    public override string GetSettingKey() => DbServer switch
    {
        "DACC" => "ArchiveService_Setting_Sel_DACC",
        _ => "ArchiveService_Setting_Sel",
    };
}