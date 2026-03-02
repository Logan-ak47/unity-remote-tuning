using System;

namespace Ashutosh.RemoteTuning
{
    public enum ConfigSource { Default, Cache, Remote }

    public readonly struct RemoteTuningSnapshot
    {
        public readonly ConfigSource Source;
        public readonly string ConfigVersion;
        public readonly DateTime FetchedAtUtc;
        public readonly bool IsStale;
        public readonly string RawJson;

        public RemoteTuningSnapshot(ConfigSource source, string configVersion, DateTime fetchedAtUtc, bool isStale, string rawJson)
        {
            Source = source;
            ConfigVersion = configVersion;
            FetchedAtUtc = fetchedAtUtc;
            IsStale = isStale;
            RawJson = rawJson;
        }
    }
}