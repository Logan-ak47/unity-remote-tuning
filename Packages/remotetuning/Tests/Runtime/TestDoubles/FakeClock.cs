using System;

namespace Ashutosh.RemoteTuning.Tests
{
    internal sealed class FakeClock : IClock
    {
        public DateTime UtcNow { get; private set; }

        public FakeClock(DateTime startUtc)
        {
            UtcNow = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc);
        }

        public void Advance(TimeSpan delta)
        {
            UtcNow = UtcNow.Add(delta);
        }

        public void Set(DateTime utc)
        {
            UtcNow = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        }
    }
}