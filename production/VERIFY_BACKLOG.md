# Dude Chaos Wars — Verification Backlog

**Golden Rule (§0 & §4.5):** An item may ONLY be marked `PASS` after a human (or real Unity Editor / Android device run with captured output) actually watched it happen. Never mark `PASS` from reading code or passing offline stub builds.

Manage this file via `python3 Tools/verify/checklist.py`:
- `python3 Tools/verify/checklist.py summary`
- `python3 Tools/verify/checklist.py list [--phase P2] [--status TODO]`
- `python3 Tools/verify/checklist.py pass V-101 --note "verified in Unity 6000.3.28f1"`
- `python3 Tools/verify/checklist.py fail V-105 --note "jump apex measured 1.9m"`
- `python3 Tools/verify/checklist.py block V-1101 --note "no Android SDK on build box"`
- `python3 Tools/verify/checklist.py reset V-101`
- `python3 Tools/verify/checklist.py next`

| ID | Phase | Status | Description | Note |
|---|---|---|---|---|
| V-101 | P1 | TODO | Walk speed measures 3.2 m/s after 0.18 dead-zone threshold in Play Mode | Needs Unity Editor Play Mode |
| V-102 | P1 | TODO | Run speed measures 6.0 m/s and Sprint reaches 8.8 m/s with +5 FOV kick | Needs Unity Editor Play Mode |
| V-103 | P1 | TODO | Coyote time (0.12 s) and jump buffer (0.10 s) trigger reliably off ledges | Needs Unity Editor Play Mode |
| V-104 | P1 | TODO | Landing classification: >-8 Soft, exact -8..-14 Medium, <-14 Hard forces ragdoll + shake | Needs Unity Editor Play Mode |
| V-105 | P1 | TODO | Crouch resizes CharacterController height 1.8 m -> 1.1 m and blocks stand-up under low ceiling | Needs Unity Editor Play Mode |
| V-106 | P1 | TODO | PlayerMotor.Awake self-resolves IPlayerInput on disk-loaded scene without runtime Bind() | Needs Unity Editor Play Mode |
| V-201 | P2 | TODO | Ragdoll Q1: Activates cleanly from Bullet, Explosion, VehicleImpact, EjectedFromVehicle, HighFall, Melee, Scripted | Mandatory P2 gate — NOT RUN |
| V-202 | P2 | TODO | Ragdoll Q2: Recovers after 1.8 s below 1.1 m/s ONLY when alive; dead bodies stay down | Mandatory P2 gate — NOT RUN |
| V-203 | P2 | TODO | Ragdoll Q3: Impulse hits the struck bone Rigidbody (Head/Torso/Limb), not uniform root push | Mandatory P2 gate — NOT RUN |
| V-204 | P2 | TODO | Ragdoll Q4: RagdollBudget caps full-sim bodies at <= 12 with one static driver and zero per-ragdoll Update | Mandatory P2 gate — NOT RUN |
| V-205 | P2 | TODO | Ragdoll Q5: Ragdolls beyond 55 m simplify/sleep cleanly and boneless prefabs do not throw | Mandatory P2 gate — NOT RUN |
| V-206 | P2 | TODO | Ragdoll Q6: PelvisFollowTarget tracks pelvis with 0.08 s lag through ragdoll and 0.9 s get-up | Mandatory P2 gate — NOT RUN |
| V-301 | P3 | TODO | 10-step shot chain fires in exact order with event-only VFX/UI and zero per-shot GC alloc | Needs Unity Editor Profiler |
| V-302 | P3 | TODO | BodyPartTag applies 3.2x Head, 1.0x Torso, 0.75x Limb multipliers on humanoid targets | Needs Unity Editor Play Mode |
| V-303 | P3 | TODO | All 9 WeaponMechanics (Auto, Semi, Burst, Pump, Bolt, Charge, Beam, Projectile, Throwable) fire in Play Mode | Needs Unity Editor Play Mode |
| V-304 | P3 | TODO | Melee light (1200 / 0.04 s stop), heavy (3800 / 0.11 s), 1.6x ragdolled, 1.8x backstab feel right | Needs human Play Mode test |
| V-305 | P3 | TODO | Explosion OverlapSphereNonAlloc applies linear falloff impulse + point-blank self-damage | Needs Unity Editor Play Mode |
| V-401 | P4 | TODO | Vehicle enter/exit 0.45 s timer windows within 3.5 m E-interact and input fallback chain work | Needs Unity Editor Play Mode |
| V-402 | P4 | TODO | Exit < 9 m/s places player cleanly; >= 9 m/s routes ExitingVehicle -> Ragdoll with 900 fwd + 420 up impulse | Needs Unity Editor Play Mode |
| V-403 | P4 | TODO | Arcade yaw handling, downforce 12, steering falloff 0.35, and brake > accel feel responsive | Needs human Play Mode test |
| V-404 | P4 | TODO | VehicleHealth stages: 50% smoke -> 25% fire -> 2.8 s delayed explosion | Needs Unity Editor Play Mode |
| V-405 | P4 | TODO | VehicleLock hotwire timer, break-in heat, and ArmourRules (Unarmoured/Light/Heavy/Air) verified | Needs Unity Editor Play Mode |
| V-501 | P5 | TODO | Wanted heat rises by 8 CrimeType weights and crosses {1,4,9,16,25,36} star thresholds | Needs Unity Editor Play Mode |
| V-502 | P5 | TODO | Cooldown ticks ONLY when out of police LOS AND > 48 m after 9 s memory window | Needs Unity Editor Play Mode |
| V-503 | P5 | TODO | PoliceUnit opens fire from star 2 and SWAT units spawn from star 4 | Needs Unity Editor Play Mode |
| V-504 | P5 | TODO | PoliceCar pursuits/rams/unwedges without NavMesh; helicopter & roadblocks appear from star 3 | Needs Unity Editor Play Mode |
| V-601 | P6 | TODO | Slice 1/11: Boot Test.unity with Mobile_URP assigned and zero pink renderers | Needs Unity Editor |
| V-602 | P6 | TODO | Slice 2/11: Walk/run/sprint/jump/crouch via keyboard/gamepad and touch MobileJoystick | Needs Unity Editor Play Mode |
| V-603 | P6 | TODO | Slice 3/11: Shoot DamageDummy and NPCs with hit marker, recoil, and casing/muzzle PrefabPool | Needs Unity Editor Play Mode |
| V-604 | P6 | TODO | Slice 4/11: Knock NPC and player into ragdoll and watch automatic recovery | Needs Unity Editor Play Mode |
| V-605 | P6 | TODO | Slice 5/11: Enter car, drive block, clean exit at low speed, high-speed eject ragdoll | Needs Unity Editor Play Mode |
| V-606 | P6 | TODO | Slice 6/11: Escalate Wanted to 3 stars, escape > 48 m out of sight, watch stars clear | Needs Unity Editor Play Mode |
| V-607 | P6 | TODO | Slice 7/11: Shoot car through smoke and fire until 2.8 s explosion throws nearby bodies | Needs Unity Editor Play Mode |
| V-608 | P6 | TODO | Slice 8/11: HUD health/armour, stars, ammo, money, objective, and SafeAreaFitter update live | Needs Unity Editor Play Mode |
| V-609 | P6 | TODO | Slice 9/11: PauseMenuController pauses/resumes cleanly and yields HitStopDriver | Needs Unity Editor Play Mode |
| V-610 | P6 | TODO | Slice 10/11: Save slot + pause autosave round-trip Format-4 payload with FNV-1a checksum | Needs Unity Editor Play Mode |
| V-611 | P6 | TODO | Slice 11/11: Complete mis_01_wake_up with 3 Vietnamese dialogue lines and star grade banner | Needs Unity Editor Play Mode |
| V-4591 | P6 | TODO | Re-run all 6 PlayModeSmokeTest tests in Unity 6000.3.28f1 (-batchmode -nographics) and bisect TestSlice PhysX crash | Pending real Unity 6.3 editor |
| V-701 | P7 | TODO | Play all 20 story missions sequentially ($200,650 base / $319,033 3-star / 1,815 rep -> Lvl 19) | Needs Unity Editor Play Mode |
| V-702 | P7 | TODO | MissionHud shows red clock <= 30 s, retry hint, star banner, and accurate NextStarHint | Needs Unity Editor Play Mode |
| V-703 | P7 | TODO | Replay cleared mission with Time Attack (-40% clock +15%), Hot Zone (+30%), and Fragile (+20%) | Needs Unity Editor Play Mode |
| V-704 | P7 | TODO | DailyChallengeHud displays 3 distinct daily tasks, midnight rollover, and 5-day streak bonus | Needs Unity Editor Play Mode |
| V-705 | P7 | TODO | AchievementToast queues max 3 toasts and restores/rescores all 13 achievements from save | Needs Unity Editor Play Mode |
| V-801 | P8 | TODO | Walk/drive across all 12 districts (2.29 km²) and verify smallest-containing Secret inside Downtown | Needs Unity Editor Play Mode |
| V-802 | P8 | TODO | InteriorPortal teleports cleanly while disabling CharacterController and enforces pass gates | Needs Unity Editor Play Mode |
| V-803 | P8 | TODO | HotelService books Basic/Standard/Deluxe/Penthouse, sleeps to 07:00, sets death spawn, refunds 50% | Needs Unity Editor Play Mode |
| V-804 | P8 | TODO | WorldStreamer keeps max 9 active 64 m cells around player and unloads distant cells on interval | Needs Unity Editor Play Mode |
| V-805 | P8 | TODO | NpcSpawner & AnimalSpawner obey 60/20/6/0 Hz tick LOD, avoidance without jitter, and fear flee | Needs Unity Editor Play Mode |
| V-901 | P9 | TODO | Render §1.9.9 #1: Run DudeChaos -> Setup -> 1, open built scenes, confirm pipeline log and zero pink | Needs Unity Editor |
| V-902 | P9 | TODO | Render §1.9.9 #2: Frame Debugger shows SRP Batcher compatible props and <= 150 draw calls in slice | Needs Unity Editor Frame Debugger |
| V-903 | P9 | TODO | Render §1.9.9 #3: QualityApplier.Apply(0/1/2) sets shadowDistance, lodBias, msaaSampleCount, targetFrameRate, RenderSettings.fogDensity; player settings never exceed tier | Needs Unity Editor |
| V-904 | P9 | TODO | Render §1.9.9 #4: DayNightCycle.SetHour(9) -> SetHour(23) updates sun, Color ambient, and single-loop street lamps | Needs Unity Editor |
| V-905 | P9 | TODO | Render §1.9.9 #5: VerifyBuiltScenesBatch passes on built scenes and fails naming missing SettingsStore on stripped copy | Needs Unity Editor batchmode |
| V-906 | P9 | TODO | AudioDirector 12/8/4/0 voice pool, DriftAudio, EN/VI LocalizationTable, colour-blind modes, textScale verified | Needs Unity Editor Play Mode |
| V-1001 | P10 | TODO | Two-actor LoopbackTransport Play Mode check: 32-byte NetworkCommand sync and AuthorityValidator rejections | Needs Unity Editor Play Mode |
| V-1002 | P10 | BLOCKED | Compile and verify FusionNetworkTransport (#if PHOTON_FUSION) against real Photon Fusion package | Photon Fusion package intentionally excluded (§1.2 / §4.4) |
| V-1101 | P11 | BLOCKED | Build signed Android ARM64 IL2CPP APK (min API 25, com.dudechaos.wars) via BuildScript + DUDECHAOS_KEYSTORE* | No Unity Editor, Android SDK/NDK, or release keystore on Linux VM |
| V-1102 | P11 | BLOCKED | Profile APK on mid-range Android device against §1.9.7 budgets (<=33 ms, <=150 DC, <=250k tris, <=350 MB tex, 0 B GC/frame, <=6 s cold start) | No physical Android device attached |
| V-1103 | P11 | TODO | Import keyed 2D sprites, 3D FBX/GLB replacements, and emissive URP/Lit materials so neon/sirens/brakes glow (R5) | Art replacement workstream pending |
