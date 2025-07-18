using AutoArchiveLog.Criteria;
using AutoArchiveLog.Models;
using Fanex.Data.Repository;
using Fanex.Logging;

namespace AutoArchiveLog.Services;

public interface LogDataRepository
{
    Task UpdateLogData(LogData log);

    Task<LogData> GetLogData(GetLogDataCriteria criteria);

    Task<bool> InsMainLogData(int siteDbGroupId, LogData log);

    Task<int> GetSiteGroupId(string dbServer);
}

public class LogDataRepositoryImp(IDynamicRepository dynamicRepository) : LogDataRepository
{
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

    public async Task UpdateLogData(LogData log)
    {
        var criteria = new UpdateLogDataCriteria
        {
            TableName = log.TableName,
            JsonInfo = log.JsonInfo,
            DbServer = log.DbServer
        };
        await dynamicRepository.ExecuteAsync(criteria);
    }

    public async Task<bool> InsMainLogData(int siteDbGroupId, LogData log)
    {
        try
        {
            var criteria = new InsLogDataCriteria
            {
                SiteDbGroupId = siteDbGroupId,
                TableName = log.TableName,
                JsonInfo = log.JsonInfo
            };
            await dynamicRepository.ExecuteAsync(criteria);
            return true;
        }
        catch (Exception e)
        {
            await LogService.ErrorAsync("Error from AutoArchiveLog Service: NAP_Set_ArchiveDataIns - ", e);
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