using UnityEngine;

namespace DudeChaos.Audio
{
    public sealed class DriftAudio : MonoBehaviour
    {
        public const float CrashCooldownSeconds = 0.45f;

        [SerializeField] private AudioSource driftLoopSource;
        [SerializeField] private AudioClip crashClip;

        private float _lastCrashTime = -99f;

        public static float ComputeLoopVolume(float slipAngleDeg, float speedMetersPerSecond)
        {
            if (speedMetersPerSecond < 6f || slipAngleDeg < 14f)
                return 0f;
            float angleFactor = Mathf.Clamp01((slipAngleDeg - 14f) / 40f);
            float speedFactor = Mathf.Clamp01((speedMetersPerSecond - 6f) / 22f);
            return angleFactor * speedFactor * 0.85f;
        }

        public void UpdateDrift(float slipAngleDeg, float speed)
        {
            if (driftLoopSource == null) return;
            float vol = ComputeLoopVolume(slipAngleDeg, speed);
            driftLoopSource.volume = vol;
            if (vol > 0.01f && !driftLoopSource.isPlaying && driftLoopSource.clip != null)
                driftLoopSource.Play();
            else if (vol <= 0.01f && driftLoopSource.isPlaying)
                driftLoopSource.Stop();
        }

        public bool TryPlayCrash(AudioDirector director, float impactSpeed, float currentTime)
        {
            if (impactSpeed < 5f || (currentTime - _lastCrashTime) < CrashCooldownSeconds)
                return false;
            _lastCrashTime = currentTime;
            float vol = Mathf.Clamp01(impactSpeed / 25f);
            return director != null && crashClip != null && director.TryPlay3D(crashClip, transform.position, vol);
        }
    }
}
