using AutoArchiveLog.Criteria;
using AutoArchiveLog.Models;
using Microsoft.Extensions.Options;

namespace AutoArchiveLog.Services;

public interface InsertLogService
{
    Task<List<LogData>> GetLogDataFromTable(string tableName);

    Task<bool> InsertLog(LogData log);
}

public class InsertLogServiceImp(LogDataRepository logDataRepository, IOptions<AppSettings> appSettings) : InsertLogService
{
    public async Task<bool> InsertLog(LogData log)
    {
        var siteDbGroupId = await logDataRepository.GetSiteGroupId(log.DbServer);

        var mainDbLog = await logDataRepository.InsMainLogData(siteDbGroupId, log);

        if (!mainDbLog)
        {
            return false;
        }

        await logDataRepository.DelLogData(log);

        return true;
    }

    public async Task<List<LogData>> GetLogDataFromTable(string tableName)
    {
        var lists = new List<LogData>();

        foreach (var server in appSettings.Value.SiteDbServer)
        {
            var criteria = new GetLogDataCriteria
            {
                TableName = tableName,
                DbServer = server,
                Date = DateTimeOffset.Now.AddDays(-1).Date,
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