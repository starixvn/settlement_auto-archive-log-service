using AutoArchiveLog.Criteria;
using AutoArchiveLog.Models;
using Microsoft.Extensions.Options;

namespace AutoArchiveLog.Services;

using Constants;

public interface ArchiveLogService
{
    Task HandleArchiveLogData();
}

public class ArchiveLogServiceImp : ArchiveLogService
{
    private readonly LogDataRepository dataRepository;
    private readonly AppSettings appSettings;

    public ArchiveLogServiceImp(LogDataRepository dataRepository, IOptions<AppSettings> appSettingsOption)
    {
        this.dataRepository = dataRepository;
        this.appSettings = appSettingsOption.Value;
    }

    public async Task HandleArchiveLogData()
    {
        foreach (var table in Constants.TableNames)
        {
            await ProcessArchiveLogData(table, GetLogDataFromTable);
        }
    }

    private async Task ProcessArchiveLogData(string table, Func<string, Task<List<LogData>>> func)
    {
        var running = false;
        var result = true;
        while (!running && result)
        {
            var logs = await func(table);
            result = await HandleInsLog(logs);
            running = logs.Count == 0;
        }
    }

    private async Task<bool> HandleInsLog(List<LogData> logs)
    {
        foreach (var item in logs)
        {
            var result = await InsertLog(item);

            if (!result)
            {
                return false;
            }
        }

        return true;
    }

    private async Task<bool> InsertLog(LogData log)
    {
        var siteDbGroupId = await dataRepository.GetSiteGroupId(log.DbServer);
        var delLogDataCriteria = new DelLogDataCriteria
        {
            TableName = log.TableName,
            JsonInfo = log.JsonInfo,
            DbServer = log.DbServer
        };

        var insLogDataCriteria = new InsLogDataCriteria
        {
            SiteDbGroupId = siteDbGroupId,
            TableName = log.TableName,
            JsonInfo = log.JsonInfo,
            DbServer = Constants.DbLog
        };

        var mainDbLog = await dataRepository.InsLogData(insLogDataCriteria);

        if (!mainDbLog)
        {
            return false;
        }

        await dataRepository.DelLogData(delLogDataCriteria);

        insLogDataCriteria.DbServer = Constants.DbLogStandby;

        return await dataRepository.InsLogData(insLogDataCriteria);
    }

    private async Task<List<LogData>> GetLogDataFromTable(string tableName)
    {
        var lists = new List<LogData>();

        foreach (var server in appSettings.SiteDbServer)
        {
            var criteria = new GetLogDataCriteria
            {
                TableName = tableName,
                DbServer = server,
                Date = DateTime.Today.AddDays(-4),
            };

            var results = await dataRepository.GetLogData(criteria);

            if (results.JsonInfo != null)
            {
                lists.Add(results);
            }
        }

        return lists;
    }
}