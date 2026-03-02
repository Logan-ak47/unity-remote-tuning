namespace Ashutosh.RemoteTuning
{
    public interface IKeyValueStore
    {
        bool TryGetString(string key, out string value);
        void SetString(string key, string value);
        void DeleteKey(string key);
    }
}