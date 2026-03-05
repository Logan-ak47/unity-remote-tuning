using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ashutosh.RemoteTuning
{
    internal sealed class RemoteConfigService
    {
        private readonly RemoteTuningOptions _options;
        private readonly IConfigTransport _transport;
        private readonly ConfigCacheRepository _cache;
        private readonly IConfigValidator _validator;
        private readonly IClock _clock;
        private readonly ILogSink _log;

        public RemoteConfigService(
            RemoteTuningOptions options,
            IConfigTransport transport,
            ConfigCacheRepository cache,
            IConfigValidator validator,
            IClock clock,
            ILogSink log)
        {
            _options = options;
            _transport = transport;
            _cache = cache;
            _validator = validator;
            _clock = clock;
            _log = log ?? new NullLogSink();
        }

        public RemoteTuningSnapshot GetBestAvailableSnapshot()
        {
            var policy = CachePolicy.FromSeconds(_options.TtlSeconds, _options.StaleSeconds);

            if (_cache.TryLoad(out var entry))
            {
                var eval = CacheStateEvaluator.Evaluate(entry, policy, _clock.UtcNow);

                if (eval.Status == CacheStatus.Fresh)
                    return MakeSnapshotFromCache(entry, eval, ConfigLoadReason.Cache_Fresh);

                if (eval.Status == CacheStatus.Stale)
                    return MakeSnapshotFromCache(entry, eval, ConfigLoadReason.Cache_Stale_Served);

                // Expired
                return MakeSnapshotFromDefaults(policy, ConfigLoadReason.Default_ExpiredCache);
            }

            return MakeSnapshotFromDefaults(policy, ConfigLoadReason.Default_NoCache);
        }

        public async Task<RemoteTuningSnapshot> RefreshAsync(CancellationToken ct)
        {
            // 1) Decide what we can serve immediately (cache/default) while we attempt remote.
            var baseline = GetBestAvailableSnapshot();

            // 2) Prepare request (ETag if available)
            string existingETag = null;
            if (_cache.TryLoad(out var cached))
                existingETag = cached.ETag;

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(existingETag))
                headers[TransportHeaderNames.IfNoneMatch] = existingETag;

            var req = new TransportRequest(_options.EndpointUrl, _options.TimeoutSeconds, headers);

            // 3) Fetch remote
            var resp = await _transport.GetAsync(req, ct);

            // 4) Handle outcomes
            if (resp.StatusCode == 304)
            {
                // Not modified: keep cached json, but extend TTL by touching fetchedAt.
                if (_cache.TryTouchFetchedAt() && _cache.TryLoad(out var touched))
                {
                    var policy = CachePolicy.FromSeconds(_options.TtlSeconds, _options.StaleSeconds);
                    var eval = CacheStateEvaluator.Evaluate(touched, policy, _clock.UtcNow);

                    var snap = MakeSnapshotFromCache(touched, eval, ConfigLoadReason.Remote_NotModified);
                    _log.Info("[RemoteTuning] Remote 304 Not Modified -> using cache (touched).");
                    return snap;
                }

                _log.Warn("[RemoteTuning] Remote 304 but no usable cache -> using baseline.");
                return baseline;
            }

            if (resp.IsSuccess && resp.StatusCode == 200)
            {
                var rawJson = resp.Body ?? string.Empty;

                // Validate before committing cache
                var vr = _validator.Validate(rawJson, out var doc);
                if (!vr.IsValid)
                {
                    _log.Warn($"[RemoteTuning] Remote config invalid ({vr.Errors.Count} errors). Not overwriting cache.");
                    for (int i = 0; i < vr.Errors.Count; i++)
                        _log.Warn($"[RemoteTuning]  - {vr.Errors[i]}");

                    // choose reason based on whether baseline is cache or default
                    return baseline.Source == ConfigSource.Cache
                        ? WithReason(baseline, ConfigLoadReason.Remote_Invalid_UsingCache)
                        : WithReason(baseline, ConfigLoadReason.Remote_Invalid_UsingDefault);
                }

                // Get ETag if present
                string newETag = null;
                if (HeaderUtils.TryGetHeader(resp.Headers, TransportHeaderNames.ETag, out var etagVal))
                    newETag = etagVal;

                _cache.Save(rawJson, newETag, doc.ConfigVersion);

                // Evaluate TTL info for the new cache entry
                _cache.TryLoad(out var saved);
                var policy2 = CachePolicy.FromSeconds(_options.TtlSeconds, _options.StaleSeconds);
                var eval2 = CacheStateEvaluator.Evaluate(saved, policy2, _clock.UtcNow);

                _log.Info("[RemoteTuning] Remote config accepted and cached.");
                return new RemoteTuningSnapshot(
                    ConfigSource.Remote,
                    ConfigLoadReason.Remote_Success,
                    eval2.Status,
                    eval2.FetchedAtUtc,
                    eval2.FreshUntilUtc,
                    eval2.StaleUntilUtc,
                    saved.ConfigVersion ?? doc.ConfigVersion ?? "unknown",
                    isStale: false,
                    rawJson: saved.RawJson);
            }

            // Non-200 success or failure
            _log.Warn($"[RemoteTuning] Remote fetch failed. Status={resp.StatusCode} Error={resp.Error}");

            return baseline.Source == ConfigSource.Cache
                ? WithReason(baseline, ConfigLoadReason.Remote_Failed_UsingCache)
                : WithReason(baseline, ConfigLoadReason.Remote_Failed_UsingDefault);
        }

        private RemoteTuningSnapshot MakeSnapshotFromCache(ConfigCacheEntry entry, CacheEvaluation eval, ConfigLoadReason reason)
        {
            return new RemoteTuningSnapshot(
                ConfigSource.Cache,
                reason,
                eval.Status,
                eval.FetchedAtUtc,
                eval.FreshUntilUtc,
                eval.StaleUntilUtc,
                entry.ConfigVersion ?? "unknown",
                isStale: eval.Status == CacheStatus.Stale,
                rawJson: entry.RawJson);
        }

        private RemoteTuningSnapshot MakeSnapshotFromDefaults(CachePolicy policy, ConfigLoadReason reason)
        {
            // Defaults are treated as a "Miss" cache status
            var now = _clock.UtcNow;
            return new RemoteTuningSnapshot(
                ConfigSource.Default,
                reason,
                CacheStatus.Miss,
                fetchedAtUtc: now,
                freshUntilUtc: now,
                staleUntilUtc: now,
                configVersion: "default",
                isStale: false,
                rawJson: ConfigDefaults.RawJson);
        }

        private static RemoteTuningSnapshot WithReason(RemoteTuningSnapshot snapshot, ConfigLoadReason reason)
        {
            return new RemoteTuningSnapshot(
                snapshot.Source,
                reason,
                snapshot.CacheStatus,
                snapshot.FetchedAtUtc,
                snapshot.FreshUntilUtc,
                snapshot.StaleUntilUtc,
                snapshot.ConfigVersion,
                snapshot.IsStale,
                snapshot.RawJson);
        }
    }
}