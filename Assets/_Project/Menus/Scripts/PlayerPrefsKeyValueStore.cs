using UnityEngine;

namespace BlockDiggerz.Menus
{
    /// <summary>Settings storage in PlayerPrefs (per user, per machine).</summary>
    public sealed class PlayerPrefsKeyValueStore : IKeyValueStore
    {
        /// <inheritdoc />
        public float GetFloat(string key, float fallback) => PlayerPrefs.GetFloat(key, fallback);

        /// <inheritdoc />
        public void SetFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);

        /// <inheritdoc />
        public string GetString(string key, string fallback) => PlayerPrefs.GetString(key, fallback);

        /// <inheritdoc />
        public void SetString(string key, string value) => PlayerPrefs.SetString(key, value);

        /// <inheritdoc />
        public void DeleteKey(string key) => PlayerPrefs.DeleteKey(key);

        /// <inheritdoc />
        public void Save() => PlayerPrefs.Save();
    }
}
