using AutoArchiveLog.Criteria;
using AutoArchiveLog.Models;
using AutoArchiveLog.Services;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace AutoArchiveLog.UnitTests;

public class ArchiveLogServiceTest
{
    private readonly LogDataRepository logDataRepository;

    private readonly ArchiveLogService service;

    public ArchiveLogServiceTest()
    {
        var settings = new AppSettings()
        {
            SiteDbServer = new[] { "Local", "PRO" }
        };

        var settingsOptions = Options.Create(settings);
        logDataRepository = Substitute.For<LogDataRepository>();
        service = new ArchiveLogServiceImp(logDataRepository, settingsOptions);
    }

    [Fact]
    public async Task HandleArchiveLogData_LogDataIsEmptied_NotCallInsLogData()
    {
        var log = new LogData();
        logDataRepository.GetLogData(Arg.Any<GetLogDataCriteria>()).Returns(log);

        await service.HandleArchiveLogData();

        await logDataRepository.DidNotReceive().GetSiteGroupId(Arg.Any<string>());
        await logDataRepository.DidNotReceive().InsMainLogData(Arg.Any<int>(), Arg.Any<LogData>());
        await logDataRepository.DidNotReceive().DelLogData(Arg.Any<LogData>());
        await logDataRepository.DidNotReceive().InsStandbyLogData(Arg.Any<int>(), Arg.Any<LogData>());
    }

    [Fact]
    public async Task HandleArchiveLogData_ExistedLogDataAndInsMainDbLogIsFail_CallExpectedMethod()
    {
        var log = new LogData
        {
            DbServer = "SiteDB01",
            JsonInfo = new byte[] { 1 }
        };

        logDataRepository.GetLogData(Arg.Any<GetLogDataCriteria>()).Returns(log);

        logDataRepository.InsMainLogData(Arg.Any<int>(), Arg.Any<LogData>()).Returns(false);

        await service.HandleArchiveLogData();

        await logDataRepository.Received().GetSiteGroupId(Arg.Any<string>());
        await logDataRepository.Received().InsMainLogData(Arg.Any<int>(), Arg.Any<LogData>());

        await logDataRepository.DidNotReceive().DelLogData(Arg.Any<LogData>());
        await logDataRepository.DidNotReceive().InsStandbyLogData(Arg.Any<int>(), Arg.Any<LogData>());
    }

    [Fact]
    public async Task HandleArchiveLogData_ExistedLogDataAndInsMainDbLogIsSuccessFul_CallExpectedMethod()
    {
        var log = new LogData
        {
            DbServer = "SiteDB01",
            JsonInfo = new byte[] { 1 }
        };

        logDataRepository.GetLogData(Arg.Any<GetLogDataCriteria>()).Returns(log);

        logDataRepository.InsMainLogData(Arg.Any<int>(), Arg.Any<LogData>()).Returns(true);
        logDataRepository.InsStandbyLogData(Arg.Any<int>(), Arg.Any<LogData>()).Returns(false);

        await service.HandleArchiveLogData();

        await logDataRepository.Received().GetSiteGroupId(Arg.Any<string>());
        await logDataRepository.Received().InsMainLogData(Arg.Any<int>(), Arg.Any<LogData>());
        await logDataRepository.Received().DelLogData(Arg.Any<LogData>());
        await logDataRepository.Received().InsStandbyLogData(Arg.Any<int>(), Arg.Any<LogData>());
    }
}