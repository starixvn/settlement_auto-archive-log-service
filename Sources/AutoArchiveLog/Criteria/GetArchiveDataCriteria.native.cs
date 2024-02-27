using Fanex.Data.Repository;

namespace AutoArchiveLog.Criteria;

public class GetArchiveDataCriteria : NonQueryCommand
{
    public DateTime Date { get; set; }

    public int BatchNo { get; set; } = 2000;

    public string TableName { get; set; } = string.Empty;

    public string LastValue { get; set; }

    public byte[] JsonInfo { get; set; }

    public override bool IsValid() => BatchNo > 0;

    public override string GetSettingKey() => "ArchiveService_Sel";
}