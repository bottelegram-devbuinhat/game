using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.City
{
    [CreateAssetMenu(fileName = "district_new", menuName = "DudeChaos/Data/District")]
    public class DistrictDefinition : ScriptableObject
    {
        public string districtId = "";
        public string displayNameKey = "";
        public DistrictKind kind = DistrictKind.Downtown;
        public Vector2 centre = Vector2.zero;
        public Vector2 size = new Vector2(300f, 300f);
        public bool requiresPass = false;
        public float cameraHeightHint = 4.5f;
        public float densityMultiplier = 1.0f;
    }
}
