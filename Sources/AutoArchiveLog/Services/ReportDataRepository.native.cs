using AutoArchiveLog.Criteria;
using AutoArchiveLog.Models;
using Fanex.Data.Repository;

namespace AutoArchiveLog.Services;

public interface ReportDataRepository
{
    Task<List<ArchiveSetting>> GetSetting(string dbServer = "");

    Task<LogData> GetData(GetArchiveDataCriteria criteria);

    Task<Result> InsertData(LogData log);
}

public class ReportDataRepositoryImp(IDynamicRepository dynamicRepository) : ReportDataRepository
{
    public async Task<List<ArchiveSetting>> GetSetting(string dbServer = "")
    {
        var criteria = new GetArchiveSettingCriteria
        {
            DbServer = dbServer
        };
        return (await dynamicRepository.FetchAsync<ArchiveSetting>(criteria)).ToList();
    }

    public async Task<LogData> GetData(GetArchiveDataCriteria criteria)
    {
        await dynamicRepository.ExecuteAsync(criteria);

        return new LogData
        {
            JsonInfo = criteria.JsonInfo,
            TableName = criteria.TableName,
        };
    }

    public async Task<Result> InsertData(LogData log)
    {
        var criteria = new InsArchiveDataCriteria
        {
            TableName = log.TableName,
            JsonInfo = log.JsonInfo
        };
        await dynamicRepository.ExecuteAsync(criteria);

        return new Result
        {
            ErrorCode = criteria.ErrCode,
            ErrorMessage = criteria.ErrMsg
        };
    }
}