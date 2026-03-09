using System;

namespace Ashutosh.RemoteTuning
{
    internal sealed class ExperimentAssigner
    {
        private readonly IStableHasher _hasher;
        private readonly AssignmentStore _store;
        private readonly ILogSink _log;

        public ExperimentAssigner(IStableHasher hasher, AssignmentStore store, ILogSink log)
        {
            _hasher = hasher;
            _store = store;
            _log = log ?? new NullLogSink();
        }

        public Assignment GetOrAssign(string userId, ExperimentSpec spec)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(spec.Key))
                return Assignment.Out();

            // 1) If already persisted, return it (stable across sessions)
            if (_store.TryGet(userId, spec.Key, out var persisted))
            {
                _log.Info($"[RemoteTuning] Experiment '{spec.Key}' persisted -> {persisted}");
                return new Assignment(true, persisted);
            }

            // 2) Rollout gate
            if (!IsInRollout(userId, spec.Key, spec.Salt, spec.RolloutPercent))
            {
                _log.Info($"[RemoteTuning] Experiment '{spec.Key}' user OUT (rollout={spec.RolloutPercent}%)");
                return Assignment.Out();
            }

            // 3) Pick variant by weights
            var variant = PickWeightedVariant(userId, spec.Key, spec.Salt, spec.Variants);
            if (string.IsNullOrWhiteSpace(variant))
            {
                _log.Warn($"[RemoteTuning] Experiment '{spec.Key}' had no valid variants -> OUT");
                return Assignment.Out();
            }

            // 4) Persist
            _store.Set(userId, spec.Key, variant);
            _log.Info($"[RemoteTuning] Experiment '{spec.Key}' assigned -> {variant} (persisted)");
            return new Assignment(true, variant);
        }

        private bool IsInRollout(string userId, string experimentKey, string salt, int rolloutPercent)
        {
            rolloutPercent = Math.Clamp(rolloutPercent, 0, 100);
            if (rolloutPercent == 0) return false;
            if (rolloutPercent == 100) return true;

            // Bucket from 0..9999
            var bucket = Bucket0To9999(userId, experimentKey, salt, "rollout");
            // rolloutPercent => threshold in 0..10000
            var threshold = rolloutPercent * 100; // e.g. 25% => 2500
            return bucket < threshold;
        }

        private string PickWeightedVariant(string userId, string experimentKey, string salt, System.Collections.Generic.IReadOnlyList<VariantSpec> variants)
        {
            if (variants == null || variants.Count == 0)
                return null;

            var total = 0;
            for (int i = 0; i < variants.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(variants[i].Key) && variants[i].Weight > 0)
                    total += variants[i].Weight;
            }

            if (total <= 0)
                return null;

            // Deterministic roll in [0, total)
            var r = DeterministicRange(userId, experimentKey, salt, "variant", total);

            var acc = 0;
            for (int i = 0; i < variants.Count; i++)
            {
                var v = variants[i];
                if (string.IsNullOrWhiteSpace(v.Key) || v.Weight <= 0)
                    continue;

                acc += v.Weight;
                if (r < acc)
                    return v.Key;
            }

            // Shouldn't happen, but safe fallback:
            return variants[0].Key;
        }

        private int Bucket0To9999(string userId, string experimentKey, string salt, string purpose)
        {
            var input = $"{userId}|{experimentKey}|{salt ?? ""}|{purpose}";
            var h = _hasher.Hash32(input);
            return (int)(h % 10000);
        }

        private int DeterministicRange(string userId, string experimentKey, string salt, string purpose, int range)
        {
            if (range <= 1) return 0;

            var input = $"{userId}|{experimentKey}|{salt ?? ""}|{purpose}";
            var h = _hasher.Hash32(input);
            return (int)(h % (uint)range);
        }
    }
}