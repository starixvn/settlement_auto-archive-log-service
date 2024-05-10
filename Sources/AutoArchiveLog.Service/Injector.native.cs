using AutoArchiveLog.Service.Jobs;
using AutoArchiveLog.Services;
using Fanex.Caching;
using Fanex.Data.Repository;
using Fanex.Logging;
using Fanex.Logging.Sentry.RavenClient;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ONEbook.UM;
using ONEbook.UM.AspNetCore;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;
using ONEbook.UM.Transport;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using SharpRaven;

namespace AutoArchiveLog.Service;

public static class Injector
{
    public static void InitializeServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDynamicRepository, DynamicRepository>();
        services.AddSingleton<IJobFactory, JobFactory>();
        services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

        services.AddSingleton<ArchiveLogService, ArchiveLogServiceImp>();
        services.AddSingleton<ArchiveReportService, ArchiveReportServiceImp>();
        services.AddSingleton<LogDataRepository, LogDataRepositoryImp>();
        services.AddSingleton<ReportDataRepository, ReportDataRepositoryImp>();
        services.AddTransient<IMaintenanceService, MaintenanceServiceImp>();

        services.AddSingleton(new UMClientConfiguration().Initialize(configuration));
        services.AddSingleton<IUMCacheService, UMCacheService>();
        services.AddSingleton<IUMRestClient, UMRestClient>();
        services.AddSingleton<ONEbook.UM.Services.IMaintenanceService, ONEbook.UM.Services.MaintenanceService>();

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        services.AddJobsServices();

        StartUmSetting(configuration);
        StartLogSetting(configuration);
    }

    private static void StartUmSetting(IConfiguration configuration)
    {
        var umConfig = new UMClientConfiguration().Initialize(configuration);

        UMClientManager
            .UseConfig(umConfig)
            .UseCacheService(new UMCacheService(new CacheService()))
            .UseRestClient(new UMRestClient(umConfig));
    }

    private static void StartLogSetting(IConfiguration configuration)
    {
        var logEngine = new List<ILogEngine>();

        var sentryEngineOptions = new SentryEngineOptions
        {
            Dsn = new Dsn(configuration.GetSection("SentryToken").Value)
        };

        logEngine.Add(new SentryLogging(sentryEngineOptions));

        LogManager.SetDefaultLogCategory(configuration.GetSection("SiteCategory").Value).Use(logEngine);
    }

    private static void AddJobsServices(this IServiceCollection services)
    {
        services.AddTransient<ArchiveLogJob>();
    }
}