using Fanex.Data.Repository;

namespace AutoArchiveLog.Criteria;

public class GetArchiveSettingCriteria : CriteriaBase
{
    public override bool IsValid() => true;

    public override string GetSettingKey() => "ArchiveService_Setting_Sel";
}