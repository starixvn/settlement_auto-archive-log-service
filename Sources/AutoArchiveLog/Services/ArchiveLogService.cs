using AutoArchiveLog.Criteria;
using AutoArchiveLog.Models;
using Microsoft.Extensions.Options;

namespace AutoArchiveLog.Services;

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
        foreach (var table in Constants.Constants.TableNames)
        {
            await ProcessArchiveLogData(table, GetLogDataFromTable);
        }
    }

    private async Task ProcessArchiveLogData(string table, Func<string, Task<List<LogData>>> func)
    {
        var running = false;
        while (!running)
        {
            var logs = await func(table);
            await HandleInsLog(logs);
            running = logs.Count == 0;
        }
    }

    private async Task HandleInsLog(List<LogData> logs)
    {
        foreach (var item in logs)
        {
            var siteDbGroupId = await dataRepository.GetSiteGroupId(item.DbServer);
            await dataRepository.InsLogData(new InsLogDataCriteria
            {
                SiteDBGroupId = siteDbGroupId,
                TableName = item.TableName,
                JsonInfo = item.JsonInfo
            });

            await dataRepository.DelLogData(new DelLogDataCriteria
            {
                TableName = item.TableName,
                JsonInfo = item.JsonInfo,
                DbServer = item.DbServer
            });
        }
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
                Date = DateTime.Today.AddDays(-1),
            };

            var results = await dataRepository.GetLogData(criteria);

            lists.Add(results);
        }

        return lists;
    }
}