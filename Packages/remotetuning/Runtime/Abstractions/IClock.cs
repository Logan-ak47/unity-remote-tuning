using System;

namespace Ashutosh.RemoteTuning
{
    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}