namespace Ashutosh.RemoteTuning
{
    public sealed class Experiments
    {
        private readonly System.Func<string> _getUserId;
        private readonly ExperimentService _svc;

        internal Experiments(System.Func<string> getUserId, ExperimentService svc)
        {
            _getUserId = getUserId;
            _svc = svc;
        }

        public string GetVariant(string experimentKey, string defaultVariant = "control")
        {
            var userId = _getUserId();
            return _svc.GetVariant(userId, experimentKey, defaultVariant, out _);
        }

        public bool IsInExperiment(string experimentKey)
        {
            var userId = _getUserId();
            return _svc.IsInExperiment(userId, experimentKey);
        }

        // Debug helpers (used by demo UI later)
        public void SetOverride(string experimentKey, string variantKey)
        {
            var userId = _getUserId();
            _svc.SetOverride(userId, experimentKey, variantKey);
        }

        public void ClearOverride(string experimentKey)
        {
            var userId = _getUserId();
            _svc.ClearOverride(userId, experimentKey);
        }
    }
}