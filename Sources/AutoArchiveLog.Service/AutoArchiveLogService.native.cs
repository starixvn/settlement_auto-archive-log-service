using AutoArchiveLog.Constants;
using AutoArchiveLog.Models;
using AutoArchiveLog.Service.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Spi;

namespace AutoArchiveLog.Service;

public class AutoArchiveLogService(
    IConfiguration configuration,
    ISchedulerFactory schedulerFactory,
    IJobFactory jobFactory)
    : BackgroundService
{
    private IScheduler? Scheduler { get; set; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine($"StartAsync {DateTime.Now}");
        await CreateScheduler();
        await RunJobSchedulerAsync();
        await StartScheduler(stoppingToken);
    }

    private async Task RunJobSchedulerAsync()
    {
        Console.WriteLine($"RunJobSchedulerAsync {DateTime.Now}");
        var jobInfo = new JobInfo(typeof(ArchiveLogJob), string.Format(CronExpressionJob.AutoArchiveLog, configuration.GetSection("HoursRunning").Value));
        await AddJobToSchedulerAsync(jobInfo);
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