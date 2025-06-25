using AutoArchiveLog.Features.SyncProvider;
using AutoArchiveLog.Models;
using AutoArchiveLog.Services;
using Fanex.Data.Repository;
using Fanex.Logging;
using Fanex.Logging.Sentry.RavenClient;
using NSubstitute;
using SharpRaven;
using Xunit;

namespace AutoArchiveLog.UnitTests;

public class SyncProviderHandlerTests
{
    private readonly IDynamicRepository dynamicRepository;
    private readonly ReportDataRepository reportDataRepository;
    private readonly SyncProviderHandler handler;

    public SyncProviderHandlerTests()
    {
        var logEngine = new List<ILogEngine>();

        var sentryEngineOptions = new SentryEngineOptions
        {
            Dsn = new Dsn("http://2dfee6c5bbc844558ad3bef5f4b59ee1@sentry.nexdev.net/9")
        };
        logEngine.Add(new SentryLogging(sentryEngineOptions));

        LogManager.SetDefaultLogCategory("NAP-Settlement-LOCAL").Use(logEngine);

        dynamicRepository = Substitute.For<IDynamicRepository>();
        reportDataRepository = Substitute.For<ReportDataRepository>();
        handler = new SyncProviderHandler(dynamicRepository, reportDataRepository);
    }

    [Fact]
    public async Task Handle_WhenSettingsAvailable_ShouldProcessUntilNoMoreData()
    {
        // Arrange
        var archiveSettings = new List<ArchiveSetting>
        {
            new() { ArchiveName = "TestTable", ArchiveValue = "0" }
        };
        reportDataRepository.GetSetting("DBARC").Returns(Task.FromResult(archiveSettings));

        var updatedArchiveSettings = new List<ArchiveSetting>
        {
            new() { ArchiveName = "TestTable", ArchiveValue = "1" }
        };
        reportDataRepository.GetSetting().Returns(Task.FromResult(updatedArchiveSettings));

        var callCount = 0;
        dynamicRepository
            .When(x => x.ExecuteAsync(Arg.Any<GetProviderResultCriteria>()))
            .Do(callInfo =>
            {
                var criteria = callInfo.Arg<GetProviderResultCriteria>();
                if (callCount == 0)
                {
                    criteria.JsonInfo = [1];
                }
                else
                {
                    criteria.JsonInfo = null;
                }
                callCount++;
            });

        // Act
        await handler.Handle();

        // Assert
        await reportDataRepository.Received(1).GetSetting("DBARC");
        await dynamicRepository.Received(2).ExecuteAsync(Arg.Any<GetProviderResultCriteria>());
        await dynamicRepository.Received(1).ExecuteAsync(Arg.Any<InsProviderResultCriteria>());
        await reportDataRepository.Received(2).GetSetting();
    }

    [Fact]
    public async Task Handle_WhenNoSettings_ShouldNotRun()
    {
        // Arrange
        reportDataRepository.GetSetting("DBARC").Returns(Task.FromResult(new List<ArchiveSetting>()));

        // Act
        await handler.Handle();

        // Assert
        await reportDataRepository.Received(1).GetSetting("DBARC");
        await dynamicRepository.DidNotReceive().ExecuteAsync(Arg.Any<GetProviderResultCriteria>());
        await dynamicRepository.DidNotReceive().ExecuteAsync(Arg.Any<InsProviderResultCriteria>());
    }

    [Fact]
    public async Task Handle_WhenInsertFails_ShouldStopProcessing()
    {
        // Arrange
        var archiveSettings = new List<ArchiveSetting>
        {
            new() { ArchiveName = "TestTable", ArchiveValue = "0" }
        };
        reportDataRepository.GetSetting("DBARC").Returns(Task.FromResult(archiveSettings));
        reportDataRepository.GetSetting().Returns(Task.FromResult(archiveSettings));

        dynamicRepository
            .When(x => x.ExecuteAsync(Arg.Any<GetProviderResultCriteria>()))
            .Do(callInfo =>
            {
                var criteria = callInfo.Arg<GetProviderResultCriteria>();
                criteria.JsonInfo = [1];
            });

        dynamicRepository
            .When(x => x.ExecuteAsync(Arg.Any<InsProviderResultCriteria>()))
            .Do(callInfo =>
            {
                var criteria = callInfo.Arg<InsProviderResultCriteria>();
                criteria.ErrCode = 1;
                criteria.ErrMsg = "Database Error";
            });

        // Act
        await handler.Handle();

        // Assert
        await reportDataRepository.Received(1).GetSetting("DBARC");
        await dynamicRepository.Received(1).ExecuteAsync(Arg.Any<GetProviderResultCriteria>());
        await dynamicRepository.Received(1).ExecuteAsync(Arg.Any<InsProviderResultCriteria>());
        await reportDataRepository.Received(1).GetSetting("DBARC");
        await reportDataRepository.Received(1).GetSetting();
    }
}