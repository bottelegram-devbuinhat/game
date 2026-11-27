using System;
using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Settings
{
    [Serializable]
    public sealed class GameSettings
    {
        public int qualityPreset = 1;
        public bool shadows = true;
        public float shadowDistance = 0f; // 0 = use tier's own shadow distance (§1.9.5)
        public bool particles = true;
        public float drawDistance = 180f; // Clamped 60..400 (§1.9.5)
        public bool vSync = false;
        public int targetFps = 60;

        public float masterVolume = 1.0f;
        public float musicVolume = 0.75f;
        public float sfxVolume = 0.90f;
        public float uiVolume = 0.85f;
        public bool mute = false;

        public float joystickSize = 1.0f;
        public float joystickOpacity = 0.75f;
        public bool leftHandedJoystick = false;
        public float lookSensitivity = 1.0f;
        public bool invertY = false;

        public LanguageCode language = LanguageCode.Vietnamese;
        public bool haptics = true;
        public bool showFps = false;
        public float textScale = 1.0f;
        public ColourBlindMode colourBlindMode = ColourBlindMode.None;
        public bool highContrast = false;

        public void Clamp()
        {
            qualityPreset = Mathf.Clamp(qualityPreset, 0, 2);
            shadowDistance = float.IsNaN(shadowDistance) ? 0f : Mathf.Clamp(shadowDistance, 0f, 100f);
            drawDistance = float.IsNaN(drawDistance) ? 180f : Mathf.Clamp(drawDistance, 60f, 400f);
            targetFps = Mathf.Clamp(targetFps, 30, 120);

            masterVolume = Mathf.Clamp01(masterVolume);
            musicVolume = Mathf.Clamp01(musicVolume);
            sfxVolume = Mathf.Clamp01(sfxVolume);
            uiVolume = Mathf.Clamp01(uiVolume);

            joystickSize = Mathf.Clamp(joystickSize, 0.6f, 1.6f);
            joystickOpacity = Mathf.Clamp(joystickOpacity, 0.2f, 1.0f);
            lookSensitivity = Mathf.Clamp(lookSensitivity, 0.2f, 4.0f);
            textScale = Mathf.Clamp(textScale, 0.8f, 1.5f);

            if (!Enum.IsDefined(typeof(LanguageCode), language))
                language = LanguageCode.English;
            if (!Enum.IsDefined(typeof(ColourBlindMode), colourBlindMode))
                colourBlindMode = ColourBlindMode.None;
        }
    }
}
