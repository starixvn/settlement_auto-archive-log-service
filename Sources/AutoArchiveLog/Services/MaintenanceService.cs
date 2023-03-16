using ONEbook.UM;
using ONEbook.UM.Services;

namespace AutoArchiveLog.Services;
public interface IMaintenanceService
{
    bool IsMaintenance { get; }
}

public class MaintenanceServiceImp : IMaintenanceService
{
    public bool IsMaintenance
    {
        get
        {
            var maintenanceService = new MaintenanceService(
                UMClientManager.RestClient,
                UMClientManager.CacheService,
                UMClientManager.UMClientConfigs,
                UMClientManager.HttpContext);
            var downTime = maintenanceService.GetUnderMaintenanceTime();

            return downTime.IsUnderMaintenanceTime;
        }
    }
}