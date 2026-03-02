using System.Threading;
using System.Threading.Tasks;

namespace Ashutosh.RemoteTuning
{
    public interface IConfigTransport
    {
        Task<TransportResponse> GetAsync(TransportRequest request, CancellationToken ct);
    }
}