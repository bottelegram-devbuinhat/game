using UnityEngine;

namespace DudeChaos.Audio
{
    // Fixed voice pool that NEVER grows; refuses silent voices; steals only barely audible ones; one 2D UI source (§1.8).
    public sealed class AudioDirector : MonoBehaviour
    {
        private AudioSource[] _pool;
        private float[] _voiceEffectiveVolumes;
        private AudioBand[] _voiceBands;
        private AudioSource _uiSource;
        private Transform _listenerTransform;

        public int PoolCapacity => _pool != null ? _pool.Length : 0;

        public void Awake()
        {
            EnsurePoolInitialised();
        }

        public void EnsurePoolInitialised(Transform listener = null)
        {
            if (listener != null) _listenerTransform = listener;
            if (_pool != null) return;

            int cap = AudioBudgetMath.TotalPoolVoices;
            _pool = new AudioSource[cap];
            _voiceEffectiveVolumes = new float[cap];
            _voiceBands = new AudioBand[cap];

            for (int i = 0; i < cap; i++)
            {
                GameObject child = new GameObject($"Voice_{i:D2}");
                child.transform.SetParent(transform, false);
                AudioSource src = child.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 1f;
                src.maxDistance = AudioBudgetMath.MaxAudibleDistance;
                _pool[i] = src;
            }

            GameObject uiGo = new GameObject("UI_Voice_2D");
            uiGo.transform.SetParent(transform, false);
            _uiSource = uiGo.AddComponent<AudioSource>();
            _uiSource.playOnAwake = false;
            _uiSource.spatialBlend = 0f;
        }

        public bool TryPlay3D(AudioClip clip, Vector3 worldPosition, float baseVolume = 1f, float pitchJitterSample = 0f)
        {
            EnsurePoolInitialised();
            if (clip == null || baseVolume <= 0.001f)
                return false;

            Vector3 listenerPos = _listenerTransform != null ? _listenerTransform.position : transform.position;
            float dist = Vector3.Distance(listenerPos, worldPosition);
            AudioBand band = AudioBudgetMath.ClassifyBand(dist);
            if (band == AudioBand.Culled)
                return false;

            float effVol = AudioBudgetMath.ComputeFalloffVolume(baseVolume, dist);
            if (effVol <= 0.005f)
                return false;

            int activeInBand = 0;
            int freeSlot = -1;
            int quietestSlot = -1;
            float quietestVol = float.MaxValue;

            for (int i = 0; i < _pool.Length; i++)
            {
                if (!_pool[i].isPlaying)
                {
                    if (freeSlot < 0) freeSlot = i;
                }
                else
                {
                    if (_voiceBands[i] == band)
                        activeInBand++;
                    if (_voiceEffectiveVolumes[i] < quietestVol)
                    {
                        quietestVol = _voiceEffectiveVolumes[i];
                        quietestSlot = i;
                    }
                }
            }

            int chosenSlot = -1;
            if (activeInBand < AudioBudgetMath.CapForBand(band) && freeSlot >= 0)
            {
                chosenSlot = freeSlot;
            }
            else if (quietestSlot >= 0 && AudioBudgetMath.CanStealVoice(effVol, quietestVol))
            {
                chosenSlot = quietestSlot;
            }

            if (chosenSlot < 0)
                return false;

            AudioSource src = _pool[chosenSlot];
            src.Stop();
            src.transform.position = worldPosition;
            src.clip = clip;
            src.volume = effVol;
            src.pitch = AudioBudgetMath.JitterPitch(1f, pitchJitterSample);
            src.Play();
            _voiceEffectiveVolumes[chosenSlot] = effVol;
            _voiceBands[chosenSlot] = band;
            return true;
        }

        public bool PlayUi2D(AudioClip clip, float volume = 0.85f)
        {
            EnsurePoolInitialised();
            if (clip == null || volume <= 0.001f || _uiSource == null)
                return false;
            _uiSource.PlayOneShot(clip, Mathf.Clamp01(volume));
            return true;
        }
    }
}
