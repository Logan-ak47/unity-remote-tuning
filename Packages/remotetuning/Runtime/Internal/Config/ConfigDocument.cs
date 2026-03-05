using Newtonsoft.Json.Linq;

namespace Ashutosh.RemoteTuning
{
    internal sealed class ConfigDocument
    {
        public readonly string RawJson;
        public readonly string ConfigVersion;
        public readonly JObject Root;

        public ConfigDocument(string rawJson, string configVersion, JObject root)
        {
            RawJson = rawJson;
            ConfigVersion = configVersion;
            Root = root;
        }
    }
}