using System;

namespace Ashutosh.RemoteTuning
{
    internal readonly struct CachePolicy
    {
        public readonly TimeSpan Ttl;
        public readonly TimeSpan StaleWindow;

        public CachePolicy(TimeSpan ttl, TimeSpan staleWindow)
        {
            Ttl = ttl < TimeSpan.Zero ? TimeSpan.Zero : ttl;
            StaleWindow = staleWindow < TimeSpan.Zero ? TimeSpan.Zero : staleWindow;
        }

        public static CachePolicy FromSeconds(int ttlSeconds, int staleSeconds)
            => new CachePolicy(TimeSpan.FromSeconds(Math.Max(0, ttlSeconds)),
                               TimeSpan.FromSeconds(Math.Max(0, staleSeconds)));
    }
}