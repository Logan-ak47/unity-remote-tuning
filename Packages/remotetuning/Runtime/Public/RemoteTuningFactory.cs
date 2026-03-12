using System;

namespace Ashutosh.RemoteTuning
{
    public static class RemoteTuningFactory
    {
        public sealed class BuildResult
        {
            public RemoteTuningClient Client { get; }
            public ToggleableTransport TransportToggle { get; }

            public BuildResult(RemoteTuningClient client, ToggleableTransport transportToggle)
            {
                Client = client;
                TransportToggle = transportToggle;
            }
        }

        public static BuildResult CreateDefault(RemoteTuningOptions options, ILogSink log = null)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            var realTransport = new UnityWebRequestTransport();
            var toggle = new ToggleableTransport(realTransport);

            var store = new PlayerPrefsKeyValueStore();
            var clock = new SystemClock();
            log ??= new UnityLogSink();
            var hasher = new Fnv1aHasher();

            var client = new RemoteTuningClient(options, toggle, store, clock, log, hasher);
            return new BuildResult(client, toggle);
        }
    }
}