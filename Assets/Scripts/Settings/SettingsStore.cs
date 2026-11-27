using System;
using UnityEngine;

namespace DudeChaos.Settings
{
    public sealed class SettingsStore : MonoBehaviour
    {
        private const string PrefsKey = "DudeChaos.GameSettings";

        [SerializeField] private QualityApplier qualityApplier;
        private GameSettings _current = new GameSettings();

        public event Action<GameSettings> Changed;

        public GameSettings Current => _current;

        public void Awake()
        {
            if (qualityApplier == null)
                qualityApplier = GetComponent<QualityApplier>();
            Load();
        }

        public void BindQualityApplier(QualityApplier applier)
        {
            qualityApplier = applier;
        }

        public void Load()
        {
            if (PlayerPrefs.HasKey(PrefsKey))
            {
                string json = PlayerPrefs.GetString(PrefsKey, string.Empty);
                try
                {
                    GameSettings parsed = JsonUtility.FromJson<GameSettings>(json);
                    if (parsed != null) _current = parsed;
                }
                catch
                {
                    _current = new GameSettings();
                }
            }
            _current.Clamp();
            Apply();
        }

        public void Save()
        {
            _current.Clamp();
            PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(_current));
            PlayerPrefs.Save();
        }

        // R6: Never write shadowDistance or targetFrameRate directly; push through QualityApplier.SetPlayerSettings.
        public void Apply()
        {
            _current.Clamp();
            if (qualityApplier == null)
                qualityApplier = GetComponent<QualityApplier>();
            if (qualityApplier != null)
            {
                qualityApplier.SetPlayerSettings(_current.shadows, _current.shadowDistance, _current.targetFps);
            }
            Changed?.Invoke(_current);
        }

        public void UpdateSettings(Action<GameSettings> mutator, bool saveToDisk = true)
        {
            mutator?.Invoke(_current);
            _current.Clamp();
            if (saveToDisk) Save();
            Apply();
        }

        public void ResetToDefaults()
        {
            _current = new GameSettings();
            Save();
            Apply();
        }
    }
}
