
using AutoArchiveLog.Features.SyncProvider;
using Fanex.Logging;

namespace AutoArchiveLog.Worker
{
    public class Test : BackgroundService
    {
        private readonly ISyncProviderHandler _syncProviderHandler;


        public Test(ISyncProviderHandler syncProviderHandler)
        {
            _syncProviderHandler = syncProviderHandler ?? throw new ArgumentNullException(nameof(syncProviderHandler));
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _syncProviderHandler.Handle();

            }
            catch (Exception ex)
            {
                await LogService.ErrorAsync("Error in Test service", ex);
                throw;
            }
        }
    }
}
