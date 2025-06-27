using AutoArchiveLog.Models;
using AutoArchiveLog.Services;
using Fanex.Data.Repository;
using Fanex.Logging;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AutoArchiveLog.Features.SyncProvider;

public interface ISyncProviderHandler
{
    Task Handle();
}

public class SyncProviderHandler(
    IDynamicRepository dynamicRepository,
    ReportDataRepository reportDataRepository,
    ILogger<SyncProviderHandler> logger) : ISyncProviderHandler
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

            logger.LogInformation($"SyncProviderHandler - Processing {archiveSetting.ArchiveName} with last value {archiveSetting.ArchiveValue}");


            result = await InsertProviderResult(providerResult);

            var newArchiveSettings = await reportDataRepository.GetSetting("DBARC");
            var newArchiveValue = newArchiveSettings.Find(i => i.ArchiveName.Equals(archiveSetting.ArchiveName)).ArchiveValue;

            logger.LogInformation($"SyncProviderHandler - Processing newArchiveValue {newArchiveValue}");

            archiveSetting.ArchiveValue = newArchiveValue;

            running = providerResult.JsonInfo == null;

            logger.LogInformation($"SyncProviderHandler - Done Processing {archiveSetting.ArchiveName} with last value {archiveSetting.ArchiveValue} providerResult {running} {result}");

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

        logger.LogInformation($"SyncProviderHandler - Processing GetProviderResults {JsonSerializer.Serialize(criteria)}");

        await dynamicRepository.ExecuteAsync(criteria);

        return new LogData
        {
            TableName = setting.ArchiveName,
            JsonInfo = criteria.JsonInfo
        };
    }

    private async Task<bool> InsertProviderResult(LogData log)
    {
        logger.LogInformation($"SyncProviderHandler - InsertProviderResult {log.TableName} {log.JsonInfo?.Length} at {DateTimeOffset.Now}");

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