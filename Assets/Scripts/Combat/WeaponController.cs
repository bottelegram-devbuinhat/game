using System;
using DudeChaos.Core;
using DudeChaos.Data;
using DudeChaos.Player;
using UnityEngine;

namespace DudeChaos.Combat
{
    public struct ShotVfxEvent
    {
        public Vector3 muzzlePosition;
        public Vector3 hitPoint;
        public Vector3 hitNormal;
        public bool didHit;
        public ImpactMaterialData impactMaterial;
    }

    // 10-step shot chain in strict order; publishes events only; zero per-shot allocations (§1.7 & §1.8).
    public sealed class WeaponController : MonoBehaviour
    {
        private static readonly int AnimFireHash = Animator.StringToHash("Fire");
        private static readonly int AnimReloadHash = Animator.StringToHash("Reload");

        [SerializeField] private WeaponData currentWeapon;
        [SerializeField] private CombatSettings settings;
        [SerializeField] private Transform muzzleOrigin;
        [SerializeField] private Animator characterAnimator;
        [SerializeField] private MonoBehaviour cameraPunchBehaviour;
        [SerializeField] private HitStopDriver hitStopDriver;

        private readonly RaycastHit[] _hitBuffer = new RaycastHit[16];
        private AttachmentMount _attachments;
        private HealthSystem _ownerHealth;
        private EquipmentData _equippedAmmo;
        private int _ammoInMag;
        private float _cooldownRemaining;
        private float _reloadRemaining;
        private int _consecutiveShots;
        private float _heat;

        public event Action<ShotVfxEvent> ShotVfxRequested;
        public event Action<Vector2> RecoilPublished;
        public event Action<bool, BodyPart> ShotResolvedUi;
        public event Action<int, int> AmmoChanged;

        public WeaponData CurrentWeapon => currentWeapon;
        public int AmmoInMagazine => _ammoInMag;
        public int MaxMagazineSize => currentWeapon == null ? 0 :
            (_attachments != null ? _attachments.ModifyMagazine(currentWeapon.magazineSize) : currentWeapon.magazineSize);
        public bool IsReloading => _reloadRemaining > 0f;
        public float Heat => _heat;

        public void Awake()
        {
            if (settings == null)
                settings = ScriptableObject.CreateInstance<CombatSettings>();
            _attachments = GetComponent<AttachmentMount>();
            _ownerHealth = GetComponent<HealthSystem>();
            if (currentWeapon != null)
                Equip(currentWeapon);
        }

        public void Equip(WeaponData weapon, EquipmentData ammo = null)
        {
            currentWeapon = weapon;
            _equippedAmmo = ammo;
            _reloadRemaining = 0f;
            _cooldownRemaining = 0f;
            _consecutiveShots = 0;
            _heat = 0f;
            _ammoInMag = MaxMagazineSize;
            AmmoChanged?.Invoke(_ammoInMag, MaxMagazineSize);
        }

        public void SetCameraPunch(ICameraPunch punch)
        {
            cameraPunchBehaviour = punch as MonoBehaviour;
            _explicitPunch = punch;
        }

        private ICameraPunch _explicitPunch;
        private ICameraPunch ResolveCameraPunch() => _explicitPunch ?? (cameraPunchBehaviour as ICameraPunch);

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f) return;
            if (_cooldownRemaining > 0f)
                _cooldownRemaining = Mathf.Max(0f, _cooldownRemaining - deltaTime);

            if (_reloadRemaining > 0f)
            {
                _reloadRemaining -= deltaTime;
                if (_reloadRemaining <= 0f)
                {
                    _reloadRemaining = 0f;
                    _ammoInMag = MaxMagazineSize;
                    AmmoChanged?.Invoke(_ammoInMag, MaxMagazineSize);
                }
            }

            if (_cooldownRemaining <= 0f && _consecutiveShots > 0)
                _consecutiveShots = 0;

            if (_heat > 0f)
                _heat = Mathf.Max(0f, _heat - deltaTime * 0.25f);
        }

        public void Update() => Tick(Time.deltaTime);

        public bool StartReload()
        {
            if (currentWeapon == null || currentWeapon.weaponClass == WeaponClass.Melee || currentWeapon.magazineSize <= 0)
                return false;
            if (IsReloading || _ammoInMag >= MaxMagazineSize)
                return false;

            float mult = _attachments != null ? _attachments.ReloadMultiplier : 1f;
            _reloadRemaining = Mathf.Max(0.1f, currentWeapon.reloadTime * mult);
            characterAnimator?.SetTrigger(AnimReloadHash);
            return true;
        }

        // Exact 10-step shot chain (§1.7):
        // 1 input gate -> 2 fire-rate gate -> 3 ammo -> 4 spread ray -> 5 hit resolve ->
        // 6 damage by body part -> 7 impulse + ragdoll -> 8 VFX/audio pool -> 9 recoil/camera punch -> 10 hit-stop/UI event.
        public bool TryFire(bool triggerHeld, bool triggerPressedThisFrame, Vector3 aimDirection = default)
        {
            // Step 1: Input gate
            if (currentWeapon == null || IsReloading)
                return false;
            if (currentWeapon.weaponClass == WeaponClass.Melee)
                return false;
            bool allowHold = currentWeapon.isAutomatic || WeaponMechanics.ResolveKind(currentWeapon) == WeaponMechanicKind.Beam;
            if (!(allowHold ? triggerHeld : triggerPressedThisFrame))
                return false;

            // Step 2: Fire-rate gate
            if (_cooldownRemaining > 0f)
                return false;

            // Step 3: Ammo gate
            if (_ammoInMag <= 0)
                return false;

            WeaponMechanicKind mechanic = WeaponMechanics.ResolveKind(currentWeapon);
            int shotsInPull = mechanic == WeaponMechanicKind.Beam ? 1 : Mathf.Min(_ammoInMag, Mathf.Max(1, WeaponMechanics.ShotsPerPull(currentWeapon)));
            _ammoInMag -= shotsInPull;
            _cooldownRemaining = WeaponMechanics.EffectiveCycleSeconds(currentWeapon);
            _heat = Mathf.Clamp01(_heat + WeaponMechanics.ComputeHeatDelta(currentWeapon, Time.deltaTime));
            AmmoChanged?.Invoke(_ammoInMag, MaxMagazineSize);
            characterAnimator?.SetTrigger(AnimFireHash);

            Transform originTf = muzzleOrigin != null ? muzzleOrigin : transform;
            Vector3 origin = originTf.position;
            Vector3 baseForward = aimDirection.sqrMagnitude > 1e-4f ? aimDirection.normalized : originTf.forward;

            float spreadMult = _attachments != null ? _attachments.SpreadMultiplier : 1f;
            float ammoDmgMult = (_equippedAmmo != null && _equippedAmmo.kind == EquipmentKind.Ammo && _equippedAmmo.valueA > 0f)
                ? _equippedAmmo.valueA
                : 1f;

            bool anyHitDamageable = false;
            BodyPart lastHitPart = BodyPart.Torso;
            Vector3 finalHitPoint = origin + baseForward * currentWeapon.range;
            Vector3 finalHitNormal = -baseForward;
            bool anyGeometryHit = false;
            ImpactMaterialData impactMat = null;

            int pellets = Mathf.Max(1, currentWeapon.pellets);
            int totalRays = shotsInPull * pellets;

            for (int r = 0; r < totalRays; r++)
            {
                // Step 4: Spread ray
                Vector2 sample = totalRays == 1 && currentWeapon.spread <= 0.01f
                    ? Vector2.zero
                    : new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
                Vector3 rayDir = CombatMath.ApplySpread(baseForward, currentWeapon.spread * spreadMult, sample);

                // Step 5: Hit resolve (non-alloc, no GetComponentsInParent)
                int hitCount = Physics.RaycastNonAlloc(origin, rayDir, _hitBuffer, currentWeapon.range, -1, QueryTriggerInteraction.Ignore);
                int bestIdx = -1;
                float bestDist = float.MaxValue;
                for (int i = 0; i < hitCount; i++)
                {
                    Collider col = _hitBuffer[i].collider;
                    if (col == null || col.gameObject == gameObject) continue;
                    if (_hitBuffer[i].distance < bestDist)
                    {
                        bestDist = _hitBuffer[i].distance;
                        bestIdx = i;
                    }
                }

                if (bestIdx >= 0)
                {
                    RaycastHit hit = _hitBuffer[bestIdx];
                    anyGeometryHit = true;
                    finalHitPoint = hit.point;
                    finalHitNormal = hit.normal;

                    BodyPart part = BodyPart.Torso;
                    IDamageable target = null;
                    if (hit.collider.TryGetComponent(out BodyPartTag tag))
                    {
                        part = tag.Part;
                        target = tag.Damageable;
                        impactMat = tag.ImpactMaterial;
                    }
                    if (target == null)
                    {
                        hit.collider.TryGetComponent(out target);
                    }

                    // Step 6: Damage by body part (head 3.2 / torso 1.0 / limb 0.75)
                    float dmg = CombatMath.ComputeShotDamage(currentWeapon.damage, part, currentWeapon, ammoDmgMult);

                    // Step 7: Impulse + ragdoll
                    Vector3 impulseVec = CombatMath.ComputeShotImpulse(
                        rayDir,
                        settings.baseShotImpulse,
                        settings.shotUpwardBias,
                        currentWeapon.ragdollMultiplier);

                    if (target != null)
                    {
                        anyHitDamageable = true;
                        lastHitPart = part;
                        target.ApplyDamage(new DamageInfo(
                            dmg,
                            part,
                            hit.point,
                            rayDir,
                            impulseVec.magnitude,
                            RagdollActivationReason.Bullet,
                            gameObject));
                    }

                    if (hit.collider.TryGetComponent(out RagdollController targetRagdoll) &&
                        settings != null &&
                        impulseVec.magnitude >= settings.baseShotImpulse)
                    {
                        targetRagdoll.Activate(RagdollActivationReason.Bullet, impulseVec, part);
                    }

                    float selfDmg = WeaponMechanics.SelfDamageAtDistance(currentWeapon, hit.distance, settings);
                    if (selfDmg > 0f && _ownerHealth != null)
                    {
                        _ownerHealth.ApplyDamage(new DamageInfo(selfDmg, BodyPart.Torso, origin, -rayDir, 0f, RagdollActivationReason.Explosion, gameObject));
                    }
                }
            }

            // Step 8: VFX / audio pool event
            ShotVfxRequested?.Invoke(new ShotVfxEvent
            {
                muzzlePosition = origin,
                hitPoint = finalHitPoint,
                hitNormal = finalHitNormal,
                didHit = anyGeometryHit,
                impactMaterial = impactMat
            });

            // Step 9: Recoil / camera punch
            _consecutiveShots++;
            float curveEval = currentWeapon.recoilCurve != null
                ? currentWeapon.recoilCurve.Evaluate(Mathf.Clamp01(_consecutiveShots / 10f))
                : 0.5f;
            float recoilMult = _attachments != null ? _attachments.RecoilMultiplier : 1f;
            float yawSign = (_consecutiveShots % 2 == 0) ? 1f : -1f;
            Vector2 recoilStep = new Vector2(yawSign * 0.35f, 1.0f) * (curveEval * recoilMult);
            RecoilPublished?.Invoke(recoilStep);
            ResolveCameraPunch()?.Punch(Mathf.Clamp(currentWeapon.damage / 45f, 0.25f, 3f));

            // Step 10: Hit-stop / UI event
            if (anyHitDamageable && lastHitPart == BodyPart.Head && hitStopDriver != null)
            {
                hitStopDriver.Trigger(settings.lightHitStop * 0.5f);
            }
            ShotResolvedUi?.Invoke(anyHitDamageable, lastHitPart);

            return true;
        }
    }
}
