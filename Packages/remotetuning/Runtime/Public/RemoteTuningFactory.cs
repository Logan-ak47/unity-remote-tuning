using System;

namespace Ashutosh.RemoteTuning
{
    public static class RemoteTuningFactory
    {
        /// <summary>
        /// Creates a RemoteTuningClient using built-in runtime adapters:
        /// UnityWebRequest transport, PlayerPrefs storage, SystemClock, UnityLogSink, Fnv1a hasher.
        /// </summary>
        public static RemoteTuningClient CreateDefault(RemoteTuningOptions options, ILogSink log = null)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            // internal adapters (safe because this factory is inside the package runtime assembly)
            var transport = new UnityWebRequestTransport();
            var store = new PlayerPrefsKeyValueStore();
            var clock = new SystemClock();
            log ??= new UnityLogSink();
            var hasher = new Fnv1aHasher();

            return new RemoteTuningClient(options, transport, store, clock, log, hasher);
        }
    }
}