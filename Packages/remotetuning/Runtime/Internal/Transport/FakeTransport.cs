using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ashutosh.RemoteTuning
{
    internal sealed class FakeTransport : IConfigTransport
    {
        private Func<TransportRequest, TransportResponse> _handler;

        public FakeTransport(Func<TransportRequest, TransportResponse> handler = null)
        {
            _handler = handler ?? (_ => new TransportResponse(0, null, EmptyHeaders(), "No handler set."));
        }

        public void SetHandler(Func<TransportRequest, TransportResponse> handler)
        {
            _handler = handler ?? (_ => new TransportResponse(0, null, EmptyHeaders(), "Handler was null."));
        }

        public Task<TransportResponse> GetAsync(TransportRequest request, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return Task.FromCanceled<TransportResponse>(ct);

            var resp = _handler(request);
            return Task.FromResult(resp);
        }

        private static IReadOnlyDictionary<string, string> EmptyHeaders()
            => new Dictionary<string, string>(0);
    }
}