using UnityEngine;

namespace DudeChaos.Data
{
    [CreateAssetMenu(fileName = "VehicleSettings", menuName = "DudeChaos/Settings/Vehicle")]
    public class VehicleSettings : ScriptableObject
    {
        public float enterDuration = 0.45f;
        public float exitDuration = 0.45f;
        public float ejectSpeedThreshold = 9.0f;
        public float ejectForwardImpulse = 900f;
        public float ejectUpImpulse = 420f;
        public float exitOffset = 1.4f;
        public float smokeHealthRatio = 0.50f;
        public float criticalHealthRatio = 0.25f;
        public float destructionDelay = 2.8f;
        public float steeringFalloff = 0.35f;
        public float downforce = 12.0f;
    }
}
