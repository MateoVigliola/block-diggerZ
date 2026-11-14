using System.Collections.Generic;

namespace BlockDiggerz.Menus.Tests
{
    /// <summary>In-memory settings storage for tests, so they never touch the real PlayerPrefs.</summary>
    internal sealed class MemoryKeyValueStore : IKeyValueStore
    {
        public readonly Dictionary<string, object> Values = new();

        public int SaveCount { get; private set; }

        public float GetFloat(string key, float fallback) => Values.TryGetValue(key, out object v) ? (float)v : fallback;

        public void SetFloat(string key, float value) => Values[key] = value;

        public string GetString(string key, string fallback) => Values.TryGetValue(key, out object v) ? (string)v : fallback;

        public void SetString(string key, string value) => Values[key] = value;

        public void DeleteKey(string key) => Values.Remove(key);

        public void Save() => SaveCount++;
    }
}
