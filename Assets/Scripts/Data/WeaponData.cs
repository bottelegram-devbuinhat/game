using UnityEngine;

namespace DudeChaos.Data
{
    [CreateAssetMenu(fileName = "WPN_New", menuName = "DudeChaos/Data/Weapon")]
    public class WeaponData : ScriptableObject
    {
        public string weaponId = "";
        public string displayName = "";
        public WeaponClass weaponClass = WeaponClass.Pistol;
        public float damage = 20f;
        public int pellets = 1;
        public float headMultiplier = 3.2f;
        public float limbMultiplier = 0.75f;
        public float fireRate = 0.2f;
        public float spread = 1.2f;
        public AnimationCurve recoilCurve = AnimationCurve.Linear(0f, 0.2f, 1f, 1f);
        public float range = 60f;
        public int magazineSize = 12;
        public float reloadTime = 1.6f;
        public float ragdollMultiplier = 1.0f;
        public bool isAutomatic = false;
        public string mechanicName = "";
        public int burstCount = 1;
        public float chargeSeconds = 0f;
        public float projectileSpeed = 0f;
        public float selfDamage = 0f;
        public float heatPerShot = 0f;
        public float muzzleEnergyJoules = 482f;
        public AudioClip fireSound;
        public AudioClip reloadSound;
        public GameObject muzzleFlashPrefab;
    }
}
