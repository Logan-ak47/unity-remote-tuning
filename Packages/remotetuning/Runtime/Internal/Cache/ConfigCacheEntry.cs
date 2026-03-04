namespace Ashutosh.RemoteTuning
{
    // Stored as JSON via Newtonsoft
    internal sealed class ConfigCacheEntry
    {
        public string RawJson;
        public string ETag;
        public string ConfigVersion;
        public long FetchedAtUnixMs;
    }
}