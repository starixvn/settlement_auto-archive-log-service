using AutoArchiveLog.Constants;
using AutoArchiveLog.Models;
using AutoArchiveLog.Service.Jobs;
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
using Quartz;
using Quartz.Spi;
using SharpRaven;

namespace AutoArchiveLog.Service;

public class AutoArchiveLogService : IHostedService
{
    private readonly IConfiguration configuration;

    private readonly ISchedulerFactory schedulerFactory;

    private readonly IJobFactory jobFactory;

    private IScheduler? Scheduler { get; set; }

    public AutoArchiveLogService(IConfiguration configuration, ISchedulerFactory schedulerFactory, IJobFactory jobFactory)
    {
        this.configuration = configuration;
        this.schedulerFactory = schedulerFactory;
        this.jobFactory = jobFactory;
        InitializeSetting();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"StartAsync {DateTime.Now}");
        await CreateScheduler();
        await RunJobSchedulerAsync();
        await StartScheduler(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"StopAsync {DateTime.Now}");

        await Scheduler?.Shutdown(cancellationToken)!;
    }

    private async Task RunJobSchedulerAsync()
    {
        Console.WriteLine($"RunJobSchedulerAsync {DateTime.Now}");
        var jobInfo = new JobInfo(typeof(ArchiveLogJob), string.Format(CronExpressionJob.AutoArchiveLog, configuration.GetSection("HoursRunning").Value));
        await AddJobToSchedulerAsync(jobInfo);
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
                x => x.GetValue<string>("Name") ?? "",
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

    private async Task CreateScheduler()
    {
        Scheduler = await schedulerFactory.GetScheduler();
        Scheduler.JobFactory = jobFactory;
    }

    private async Task AddJobToSchedulerAsync(JobInfo jobInfo)
    {
        var jobDetail = CreateJob(jobInfo);

        var trigger = CreateTrigger(jobInfo);

        await Scheduler!.ScheduleJob(jobDetail, trigger);
    }

    private async Task StartScheduler(CancellationToken cancellationToken)
    {
        Console.WriteLine($"StartScheduler {DateTime.Now}");

        await Scheduler!.Start(cancellationToken);
    }

    private static ITrigger CreateTrigger(JobInfo jobInfo)
        => TriggerBuilder
            .Create()
            .WithIdentity($"trigger{jobInfo.Type.Name}", $"group{jobInfo.Type.Name}")
            .WithDescription(jobInfo.Type.Name)
    .WithCronSchedule(jobInfo.CronExpression)
            .Build();

    private static IJobDetail CreateJob(JobInfo jobInfo)
        => JobBuilder
            .Create(jobInfo.Type)
            .WithIdentity($"sync{jobInfo.Type.Name}", $"group{jobInfo.Type.Name}")
            .Build();
}