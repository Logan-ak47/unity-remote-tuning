using System.Collections.Generic;

namespace Ashutosh.RemoteTuning
{
    public readonly struct TransportRequest
    {
        public readonly string Url;
        public readonly int TimeoutSeconds;
        public readonly IReadOnlyDictionary<string, string> Headers;

        public TransportRequest(string url, int timeoutSeconds, IReadOnlyDictionary<string, string> headers)
        {
            Url = url;
            TimeoutSeconds = timeoutSeconds;
            Headers = headers;
        }
    }
}