using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Ashutosh.RemoteTuning
{
    internal static class UnityWebRequestTask
    {
        public static Task<UnityWebRequest> SendAsync(UnityWebRequest request, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<UnityWebRequest>();

            UnityWebRequestAsyncOperation op;
            try
            {
                op = request.SendWebRequest();
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
                return tcs.Task;
            }

            op.completed += _ => tcs.TrySetResult(request);

            if (ct.CanBeCanceled)
            {
                ct.Register(() =>
                {
                    try { request.Abort(); } catch { /* ignore */ }
                    tcs.TrySetCanceled(ct);
                });
            }

            return tcs.Task;
        }
    }
}