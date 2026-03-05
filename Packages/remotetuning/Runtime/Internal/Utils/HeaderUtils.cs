using System.Collections.Generic;

namespace Ashutosh.RemoteTuning
{
    internal static class HeaderUtils
    {
        public static bool TryGetHeader(IReadOnlyDictionary<string, string> headers, string name, out string value)
        {
            value = null;
            if (headers == null || string.IsNullOrEmpty(name))
                return false;

            foreach (var kv in headers)
            {
                if (string.Equals(kv.Key, name, System.StringComparison.OrdinalIgnoreCase))
                {
                    value = kv.Value;
                    return true;
                }
            }

            return false;
        }
    }
}