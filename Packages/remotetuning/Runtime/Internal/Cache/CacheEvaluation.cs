using System;

namespace Ashutosh.RemoteTuning
{
    internal readonly struct CacheEvaluation
    {
        public readonly CacheStatus Status;
        public readonly DateTime FetchedAtUtc;
        public readonly DateTime FreshUntilUtc;
        public readonly DateTime StaleUntilUtc;

        public bool CanServe => Status == CacheStatus.Fresh || Status == CacheStatus.Stale;
        public bool ShouldRevalidate => Status == CacheStatus.Stale || Status == CacheStatus.Expired || Status == CacheStatus.Miss;

        public CacheEvaluation(CacheStatus status, DateTime fetchedAtUtc, DateTime freshUntilUtc, DateTime staleUntilUtc)
        {
            Status = status;
            FetchedAtUtc = fetchedAtUtc;
            FreshUntilUtc = freshUntilUtc;
            StaleUntilUtc = staleUntilUtc;
        }
    }
}