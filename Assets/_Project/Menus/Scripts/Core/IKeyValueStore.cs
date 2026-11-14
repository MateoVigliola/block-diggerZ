namespace BlockDiggerz.Menus
{
    /// <summary>Where settings are kept. The game uses PlayerPrefs; tests use an in-memory store.</summary>
    public interface IKeyValueStore
    {
        /// <summary>The stored number, or <paramref name="fallback"/> if none.</summary>
        float GetFloat(string key, float fallback);

        /// <summary>Stores a number.</summary>
        void SetFloat(string key, float value);

        /// <summary>The stored text, or <paramref name="fallback"/> if none.</summary>
        string GetString(string key, string fallback);

        /// <summary>Stores text.</summary>
        void SetString(string key, string value);

        /// <summary>Removes a key.</summary>
        void DeleteKey(string key);

        /// <summary>Writes pending changes to disk.</summary>
        void Save();
    }
}
