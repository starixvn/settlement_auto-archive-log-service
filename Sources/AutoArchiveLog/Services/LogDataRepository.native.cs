using AutoArchiveLog.Criteria;
using AutoArchiveLog.Models;
using Fanex.Data.Repository;
using Fanex.Logging;

namespace AutoArchiveLog.Services;

public interface LogDataRepository
{
    Task DelLogData(DelLogDataCriteria criteria);

    Task<LogData> GetLogData(GetLogDataCriteria criteria);

    Task<bool> InsLogData(InsLogDataCriteria criteria);

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

    public async Task<bool> InsLogData(InsLogDataCriteria criteria)
    {
        try
        {
            await dynamicRepository.ExecuteAsync(criteria);
            return true;
        }
        catch (Exception e)
        {
            await LogService.ErrorAsync($"Fanex.AutoArchiveLog.Service: {criteria.DbServer} - ", e);
            return false;
        }
    }

    public async Task<int> GetSiteGroupId(string dbServer)
    {
        var criteria = new GetSiteGroupCriteria { DbServer = dbServer };
        var site = await dynamicRepository.GetAsync<SiteGroup>(criteria);
        return site.SiteDbGroupId;
    }
}