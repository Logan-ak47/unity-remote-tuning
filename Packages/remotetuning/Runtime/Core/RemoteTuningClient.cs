using System.Threading;
using System.Threading.Tasks;

namespace Ashutosh.RemoteTuning
{
    public sealed class RemoteTuningClient
    {
        public RemoteTuningSnapshot Current { get; private set; }

        public string UserId { get; private set; } = "user_1";

        public FeatureFlags Flags { get; }
        public RemoteTuningValues Values { get; }
        public Experiments Experiments { get; }

        private readonly RemoteConfigService _remoteConfig;
        private readonly ConfigAccessor _accessor;

        public RemoteTuningClient(
            RemoteTuningOptions options,
            IConfigTransport transport,
            IKeyValueStore store,
            IClock clock,
            ILogSink log,
            IStableHasher hasher)
        {
            log ??= new NullLogSink();
            hasher ??= new Fnv1aHasher();

            var cacheRepo = new ConfigCacheRepository(store, clock, log);
            var validator = new DefaultConfigValidator();

            _remoteConfig = new RemoteConfigService(options, transport, cacheRepo, validator, clock, log);

            Current = _remoteConfig.GetBestAvailableSnapshot();

            _accessor = new ConfigAccessor(log);
            _accessor.Bind(Current.RawJson);

            // A/B core wiring
            var assignmentStore = new AssignmentStore(store);
            var assigner = new ExperimentAssigner(hasher, assignmentStore, log);
            var overrideStore = new ExperimentOverrideStore(store);
            var experimentSvc = new ExperimentService(_accessor, assigner, overrideStore, log);

            Experiments = new Experiments(() => UserId, experimentSvc);

            Flags = new FeatureFlags(_accessor, log, Experiments);
            Values = new RemoteTuningValues(_accessor, log);
        }

        public void SetUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return;

            UserId = userId.Trim();
        }

        public async Task RefreshAsync(CancellationToken ct = default)
        {
            Current = await _remoteConfig.RefreshAsync(ct);
            _accessor.Bind(Current.RawJson);
        }
    }
}