using System.Reflection;
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
        return serviceProvider.GetService(jobDetail.JobType) as IJob;
    }

    public void ReturnJob(IJob job)
    {
    }
}