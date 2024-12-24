using AutoArchiveLog;
using AutoArchiveLog.Models;
using AutoArchiveLog.Worker.Jobs;
using Fanex.Data.SqlServer;
using Fanex.Logging.Sentry.RavenClient;
using ONEbook.UM.DependencyInjection;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

builder.Services.Configure<AppSettings>(builder.Configuration);

builder.Services.AddQuartz(q =>
{
    var jobs = JobConfigurations.GetJobs(builder.Configuration);
    foreach (var job in jobs)
    {
        var jobKey = new JobKey(job.JobName);

        q.AddJob(job.JobType, jobKey);

        q.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity(job.TriggerName)
            .WithCronSchedule(job.CronExpression));
    }
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddOneBookUm(builder.Configuration);

builder.Services.AddFanexLogSentry(builder.Configuration);

builder.Services.AddFanexSql(builder.Configuration);

builder.AddAutoArchiveLog();

var app = builder.Build();

app.MapHealthChecks("/");

app.Run();