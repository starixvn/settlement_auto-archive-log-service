using AutoArchiveLog.Criteria;
using AutoArchiveLog.Models;

namespace AutoArchiveLog.Services;

public interface ArchiveReportService
{
    Task HandleArchiveReportData();
}

public class ArchiveReportServiceImp : ArchiveReportService
{
    private readonly ReportDataRepository reportDataRepository;

    public ArchiveReportServiceImp(ReportDataRepository reportDataRepository)
    {
        this.reportDataRepository = reportDataRepository;
    }

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
        while (!running)
        {
            var logs = await func(setting);
            if (logs.JsonInfo != null)
            {
                await reportDataRepository.InsertData(logs);
            }

            running = logs.JsonInfo == null;
        }
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