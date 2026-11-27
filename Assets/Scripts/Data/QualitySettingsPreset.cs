using UnityEngine;

namespace DudeChaos.Data
{
    [CreateAssetMenu(fileName = "QualityPreset", menuName = "DudeChaos/Settings/QualityPreset")]
    public class QualitySettingsPreset : ScriptableObject
    {
        public string presetName = "Medium";
        public float shadowDistance = 45f;
        public int particleBudget = 256;
        [Tooltip("Low 120 / Medium 180 / High 250 / Ultra 350")]
        public float drawDistance = 180f;
        public float streamerUnloadInterval = 2.0f;
    }
}
