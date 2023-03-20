using AutoArchiveLog.Models;
using AutoArchiveLog.Services;
using Fanex.Caching;
using Fanex.Data;
using Fanex.Data.SqlServer;
using Fanex.Logging;
using Fanex.Logging.Sentry.RavenClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ONEbook.UM;
using ONEbook.UM.AspNetCore;
using ONEbook.UM.Configurations;
using ONEbook.UM.Transport;
using SharpRaven;

namespace AutoArchiveLog.Service;
public class AutoArchiveLogService : IHostedService, IAsyncDisposable
{
    private readonly ArchiveLogService archiveLogService;
    private readonly IMaintenanceService maintenanceService;

    private readonly IConfiguration configuration;
    private readonly AppSettings appSettings;
    private Timer? timer;

    public AutoArchiveLogService(
        IConfiguration configuration,
        ArchiveLogService archiveLogService,
        IOptions<AppSettings> appSettingsOptions,
        IMaintenanceService maintenanceService)
    {
        this.configuration = configuration;
        this.archiveLogService = archiveLogService;
        this.maintenanceService = maintenanceService;

        this.appSettings = appSettingsOptions.Value;

        InitializeSetting();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"AutoArchiveLog Starting at {DateTime.Now}");
        var currentDate = DateTime.Now;
        var runningDate = DateTime.Today.AddHours(appSettings.HoursRunning);
        Console.WriteLine($"AutoArchiveLog Begin Running at {runningDate}");

        if (runningDate > currentDate)
        {
            var firstInterval = (runningDate - currentDate).TotalHours;

            Task.Delay(TimeSpan.FromHours(firstInterval)).Wait();

            timer = new Timer(Process, null, TimeSpan.Zero, TimeSpan.FromHours(appSettings.HoursInterval));
        }

        return Task.CompletedTask;
    }

    private void Process(object? state)
    {
        if (!maintenanceService.IsMaintenance)
        {
            Console.WriteLine($"AutoArchiveLog Running at {DateTime.Now}");
            MainProcess();
        }
    }

    private void MainProcess()
    {
        try
        {
            Task.Run(() => archiveLogService.HandleArchiveLogData());
        }
        catch (Exception ex)
        {
            LogService.Error("Fanex.AutoArchiveLog.Service:", ex);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"AutoArchiveLog Stopping at {DateTime.Now}");
        timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private void InitializeSetting()
    {
        StartUmSetting();
        StartDataBaseSetting();
        StartLogSetting();
    }


    private void StartUmSetting()
    {
        var umConfig = new UMClientConfiguration().Initialize(configuration);

        UMClientManager
            .UseConfig(umConfig)
            .UseCacheService(new UMCacheService(new CacheService()))
            .UseRestClient(new UMRestClient(umConfig));
    }

    private void StartDataBaseSetting()
    {
        var connections = configuration.GetSection("ConnectionStrings")
            .GetChildren()
            .AsEnumerable()
            .ToDictionary(
                x => x.GetValue<string>("Name"),
                y => new ConnectionConfiguration(y.GetValue<string>("Name"), y.GetValue<string>("ConnectionString"), y.GetValue<string>("ProviderName"))
            );
        var spPath = configuration.GetSection("StoredProcedurePath").Value;

        DbSettingProviderManager
            .StartNewSession()
            .Use(connections)
            .WithSqlServer()
            .UseDefaultDbSettingProvider(resourcePath: spPath)
            .Run();
    }

    private void StartLogSetting()
    {
        var logEngine = new List<ILogEngine>();

        var sentryEngineOptions = new SentryEngineOptions
        {
            Dsn = new Dsn(configuration.GetSection("SentryToken").Value)
        };

        logEngine.Add(new SentryLogging(sentryEngineOptions));

        LogManager.SetDefaultLogCategory(configuration.GetSection("SiteCategory").Value).Use(logEngine);
    }

    public async ValueTask DisposeAsync()
    {
        if (timer is IAsyncDisposable tempTimer)
        {
            await tempTimer.DisposeAsync();
        }

        timer = null;
    }
}
