using System.Threading;
using System.Threading.Tasks;

namespace Ashutosh.RemoteTuning
{
    public sealed class RemoteTuningClient
    {
        public RemoteTuningSnapshot Current { get; private set; }

        private readonly RemoteConfigService _remoteConfig;

        public RemoteTuningClient(
            RemoteTuningOptions options,
            IConfigTransport transport,
            IKeyValueStore store,
            IClock clock,
            ILogSink log,
            IStableHasher hasher)
        {
            // hasher unused until Day 7; keep it in the signature for stable construction.
            log ??= new NullLogSink();

            var cacheRepo = new ConfigCacheRepository(store, clock, log);
            var validator = new DefaultConfigValidator();

            _remoteConfig = new RemoteConfigService(options, transport, cacheRepo, validator, clock, log);

            Current = _remoteConfig.GetBestAvailableSnapshot();
        }

        public async Task RefreshAsync(CancellationToken ct = default)
        {
            Current = await _remoteConfig.RefreshAsync(ct);
        }
    }
}