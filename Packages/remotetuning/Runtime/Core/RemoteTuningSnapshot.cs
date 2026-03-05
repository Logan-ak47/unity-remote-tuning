using System;

namespace Ashutosh.RemoteTuning
{
    public enum ConfigSource { Default, Cache, Remote }

    public readonly struct RemoteTuningSnapshot
    {
        public readonly ConfigSource Source;
        public readonly ConfigLoadReason Reason;

        public readonly CacheStatus CacheStatus;
        public readonly DateTime FetchedAtUtc;
        public readonly DateTime FreshUntilUtc;
        public readonly DateTime StaleUntilUtc;

        public readonly string ConfigVersion;
        public readonly bool IsStale;
        public readonly string RawJson;

        public RemoteTuningSnapshot(
            ConfigSource source,
            ConfigLoadReason reason,
            CacheStatus cacheStatus,
            DateTime fetchedAtUtc,
            DateTime freshUntilUtc,
            DateTime staleUntilUtc,
            string configVersion,
            bool isStale,
            string rawJson)
        {
            Source = source;
            Reason = reason;
            CacheStatus = cacheStatus;
            FetchedAtUtc = fetchedAtUtc;
            FreshUntilUtc = freshUntilUtc;
            StaleUntilUtc = staleUntilUtc;
            ConfigVersion = configVersion;
            IsStale = isStale;
            RawJson = rawJson;
        }
    }
}