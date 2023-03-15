using AutoArchiveLog.Services;
using Fanex.Caching;
using Fanex.Data;
using Fanex.Data.SqlServer;
using Fanex.Logging;
using Fanex.Logging.Sentry.RavenClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ONEbook.UM;
using ONEbook.UM.AspNetCore;
using ONEbook.UM.Configurations;
using ONEbook.UM.Transport;
using SharpRaven;

namespace AutoArchiveLog.Service;
public class AutoArchiveLogHostedService : IHostedService, IAsyncDisposable
{
    private readonly AutoArchiveLogService autoArchiveLogService;
    private readonly IConfiguration configuration;
    private Timer? timer;

    public AutoArchiveLogHostedService(IConfiguration configuration, AutoArchiveLogService autoArchiveLogService)
    {
        this.configuration = configuration;
        this.autoArchiveLogService = autoArchiveLogService;

        InitializeSetting();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"AutoArchiveLog Starting at {DateTime.Now}");

        timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        //Task.Run(() => autoArchiveLogService.HandleArchiveLogData());

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
