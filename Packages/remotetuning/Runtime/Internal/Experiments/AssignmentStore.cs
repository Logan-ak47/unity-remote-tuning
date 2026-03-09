namespace Ashutosh.RemoteTuning
{
    internal sealed class AssignmentStore
    {
        private readonly IKeyValueStore _store;

        public AssignmentStore(IKeyValueStore store)
        {
            _store = store;
        }

        public bool TryGet(string userId, string experimentKey, out string variantKey)
        {
            variantKey = null;
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(experimentKey))
                return false;

            var key = ExperimentKeyUtil.BuildAssignmentKey(userId, experimentKey);
            return _store.TryGetString(key, out variantKey) && !string.IsNullOrWhiteSpace(variantKey);
        }

        public void Set(string userId, string experimentKey, string variantKey)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(experimentKey) || string.IsNullOrWhiteSpace(variantKey))
                return;

            var key = ExperimentKeyUtil.BuildAssignmentKey(userId, experimentKey);
            _store.SetString(key, variantKey);
        }

        public void Clear(string userId, string experimentKey)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(experimentKey))
                return;

            var key = ExperimentKeyUtil.BuildAssignmentKey(userId, experimentKey);
            _store.DeleteKey(key);
        }
    }
}