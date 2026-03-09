namespace Ashutosh.RemoteTuning
{
    internal sealed class ExperimentOverrideStore
    {
        private readonly IKeyValueStore _store;
        private const string Prefix = "Ashutosh.RemoteTuning.ExpOverrides.v1.";

        public ExperimentOverrideStore(IKeyValueStore store)
        {
            _store = store;
        }

        private static string Key(string userId, string experimentKey)
            => $"{Prefix}{userId}::{experimentKey}";

        public bool TryGet(string userId, string experimentKey, out string variantKey)
        {
            variantKey = null;
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(experimentKey))
                return false;

            return _store.TryGetString(Key(userId, experimentKey), out variantKey) &&
                   !string.IsNullOrWhiteSpace(variantKey);
        }

        public void Set(string userId, string experimentKey, string variantKey)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(experimentKey) || string.IsNullOrWhiteSpace(variantKey))
                return;

            _store.SetString(Key(userId, experimentKey), variantKey);
        }

        public void Clear(string userId, string experimentKey)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(experimentKey))
                return;

            _store.DeleteKey(Key(userId, experimentKey));
        }
    }
}