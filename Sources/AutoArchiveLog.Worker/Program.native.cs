using AutoArchiveLog;
using AutoArchiveLog.Models;
using AutoArchiveLog.Worker.Jobs;
using Fanex.Data.SqlServer;
using Fanex.Logging.Sentry.RavenClient;
using ONEbook.UM.DependencyInjection;
using Quartz;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddHealthChecks();

services.Configure<AppSettings>(builder.Configuration);

services.AddQuartz(q =>
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

services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

services.AddOneBookUmForService(builder.Configuration);

services.AddFanexLogSentry(builder.Configuration);

services.AddFanexSql(builder.Configuration);

builder.AddAutoArchiveLog();

var app = builder.Build();

app.MapHealthChecks("/healthz");

app.Run();