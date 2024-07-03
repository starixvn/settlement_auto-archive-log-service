using AutoArchiveLog.Models;

namespace AutoArchiveLog.Services;

using Constants;

public interface ArchiveLogService
{
    Task HandleArchiveLogData();
}

public class ArchiveLogServiceImp(InsertLogService insertLogService)
    : ArchiveLogService
{
    public async Task HandleArchiveLogData()
    {
        foreach (var table in Constants.TableNames)
        {
            await ProcessArchiveLogData(table, insertLogService.GetLogDataFromTable);
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
            var result = await insertLogService.InsertLog(item);

            if (!result)
            {
                return false;
            }
        }

        return true;
    }
}