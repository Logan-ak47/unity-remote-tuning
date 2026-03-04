using System;

namespace Ashutosh.RemoteTuning
{
    internal static class CacheStateEvaluator
    {
        public static CacheEvaluation Evaluate(ConfigCacheEntry entry, CachePolicy policy, DateTime utcNow)
        {
            if (entry == null)
                return new CacheEvaluation(CacheStatus.Miss, default, default, default);

            var fetchedAt = UnixTime.FromUnixMs(entry.FetchedAtUnixMs);
            var freshUntil = fetchedAt + policy.Ttl;
            var staleUntil = freshUntil + policy.StaleWindow;

            CacheStatus status;
            if (utcNow < freshUntil) status = CacheStatus.Fresh;
            else if (utcNow < staleUntil) status = CacheStatus.Stale;
            else status = CacheStatus.Expired;

            return new CacheEvaluation(status, fetchedAt, freshUntil, staleUntil);
        }
    }
}