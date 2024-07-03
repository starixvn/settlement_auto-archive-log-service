using AutoArchiveLog.Criteria;
using AutoArchiveLog.Models;
using AutoArchiveLog.Services;
using Fanex.Logging;
using Fanex.Logging.Sentry.RavenClient;
using NSubstitute;
using SharpRaven;
using Xunit;

namespace AutoArchiveLog.UnitTests;

public class ArchiveReportServiceTests
{
    private readonly ReportDataRepository reportDataRepository;

    private readonly ArchiveReportService service;

    public ArchiveReportServiceTests()
    {
        var logEngine = new List<ILogEngine>();

        var sentryEngineOptions = new SentryEngineOptions
        {
            Dsn = new Dsn("http://2dfee6c5bbc844558ad3bef5f4b59ee1@10.18.200.131:800/9")
        };
        logEngine.Add(new SentryLogging(sentryEngineOptions));

        LogManager.SetDefaultLogCategory("NAP-Settlement-LOCAL").Use(logEngine);

        reportDataRepository = Substitute.For<ReportDataRepository>();
        service = new ArchiveReportServiceImp(reportDataRepository);
    }

    [Fact]
    public async Task HandleArchiveReportData_EmptySetting_NotCallInsertDataAndBreakWhile()
    {
        var list = new List<ArchiveSetting>();
        reportDataRepository.GetSetting().Returns(list);

        await service.HandleArchiveReportData();

        await reportDataRepository.DidNotReceive().InsertData(Arg.Any<LogData>());
    }

    [Fact]
    public async Task HandleArchiveReportData_ExistedSetting_CallInsertDataErrorAndBreakWhile()
    {
        var list = new List<ArchiveSetting>
        {
            new()
            {
                ArchiveName = "ArchiveName",
                ArchiveValue = "ArchiveName"
            }
        };

        var result = new Result
        {
            ErrorCode = 1,
            ErrorMessage = "Error"
        };

        var log = new LogData
        {
            DbServer = "SiteDB01",
            JsonInfo = [1]
        };

        reportDataRepository.GetData(Arg.Any<GetArchiveDataCriteria>()).Returns(log);
        reportDataRepository.GetSetting().Returns(list);
        reportDataRepository.InsertData(Arg.Any<LogData>()).Returns(result);
        await service.HandleArchiveReportData();

        await reportDataRepository.Received().InsertData(Arg.Any<LogData>());
    }

    [Fact]
    public async Task HandleArchiveReportData_ExistedSettingAndJsonInfoIsNull_NotCallInsertDataAndBreakWhile()
    {
        var list = new List<ArchiveSetting>
        {
            new()
            {
                ArchiveName = "ArchiveName",
                ArchiveValue = "ArchiveName"
            }
        };

        var result = new Result();

        var log = new LogData
        {
            DbServer = "SiteDB01",
            JsonInfo = null
        };

        reportDataRepository.GetData(Arg.Any<GetArchiveDataCriteria>()).Returns(log);
        reportDataRepository.GetSetting().Returns(list);
        reportDataRepository.InsertData(Arg.Any<LogData>()).Returns(result);
        await service.HandleArchiveReportData();

        await reportDataRepository.DidNotReceive().InsertData(Arg.Any<LogData>());
    }
}