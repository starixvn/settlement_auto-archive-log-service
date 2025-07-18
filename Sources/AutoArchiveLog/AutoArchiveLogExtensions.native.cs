using AutoArchiveLog.Features.SyncProvider;
using AutoArchiveLog.Services;
using Fanex.Caching;
using Fanex.Caching.Redis;
using Fanex.Data.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace AutoArchiveLog;

public static class AutoArchiveLogExtensions
{
    public static WebApplicationBuilder AddAutoArchiveLog(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddSingleton<ICacheService, CacheService>()
            .AddSingleton<IDynamicRepository, DynamicRepository>()
            .AddSingleton<ArchiveLogService, ArchiveLogServiceImp>()
            .AddSingleton<InsertLogService, InsertLogServiceImp>()
            .AddSingleton<ArchiveReportService, ArchiveReportServiceImp>()
            .AddSingleton<LogDataRepository, LogDataRepositoryImp>()
            .AddSingleton<ReportDataRepository, ReportDataRepositoryImp>()
            .AddSingleton<ISyncProviderHandler, SyncProviderHandler>();

        return builder;
    }

    public static void AddRedisCacheConfig(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConfig = configuration.GetSection("Fanex.Caching.Redis:RedisConfig").Value;
        var applicationName = configuration.GetSection("Fanex.Caching.Redis:ChannelPrefix").Value;
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect($"{redisConfig},ChannelPrefix={applicationName}"));
        services.AddSingleton<ICache>(new RedisCache(new RedisCacheOptions
        {
            ConnectionString = redisConfig,
            ApplicationName = applicationName
        }));
    }
}