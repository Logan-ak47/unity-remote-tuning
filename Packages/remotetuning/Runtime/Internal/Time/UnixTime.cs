using System;

namespace Ashutosh.RemoteTuning
{
    internal static class UnixTime
    {
        public static long ToUnixMs(DateTime utc)
        {
            if (utc.Kind != DateTimeKind.Utc)
                utc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);

            return new DateTimeOffset(utc).ToUnixTimeMilliseconds();
        }

        public static DateTime FromUnixMs(long unixMs)
            => DateTimeOffset.FromUnixTimeMilliseconds(unixMs).UtcDateTime;
    }
}