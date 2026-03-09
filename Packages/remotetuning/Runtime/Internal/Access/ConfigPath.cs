using System;

namespace Ashutosh.RemoteTuning
{
    internal static class ConfigPath
    {
        public static string[] Split(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return Array.Empty<string>();

            // Simple dotted path: "tuning.adCooldownSeconds"
            return path.Split('.');
        }
    }
}