using System;
using DudeChaos.CameraRig;
using DudeChaos.City;
using DudeChaos.Combat;
using DudeChaos.Data;
using DudeChaos.GameFlow;
using DudeChaos.Npc;
using DudeChaos.Player;
using DudeChaos.Progression;
using DudeChaos.Save;
using DudeChaos.UI;
using DudeChaos.Wanted;
using Unity.Cinemachine;
using UnityEngine;

namespace DudeChaos.LogicTests
{
    internal static class ExtendedSystemTests
    {
        public static void Run(Action<bool, string> check, Action<float, float, float, string> checkNear)
        {
            // 1. All 78 PropKind entries have valid positive footprint and height
            for (int i = 0; i < PropRules.PropKindCount; i++)
            {
                PropKind k = (PropKind)i;
                PropSpec spec = PropRules.GetSpec(k);
                check(spec.kind == k && spec.footprint.x > 0f && spec.footprint.y > 0f && spec.height > 0f,
                    $"PropKind {k} spec valid");
            }

            // 2. All 12 DistrictKind roles (day vs night crowd + police weight)
            foreach (DistrictKind dk in Enum.GetValues(typeof(DistrictKind)))
            {
                float dayCrowd = DistrictRules.CrowdDensityMultiplier(dk, 12f);
                float nightCrowd = DistrictRules.CrowdDensityMultiplier(dk, 23f);
                float pw = DistrictRules.PoliceResponseWeight(dk);
                check(dayCrowd > 0f && nightCrowd > 0f && pw > 0f, $"DistrictKind {dk} rules positive");
            }
            check(DistrictRules.CrowdDensityMultiplier(DistrictKind.Slums, 23f) > DistrictRules.CrowdDensityMultiplier(DistrictKind.Slums, 12f),
                "Slums busier at night");
            check(DistrictRules.CrowdDensityMultiplier(DistrictKind.Market, 12f) > DistrictRules.CrowdDensityMultiplier(DistrictKind.Market, 23f),
                "Market empties at night");
            check(DistrictRules.CrowdDensityMultiplier(DistrictKind.Restaurant, 22f) > DistrictRules.CrowdDensityMultiplier(DistrictKind.Restaurant, 12f),
                "Restaurant busier at night");

            // 3. All 7 AnimalSpecies specs
            foreach (AnimalSpecies sp in Enum.GetValues(typeof(AnimalSpecies)))
            {
                AnimalSpec spec = AnimalRules.GetSpec(sp);
                check(spec.fleeSpeed > spec.wanderSpeed && spec.fearRadius > 0f, $"AnimalSpecies {sp} flees faster than wander");
            }

            // 4. MissionSelectRules 20-mission linear unlock chain & MissionBanterRules 3-line memory
            MissionData[] chain = new MissionData[20];
            for (int i = 0; i < 20; i++)
            {
                var m = ScriptableObject.CreateInstance<MissionData>();
                m.missionId = $"mis_{i + 1:D2}";
                m.dialogueLines = new[] { $"intro_{i}", $"mid_{i}", $"payoff_{i}" };
                chain[i] = m;
            }
            var completed = new System.Collections.Generic.HashSet<string>();
            check(MissionSelectRules.IsUnlocked(0, chain, completed), "Mission 1 unlocked at start");
            check(!MissionSelectRules.IsUnlocked(1, chain, completed), "Mission 2 locked before Mission 1");
            completed.Add("mis_01");
            check(MissionSelectRules.IsUnlocked(1, chain, completed), "Mission 2 unlocked after Mission 1");
            checkNear(MissionSelectRules.ComputeProgressPercent(20, completed), 5.0f, 0.01f, "1/20 = 5% progress");

            var banter = new MissionBanterRules();
            banter.GetIntroLine(chain[0]);
            banter.GetMidOrFailLine(chain[0]);
            banter.GetPayoffLine(chain[0]);
            banter.GetIntroLine(chain[1]);
            check(banter.RecentLines.Count == 3, "MissionBanterRules remembers only last 3 lines");

            // 5. ThirdPersonCameraRig Cinemachine 3 Lens struct copy-edit-write + punch + sprint kick
            var rigGo = new GameObject("CamRigTest");
            var yawGo = new GameObject("Yaw");
            var pitchGo = new GameObject("Pitch");
            var vcamGo = new GameObject("VCam");
            var vcam = vcamGo.AddComponent<CinemachineCamera>();
            var moveSettings = ScriptableObject.CreateInstance<MovementSettings>();
            var camRig = rigGo.AddComponent<ThirdPersonCameraRig>();
            camRig.Configure(yawGo.transform, pitchGo.transform, vcam, null, moveSettings);
            camRig.Punch(10f); // Must clamp to MaxPunchStrength = 3.0 -> +4.8 deg
            camRig.Step(0.001f);
            check(vcam.Lens.FieldOfView > 64.0f && vcam.Lens.FieldOfView <= 64.9f, "Camera punch updates Cinemachine Lens struct capped at 3*1.6");
            camRig.Step(0.20f);
            checkNear(vcam.Lens.FieldOfView, 60.0f, 0.05f, "Camera punch decays in 0.09s");

            // 6. DayNightCycle single lamp array loop
            var dnGo = new GameObject("DayNight");
            var sunGo = new GameObject("Sun");
            var sun = sunGo.AddComponent<Light>();
            var lampGo = new GameObject("Lamp");
            var lamp = lampGo.AddComponent<Light>();
            var dn = dnGo.AddComponent<DayNightCycle>();
            dn.Configure(sun, new[] { lamp }, 9f);
            check(sun.enabled && !lamp.enabled, "09:00 sun on, street lamp off");
            dn.SetHour(23f);
            check(!sun.enabled && lamp.enabled && lamp.intensity > 2.0f, "23:00 sun off, street lamp lit");

            // 7. AccessibilityPalette all 4 colour-blind modes
            foreach (ColourBlindMode cb in Enum.GetValues(typeof(ColourBlindMode)))
            {
                UiPalette pal = AccessibilityPalette.Resolve(cb, true);
                check(pal.panelBackground == Color.black && pal.bodyText == Color.white, $"Palette {cb} high-contrast valid");
            }

            // 8. SaveCoordinator + ChallengeBinder end-to-end event bridge
            PlayerPrefs.DeleteAll();
            var flowGo = new GameObject("FlowRoot");
            var pHealth = flowGo.AddComponent<HealthSystem>();
            pHealth.Configure(100f, 82f, 100f, 30f);
            var wallet = new Wallet(1200);
            var shop = new ShopSystem(wallet);
            var inv = new WeaponInventory();
            var missions = flowGo.AddComponent<MissionSystem>();
            missions.Bind(wallet, pHealth);
            var wanted = flowGo.AddComponent<WantedSystem>();
            wanted.Awake();
            var hotel = flowGo.AddComponent<HotelService>();
            hotel.RestoreNights(3);
            var stunts = new StuntSystem(wallet);
            var bounty = new BountySystem(wallet, wanted);
            var sideJobs = flowGo.AddComponent<SideJobDirector>();
            var dailies = new DailyChallengeSystem(wallet);
            dailies.SetDayOverride(() => new DateTime(2026, 11, 25));
            dailies.EnsureCurrentDay(2);
            var achievements = new AchievementSystem(wallet);
            sideJobs.Bind(wallet, achievements);

            var binder = flowGo.AddComponent<ChallengeBinder>();
            binder.Bind(dailies, stunts, bounty, sideJobs, missions, wanted);
            sideJobs.CompleteJob(new SideJobOffer { kind = SideJobKind.CourierDrop, rewardMoney = 400, timeLimitSeconds = 120f });
            check(achievements.SideJobsCompleted == 1, "SideJobDirector notifies AchievementSystem");

            var coord = flowGo.AddComponent<SaveCoordinator>();
            coord.BindSystems(flowGo.transform, pHealth, wallet, shop, inv, missions, wanted, dn, hotel, stunts, sideJobs, dailies, achievements);
            coord.OnApplicationPause(true); // Triggers autosave
            check(coord.SaveSystem.TryLoad(SaveSystem.AutosaveSlot, out SaveData autoLoaded), "OnApplicationPause(true) writes valid autosave");
            check(autoLoaded.hotelNightsLeft == 3 && autoLoaded.sideJobsCompleted == 1, "Autosave preserves hotel & side-job counts");

            // 9. PlayerMotor.Update drives static RagdollBudget.Tick for distant & NPC ragdolls (§1.3 Rule 10 & §1.8)
            RagdollBudget.ClearAll();
            var playerGo = new GameObject("BudgetViewer");
            var playerMotor = playerGo.AddComponent<PlayerMotor>();
            playerMotor.Awake();
            var nearNpcGo = new GameObject("NearNpcRagdoll");
            nearNpcGo.transform.position = new Vector3(5f, 0f, 0f);
            var nearRc = nearNpcGo.AddComponent<RagdollController>();
            nearRc.Awake();
            nearRc.OnEnable();
            nearRc.Activate(RagdollActivationReason.Bullet);

            var farNpcGo = new GameObject("FarNpcRagdoll");
            farNpcGo.transform.position = new Vector3(80f, 0f, 0f); // > 55m fullSimRadius
            var farRc = farNpcGo.AddComponent<RagdollController>();
            farRc.Awake();
            farRc.OnEnable();
            farRc.Activate(RagdollActivationReason.Explosion);

            Time.deltaTime = 2.0f;
            playerMotor.Update();
            check(!farRc.IsFullSimulation, "PlayerMotor.Update demotes >55m ragdoll via RagdollBudget.Tick");
            check(!nearRc.IsActiveRagdoll, "PlayerMotor.Update steps NPC RagdollController recovery via RagdollBudget.Tick");
            Time.deltaTime = 0.016f;
            RagdollBudget.ClearAll();
        }
    }
}
