using System.Collections.Generic;

namespace Ashutosh.RemoteTuning
{
    public readonly struct TransportResponse
    {
        public readonly long StatusCode;
        public readonly string Body;
        public readonly IReadOnlyDictionary<string, string> Headers;
        public readonly string Error;

        public bool IsSuccess => StatusCode >= 200 && StatusCode <= 299;

        public TransportResponse(long statusCode, string body, IReadOnlyDictionary<string, string> headers, string error)
        {
            StatusCode = statusCode;
            Body = body;
            Headers = headers;
            Error = error;
        }
    }
}