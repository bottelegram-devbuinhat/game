using System;
using System.Collections.Generic;
using DudeChaos.Audio;
using DudeChaos.CameraRig;
using DudeChaos.City;
using DudeChaos.Combat;
using DudeChaos.Core;
using DudeChaos.Data;
using DudeChaos.GameFlow;
using DudeChaos.Localization;
using DudeChaos.Net;
using DudeChaos.Npc;
using DudeChaos.Player;
using DudeChaos.Player.Input;
using DudeChaos.Progression;
using DudeChaos.Save;
using DudeChaos.Settings;
using DudeChaos.UI;
using DudeChaos.Vehicles;
using DudeChaos.Visuals;
using DudeChaos.Wanted;
using DudeChaos.World;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

namespace DudeChaos.LogicTests
{
    internal static class Program
    {
        private static int _passed;
        private static int _failed;
        private static readonly List<string> Failures = new List<string>();

        private static void Check(bool condition, string label)
        {
            if (condition)
            {
                _passed++;
            }
            else
            {
                _failed++;
                string msg = $"FAIL: {label}";
                Failures.Add(msg);
                Console.WriteLine(msg);
            }
        }

        private static void CheckNear(float actual, float expected, float tol, string label)
        {
            Check(Mathf.Abs(actual - expected) <= tol, $"{label} (expected {expected}, got {actual})");
        }

        public static int Main()
        {
            TestMovement();
            TestStateMachineAllPairs();
            TestRagdollAndBudget();
            TestCombatAndMechanics();
            TestVehiclesAndArmour();
            TestWantedAndPolice();
            TestMissionsAndGrading();
            TestDatesAndDailies400Days();
            TestSaveAndMigration();
            TestProgressionEconomyAchievementsStuntsBounty();
            TestWorldCityPropsDayNightNpc();
            TestAudioVisualsLocalization();
            TestQualityFogAndSettings();
            TestNetAndLoopback();
            TestSceneSystemsContract();
            ExtendedSystemTests.Run(Check, CheckNear);

            Console.WriteLine($"PASSED {_passed}  FAILED {_failed}");
            if (_failed > 0)
            {
                foreach (string f in Failures)
                    Console.Error.WriteLine(f);
                return 1;
            }
            return 0;
        }

        // ==================== 1. Movement ====================
        private static void TestMovement()
        {
            var s = ScriptableObject.CreateInstance<MovementSettings>();
            CheckNear(s.deadZone, 0.18f, 1e-4f, "Movement.deadZone");
            CheckNear(s.walkSpeed, 3.2f, 1e-4f, "Movement.walkSpeed");
            CheckNear(s.runSpeed, 6.0f, 1e-4f, "Movement.runSpeed");
            CheckNear(s.sprintSpeed, 8.8f, 1e-4f, "Movement.sprintSpeed");
            CheckNear(s.sprintFovKick, 5.0f, 1e-4f, "Movement.sprintFovKick");
            CheckNear(s.coyoteTime, 0.12f, 1e-4f, "Movement.coyoteTime");
            CheckNear(s.jumpBufferTime, 0.10f, 1e-4f, "Movement.jumpBufferTime");
            CheckNear(s.gravity, -22.0f, 1e-4f, "Movement.gravity");

            Check(LocomotionMath.ApplyDeadZone(new Vector2(0.10f, 0.05f), s.deadZone) == Vector2.zero, "DeadZone zeroes small input");
            Check(LocomotionMath.ApplyDeadZone(new Vector2(0.18f, 0f), s.deadZone) == Vector2.zero, "DeadZone exact boundary is zero");
            Check(LocomotionMath.ApplyDeadZone(new Vector2(1f, 0f), s.deadZone).x > 0.99f, "Full stick reaches 1");

            CheckNear(LocomotionMath.ComputeTargetSpeed(new Vector2(0f, 0.55f), false, false, s), 3.2f, 0.02f, "Walk speed at 0.55");
            CheckNear(LocomotionMath.ComputeTargetSpeed(new Vector2(0f, 1.0f), false, false, s), 6.0f, 0.02f, "Run speed at 1.0");
            CheckNear(LocomotionMath.ComputeTargetSpeed(new Vector2(0f, 1.0f), true, false, s), 8.8f, 0.02f, "Sprint speed at 1.0 + shift");
            CheckNear(LocomotionMath.ComputeTargetSpeed(new Vector2(0f, 1.0f), false, true, s), s.crouchSpeed, 0.02f, "Crouch speed");

            Check(LocomotionMath.CanConsumeJump(0.11f, s.coyoteTime, 0.09f, s.jumpBufferTime), "Coyote + jump buffer inside window");
            Check(!LocomotionMath.CanConsumeJump(0.15f, s.coyoteTime, 0.05f, s.jumpBufferTime), "Coyote expired");
            Check(!LocomotionMath.CanConsumeJump(0.05f, s.coyoteTime, 0.15f, s.jumpBufferTime), "Jump buffer expired");

            float jv = LocomotionMath.ComputeJumpVelocity(s.jumpHeight, s.gravity);
            CheckNear((jv * jv) / (2f * Mathf.Abs(s.gravity)), 1.35f, 0.02f, "Jump apex height ~1.35m");

            // Bound check §1.7: landing at exact -8 is Medium, NOT Soft!
            Check(s.ClassifyLanding(-2.0f) == LandingTier.Soft, "Landing -2 is Soft");
            Check(s.ClassifyLanding(-7.99f) == LandingTier.Soft, "Landing -7.99 is Soft");
            Check(s.ClassifyLanding(-8.0f) == LandingTier.Medium, "Landing exact -8.0 is Medium");
            Check(s.ClassifyLanding(-14.0f) == LandingTier.Medium, "Landing exact -14.0 is Medium");
            Check(s.ClassifyLanding(-14.01f) == LandingTier.Hard, "Landing -14.01 is Hard");
            Check(LocomotionMath.ComputeFallDamage(-8.0f, s) == 0f, "Medium landing deals 0 fall damage");
            Check(LocomotionMath.ComputeFallDamage(-18.0f, s) > 0f, "Hard landing deals fall damage");

            // Landmine #11: PlayerMotor.Awake self-resolves GetComponent<IPlayerInput>()
            var go = new GameObject("MotorPlayer");
            var inp = go.AddComponent<InputSystemPlayerInput>();
            inp.SetVirtualMove(new Vector2(0f, 1f));
            var motor = go.AddComponent<PlayerMotor>();
            motor.Awake();
            Check(ReferenceEquals(motor.Input, inp), "PlayerMotor.Awake self-resolves IPlayerInput");
            motor.Tick(0.1f);
            Check(motor.transform.position.z > 0.1f, "PlayerMotor moves forward with resolved input");

            // HealthSystem burns armour before health (§1.8 & §4.6)
            var hgo = new GameObject("HealthTest");
            var hs = hgo.AddComponent<HealthSystem>();
            bool depleted = false;
            hs.Depleted += () => depleted = true;
            hs.Configure(100f, 100f, 100f, 40f);
            hs.ApplyDamage(new DamageInfo(25f));
            CheckNear(hs.CurrentArmour, 15f, 0.01f, "Armour absorbs first 25");
            CheckNear(hs.CurrentHealth, 100f, 0.01f, "Health untouched while armour > 0");
            hs.ApplyDamage(new DamageInfo(35f));
            CheckNear(hs.CurrentArmour, 0f, 0.01f, "Armour depleted");
            CheckNear(hs.CurrentHealth, 80f, 0.01f, "Overflow spills to health");
            hs.ApplyDamage(new DamageInfo(100f));
            Check(!hs.IsAlive && depleted, "Depleted event fires at 0 HP");
        }

        // ==================== 2. State Machine (14x14 = 196 pairs + ragdoll rules) ====================
        private static void TestStateMachineAllPairs()
        {
            PlayerState[] allStates = (PlayerState[])Enum.GetValues(typeof(PlayerState));
            Check(allStates.Length == 14, "PlayerState has exactly 14 states");

            foreach (PlayerState from in allStates)
            {
                foreach (PlayerState to in allStates)
                {
                    var sm = new PlayerStateMachine();
                    ForceStateForTest(sm, from);
                    bool expected = PlayerStateMachine.IsTransitionLegal(from, to);
                    bool actual = sm.TryTransition(to);
                    Check(actual == expected, $"Transition {from} -> {to} matches table ({expected})");
                }
            }

            // Landmine #13 explicit checks
            Check(!PlayerStateMachine.CanRagdollFrom(PlayerState.InVehicle), "InVehicle CANNOT ragdoll directly");
            Check(PlayerStateMachine.CanRagdollFrom(PlayerState.ExitingVehicle), "ExitingVehicle CAN ragdoll");
            Check(!PlayerStateMachine.CanRagdollFrom(PlayerState.Dead), "Dead CANNOT ragdoll");
            Check(!PlayerStateMachine.CanRagdollFrom(PlayerState.Ragdoll), "Ragdoll does not re-enter Ragdoll");

            foreach (RagdollActivationReason reason in Enum.GetValues(typeof(RagdollActivationReason)))
            {
                var sm = new PlayerStateMachine();
                sm.TryTransition(PlayerState.EnteringVehicle);
                sm.TryTransition(PlayerState.InVehicle);
                Check(!sm.TryEnterRagdoll(reason), $"InVehicle rejects TryEnterRagdoll({reason})");
                Check(sm.TryTransition(PlayerState.ExitingVehicle), $"InVehicle -> ExitingVehicle for {reason}");
                Check(sm.TryEnterRagdoll(reason), $"ExitingVehicle accepts TryEnterRagdoll({reason})");
            }

            var lockSm = new PlayerStateMachine { VehicleLockout = true };
            Check(!lockSm.TryTransition(PlayerState.EnteringVehicle), "VehicleLockout blocks EnteringVehicle");
        }

        private static void ForceStateForTest(PlayerStateMachine sm, PlayerState target)
        {
            var field = typeof(PlayerStateMachine).GetField("_current", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field.SetValue(sm, target);
        }

        // ==================== 3. Ragdoll & Center-of-Mass Guards ====================
        private static void TestRagdollAndBudget()
        {
            var rs = ScriptableObject.CreateInstance<RagdollSettings>();
            CheckNear(rs.totalMass, 78f, 1e-4f, "Ragdoll totalMass 78");
            Check(rs.maxFullSimRagdolls == 12, "Ragdoll maxFullSimRagdolls 12");
            CheckNear(rs.fullSimRadius, 55f, 1e-4f, "Ragdoll fullSimRadius 55");
            CheckNear(rs.recoveryVelocity, 1.1f, 1e-4f, "Ragdoll recoveryVelocity 1.1");
            CheckNear(rs.recoveryHoldTime, 1.8f, 1e-4f, "Ragdoll recoveryHoldTime 1.8");
            CheckNear(rs.getUpDuration, 0.9f, 1e-4f, "Ragdoll getUpDuration 0.9");
            CheckNear(rs.pelvisFollowLag, 0.08f, 1e-4f, "Ragdoll pelvisFollowLag 0.08");

            // Budget selection: 18 active inside 55m + 4 beyond 55m -> picks closest 12
            var candidates = new List<RagdollBudgetModel.Candidate>();
            for (int i = 0; i < 18; i++)
            {
                candidates.Add(new RagdollBudgetModel.Candidate
                {
                    id = i,
                    position = new Vector3(i * 3f, 0f, 0f), // 0..51 m
                    active = true
                });
            }
            candidates.Add(new RagdollBudgetModel.Candidate { id = 100, position = new Vector3(60f, 0f, 0f), active = true });
            candidates.Add(new RagdollBudgetModel.Candidate { id = 101, position = new Vector3(1f, 0f, 0f), active = false });

            var selected = new HashSet<int>();
            RagdollBudgetModel.SelectFullSimulationIds(candidates, Vector3.zero, 12, 55f, selected);
            Check(selected.Count == 12, "RagdollBudgetModel caps at 12");
            for (int i = 0; i < 12; i++)
                Check(selected.Contains(i), $"Closest candidate {i} selected");
            Check(!selected.Contains(15), "Overflow candidate 15 excluded");
            Check(!selected.Contains(100), "Beyond 55m candidate excluded");
            Check(!selected.Contains(101), "Inactive candidate excluded");

            // Landmine #18: Boneless ragdoll registered in RagdollBudget MUST NOT throw
            RagdollBudget.ClearAll();
            var bonelessGo = new GameObject("BonelessRagdoll");
            var bonelessRc = bonelessGo.AddComponent<RagdollController>();
            bonelessRc.Awake();
            bonelessRc.OnEnable();
            bonelessRc.Activate(RagdollActivationReason.Scripted);
            Check(bonelessRc.ReadBonePositions().Length == 0, "Boneless prefab returns empty bone array");
            RagdollBudget.Tick(Vector3.zero);
            Check(bonelessRc.IsFullSimulation, "Boneless ragdoll ticked safely via Position");
            RagdollBudget.ClearAll();

            // Recovery & get-up flow + death blocks recovery
            var rgo = new GameObject("RagdollActor");
            var boneGo = new GameObject("PelvisBone");
            var boneRb = boneGo.AddComponent<Rigidbody>();
            var boneCol = boneGo.AddComponent<SphereCollider>();
            var sm = new PlayerStateMachine();
            var rc = rgo.AddComponent<RagdollController>();
            rc.Awake();
            rc.Bind(sm, rs);
            rc.ConfigureBones(boneGo.transform, new[] { boneRb }, new[] { boneCol });
            CheckNear(boneRb.mass, 78f, 0.1f, "Single bone receives 78kg mass");
            Check(rc.Activate(RagdollActivationReason.Bullet, new Vector3(0f, 0f, 80f)), "Ragdoll activates from Idle");
            Check(sm.Current == PlayerState.Ragdoll, "State is Ragdoll");
            boneRb.linearVelocity = new Vector3(0f, 0f, 0.2f);
            rc.Tick(1.0f);
            Check(sm.Current == PlayerState.Ragdoll, "Still Ragdoll at 1.0s < 1.8s");
            rc.Tick(0.9f);
            Check(sm.Current == PlayerState.GettingUp, "Transitions to GettingUp after 1.8s low velocity");
            rc.Tick(0.95f);
            Check(sm.Current == PlayerState.Idle && !rc.IsActiveRagdoll, "Recovers to Idle after 0.9s get-up");

            rc.HandleDeath();
            Check(rc.IsDead && sm.Current == PlayerState.Dead, "Death enters Dead state");
            rc.Tick(5.0f);
            Check(sm.Current == PlayerState.Dead, "Dead ragdoll never recovers");

            // PelvisFollowTarget smooth damp
            var followGo = new GameObject("FollowTarget");
            var follow = followGo.AddComponent<PelvisFollowTarget>();
            boneGo.transform.position = new Vector3(10f, 2f, 0f);
            follow.Configure(rc, rgo.transform, rs);
            CheckNear(follow.LagSeconds, 0.08f, 1e-4f, "PelvisFollowLag is 0.08s");
            follow.Step(0.04f);
            Check(followGo.transform.position.x > 0.1f && followGo.transform.position.x < 10f, "PelvisFollow smooth-damps toward pelvis");

            // Landmine #1: Center of mass validation
            Check(!VehicleMath.IsUsableCenterOfMass(new Vector3(float.NaN, 0f, 0f)), "NaN X rejected");
            Check(!VehicleMath.IsUsableCenterOfMass(new Vector3(0f, float.NaN, 0f)), "NaN Y rejected");
            Check(!VehicleMath.IsUsableCenterOfMass(new Vector3(0f, 0f, float.PositiveInfinity)), "Inf Z rejected");
            Check(!VehicleMath.IsUsableCenterOfMass(new Vector3(0f, -5.01f, 0f)), "Below -5 rejected");
            Check(!VehicleMath.IsUsableCenterOfMass(new Vector3(6f, 0f, 0f)), "Above +5 rejected");
            Check(VehicleMath.IsUsableCenterOfMass(new Vector3(0f, -0.45f, 0f)), "Valid -0.45 accepted");
            Check(VehicleMath.SafeCenterOfMass(new Vector3(float.NaN, 0f, 0f)) == new Vector3(0f, -0.35f, 0f), "SafeCenterOfMass fallback (0,-0.35,0)");
        }

        // ==================== 4. Combat & All 9 Mechanics ====================
        private static void TestCombatAndMechanics()
        {
            var wpn = ScriptableObject.CreateInstance<WeaponData>();
            wpn.damage = 20f;
            CheckNear(CombatMath.BodyPartMultiplier(BodyPart.Head, wpn), 3.2f, 1e-4f, "Head multiplier 3.2");
            CheckNear(CombatMath.BodyPartMultiplier(BodyPart.Torso, wpn), 1.0f, 1e-4f, "Torso multiplier 1.0");
            CheckNear(CombatMath.BodyPartMultiplier(BodyPart.Limb, wpn), 0.75f, 1e-4f, "Limb multiplier 0.75");
            CheckNear(CombatMath.ComputeShotDamage(20f, BodyPart.Head, wpn), 64f, 1e-4f, "Head shot damage 64");
            CheckNear(CombatMath.ComputeShotDamage(20f, BodyPart.Limb, wpn), 15f, 1e-4f, "Limb shot damage 15");

            Vector3 imp = CombatMath.ComputeShotImpulse(Vector3.forward, 45f, 0.25f, 1f);
            Check(imp.z > 0f && imp.y > 0f, "Shot impulse has forward + upward bias");
            CheckNear(imp.magnitude, 45f, 0.05f, "Shot impulse magnitude 45");

            CheckNear(CombatMath.ComputeExplosionFalloff(0f, 6f), 1.0f, 1e-4f, "Blast falloff at 0m = 1");
            CheckNear(CombatMath.ComputeExplosionFalloff(3f, 6f), 0.5f, 1e-4f, "Blast falloff at 3m = 0.5");
            CheckNear(CombatMath.ComputeExplosionFalloff(6f, 6f), 0.0f, 1e-4f, "Blast falloff at 6m = 0");
            CheckNear(CombatMath.ComputeExplosionFalloff(9f, 6f), 0.0f, 1e-4f, "Blast falloff beyond radius = 0");

            var cs = ScriptableObject.CreateInstance<CombatSettings>();
            Check(CombatMath.IsBackstab(Vector3.forward, Vector3.forward, cs.backstabDotThreshold), "Same forward behind target is backstab");
            Check(!CombatMath.IsBackstab(Vector3.forward, Vector3.back, cs.backstabDotThreshold), "Face-to-face is not backstab");
            CheckNear(CombatMath.ComputeMeleeDamage(100f, true, false, cs), 160f, 0.01f, "Ragdolled melee x1.6");
            CheckNear(CombatMath.ComputeMeleeDamage(100f, false, true, cs), 180f, 0.01f, "Backstab melee x1.8");
            CheckNear(CombatMath.ComputeMeleeDamage(100f, true, true, cs), 288f, 0.01f, "Ragdolled + backstab stacks");

            // All 9 mechanics (§1.8 & §4.2)
            string[] mechNames = { "Auto", "Semi", "Burst", "Pump", "Bolt", "Charge", "Beam", "Projectile", "Throwable" };
            foreach (string mName in mechNames)
            {
                var mw = ScriptableObject.CreateInstance<WeaponData>();
                mw.mechanicName = mName;
                mw.fireRate = 0.25f;
                mw.burstCount = 3;
                mw.chargeSeconds = 0.8f;
                mw.heatPerShot = 0.1f;
                mw.damage = 80f;
                WeaponMechanicKind kind = WeaponMechanics.ResolveKind(mw);
                Check(kind.ToString() == mName, $"Mechanic {mName} resolves");

                int shots = WeaponMechanics.ShotsPerPull(mw);
                if (kind == WeaponMechanicKind.Beam)
                    Check(shots == 0, "Beam has 0 discrete shots-per-pull");
                else if (kind == WeaponMechanicKind.Burst)
                    Check(shots == 3, "Burst fires 3 shots");
                else
                    Check(shots == 1, $"{mName} fires 1 shot per pull");

                float cycle = WeaponMechanics.EffectiveCycleSeconds(mw);
                if (kind == WeaponMechanicKind.Pump) CheckNear(cycle, 0.37f, 0.001f, "Pump adds cycle");
                if (kind == WeaponMechanicKind.Bolt) CheckNear(cycle, 0.45f, 0.001f, "Bolt adds cycle");
                if (kind == WeaponMechanicKind.Charge) CheckNear(cycle, 1.05f, 0.001f, "Charge adds chargeSeconds");
                if (kind == WeaponMechanicKind.Throwable) Check(cycle >= 0.8f, "Throwable floors at >= 0.8s");

                float heat = WeaponMechanics.ComputeHeatDelta(mw, 0.5f);
                Check(heat > 0f, $"{mName} heat > 0");

                float selfPb = WeaponMechanics.SelfDamageAtDistance(mw, 1.0f, cs);
                if (kind == WeaponMechanicKind.Projectile || kind == WeaponMechanicKind.Throwable)
                    Check(selfPb > 0f, $"{mName} deals self-damage at point blank");
                else
                    Check(selfPb == 0f, $"{mName} deals no self-damage");
            }

            // Attachments
            var optic = ScriptableObject.CreateInstance<EquipmentData>();
            optic.kind = EquipmentKind.Attachment;
            optic.slot = AttachmentSlot.Optic;
            optic.valueA = 4f;
            CheckNear(AttachmentRules.GetOpticZoom(optic), 4f, 1e-4f, "Optic 4x zoom");
            Check(!AttachmentRules.CanAttach(optic, WeaponClass.Melee), "Melee rejects attachments");

            // Full 10-step WeaponController chain against registered BodyPartTag + DamageDummy
            Physics.RegisteredColliders.Clear();
            var dummyGo = new GameObject("TargetDummy");
            dummyGo.transform.position = new Vector3(0f, 0f, 10f);
            var dummyCol = dummyGo.AddComponent<SphereCollider>();
            dummyCol.radius = 1.0f;
            var dummy = dummyGo.AddComponent<DamageDummy>();
            dummy.Awake();
            var headTag = dummyGo.AddComponent<BodyPartTag>();
            headTag.Configure(BodyPart.Head, dummy);
            Physics.RegisteredColliders.Add(dummyCol);

            var shooterGo = new GameObject("Shooter");
            var rig = shooterGo.AddComponent<ThirdPersonCameraRig>();
            var vcamGo = new GameObject("VCam");
            var vcam = vcamGo.AddComponent<CinemachineCamera>();
            rig.Configure(shooterGo.transform, shooterGo.transform, vcam);
            var hitStop = shooterGo.AddComponent<HitStopDriver>();
            var wc = shooterGo.AddComponent<WeaponController>();
            wc.Awake();
            wc.SetCameraPunch(rig);

            var pistol = ScriptableObject.CreateInstance<WeaponData>();
            pistol.weaponId = "wpn_9mm";
            pistol.weaponClass = WeaponClass.Pistol;
            pistol.damage = 25f;
            pistol.spread = 0f;
            pistol.fireRate = 0.25f;
            pistol.magazineSize = 2;
            pistol.range = 60f;
            wc.Equip(pistol);

            bool vfxFired = false;
            bool recoilFired = false;
            bool uiFired = false;
            wc.ShotVfxRequested += _ => vfxFired = true;
            wc.RecoilPublished += _ => recoilFired = true;
            wc.ShotResolvedUi += (hit, part) => uiFired = hit && part == BodyPart.Head;

            Check(wc.TryFire(true, true, Vector3.forward), "First shot fires");
            Check(!wc.TryFire(true, true, Vector3.forward), "Fire-rate gate blocks immediate second shot");
            wc.Tick(0.30f);
            Check(wc.TryFire(true, true, Vector3.forward), "Second shot fires after cooldown");
            wc.Tick(0.30f);
            Check(!wc.TryFire(true, true, Vector3.forward), "Empty magazine blocks third shot");
            Check(wc.StartReload(), "Reload starts");
            wc.Tick(2.0f);
            Check(wc.AmmoInMagazine == 2, "Reload refills magazine");
            Check(vfxFired && recoilFired && uiFired, "All shot chain events published");
            CheckNear(dummy.CurrentHealth, 200f - (25f * 3.2f * 2f), 0.1f, "Two headshots dealt 160 damage");
            Physics.RegisteredColliders.Clear();

            // HitStop yields to pause
            var gmGo = new GameObject("GM");
            GameManager.ResetStaticForTests();
            var gm = gmGo.AddComponent<GameManager>();
            gm.Awake();
            hitStop.Trigger(0.10f);
            Check(hitStop.IsActive, "HitStop active");
            gm.SetPaused(true);
            hitStop.Tick(0.01f);
            Check(!hitStop.IsActive && Time.timeScale == 0f, "HitStop yields to pause");
            gm.SetPaused(false);
            GameManager.ResetStaticForTests();
        }

        // ==================== 5. Vehicles & Armour ====================
        private static void TestVehiclesAndArmour()
        {
            var vs = ScriptableObject.CreateInstance<VehicleSettings>();
            Check(!VehicleMath.ShouldEjectOnExit(8.99f, vs), "8.99 m/s exits cleanly");
            Check(VehicleMath.ShouldEjectOnExit(9.0f, vs), "Exact 9.0 m/s ejects");
            Vector3 ej = VehicleMath.ComputeEjectImpulse(Vector3.forward, vs);
            CheckNear(ej.z, 900f, 0.1f, "Eject forward 900");
            CheckNear(ej.y, 420f, 0.1f, "Eject up 420");
            CheckNear(VehicleMath.ComputeSteeringFactor(30f, 30f, vs), 0.65f, 0.01f, "Steering falloff at top speed");

            Check(VehicleMath.EvaluateDamageStage(800f, 1000f, vs) == VehicleDamageStage.Healthy, "80% HP Healthy");
            Check(VehicleMath.EvaluateDamageStage(500f, 1000f, vs) == VehicleDamageStage.Smoking, "50% HP Smoking");
            Check(VehicleMath.EvaluateDamageStage(250f, 1000f, vs) == VehicleDamageStage.Burning, "25% HP Burning");
            Check(VehicleMath.EvaluateDamageStage(0f, 1000f, vs) == VehicleDamageStage.Exploded, "0 HP Exploded");

            Check(ArmourRules.BulletDamageMultiplier(ArmourClass.Heavy) < ArmourRules.BulletDamageMultiplier(ArmourClass.Light), "Heavy resists bullets better than Light");
            Check(ArmourRules.CanHotwire(ArmourClass.Unarmoured) && !ArmourRules.CanHotwire(ArmourClass.Heavy), "Heavy cannot be hotwired");

            for (int c = 0; c < 6; c++)
            {
                var cat = (TuningCategory)c;
                Check(TuningRules.GetUpgradeCost(cat, 0) > 0, $"Tuning {cat} lv0 cost > 0");
                Check(TuningRules.GetUpgradeCost(cat, 5) == 0, $"Tuning {cat} max lv5 cost == 0");
            }

            // Landmines #13, #19, #20: Enter -> still entering (TryExit refused -> _current NOT cleared!) -> InVehicle -> high-speed eject -> Ragdoll
            var carGo = new GameObject("TestCar");
            var rb = carGo.AddComponent<Rigidbody>();
            var vData = ScriptableObject.CreateInstance<VehicleData>();
            vData.centerOfMassOffset = new Vector3(float.NaN, 0f, 0f); // Bad CoM guarded at Awake!
            var vc = carGo.AddComponent<VehicleController>();
            vc.Configure(vData, vs);
            Check(rb.centerOfMass == new Vector3(0f, -0.35f, 0f), "Bad vehicle CoM replaced with safe fallback");

            var pGo = new GameObject("Driver");
            var pInp = pGo.AddComponent<InputSystemPlayerInput>();
            var pRc = pGo.AddComponent<RagdollController>();
            pRc.Awake();
            var pMotor = pGo.AddComponent<PlayerMotor>();
            pMotor.Awake();
            var vInt = pGo.AddComponent<VehicleInteraction>();
            vInt.Bind(pMotor, pInp);

            Check(vInt.TryInteract(vc), "Driver enters vehicle");
            Check(vInt.CurrentVehicle == vc, "CurrentVehicle assigned");
            // While still _entering, TryExit returns false -> Landmine #19 must NOT null CurrentVehicle!
            Check(!vInt.TryInteract(), "TryExit refused while still entering");
            Check(vInt.CurrentVehicle == vc, "Landmine #19: CurrentVehicle preserved when TryExit refuses");

            vc.Step(0.50f); // Complete 0.45s enter window
            Check(pMotor.StateMachine.Current == PlayerState.InVehicle, "Driver now InVehicle");
            pInp.SetVirtualMove(new Vector2(0f, 1f));
            vc.Step(0.20f); // Landmine #20: driverInput Behaviour is null, fallback resolves pMotor.Input!
            Check(vc.SpeedMetersPerSecond > 0.1f, "Landmine #20: Fallback input accelerates car");

            rb.linearVelocity = new Vector3(0f, 0f, 15f); // >= 9 m/s eject!
            Check(vInt.TryInteract(), "High-speed exit succeeds");
            Check(vInt.CurrentVehicle == null, "CurrentVehicle cleared after successful exit");
            Check(pMotor.StateMachine.Current == PlayerState.Ragdoll, "Landmine #13: High-speed eject enters Ragdoll without softlock");
        }

        // ==================== 6. Wanted & Police ====================
        private static void TestWantedAndPolice()
        {
            var ws = ScriptableObject.CreateInstance<WantedSettings>();
            Check(WantedMath.ComputeLevelFromHeat(0f, ws.thresholds) == 0, "Heat 0 -> 0 stars");
            Check(WantedMath.ComputeLevelFromHeat(1f, ws.thresholds) == 1, "Heat 1 -> 1 star");
            Check(WantedMath.ComputeLevelFromHeat(4f, ws.thresholds) == 2, "Heat 4 -> 2 stars");
            Check(WantedMath.ComputeLevelFromHeat(9f, ws.thresholds) == 3, "Heat 9 -> 3 stars");
            Check(WantedMath.ComputeLevelFromHeat(16f, ws.thresholds) == 4, "Heat 16 -> 4 stars");
            Check(WantedMath.ComputeLevelFromHeat(25f, ws.thresholds) == 5, "Heat 25 -> 5 stars");
            Check(WantedMath.ComputeLevelFromHeat(36f, ws.thresholds) == 6, "Heat 36 -> 6 stars");
            Check(WantedMath.ComputeLevelFromHeat(2f, new[] { 0, 0, 1, 5 }) == 1, "Zero thresholds skipped");

            Check(!WantedMath.CanCoolDown(2, true, 80f, 15f, ws), "Cannot cool while in LOS");
            Check(!WantedMath.CanCoolDown(2, false, 40f, 15f, ws), "Cannot cool inside 48m");
            Check(!WantedMath.CanCoolDown(2, false, 60f, 8.5f, ws), "Cannot cool before 9s memory");
            Check(WantedMath.CanCoolDown(2, false, 60f, 9.0f, ws), "Cools when out of LOS, >48m, >=9s");

            var r1 = WantedMath.GetResponse(1, ws);
            var r2 = WantedMath.GetResponse(2, ws);
            var r3 = WantedMath.GetResponse(3, ws);
            var r4 = WantedMath.GetResponse(4, ws);
            Check(!r1.openFire && r2.openFire, "Police open fire from level 2");
            Check(!r2.spawnHelicopter && r3.spawnHelicopter && r3.spawnRoadblock, "Heli & roadblock from level 3");
            Check(!r3.spawnSwat && r4.spawnSwat, "SWAT from level 4");

            var wGo = new GameObject("Wanted");
            var wanted = wGo.AddComponent<WantedSystem>();
            wanted.Awake();
            wanted.ReportCrime(CrimeType.KillPolice); // +3
            wanted.ReportCrime(CrimeType.AssaultPolice); // +2 -> 5 -> level 2
            Check(wanted.Level == 2, "Crimes raise wanted to 2");
            wanted.Tick(10f, false, 65f);
            wanted.Tick(30f, false, 65f);
            Check(wanted.Level == 0, "Wanted cools back to 0");

            var spGo = new GameObject("PoliceSpawner");
            var spawner = spGo.AddComponent<PoliceSpawner>();
            spawner.Configure(wanted, wGo.transform);
            spawner.ReconcileCounts(3);
            Check(spawner.ActiveUnitCount == 4, "Level 3 spawns 4 units");
            spawner.ReconcileCounts(0);
            Check(spawner.ActiveUnitCount == 0 && spawner.ActiveCarCount == 0, "Level 0 despawns all police");
        }

        // ==================== 7. Missions & Grading ====================
        private static void TestMissionsAndGrading()
        {
            // Landmine #14: Duplicate objective keys MUST still advance and complete
            var dupMission = ScriptableObject.CreateInstance<MissionData>();
            dupMission.missionId = "mis_dup";
            dupMission.rewardsMoney = 1000;
            dupMission.timeLimitSeconds = 100f;
            dupMission.timeBonusPerSecond = 20f; // huge rate to test 34% cap!
            dupMission.failAtWantedLevel = 3;
            dupMission.completionConditions = new[] { "wave_cleared", "wave_cleared", "wave_cleared" };

            var run = new MissionRun();
            run.Start(dupMission);
            Check(run.ReportCondition("wave_cleared") && run.State == MissionRunState.Running, "Dup key step 1");
            Check(run.ReportCondition("wave_cleared") && run.State == MissionRunState.Running, "Dup key step 2");
            Check(run.ReportCondition("wave_cleared") && run.State == MissionRunState.Completed, "Dup key step 3 completes!");
            Check(run.ComputeTimeBonusMoney() == 340, "Time bonus capped at 34% of base reward (340)");

            // Fail reasons & 3-retry cap
            run.Start(dupMission);
            run.Tick(10f, 3, false);
            Check(run.State == MissionRunState.Failed && run.FailReason == MissionFailReason.WantedTooHigh, "Wanted fail");
            Check(run.TryRetry() && run.Retries == 1, "Retry 1");
            run.Tick(10f, 0, true);
            Check(run.FailReason == MissionFailReason.PlayerDied, "Death fail");
            Check(run.TryRetry() && run.Retries == 2, "Retry 2");
            run.Tick(105f, 0, false);
            Check(run.FailReason == MissionFailReason.Timeout, "Timeout fail");
            Check(run.TryRetry() && run.Retries == 3, "Retry 3");
            run.Abandon();
            Check(run.FailReason == MissionFailReason.Abandoned, "Abandon fail");
            Check(!run.TryRetry(), "4th retry refused (3-retry cap)");

            // Grading: 3 stars inside par, 0 retries, 0 wanted
            var g3 = MissionGradeRules.Evaluate(1000, 100, 65f, 100f, 0, 0, 0f);
            Check(g3.stars == 3 && g3.totalScore == 100, "Perfect par run earns 3 stars & 100 pts");
            Check(g3.totalMoneyPayout == 1350, "3-star +25% on base (1250) + 100 time bonus = 1350");

            // Retrying blocks 3 stars even if score >= 85
            var gRetry = MissionGradeRules.Evaluate(1000, 100, 50f, 100f, 1, 0, 0f);
            Check(gRetry.stars == 2 && gRetry.nextStarHint.Contains("retries"), "1 retry caps at 2 stars and names blocker");

            // Landmine #28: No clock tops out at 2 stars
            var gNoClock = MissionGradeRules.Evaluate(1000, 0, 10f, 0f, 0, 0, 0f);
            Check(gNoClock.totalScore == 80 && gNoClock.stars == 2, "No-clock mission tops out at two stars (score 80)");

            // Variants gated behind completion
            Check(!MissionModifierRules.CanRunVariant(MissionVariant.TimeAttack, false), "Variant locked before first clear");
            Check(MissionModifierRules.CanRunVariant(MissionVariant.TimeAttack, true), "Variant unlocked after clear");
            CheckNear(MissionModifierRules.PayoutMultiplier(MissionVariant.HotZone), 1.30f, 1e-4f, "HotZone +30%");
        }

        // ==================== 8. Dates & 400 Consecutive Days ====================
        private static void TestDatesAndDailies400Days()
        {
            // Landmine #15: Invalid dates never throw
            Check(!DailyChallengeRules.TryDateOf(20260431, out _), "31 April rejected safely");
            Check(!DailyChallengeRules.TryDateOf(20250229, out _), "29 Feb non-leap rejected safely");
            Check(DailyChallengeRules.TryDateOf(20240229, out _), "29 Feb leap year accepted");
            Check(!DailyChallengeRules.TryDateOf(100000101, out _), "Year > 9999 rejected safely");
            Check(!DailyChallengeRules.TryDateOf(-50, out _), "Negative dayKey rejected safely");
            Check(!DailyChallengeRules.TryDateOf(20261301, out _), "Month 13 rejected safely");

            // §4.2: 400 consecutive days produce 3 distinct kinds each!
            DateTime cursor = new DateTime(2026, 1, 1);
            for (int i = 0; i < 400; i++)
            {
                int key = DailyChallengeRules.EncodeDayKey(cursor.AddDays(i));
                DailyChallengeSpec[] set = DailyChallengeRules.GenerateDailySet(key, 5);
                bool distinct = set.Length == 3 &&
                                set[0].kind != set[1].kind &&
                                set[0].kind != set[2].kind &&
                                set[1].kind != set[2].kind;
                Check(distinct, $"Day {i} ({key}) has 3 distinct daily kinds");
            }

            // Determinism + streak + full-set 30% bonus
            var a = DailyChallengeRules.GenerateDailySet(20261125, 7);
            var b = DailyChallengeRules.GenerateDailySet(20261125, 7);
            Check(a[0].kind == b[0].kind && a[0].target == b[0].target, "Same day produces identical set");
            Check(DailyChallengeRules.ComputeUpdatedStreak(2, 20261124, 20261125) == 3, "Consecutive day increments streak");
            Check(DailyChallengeRules.ComputeUpdatedStreak(4, 20261120, 20261125) == 1, "Missed day resets streak to 1");
            CheckNear(DailyChallengeRules.StreakMultiplier(1), 1.0f, 1e-4f, "Streak 1 = 1.0x");
            CheckNear(DailyChallengeRules.StreakMultiplier(5), 1.5f, 1e-4f, "Streak 5 = 1.5x");
            CheckNear(DailyChallengeRules.StreakMultiplier(9), 1.5f, 1e-4f, "Streak capped at 1.5x");

            var wallet = new Wallet(0);
            var dSys = new DailyChallengeSystem(wallet);
            dSys.SetDayOverride(() => new DateTime(2026, 11, 25));
            dSys.EnsureCurrentDay(3);
            foreach (var spec in dSys.Specs)
                dSys.ReportActivity(spec.kind, spec.target);
            Check(dSys.BonusPaid && dSys.FullSetsCompleted == 1 && wallet.Balance > 0, "Completing all 3 dailies pays full-set bonus");
        }

        // ==================== 9. Save & Migration ====================
        private static void TestSaveAndMigration()
        {
            PlayerPrefs.DeleteAll();
            var saveSys = new SaveSystem();
            var original = new SaveData
            {
                position = new Vector3(12f, 1f, -34f),
                money = 4500,
                reputation = 320,
                currentMissionId = "mis_03_rooftop", // Must be cleared on sanitise!
                missionStars = new[] { "mis_01_wake_up:3", "mis_02_delivery:999999999999999999999999", "bad_row" }
            };

            Check(saveSys.Save(0, original), "Save to slot 0");
            Check(saveSys.TryLoad(0, out SaveData loaded), "Load from slot 0");
            Check(loaded.money == 4500 && loaded.reputation == 320, "Money & rep round-tripped");
            Check(string.IsNullOrEmpty(loaded.currentMissionId), "Half-finished mission cleared");
            // Landmine #16: Overflow integer string rejected safely without OverflowException
            Check(loaded.missionStars.Length == 1 && loaded.missionStars[0] == "mis_01_wake_up:3", "Landmine #16: Overflow star entry safely stripped");

            // Tamper checksum -> rejected
            string key0 = SaveSystem.KeyForSlot(0);
            string raw = PlayerPrefs.GetString(key0).Replace("4500", "9999");
            PlayerPrefs.SetString(key0, raw);
            bool corruptEvent = false;
            saveSys.CorruptSlotRejected += _ => corruptEvent = true;
            Check(!saveSys.TryLoad(0, out _) && corruptEvent, "Corrupt checksum rejected");

            // Unsupported version rejected
            var badVer = new SaveEnvelope { formatVersion = 99, payload = "{}" };
            badVer.checksum = SaveIntegrity.ComputeFnv1a(badVer.payload);
            Check(!SaveIntegrity.TryUnpack(JsonUtility.ToJson(badVer), out _), "Future version 99 rejected");

            // v1 -> v4 migration & array-length repair
            var v1Data = new SaveData { hourOfDay = -5f, dailyProgress = new int[10], unlockedItemIds = new string[300] };
            string v1Payload = JsonUtility.ToJson(v1Data);
            var v1Env = new SaveEnvelope { formatVersion = 1, checksum = SaveIntegrity.ComputeFnv1a(v1Payload), payload = v1Payload };
            Check(SaveIntegrity.TryUnpack(JsonUtility.ToJson(v1Env), out SaveData migrated), "v1 save migrates to v4");
            Check(migrated.dailyProgress.Length == 3, "dailyProgress repaired to length 3");
            Check(migrated.unlockedItemIds.Length == SaveIntegrity.MaxArrayLength, "Oversized array trimmed");
        }

        // ==================== 10. Progression, Shop, Achievements, Stunts, Bounty ====================
        private static void TestProgressionEconomyAchievementsStuntsBounty()
        {
            var wallet = new Wallet(500);
            Check(!wallet.TrySpend(600) && wallet.Balance == 500, "Wallet never goes negative");
            Check(wallet.TrySpend(200) && wallet.Balance == 300, "Wallet spends 200");

            Check(ProgressionCurve.LevelForReputation(0) == 1, "0 rep = level 1");
            Check(ProgressionCurve.LevelForReputation(1815) == 19, "1815 rep = level 19");

            var shop = new ShopSystem(wallet) { Reputation = 100 }; // level 2
            var item = ScriptableObject.CreateInstance<ShopItem>();
            item.itemId = "weapon_uzi";
            item.price = 250;
            item.unlockLevel = 2;
            Check(shop.TryBuy(item) == ShopPurchaseResult.Success, "Shop buys unlocked affordable item");
            Check(shop.TryBuy(item) == ShopPurchaseResult.AlreadyOwned, "Duplicate purchase blocked");

            // Achievements: toast cap 3 + Restore rescore + NotifySideJobs
            var achWallet = new Wallet(0);
            var ach = new AchievementSystem(achWallet);
            ach.NotifySideJobs(12);
            ach.Restore(Array.Empty<string>(), new AchievementProgressSnapshot
            {
                completedMissions = 20,
                totalThreeStarMissions = 20,
                fullDailySetsWon = 2,
                bestDailyStreak = 5,
                stuntMoneyEarned = 15000,
                totalMoneyEarned = 200000,
                variantsCleared = 4
            });
            Check(ach.UnlockedIds.Count == 13, "All 13 achievements unlocked on full snapshot");
            Check(ach.ToastQueueCount == 3, "Achievement toast queue capped at 3");

            // Stunts
            var stuntSys = new StuntSystem(achWallet);
            stuntSys.AddDrift(35f, 20f, 1.0f);
            Check(stuntSys.PendingPoints > 0f, "Drift adds pending points");
            stuntSys.OnCrash();
            Check(stuntSys.PendingPoints == 0f, "Crash wipes pending stunt points");
            stuntSys.AddDrift(35f, 20f, 1.0f);
            int earned = stuntSys.BankNow();
            Check(earned > 0, "Stunt settle banks money");

            // Bounty diminishing returns & 1.5s gap
            var bountyWallet = new Wallet(0);
            var bounty = new BountySystem(bountyWallet);
            int p1 = bounty.ProcessCrime(CrimeType.KillPolice, 10f);
            int pDup = bounty.ProcessCrime(CrimeType.KillPolice, 10.5f);
            Check(p1 > 0 && pDup == 0, "1.5s gap prevents double bounty payout");
            for (int i = 1; i <= 20; i++)
                bounty.ProcessCrime(CrimeType.KillPolice, 10f + i * 2f);
            CheckNear(BountyRules.DiminishingMultiplier(25), 0.25f, 1e-4f, "Bounty diminishing floors at 25%");

            // Story endings (§2.7)
            Check(GameFlowController.ResolveEnding(19, 0, 0, 0) == StoryEnding.None, "<20 missions -> None");
            Check(GameFlowController.ResolveEnding(20, 1, 0, 0) == StoryEnding.Clean, "Clean ending");
            Check(GameFlowController.ResolveEnding(20, 4, 3, 5) == StoryEnding.King, "King ending");
            Check(GameFlowController.ResolveEnding(20, 0, 4, 25) == StoryEnding.Ash, "Ash ending overrides on >=20 wrecked cars");
        }

        // ==================== 11. World, City, Props, Day/Night, NPC ====================
        private static void TestWorldCityPropsDayNightNpc()
        {
            var ring = new List<Vector2Int>();
            WorldStreamMath.CollectResidentRing(new Vector2Int(2, -3), 1, ring);
            Check(ring.Count == 9, "3x3 resident cell ring has 9 cells");
            Check(WorldStreamMath.ChebyshevDistance(new Vector2Int(0, 0), new Vector2Int(-3, 2)) == 3, "Chebyshev distance");

            var wsGo = new GameObject("Streamer");
            var streamer = wsGo.AddComponent<WorldStreamer>();
            streamer.UpdateForPosition(Vector3.zero);
            Check(streamer.LoadedCellCount == 9, "Streamer loads 9 cells");
            streamer.UpdateForPosition(new Vector3(500f, 0f, 500f));
            Check(streamer.LoadedCellCount == 9, "Streamer unloads distant cells and keeps 9");

            // Smallest containing district wins (Secret inside Downtown)
            var downtown = ScriptableObject.CreateInstance<DistrictDefinition>();
            downtown.districtId = "district_downtown";
            downtown.centre = Vector2.zero;
            downtown.size = new Vector2(600f, 600f);

            var secret = ScriptableObject.CreateInstance<DistrictDefinition>();
            secret.districtId = "district_secret";
            secret.centre = new Vector2(100f, -100f);
            secret.size = new Vector2(120f, 120f);

            var resolved = DistrictBounds.ResolveContaining(new[] { downtown, secret }, new Vector3(100f, 0f, -100f));
            Check(resolved == secret, "Secret inside Downtown wins by smallest area");
            var resolvedOuter = DistrictBounds.ResolveContaining(new[] { downtown, secret }, new Vector3(-200f, 0f, 200f));
            Check(resolvedOuter == downtown, "Outer Downtown resolves to Downtown");

            // Day/Night
            CheckNear(DayNightRules.SunPitch(12f), 68f, 0.01f, "Noon sun pitch 68 deg");
            Check(DayNightRules.AmbientFor(3f) >= 0.279f, "03:00 ambient >= 0.28 (never 0)");
            CheckNear(DayNightRules.LightIntensity(18.6f), 0f, 0.01f, "Lamps start fading in at 18:36");
            CheckNear(DayNightRules.LightIntensity(22.0f), 1f, 0.01f, "Lamps full at night");
            CheckNear(DayNightRules.LightIntensity(12.0f), 0f, 0.01f, "Lamps off at noon");

            // Props
            Check(PropRules.TotalPropKinds == 78, "78 prop kinds");
            Check(PropRules.ComputeChunkPropCount(5.0f) == 46, "Chunk prop cap 46");
            Check(PropRules.RenderTier(30f) == PropRenderTier.FullShadowed, "<=40m full+shadows");
            Check(PropRules.RenderTier(60f) == PropRenderTier.SimplifiedNoShadow, "40-85m simplified");
            Check(PropRules.RenderTier(100f) == PropRenderTier.FlatDecal, "85-130m flat decal");
            Check(PropRules.RenderTier(140f) == PropRenderTier.Hidden, ">130m hidden");
            Check(PropRules.BatchDrawCalls(true) <= 8, "<=8 material groups per district");

            // Hotel
            Check(!HotelRules.CanBook(HotelTier.Penthouse, 2, false, false), "Penthouse requires pass or rental");
            Check(HotelRules.CanBook(HotelTier.Penthouse, 2, true, false), "Penthouse books with pass");
            Check(HotelRules.ComputeCheckoutRefund(HotelTier.Standard, 4) == 800, "50% early checkout refund");

            // NPC LOD, Steering, Avoidance
            Check(NpcTickLod.TickRateHz(10f) == 60, "Near NPC 60Hz");
            Check(NpcTickLod.TickRateHz(40f) == 20, "Mid NPC 20Hz");
            Check(NpcTickLod.TickRateHz(80f) == 6, "Far NPC 6Hz");
            Check(NpcTickLod.TickRateHz(120f) == 0, "Culled NPC 0Hz");
            CheckNear(NpcTickLod.TickMultiplier(20), 3f, 0.01f, "20Hz multiplier = 3");

            Check(NpcSteering.ArriveVelocity(Vector3.zero, new Vector3(0f, 0f, 1.5f), 4f, 3f).z < 4f, "Arrive brakes inside slowing radius");
            Vector3 d1 = NpcAvoidance.SteerClear(Vector3.forward, true, 1.0f, true, 1.0f, 7);
            Vector3 d2 = NpcAvoidance.SteerClear(Vector3.forward, true, 1.0f, true, 1.0f, 7);
            Check(d1 == d2 && d1.x != 0f, "Avoidance both-blocked is deterministic (no jitter)");
        }

        // ==================== 12. Audio, Visuals, Localization ====================
        private static void TestAudioVisualsLocalization()
        {
            Check(AudioBudgetMath.CapForBand(AudioBand.Near) == 12, "Audio near cap 12");
            Check(AudioBudgetMath.CapForBand(AudioBand.Mid) == 8, "Audio mid cap 8");
            Check(AudioBudgetMath.CapForBand(AudioBand.Far) == 4, "Audio far cap 4");
            Check(AudioBudgetMath.ClassifyBand(95f) == AudioBand.Culled, "Audio >90m culled");
            Check(AudioBudgetMath.ComputeFalloffVolume(1f, 95f) == 0f, "Audio falloff >90m is 0");
            Check(AudioBudgetMath.CanStealVoice(0.8f, 0.10f), "Loud voice steals barely audible voice");
            Check(!AudioBudgetMath.CanStealVoice(0.8f, 0.50f), "Cannot steal clearly audible voice");

            var adGo = new GameObject("AudioDir");
            var director = adGo.AddComponent<AudioDirector>();
            director.Awake();
            Check(director.PoolCapacity == 24, "Fixed 24-voice pool");
            var clip = new AudioClip();
            for (int i = 0; i < 40; i++)
                director.TryPlay3D(clip, new Vector3(1f, 0f, 0f), 1f);
            Check(director.PoolCapacity == 24, "Audio pool NEVER grows");
            Check(!director.TryPlay3D(clip, Vector3.zero, 0f), "Refuses silent voice");

            CheckNear(AnimationMath.WheelSpinDeltaDegrees(Mathf.PI * 0.34f, 0.34f), 180f, 0.1f, "Half circumference = 180 deg spin");
            Check(LocalizationTable.Get("ui.district.market", LanguageCode.Vietnamese).Contains("Bến Thành"), "VI localization resolves");
            Check(LocalizationTable.Get("missing.key.xyz", LanguageCode.Vietnamese) == "missing.key.xyz", "Missing key falls back to key");
        }

        // ==================== 13. Quality, Fog (1.9.3 / R3), R6 Clamping ====================
        private static void TestQualityFogAndSettings()
        {
            Check(QualityRules.PickTier(8192, 8) == 2, "8GB/8core -> High tier 2");
            Check(QualityRules.PickTier(2048, 8) == 0, "2GB -> Low tier 0");
            Check(QualityRules.PickTier(4096, 6) == 1, "4GB/6core -> Mid tier 1");

            // R6: Player settings may lower cost, NEVER raise above tier!
            Check(QualityRules.ShadowDistanceFor(0, true, 100f) == 0f, "Tier 0 keeps shadows off even if player asks 100m");
            CheckNear(QualityRules.ShadowDistanceFor(1, true, 100f), 45f, 0.01f, "Tier 1 caps shadowDistance at 45m");
            CheckNear(QualityRules.ShadowDistanceFor(1, true, 25f), 25f, 0.01f, "Player can shorten Tier 1 shadowDistance to 25m");
            Check(QualityRules.TargetFrameRateFor(1, 120) == 30, "Tier 1 caps targetFps at 30 even if player asks 120");
            CheckNear(QualityRules.FarClipFor(900f), 500f, 0.01f, "FarClip clamped <= 500m");

            // §1.9.3 / R3: QualityApplier.Apply MUST write RenderSettings.fog & RenderSettings.fogDensity!
            RenderSettings.fog = false;
            RenderSettings.fogDensity = 0f;
            var qaGo = new GameObject("Systems");
            var qa = qaGo.AddComponent<QualityApplier>();
            qa.Apply(0);
            Check(RenderSettings.fog && Mathf.Approximately(RenderSettings.fogDensity, 0.012f), "Tier 0 writes fogDensity 0.012");
            qa.Apply(1);
            Check(Mathf.Approximately(RenderSettings.fogDensity, 0.009f), "Tier 1 writes fogDensity 0.009");
            qa.Apply(2);
            Check(Mathf.Approximately(RenderSettings.fogDensity, 0.006f), "Tier 2 writes fogDensity 0.006");
        }

        // ==================== 14. Net & Loopback ====================
        private static void TestNetAndLoopback()
        {
            var cmd = new NetworkCommand
            {
                tick = 42,
                position = new Vector3(10.5f, 1.2f, -8.25f),
                moveX = -0.5f,
                moveY = 1.0f,
                aimYaw = 135.5f,
                buttonsMask = AuthorityValidator.FireButtonBit,
                weaponSlot = 2,
                claimedDamage = 30
            };

            byte[] wire = new byte[NetworkCommand.ByteSize];
            cmd.WriteTo(wire, 0);
            NetworkCommand unpacked = NetworkCommand.ReadFrom(wire, 0);
            Check(unpacked.tick == 42 && unpacked.weaponSlot == 2 && unpacked.claimedDamage == 30, "32-byte header round-trip");
            CheckNear(unpacked.position.x, 10.5f, 1e-4f, "32-byte pos.x round-trip");
            CheckNear(unpacked.aimYaw, 135.5f, 1e-4f, "32-byte aimYaw round-trip");

            var (client, server) = LoopbackTransport.CreateLinkedPair();
            client.SendCommand(cmd);
            Check(server.TryReceiveCommand(out NetworkCommand recv) && recv.tick == 42, "Loopback transport delivers packet");

            Check(AuthorityValidator.Validate(cmd, 42, cmd.position, 0.02f, 9f, 0, 5, 50) == ValidationVerdict.StaleTick, "Stale tick rejected");
            var teleport = cmd; teleport.tick = 43; teleport.position = new Vector3(100f, 0f, 0f);
            Check(AuthorityValidator.Validate(teleport, 42, Vector3.zero, 0.02f, 9f, 0, 5, 50) == ValidationVerdict.TeleportDetected, "Teleport rejected");
            var speedHack = cmd; speedHack.tick = 45; speedHack.position = new Vector3(12f, 0f, 0f);
            Check(AuthorityValidator.Validate(speedHack, 42, Vector3.zero, 0.02f, 9f, 0, 5, 50) == ValidationVerdict.SpeedHackDetected, "Speed hack rejected");
            var rapidFire = cmd; rapidFire.tick = 44; rapidFire.position = Vector3.zero;
            Check(AuthorityValidator.Validate(rapidFire, 42, Vector3.zero, 0.02f, 9f, 42, 5, 50) == ValidationVerdict.FireRateAbuse, "Fire-rate abuse rejected");
            var dmgHack = cmd; dmgHack.tick = 50; dmgHack.position = Vector3.zero; dmgHack.claimedDamage = 250;
            Check(AuthorityValidator.Validate(dmgHack, 42, Vector3.zero, 0.02f, 9f, 40, 5, 60) == ValidationVerdict.InvalidDamageClaim, "Invalid damage claim rejected");
            var clean = cmd; clean.tick = 50; clean.position = new Vector3(0.5f, 0f, 0f);
            Check(AuthorityValidator.Validate(clean, 42, Vector3.zero, 0.02f, 9f, 40, 5, 60) == ValidationVerdict.Accepted, "Valid command accepted");
        }

        // ==================== 15. Scene Systems Contract ====================
        private static void TestSceneSystemsContract()
        {
            var valid = new SceneSystemsSnapshot
            {
                sceneName = "Test",
                stores = new[] { new SceneComponentDescriptor { objectName = "Systems", objectInstanceId = 100 } },
                qualityAppliers = new[] { new SceneComponentDescriptor { objectName = "Systems", objectInstanceId = 100 } },
                settingsAppliers = new[] { new SceneComponentDescriptor { objectName = "Systems", objectInstanceId = 100, applierStoreMatchesSceneStore = true } }
            };
            Check(SceneSystemsContract.ValidateScene(valid).Count == 0, "Clean Systems object passes contract");
            Check(SceneSystemsContract.ValidateBuiltScenes(Array.Empty<SceneSystemsSnapshot>()).Count == 1, "Empty built scene list fails");

            var missingStore = valid; missingStore.stores = Array.Empty<SceneComponentDescriptor>();
            Check(SceneSystemsContract.ValidateScene(missingStore).Exists(e => e.Contains("missing SettingsStore")), "Missing SettingsStore named");

            var dupQa = valid; dupQa.qualityAppliers = new[] { valid.qualityAppliers[0], valid.qualityAppliers[0] };
            Check(SceneSystemsContract.ValidateScene(dupQa).Exists(e => e.Contains("duplicated QualityApplier")), "Duplicated QualityApplier named");

            var wrongName = valid;
            wrongName.stores = new[] { new SceneComponentDescriptor { objectName = "Boot", objectInstanceId = 100 } };
            Check(SceneSystemsContract.ValidateScene(wrongName).Exists(e => e.Contains("Systems")), "Wrong GameObject name caught");

            var splitObj = valid;
            splitObj.qualityAppliers = new[] { new SceneComponentDescriptor { objectName = "Systems", objectInstanceId = 200 } };
            Check(SceneSystemsContract.ValidateScene(splitObj).Exists(e => e.Contains("split")), "Split Systems GameObjects caught");

            var unwired = valid;
            unwired.settingsAppliers = new[] { new SceneComponentDescriptor { objectName = "Systems", objectInstanceId = 100, applierStoreMatchesSceneStore = false } };
            Check(SceneSystemsContract.ValidateScene(unwired).Exists(e => e.Contains("not wired")), "Unwired SettingsApplier.store caught");

            var twoFaults = valid;
            twoFaults.stores = Array.Empty<SceneComponentDescriptor>();
            twoFaults.qualityAppliers = Array.Empty<SceneComponentDescriptor>();
            Check(SceneSystemsContract.ValidateScene(twoFaults).Count >= 2, "Multiple faults reported at once");
        }
    }
}
