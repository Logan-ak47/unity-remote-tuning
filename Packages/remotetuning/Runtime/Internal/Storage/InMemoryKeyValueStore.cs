using System.Collections.Generic;

namespace Ashutosh.RemoteTuning
{
    internal sealed class InMemoryKeyValueStore : IKeyValueStore
    {
        private readonly Dictionary<string, string> _map = new();

        public bool TryGetString(string key, out string value)
            => _map.TryGetValue(key, out value);

        public void SetString(string key, string value)
            => _map[key] = value;

        public void DeleteKey(string key)
            => _map.Remove(key);
    }
}