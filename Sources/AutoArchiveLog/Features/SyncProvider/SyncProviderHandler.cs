using AutoArchiveLog.Models;
using AutoArchiveLog.Services;
using Fanex.Data.Repository;
using Fanex.Logging;

namespace AutoArchiveLog.Features.SyncProvider;

public interface ISyncProviderHandler
{
    Task Handle();
}

public class SyncProviderHandler(
    IDynamicRepository dynamicRepository,
    ReportDataRepository reportDataRepository) : ISyncProviderHandler
{
    public async Task Handle()
    {
        var archiveSettings = await reportDataRepository.GetSetting("DBARC");
        foreach (var item in archiveSettings)
        {
            await ProcessSyncProviderResult(item);
        }
    }

    private async Task ProcessSyncProviderResult(ArchiveSetting archiveSetting)
    {
        var running = false;
        var result = true;
        while (!running && result)
        {
            var providerResult = await GetProviderResults(archiveSetting);

            result = await InsertProviderResult(providerResult);

            var newArchiveSettings = await reportDataRepository.GetSetting("DBARC");
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
            Date = DateTimeOffset.Now.Date
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
        if (log.JsonInfo == null)
        {
            return true;
        }

        var criteria = new InsProviderResultCriteria
        {
            TableName = log.TableName,
            JsonInfo = log.JsonInfo
        };
        await dynamicRepository.ExecuteAsync(criteria);

        if (criteria.ErrCode > 0)
        {
            await LogService.ErrorAsync("Error from AutoArchiveLog Service: InsertProviderResult - ", criteria.ErrMsg);
            return false;
        }

        return true;
    }
}