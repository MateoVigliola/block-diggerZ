using UnityEngine;
using UnityEngine.InputSystem;

namespace BlockDiggerz.Menus
{
    /// <summary>
    /// Player settings that persist between sessions: music and sound effect volumes (placeholders until audio
    /// exists) and keyboard binding overrides (saved as the Input System's override JSON).
    /// </summary>
    public sealed class GameSettings
    {
        /// <summary>Storage key for the music volume.</summary>
        public const string MusicVolumeKey = "BlockDiggerz.Settings.MusicVolume";

        /// <summary>Storage key for the sound effect volume.</summary>
        public const string SfxVolumeKey = "BlockDiggerz.Settings.SfxVolume";

        /// <summary>Storage key for the binding overrides JSON.</summary>
        public const string BindingsKey = "BlockDiggerz.Settings.Bindings";

        /// <summary>Lowest volume value.</summary>
        public const float MinVolume = 0f;

        /// <summary>Highest volume value.</summary>
        public const float MaxVolume = 100f;

        /// <summary>Volume before the player changes it.</summary>
        public const float DefaultVolume = 80f;

        private readonly IKeyValueStore _store;

        /// <summary>Reads saved volumes from <paramref name="store"/>.</summary>
        public GameSettings(IKeyValueStore store)
        {
            _store = store;
            MusicVolume = Clamp(store.GetFloat(MusicVolumeKey, DefaultVolume));
            SfxVolume = Clamp(store.GetFloat(SfxVolumeKey, DefaultVolume));
        }

        /// <summary>Music volume, 0 to 100.</summary>
        public float MusicVolume { get; private set; }

        /// <summary>Sound effect volume, 0 to 100.</summary>
        public float SfxVolume { get; private set; }

        /// <summary>Sets and saves the music volume (clamped to 0 to 100).</summary>
        public void SetMusicVolume(float value)
        {
            MusicVolume = Clamp(value);
            _store.SetFloat(MusicVolumeKey, MusicVolume);
            _store.Save();
        }

        /// <summary>Sets and saves the sound effect volume (clamped to 0 to 100).</summary>
        public void SetSfxVolume(float value)
        {
            SfxVolume = Clamp(value);
            _store.SetFloat(SfxVolumeKey, SfxVolume);
            _store.Save();
        }

        /// <summary>Puts both volumes back to the default and saves.</summary>
        public void ResetVolumes()
        {
            SetMusicVolume(DefaultVolume);
            SetSfxVolume(DefaultVolume);
        }

        /// <summary>Saves the binding overrides currently applied to <paramref name="actions"/>.</summary>
        public void SaveBindings(InputActionAsset actions)
        {
            if (actions == null)
            {
                return;
            }

            _store.SetString(BindingsKey, actions.SaveBindingOverridesAsJson());
            _store.Save();
        }

        /// <summary>Replaces the overrides on <paramref name="actions"/> with the saved ones (none if nothing saved).</summary>
        public void LoadBindings(InputActionAsset actions)
        {
            if (actions == null)
            {
                return;
            }

            string json = _store.GetString(BindingsKey, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                actions.RemoveAllBindingOverrides();
                return;
            }

            actions.LoadBindingOverridesFromJson(json);
        }

        /// <summary>Removes every override from <paramref name="actions"/> and forgets the saved ones.</summary>
        public void ResetBindings(InputActionAsset actions)
        {
            actions?.RemoveAllBindingOverrides();
            _store.DeleteKey(BindingsKey);
            _store.Save();
        }

        private static float Clamp(float value) => Mathf.Clamp(value, MinVolume, MaxVolume);
    }
}
