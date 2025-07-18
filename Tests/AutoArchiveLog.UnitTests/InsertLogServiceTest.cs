using AutoArchiveLog.Criteria;
using AutoArchiveLog.Models;
using AutoArchiveLog.Services;
using Fanex.SiteDbConnectionManager.Client.Models;
using Fanex.SiteDbConnectionManager.Client.Services;
using NSubstitute;
using Xunit;

namespace AutoArchiveLog.UnitTests;

public class InsertLogServiceTest
{
    private readonly LogDataRepository logDataRepository;

    private readonly InsertLogServiceImp service;

    public InsertLogServiceTest()
    {
        logDataRepository = Substitute.For<LogDataRepository>();
        var siteDbClientService = Substitute.For<SiteDbClientService>();
        siteDbClientService.GetActiveSiteDbAsync().Returns([new SiteDb { SiteName = "SiteDB1", SiteDbGroupId = 1 }]);
        service = new InsertLogServiceImp(logDataRepository, siteDbClientService);
    }

    [Fact]
    public async Task InsertLog_InsMainLogDataIsFalse_ReturnsFalse()
    {
        logDataRepository.InsMainLogData(Arg.Any<int>(), Arg.Any<LogData>()).Returns(false);

        var logData = new LogData();
        var actual = await service.InsertLog(logData);

        await logDataRepository.DidNotReceive().UpdateLogData(Arg.Any<LogData>());
        Assert.False(actual);
    }

    [Fact]
    public async Task InsertLog_InsMainLogDataIsFalse_ReturnsTrueAndCallDelLogData()
    {
        logDataRepository.InsMainLogData(Arg.Any<int>(), Arg.Any<LogData>()).Returns(true);

        var logData = new LogData();
        var actual = await service.InsertLog(logData);

        await logDataRepository.Received().UpdateLogData(Arg.Any<LogData>());
        Assert.True(actual);
    }

    [Fact]
    public async Task GetLogDataFromTable_ExistedLog_ReturnsListLog()
    {
        var log = new LogData
        {
            DbServer = "DBSITE01",
            JsonInfo = [1]
        };

        logDataRepository.GetLogData(Arg.Any<GetLogDataCriteria>()).Returns(log);

        var actual = await service.GetLogDataFromTable(string.Empty);

        Assert.True(actual.Count > 0);
    }

    [Fact]
    public async Task GetLogDataFromTable_ExistedLogAndJsonInfoIsNull_ReturnsListEmpty()
    {
        var log = new LogData
        {
            DbServer = "DBSITE01",
            JsonInfo = null
        };

        logDataRepository.GetLogData(Arg.Any<GetLogDataCriteria>()).Returns(log);

        var actual = await service.GetLogDataFromTable(string.Empty);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task GetLogDataFromTable_NotExistedLog_ReturnsListEmpty()
    {
        var log = new LogData();

        logDataRepository.GetLogData(Arg.Any<GetLogDataCriteria>()).Returns(log);

        var actual = await service.GetLogDataFromTable(string.Empty);

        Assert.Empty(actual);
    }
}