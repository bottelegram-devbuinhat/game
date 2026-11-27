using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace DudeChaos.City
{
    // §1.9.4 & R4: One component owns all street lamps (no per-lamp Update); sun yaw consistent at -35 deg.
    public sealed class DayNightCycle : MonoBehaviour
    {
        public const float SunYawDegrees = -35f;
        private static readonly Color NightAmbient = new Color(0.08f, 0.09f, 0.14f, 1f);
        private static readonly Color DuskColor = new Color(1.0f, 0.55f, 0.30f, 1f);
        private static readonly Color DayColor = new Color(1.0f, 0.96f, 0.88f, 1f);

        [SerializeField] private Light sunLight;
        [SerializeField] private Light[] streetLamps = Array.Empty<Light>();
        [SerializeField] private float currentHour = 9.0f;

        public event Action<float> HourUpdated;

        public float CurrentHour => currentHour;

        public void Awake()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            ApplyCurrentHour();
        }

        public void Configure(Light directionalSun, Light[] lamps, float startHour = 9.0f)
        {
            sunLight = directionalSun;
            streetLamps = lamps ?? Array.Empty<Light>();
            SetHour(startHour);
        }

        public void SetHour(float hour)
        {
            currentHour = Mathf.Repeat(hour, 24f);
            ApplyCurrentHour();
            HourUpdated?.Invoke(currentHour);
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f) return;
            currentHour = DayNightRules.AdvanceHour(currentHour, deltaTime);
            ApplyCurrentHour();
            HourUpdated?.Invoke(currentHour);
        }

        public void Update() => Tick(Time.deltaTime);

        private void ApplyCurrentHour()
        {
            float pitch = DayNightRules.SunPitch(currentHour);
            if (sunLight != null)
            {
                sunLight.transform.rotation = Quaternion.Euler(pitch, SunYawDegrees, 0f);
                bool active = pitch > -2f;
                sunLight.enabled = active;
                sunLight.intensity = active ? Mathf.Clamp01(pitch / DayNightRules.NoonSunPitch) * 1.2f : 0f;
                sunLight.color = Color.Lerp(DuskColor, DayColor, Mathf.Clamp01(pitch / 30f));
            }

            float ambFactor = Mathf.Clamp01(DayNightRules.AmbientFor(currentHour));
            RenderSettings.ambientLight = Color.Lerp(NightAmbient, Color.white, ambFactor);

            float lampFactor = DayNightRules.LightIntensity(currentHour);
            float lampIntensity = 2.4f * lampFactor;
            if (streetLamps != null)
            {
                for (int i = 0; i < streetLamps.Length; i++)
                {
                    Light lamp = streetLamps[i];
                    if (lamp == null) continue;
                    lamp.intensity = lampIntensity;
                    lamp.enabled = lampIntensity >= 0.01f;
                }
            }
        }
    }
}
