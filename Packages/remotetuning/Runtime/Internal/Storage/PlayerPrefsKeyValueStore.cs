using UnityEngine;

namespace Ashutosh.RemoteTuning
{
    internal sealed class PlayerPrefsKeyValueStore : IKeyValueStore
    {
        public bool TryGetString(string key, out string value)
        {
            if (PlayerPrefs.HasKey(key))
            {
                value = PlayerPrefs.GetString(key, string.Empty);
                return true;
            }

            value = null;
            return false;
        }

        public void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value ?? string.Empty);
            PlayerPrefs.Save();
        }

        public void DeleteKey(string key)
        {
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
            }
        }
    }
}