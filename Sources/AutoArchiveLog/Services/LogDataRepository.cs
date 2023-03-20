using AutoArchiveLog.Criteria;
using AutoArchiveLog.Models;
using Fanex.Data.Repository;

namespace AutoArchiveLog.Services;

public interface LogDataRepository
{
    Task DelLogData(DelLogDataCriteria criteria);

    Task<LogData> GetLogData(GetLogDataCriteria criteria);

    Task InsLogData(InsLogDataCriteria criteria);

    Task<int> GetSiteGroupId(string dbServer);
}

public class LogDataRepositoryImp : LogDataRepository
{
    private readonly IDynamicRepository dynamicRepository;

    public LogDataRepositoryImp(IDynamicRepository dynamicRepository)
    {
        this.dynamicRepository = dynamicRepository;
    }

    public async Task<LogData> GetLogData(GetLogDataCriteria criteria)
    {
        await dynamicRepository.ExecuteAsync(criteria);

        return new LogData
        {
            DbServer = criteria.DbServer,
            JsonInfo = criteria.JsonInfo,
            TableName = criteria.TableName,
        };
    }

    public async Task DelLogData(DelLogDataCriteria criteria)
    {
        await dynamicRepository.ExecuteAsync(criteria);
    }

    public async Task InsLogData(InsLogDataCriteria criteria)
    {
        await dynamicRepository.ExecuteAsync(criteria);
    }

    public async Task<int> GetSiteGroupId(string dbServer)
    {
        var criteria = new GetSiteGroupCriteria { DbServer = dbServer };
        var site = await dynamicRepository.GetAsync<SiteGroup>(criteria);
        return site.SiteDbGroupId;
    }
}