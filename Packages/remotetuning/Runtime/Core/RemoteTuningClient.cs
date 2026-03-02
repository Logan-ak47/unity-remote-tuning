using System.Threading;
using System.Threading.Tasks;

namespace Ashutosh.RemoteTuning
{
    public sealed class RemoteTuningClient
    {
        public RemoteTuningSnapshot Current { get; private set; }

        public RemoteTuningClient(
            RemoteTuningOptions options,
            IConfigTransport transport,
            IKeyValueStore store,
            IClock clock,
            ILogSink log,
            IStableHasher hasher)
        {
            Current = new RemoteTuningSnapshot(
                ConfigSource.Default,
                "default",
                clock.UtcNow,
                isStale: false,
                rawJson: "{}");
        }

        public Task RefreshAsync(CancellationToken ct = default)
        {
            // Day 5: real implementation
            return Task.CompletedTask;
        }
    }
}