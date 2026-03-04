using System;

namespace Ashutosh.RemoteTuning
{
    internal sealed class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}