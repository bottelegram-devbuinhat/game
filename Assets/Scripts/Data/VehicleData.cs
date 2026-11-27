using UnityEngine;

namespace DudeChaos.Data
{
    [CreateAssetMenu(fileName = "VEH_New", menuName = "DudeChaos/Data/Vehicle")]
    public class VehicleData : ScriptableObject
    {
        public string vehicleId = "";
        public string vehicleName = "";
        public HandlingProfile handlingProfile = HandlingProfile.Sports;
        public ArmourClass armourClass = ArmourClass.Unarmoured;
        public float maxHealth = 900f;
        public int seats = 4;
        public Vector3 centerOfMassOffset = new Vector3(0f, -0.35f, 0f);
        public float maxSpeed = 33f;
        public float acceleration = 14f;
        public float brakeForce = 26f;
        public float steerAngle = 38f;
        public AnimationClip enterClip;
        public AnimationClip exitClip;
        public AudioClip engineSound;
        public AudioClip hornSound;
    }
}
