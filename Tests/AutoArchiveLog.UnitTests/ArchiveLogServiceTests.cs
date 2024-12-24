using AutoArchiveLog.Models;
using AutoArchiveLog.Services;
using NSubstitute;
using Xunit;

namespace AutoArchiveLog.UnitTests;

public class ArchiveLogServiceTests
{
    private readonly InsertLogService insertLogService;

    private readonly ArchiveLogService service;

    public ArchiveLogServiceTests()
    {
        insertLogService = Substitute.For<InsertLogService>();
        service = new ArchiveLogServiceImp(insertLogService);
    }

    [Fact]
    public async Task HandleArchiveLogData_LogDataIsEmptied_NotCallInsLogAndBreakWhile()
    {
        var logs = new List<LogData>();
        insertLogService.GetLogDataFromTable(Arg.Any<string>()).Returns(logs);

        await service.HandleArchiveLogData();

        await insertLogService.DidNotReceive().InsertLog(Arg.Any<LogData>());
    }

    [Fact]
    public async Task HandleArchiveLogData_ExistedLogDataAndInsMainDbLogIsFail_CallInsertLogAndBreakWhile()
    {
        var logs = new List<LogData>
        {
            new()
            {
                DbServer = "DBSITE01",
                JsonInfo = [1]
            }
        };

        insertLogService.GetLogDataFromTable(Arg.Any<string>()).Returns(logs);

        insertLogService.InsertLog(Arg.Any<LogData>()).Returns(false);

        await service.HandleArchiveLogData();

        await insertLogService.Received().InsertLog(Arg.Any<LogData>());
    }
}