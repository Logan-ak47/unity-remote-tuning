namespace Ashutosh.RemoteTuning
{
    internal sealed class ExperimentService
    {
        private readonly ConfigAccessor _accessor;
        private readonly ExperimentAssigner _assigner;
        private readonly ExperimentOverrideStore _overrides;
        private readonly ILogSink _log;

        public ExperimentService(ConfigAccessor accessor, ExperimentAssigner assigner, ExperimentOverrideStore overrides, ILogSink log)
        {
            _accessor = accessor;
            _assigner = assigner;
            _overrides = overrides;
            _log = log ?? new NullLogSink();
        }

        public bool IsInExperiment(string userId, string experimentKey)
        {
            var v = GetVariant(userId, experimentKey, defaultVariant: null, out var inExp);
            return inExp && !string.IsNullOrWhiteSpace(v);
        }

        public string GetVariant(string userId, string experimentKey, string defaultVariant, out bool inExperiment)
        {
            inExperiment = false;

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(experimentKey))
                return defaultVariant;

            // 1) Debug override
            if (_overrides.TryGet(userId, experimentKey, out var forced))
            {
                inExperiment = true;
                _log.Info($"[RemoteTuning] Experiment '{experimentKey}' override -> {forced}");
                return forced;
            }

            // 2) Parse spec from config
            if (!ExperimentSpecParser.TryParse(_accessor, experimentKey, out var spec))
            {
                _log.Warn($"[RemoteTuning] Experiment '{experimentKey}' missing/invalid spec -> default");
                return defaultVariant;
            }

            // 3) Assign/persist
            var a = _assigner.GetOrAssign(userId, spec);
            if (!a.InExperiment || string.IsNullOrWhiteSpace(a.VariantKey))
                return defaultVariant;

            inExperiment = true;
            return a.VariantKey;
        }

        public void SetOverride(string userId, string experimentKey, string variantKey)
        {
            _overrides.Set(userId, experimentKey, variantKey);
            _log.Info($"[RemoteTuning] Override set '{experimentKey}'='{variantKey}' for user '{userId}'");
        }

        public void ClearOverride(string userId, string experimentKey)
        {
            _overrides.Clear(userId, experimentKey);
            _log.Info($"[RemoteTuning] Override cleared '{experimentKey}' for user '{userId}'");
        }
    }
}