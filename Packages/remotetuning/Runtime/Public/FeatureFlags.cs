namespace Ashutosh.RemoteTuning
{
    public sealed class FeatureFlags
    {
        private readonly ConfigAccessor _accessor;
        private readonly ILogSink _log;
        private readonly Experiments _experiments;

        internal FeatureFlags(ConfigAccessor accessor, ILogSink log, Experiments experiments)
        {
            _accessor = accessor;
            _log = log ?? new NullLogSink();
            _experiments = experiments;
        }

        /// <summary>Reads a bool at: features.{key}</summary>
        public bool IsEnabled(string key, bool defaultValue = false)
        {
            if (string.IsNullOrWhiteSpace(key))
                return defaultValue;

            var path = $"features.{key}";
            if (_accessor.TryGetBool(path, out var val))
            {
                _log.Info($"[RemoteTuning] Flag '{key}'={val}");
                return val;
            }

            _log.Warn($"[RemoteTuning] Flag '{key}' missing/invalid -> default={defaultValue}");
            return defaultValue;
        }

        /// <summary>Experiment variant for current user (uses Experiments service).</summary>
        public string GetVariant(string experimentKey, string defaultVariant = "control")
        {
            if (_experiments == null)
                return defaultVariant;

            return _experiments.GetVariant(experimentKey, defaultVariant);
        }
    }
}