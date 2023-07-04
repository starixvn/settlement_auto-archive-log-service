namespace AutoArchiveLog.Models;

public class SiteGroup
{
    public int SiteDbGroupId { get; set; }

    protected SiteGroup(int siteDbGroupId)
    {
        SiteDbGroupId = siteDbGroupId;
    }
}