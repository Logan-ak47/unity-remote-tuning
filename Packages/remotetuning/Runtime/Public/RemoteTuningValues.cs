namespace Ashutosh.RemoteTuning
{
    public sealed class RemoteTuningValues
    {
        private readonly ConfigAccessor _accessor;
        private readonly ILogSink _log;

        internal RemoteTuningValues(ConfigAccessor accessor, ILogSink log)
        {
            _accessor = accessor;
            _log = log ?? new NullLogSink();
        }

        public float GetDifficultyScalar(float defaultValue = 1.0f)
            => GetFloat("tuning.difficultyScalar", defaultValue);

        public int GetAdCooldownSeconds(int defaultValue = 60)
            => GetInt("tuning.adCooldownSeconds", defaultValue);

        public float GetRewardMultiplier(float defaultValue = 1.0f)
            => GetFloat("tuning.rewardMultiplier", defaultValue);

        public bool GetBool(string dottedPath, bool defaultValue)
        {
            if (_accessor.TryGetBool(dottedPath, out var v)) return v;
            _log.Warn($"[RemoteTuning] GetBool '{dottedPath}' missing/invalid -> default={defaultValue}");
            return defaultValue;
        }

        public int GetInt(string dottedPath, int defaultValue)
        {
            if (_accessor.TryGetInt(dottedPath, out var v)) return v;
            _log.Warn($"[RemoteTuning] GetInt '{dottedPath}' missing/invalid -> default={defaultValue}");
            return defaultValue;
        }

        public float GetFloat(string dottedPath, float defaultValue)
        {
            if (_accessor.TryGetFloat(dottedPath, out var v)) return v;
            _log.Warn($"[RemoteTuning] GetFloat '{dottedPath}' missing/invalid -> default={defaultValue}");
            return defaultValue;
        }

        public string GetString(string dottedPath, string defaultValue = "")
        {
            if (_accessor.TryGetString(dottedPath, out var v)) return v;
            _log.Warn($"[RemoteTuning] GetString '{dottedPath}' missing/invalid -> default");
            return defaultValue;
        }
    }
}