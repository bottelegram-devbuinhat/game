using UnityEngine;

namespace DudeChaos.Data
{
    [CreateAssetMenu(fileName = "EMO_New", menuName = "DudeChaos/Data/Emote")]
    public class EmoteData : ScriptableObject
    {
        public string id = "";
        public string displayName = "";
        public AnimationClip clip;
        public float duration = 2.5f;
    }
}
