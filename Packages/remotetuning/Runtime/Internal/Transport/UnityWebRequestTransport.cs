using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Ashutosh.RemoteTuning
{
    /// <summary>
    /// Runtime transport adapter over UnityWebRequest.
    /// Kept internal so the public API stays stable.
    /// </summary>
    internal sealed class UnityWebRequestTransport : IConfigTransport
    {
        public async Task<TransportResponse> GetAsync(TransportRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Url))
                return new TransportResponse(0, null, EmptyHeaders(), "Invalid URL (empty).");

            UnityWebRequest uwr = null;
            try
            {
                uwr = UnityWebRequest.Get(request.Url);
                uwr.timeout = Math.Max(1, request.TimeoutSeconds);

                // Default accept header (optional, but nice)
                uwr.SetRequestHeader(TransportHeaderNames.Accept, "application/json");

                // Custom headers (ETag, auth, etc.)
                if (request.Headers != null)
                {
                    foreach (var kv in request.Headers)
                    {
                        if (string.IsNullOrEmpty(kv.Key)) continue;
                        uwr.SetRequestHeader(kv.Key, kv.Value ?? string.Empty);
                    }
                }

                await UnityWebRequestTask.SendAsync(uwr, ct);

                // Read response headers
                var respHeaders = CopyHeaders(uwr.GetResponseHeaders());

                // Map result to a stable response
#if UNITY_2020_2_OR_NEWER
                // Unity uses uwr.result
                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    return new TransportResponse(uwr.responseCode, uwr.downloadHandler?.text, respHeaders, null);
                }

                // 304 often comes through as "ProtocolError" but we still want to treat it as a valid HTTP response
                if (uwr.responseCode == 304)
                {
                    return new TransportResponse(304, null, respHeaders, null);
                }

                var err = string.IsNullOrWhiteSpace(uwr.error) ? "Request failed." : uwr.error;
                return new TransportResponse(uwr.responseCode, uwr.downloadHandler?.text, respHeaders, err);
#else
                // Older Unity fallback
                if (!uwr.isNetworkError && !uwr.isHttpError)
                {
                    return new TransportResponse(uwr.responseCode, uwr.downloadHandler?.text, respHeaders, null);
                }

                if (uwr.responseCode == 304)
                {
                    return new TransportResponse(304, null, respHeaders, null);
                }

                var err = string.IsNullOrWhiteSpace(uwr.error) ? "Request failed." : uwr.error;
                return new TransportResponse(uwr.responseCode, uwr.downloadHandler?.text, respHeaders, err);
#endif
            }
            catch (OperationCanceledException)
            {
                // CancellationToken triggered (we aborted)
                return new TransportResponse(0, null, EmptyHeaders(), "Canceled.");
            }
            catch (Exception ex)
            {
                return new TransportResponse(0, null, EmptyHeaders(), ex.Message);
            }
            finally
            {
                uwr?.Dispose();
            }
        }

        private static IReadOnlyDictionary<string, string> EmptyHeaders()
            => new Dictionary<string, string>(0);

        private static IReadOnlyDictionary<string, string> CopyHeaders(Dictionary<string, string> headers)
        {
            if (headers == null || headers.Count == 0)
                return EmptyHeaders();

            // Copy to avoid Unity-owned dict being mutated unexpectedly.
            return new Dictionary<string, string>(headers);
        }
    }
}