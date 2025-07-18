using AutoArchiveLog.Criteria;
using AutoArchiveLog.Models;
using Fanex.SiteDbConnectionManager.Client.Services;

namespace AutoArchiveLog.Services;

public interface InsertLogService
{
    Task<List<LogData>> GetLogDataFromTable(string tableName);

    Task<bool> InsertLog(LogData log);
}

public class InsertLogServiceImp(
    LogDataRepository logDataRepository,
    SiteDbClientService siteDbClientService) : InsertLogService
{
    public async Task<bool> InsertLog(LogData log)
    {
        var siteDbGroupId = await logDataRepository.GetSiteGroupId(log.DbServer);

        var mainDbLog = await logDataRepository.InsMainLogData(siteDbGroupId, log);

        if (!mainDbLog)
        {
            return false;
        }

        await logDataRepository.UpdateLogData(log);

        return true;
    }

    public async Task<List<LogData>> GetLogDataFromTable(string tableName)
    {
        var lists = new List<LogData>();
        var sitesDb = await siteDbClientService.GetActiveSiteDbAsync();
        foreach (var site in sitesDb)
        {
            var criteria = new GetLogDataCriteria
            {
                TableName = tableName,
                DbServer = site.SiteName
            };

            var results = await logDataRepository.GetLogData(criteria);

            if (results.JsonInfo != null)
            {
                lists.Add(results);
            }
        }

        return lists;
    }
}