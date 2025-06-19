using AutoArchiveLog.Models;
using AutoArchiveLog.Services;
using Fanex.Data.Repository;

namespace AutoArchiveLog.Features.SyncProvider;

public interface ISyncProviderHandler
{
    Task Handle();
}

public class SyncProviderHandler(IDynamicRepository dynamicRepository, ReportDataRepository reportDataRepository) : ISyncProviderHandler
{
    public async Task Handle()
    {
        var archiveSettings = await reportDataRepository.GetSetting("DBACC");
        foreach (var item in archiveSettings)
        {
            await ProccessSyncProviderResult(item);
        }
    }

    private async Task ProccessSyncProviderResult(ArchiveSetting archiveSetting)
    {
        var running = false;
        var result = true;
        while (!running && result)
        {
            var providerResult = await GetProviderResults(archiveSetting);

            await InsertProviderResult(providerResult);

            var newArchiveSettings = await reportDataRepository.GetSetting();
            var newArchiveValue = newArchiveSettings.Find(i => i.ArchiveName.Equals(archiveSetting.ArchiveName)).ArchiveValue;
            archiveSetting.ArchiveValue = newArchiveValue;

            running = providerResult.JsonInfo == null;
        }
    }

    private async Task<LogData> GetProviderResults(ArchiveSetting setting)
    {
        var criteria = new GetProviderResultCriteria
        {
            TableName = setting.ArchiveName,
            LastValue = setting.ArchiveValue,
            Date = DateTimeOffset.Now.AddDays(-1).Date
        };
        await dynamicRepository.ExecuteAsync(criteria);

        return new LogData
        {
            TableName = setting.ArchiveName,
            JsonInfo = criteria.JsonInfo
        };
    }

    private async Task<bool> InsertProviderResult(LogData log)
    {
        var criteria = new InsProviderResultCriteria
        {
            TableName = log.TableName,
            JsonInfo = log.JsonInfo
        };
        await dynamicRepository.ExecuteAsync(criteria);

        return criteria.ErrCode == 0;
    }
}
