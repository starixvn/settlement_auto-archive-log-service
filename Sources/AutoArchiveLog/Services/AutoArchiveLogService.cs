using AutoArchiveLog.Criteria;
using AutoArchiveLog.Models;
using Microsoft.Extensions.Options;

namespace AutoArchiveLog.Services;

public interface AutoArchiveLogService
{
    Task HandleArchiveLogData();
}
public class AutoArchiveLogServiceImp : AutoArchiveLogService
{
    private readonly LogDataRepository dataRepository;
    private readonly AppSettings appSettings;


    public AutoArchiveLogServiceImp(LogDataRepository dataRepository, IOptions<AppSettings> appSettingsOption)
    {
        this.dataRepository = dataRepository;
        this.appSettings = appSettingsOption.Value;
    }

    public async Task HandleArchiveLogData()
    {
        foreach (var table in Constants.tableNames)
        {
            await ProccessArchiveLogData(table, GetLogDataFromTable);
        }
    }

    private async Task ProccessArchiveLogData(string table, Func<string, Task<List<LogData>>> func)
    {
        try
        {
            bool runing = false;
            while (!runing)
            {
                var logs = await func(table);
                await HandleInsLog(logs);
                runing = logs.Count == 0;
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task HandleInsLog(List<LogData> logs)
    {
        foreach (var item in logs)
        {
            await dataRepository.InsLogData(new InsLogDataCriteria
            {
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
                Date = DateTime.Now,
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


