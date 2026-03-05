using Newtonsoft.Json;
using System;

namespace Ashutosh.RemoteTuning
{
    internal sealed class ConfigCacheRepository
    {
        private readonly IKeyValueStore _store;
        private readonly IClock _clock;
        private readonly ILogSink _log;

        public ConfigCacheRepository(IKeyValueStore store, IClock clock, ILogSink log = null)
        {
            _store = store;
            _clock = clock;
            _log = log ?? new NullLogSink();
        }

        public bool TryLoad(out ConfigCacheEntry entry)
        {
            entry = null;

            if (!_store.TryGetString(CacheKeys.RemoteConfigEntry, out var json) || string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
                entry = JsonConvert.DeserializeObject<ConfigCacheEntry>(json);
                if (entry == null || string.IsNullOrWhiteSpace(entry.RawJson) || entry.FetchedAtUnixMs <= 0)
                {
                    _log.Warn("[RemoteTuning] Cache entry invalid shape; clearing.");
                    Clear();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _log.Warn($"[RemoteTuning] Cache parse failed; clearing. {ex.Message}");
                Clear();
                return false;
            }
        }

        public void Save(string rawJson, string eTag, string configVersion)
        {
            var entry = new ConfigCacheEntry
            {
                RawJson = rawJson ?? "{}",
                ETag = eTag,
                ConfigVersion = configVersion,
                FetchedAtUnixMs = UnixTime.ToUnixMs(_clock.UtcNow)
            };

            var json = JsonConvert.SerializeObject(entry);
            _store.SetString(CacheKeys.RemoteConfigEntry, json);
        }

        public void Clear()
        {
            _store.DeleteKey(CacheKeys.RemoteConfigEntry);
        }

        public CacheEvaluation Evaluate(CachePolicy policy)
        {
            if (!TryLoad(out var entry))
                return new CacheEvaluation(CacheStatus.Miss, default, default, default);

            return CacheStateEvaluator.Evaluate(entry, policy, _clock.UtcNow);
        }

        public bool TryTouchFetchedAt()
        {
            if (!TryLoad(out var entry))
                return false;

            // Re-save the same data, but update fetchedAt to now.
            Save(entry.RawJson, entry.ETag, entry.ConfigVersion);
            return true;
        }
    }
}