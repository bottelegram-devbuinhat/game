using DudeChaos.Settings;
using UnityEngine;

namespace DudeChaos.UI
{
    // §1.9.2, §1.9.5 & R6: Wired to SettingsStore on the Systems object.
    // Pushes audio, vSync, camera far clip (QualityRules.FarClipFor), HUD, joystick,
    // and delegates shadow/fps globals exclusively to QualityApplier.SetPlayerSettings.
    public sealed class SettingsApplier : MonoBehaviour
    {
        [SerializeField] private SettingsStore store;
        [SerializeField] private QualityApplier qualityApplier;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private HudController hud;
        [SerializeField] private MobileJoystick joystick;

        public SettingsStore Store => store;

        public void Awake()
        {
            if (store == null) store = GetComponent<SettingsStore>();
            if (qualityApplier == null) qualityApplier = GetComponent<QualityApplier>();
            if (store != null)
            {
                store.Changed += Apply;
                Apply(store.Current);
            }
        }

        public void OnDestroy()
        {
            if (store != null)
                store.Changed -= Apply;
        }

        public void Bind(SettingsStore settingsStore, QualityApplier applier = null, Camera cam = null)
        {
            if (store != null) store.Changed -= Apply;
            store = settingsStore;
            if (applier != null) qualityApplier = applier;
            if (cam != null) targetCamera = cam;
            if (store != null)
            {
                store.Changed += Apply;
                Apply(store.Current);
            }
        }

        public void Apply(GameSettings settings)
        {
            if (settings == null) return;
            settings.Clamp();

            if (qualityApplier == null) qualityApplier = GetComponent<QualityApplier>();
            qualityApplier?.SetPlayerSettings(settings.shadows, settings.shadowDistance, settings.targetFps);

            QualitySettings.vSyncCount = settings.vSync ? 1 : 0;
            AudioListener.volume = settings.mute ? 0f : settings.masterVolume;

            Camera cam = targetCamera != null ? targetCamera : Camera.main;
            if (cam != null)
            {
                cam.farClipPlane = QualityRules.FarClipFor(settings.drawDistance);
            }

            hud?.SetShowFps(settings.showFps);
            joystick?.ApplyVisualPrefs(settings.joystickSize, settings.joystickOpacity, settings.leftHandedJoystick);
        }
    }
}
