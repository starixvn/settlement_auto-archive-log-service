using Quartz;
using Quartz.Spi;

namespace AutoArchiveLog.Service.Jobs;

public class JobFactory : IJobFactory
{
    private readonly IServiceProvider serviceProvider;

    public JobFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        var jobDetail = bundle.JobDetail;
        var service = serviceProvider.GetService(jobDetail.JobType);
        if (service == null)
        {
            throw new InvalidOperationException();
        }

        return (IJob)service;
    }

    public void ReturnJob(IJob job)
    {
        var disposable = job as IDisposable;
        disposable?.Dispose();
    }
}