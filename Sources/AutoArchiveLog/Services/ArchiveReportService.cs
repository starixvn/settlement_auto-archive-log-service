using AutoArchiveLog.Criteria;
using AutoArchiveLog.Models;
using Fanex.Logging;

namespace AutoArchiveLog.Services;

public interface ArchiveReportService
{
    Task HandleArchiveReportData();
}

public class ArchiveReportServiceImp(ReportDataRepository reportDataRepository) : ArchiveReportService
{
    public async Task HandleArchiveReportData()
    {
        var tables = await reportDataRepository.GetSetting();
        foreach (var table in tables)
        {
            await ProcessArchiveReportData(table, GetDataFromTable);
        }
    }

    private async Task ProcessArchiveReportData(ArchiveSetting setting, Func<ArchiveSetting, Task<LogData>> func)
    {
        var running = false;
        var result = true;

        while (!running && result)
        {
            var logs = await func(setting);

            result = await HandleInsLog(logs);

            var tables = await reportDataRepository.GetSetting();
            var newArchiveValue = tables.Find(i => i.ArchiveName.Equals(setting.ArchiveName)).ArchiveValue;
            setting.ArchiveValue = newArchiveValue;
            running = logs.JsonInfo == null;
        }
    }

    private async Task<bool> HandleInsLog(LogData logs)
    {
        if (logs.JsonInfo == null)
        {
            return true;
        }

        var result = await reportDataRepository.InsertData(logs);
        if (result.ErrorCode > 0)
        {
            await LogService.ErrorAsync("Error from AutoArchiveLog Service: ArchiveService_Ins - ",
                result.ErrorMessage);
            return false;
        }

        return true;
    }

    private async Task<LogData> GetDataFromTable(ArchiveSetting setting)
    {
        var criteria = new GetArchiveDataCriteria
        {
            TableName = setting.ArchiveName,
            LastValue = setting.ArchiveValue,
            Date = DateTimeOffset.Now.AddDays(-1).Date,
        };

        return await reportDataRepository.GetData(criteria);
    }
}