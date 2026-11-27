using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace DudeChaos.Settings
{
    // Sole owner of QualitySettings.shadows, shadowDistance, lodBias, antiAliasing, Application.targetFrameRate,
    // and RenderSettings.fog / fogDensity (§1.9.2, §1.9.3, R2, R3, R6).
    public sealed class QualityApplier : MonoBehaviour
    {
        public const float DemoteHoldSeconds = 4.0f;
        public const float PromoteHoldSeconds = 20.0f;
        public const float FrameBudgetTolerance = 1.15f;

        private int _currentTier = 1;
        private bool _playerShadows = true;
        private float _playerShadowDistance = 0f;
        private int _playerTargetFps = 60;

        private float _emaFrameMs = 16.6f;
        private float _slowSeconds;
        private float _fastSeconds;

        public int CurrentTier => _currentTier;
        public RuntimeQualityPreset ActivePreset => QualityRules.GetPreset(_currentTier);

        public void Awake()
        {
            int detected = QualityRules.PickTier(SystemInfo.systemMemorySize, SystemInfo.processorCount);
            Apply(detected);
        }

        public void Apply(int tier)
        {
            _currentTier = Mathf.Clamp(tier, 0, 2);
            RuntimeQualityPreset preset = QualityRules.GetPreset(_currentTier);

            float effectiveShadowDist = QualityRules.ShadowDistanceFor(_currentTier, _playerShadows, _playerShadowDistance);
            bool shadowsOn = preset.shadowsEnabled && _playerShadows && effectiveShadowDist > 0f;

            QualitySettings.shadows = shadowsOn ? ShadowQuality.All : ShadowQuality.Disable;
            QualitySettings.shadowDistance = shadowsOn ? effectiveShadowDist : 0f;
            QualitySettings.lodBias = preset.lodBias;
            QualitySettings.antiAliasing = preset.antiAliasing;
            Application.targetFrameRate = QualityRules.TargetFrameRateFor(_currentTier, _playerTargetFps);

            // R2: Write msaaSampleCount on the active URP RenderPipelineAsset as well.
            RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;
            if (pipeline != null)
            {
                int msaa = preset.antiAliasing <= 1 ? 1 : preset.antiAliasing;
                PropertyInfo prop = pipeline.GetType().GetProperty("msaaSampleCount", BindingFlags.Instance | BindingFlags.Public);
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(pipeline, msaa);
                }
            }

            // §1.9.3 / R3: Apply fog per tier so 130 m prop cutoff has atmospheric falloff.
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = preset.fogDensity;
        }

        public void SetPlayerSettings(bool shadows, float shadowDistance, int targetFps)
        {
            _playerShadows = shadows;
            _playerShadowDistance = Mathf.Clamp(shadowDistance, 0f, 100f);
            _playerTargetFps = Mathf.Clamp(targetFps, 30, 120);
            Apply(_currentTier);
        }

        public void StepFrameAdaptation(float unscaledDeltaSeconds)
        {
            if (unscaledDeltaSeconds <= 0f) return;
            float frameMs = unscaledDeltaSeconds * 1000f;
            _emaFrameMs = Mathf.Lerp(_emaFrameMs, frameMs, 0.12f);

            int targetFps = Mathf.Max(30, Application.targetFrameRate);
            float budgetMs = (1000f / targetFps) * FrameBudgetTolerance;

            if (_emaFrameMs > budgetMs)
            {
                _slowSeconds += unscaledDeltaSeconds;
                _fastSeconds = 0f;
                if (_slowSeconds >= DemoteHoldSeconds && _currentTier > 0)
                {
                    _slowSeconds = 0f;
                    Apply(_currentTier - 1);
                }
            }
            else if (_emaFrameMs < (1000f / targetFps) * 0.82f)
            {
                _fastSeconds += unscaledDeltaSeconds;
                _slowSeconds = 0f;
                if (_fastSeconds >= PromoteHoldSeconds && _currentTier < 2)
                {
                    _fastSeconds = 0f;
                    Apply(_currentTier + 1);
                }
            }
            else
            {
                _slowSeconds = 0f;
                _fastSeconds = 0f;
            }
        }

        public void Update()
        {
            StepFrameAdaptation(Time.unscaledDeltaTime);
        }
    }
}
