using AutoArchiveLog.Services;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ONEbook.UM.AspNetCore;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;
using ONEbook.UM.Transport;

namespace AutoArchiveLog.Service;
public static class Injector
{
    public static void InitializeServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDynamicRepository, DynamicRepository>();

        services.AddSingleton<ArchiveLogService, ArchiveLogServiceImp>();
        services.AddSingleton<LogDataRepository, LogDataRepositoryImp>();
        services.AddTransient<IMaintenanceService, MaintenanceServiceImp>();

        services.AddSingleton(new UMClientConfiguration().Initialize(configuration));
        services.AddSingleton<IUMCacheService, UMCacheService>();
        services.AddSingleton<IUMRestClient, UMRestClient>();
        services.AddSingleton<ONEbook.UM.Services.IMaintenanceService, ONEbook.UM.Services.MaintenanceService>();

        services.AddTransient<ScaleOutSettleService, ScaleOutSettleServiceImp>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    }
}