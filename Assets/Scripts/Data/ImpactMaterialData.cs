using UnityEngine;

namespace DudeChaos.Data
{
    [CreateAssetMenu(fileName = "IMP_New", menuName = "DudeChaos/Data/ImpactMaterial")]
    public class ImpactMaterialData : ScriptableObject
    {
        public SurfaceMaterialType materialType = SurfaceMaterialType.Concrete;
        public GameObject impactPrefab;
        public GameObject decalPrefab;
        public AudioClip sound;
    }
}
