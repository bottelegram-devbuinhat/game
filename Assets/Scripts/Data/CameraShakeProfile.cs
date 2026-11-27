using UnityEngine;

namespace DudeChaos.Data
{
    [CreateAssetMenu(fileName = "SHK_New", menuName = "DudeChaos/Data/CameraShake")]
    public class CameraShakeProfile : ScriptableObject
    {
        public string profileId = "default";
        public float amplitude = 1.0f;
        public float frequency = 18.0f;
        public float duration = 0.18f;
        public AnimationCurve falloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    }
}
