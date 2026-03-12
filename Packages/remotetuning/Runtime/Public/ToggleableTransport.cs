using System.Threading;
using System.Threading.Tasks;

namespace Ashutosh.RemoteTuning
{
    /// <summary>
    /// Public transport wrapper so demos/tests can simulate offline mode without touching internals.
    /// </summary>
    public sealed class ToggleableTransport : IConfigTransport
    {
        private readonly IConfigTransport _inner;

        public bool IsOffline { get; private set; }

        public ToggleableTransport(IConfigTransport inner)
        {
            _inner = inner;
        }

        public void SetOffline(bool offline) => IsOffline = offline;

        public Task<TransportResponse> GetAsync(TransportRequest request, CancellationToken ct)
        {
            if (IsOffline)
                return Task.FromResult(new TransportResponse(0, null, null, "Offline (simulated)."));

            return _inner.GetAsync(request, ct);
        }
    }
}