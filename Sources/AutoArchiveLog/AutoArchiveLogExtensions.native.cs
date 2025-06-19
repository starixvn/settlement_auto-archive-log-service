using AutoArchiveLog.Features.SyncProvider;
using AutoArchiveLog.Services;
using Fanex.Data.Repository;
using Microsoft.AspNetCore.Builder;

namespace AutoArchiveLog;

public static class AutoArchiveLogExtensions
{
    public static WebApplicationBuilder AddAutoArchiveLog(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddSingleton<IDynamicRepository, DynamicRepository>()
            .AddSingleton<ArchiveLogService, ArchiveLogServiceImp>()
            .AddSingleton<InsertLogService, InsertLogServiceImp>()
            .AddSingleton<ArchiveReportService, ArchiveReportServiceImp>()
            .AddSingleton<LogDataRepository, LogDataRepositoryImp>()
            .AddSingleton<ReportDataRepository, ReportDataRepositoryImp>()
            .AddSingleton<ISyncProviderHandler, SyncProviderHandler>();

        return builder;
    }
}
