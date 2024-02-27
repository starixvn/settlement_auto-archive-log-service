using AutoArchiveLog.Criteria;
using AutoArchiveLog.Models;
using Fanex.Data.Repository;

namespace AutoArchiveLog.Services;

public interface ReportDataRepository
{
    Task<List<ArchiveSetting>> GetSetting();

    Task<LogData> GetData(GetArchiveDataCriteria criteria);

    Task InsertData(LogData log);
}

public class ReportDataRepositoryImp : ReportDataRepository
{
    private readonly IDynamicRepository dynamicRepository;

    public ReportDataRepositoryImp(IDynamicRepository dynamicRepository)
    {
        this.dynamicRepository = dynamicRepository;
    }

    public async Task<List<ArchiveSetting>> GetSetting()
    {
        var criteria = new GetArchiveSettingCriteria();
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

    public async Task InsertData(LogData log)
    {
        var criteria = new InsArchiveDataCriteria
        {
            TableName = log.TableName,
            JsonInfo = log.JsonInfo
        };
        await dynamicRepository.ExecuteAsync(criteria);
    }
}