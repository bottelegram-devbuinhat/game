# PROMPT_REBUILD — DUDE CHAOS WARS

A complete rebuild brief for an AI coding agent that has **no access to this repository**. It contains
the full master prompt, the per-phase prompts, the real content tables, the art pack, the verification
harness and the landmines that already broke this project once.

Read `0. HOW TO USE` first, then paste `1. MASTER PROMPT` into the agent, then the phases in
`5. PHASE PROMPTS` one at a time. Sections `2`–`4` are reference; the agent must not "improve" numbers
in them.

---

## 0. HOW TO USE THIS DOCUMENT

| Mode | What to paste | Expected result |
|---|---|---|
| **One shot** | `1. MASTER PROMPT` + `2. CONTENT PACK` + `3. ART PACK` in one message | Full C# project, data assets, tools, docs. No Unity editor needed to produce it; Unity needed to prove it. |
| **Phased** (recommended) | `1. MASTER PROMPT`, then each block from `5. PHASE PROMPTS` in order | Same result, but each phase has an acceptance gate and a checkpoint file. |
| **Audit** | `1. MASTER PROMPT` + `6. LANDMINES` + `7. VERIFICATION` | A review of an existing rebuild, not a new build. |

The receiving agent must be able to: write C#, write Unity YAML assets by hand, run `dotnet` and
`python3`, and say **NOT RUN** when it cannot prove something. If it cannot do the last one, this
project will rot: every claim in this document is tagged with how it was proven.

**Golden rule for the receiving agent:** you may only mark something PASS if you personally watched it
happen. "Compiles offline" is not "runs in Unity". "The code looks right" is not a test.

---

## 1. MASTER PROMPT

### 1.1 Mission

Build **Dude Chaos Wars**: an open-ish city, ragdoll crime sandbox for Android, in the spirit of Dude
Theft Wars and Gang Beasts. Third-person, joystick + on-screen buttons, stylised low-poly cartoon art,
maximum juice on every hit, Vietnamese city identity. Solo single-player first; multiplayer only as a
transport/damage-validation scaffold, never as shipped netcode.

**Pillars, in priority order:**
1. **The ragdoll is the joke.** Every hit, crash and fall throws a body that stays on the ground and
   gets up on its own. If the ragdoll feels wrong, nothing else matters.
2. **The city is a character.** 12 districts, walkable interiors, day/night, neon, traffic, animals.
3. **Escalation is the humour.** A small job goes wrong small; a big job takes the block with it.
4. **Mobile first.** 60 fps target on a mid phone, fixed budget for ragdolls, audio voices, draw calls.
5. **Never fake a result.** A missing feature is documented as missing, not stubbed into a green check.

### 1.2 Hard stack lock

| Item | Locked value | Notes |
|---|---|---|
| Engine | **Unity 6.3 LTS `6000.3.28f1`** | Supported to Dec 2027. Do not silently switch to 6.0/2022.3 (both EOL). |
| Pipeline | **URP** (Universal Render Pipeline) | Project renders **pink** until a URP asset is created and assigned. Not optional. |
| Language | C# (no unsafe, no DOTS) | |
| Build target | Android, **IL2CPP, ARM64 only**, min API **25**, landscape, Linear colour | |
| Bundle id | `com.dudechaos.wars` | |
| Packages | Input System 1.20.1, Cinemachine 3.1.7, URP 17.3.0, uGUI 2.0.0, Test Framework 1.4.6, TextMeshPro | If a **core** package pin errors in Package Manager, delete that pin; URP/uGUI/TMP/Test Framework ship with the editor. |
| Camera | Third-person Cinemachine only | No first person, no free camera in gameplay. |
| Language of game text | English + Vietnamese, key-based | `LocalizationTable` falls back EN, then the key. |
| Physics | Fixed Timestep **0.02**, Auto Sync Transforms **off**, non-alloc queries only | |
| Package size | 200–400 MB final APK | Art budget follows from this. |

Rejected on purpose: DOTween (Asset Store, adds a manual import step for a few easings — use coroutines),
Photon Fusion at runtime (API cannot be verified without the package; see 4.4), WheelCollider driving
simulation (arcade yaw model is the design).

### 1.3 Engineering rules (non-negotiable)

1. **No false claims.** Every report line is one of: `PASS` (with the command/output that proves it),
   `FAIL` (with the error), `NOT RUN` (with the reason), `BLOCKED` (with what is missing).
2. **Phase gates are real.** A phase is done when its code compiles, its checks actually ran (or are
   explicitly marked NOT RUN), and the checkpoint file records both.
3. **One global manager.** A single `GameManager` root object; systems register with it. No
   `WantedManager.Instance` / `AudioManager.Instance` / `SaveManager.Instance` pile.
4. **Assembly graph is a one-way tree.** No cycles, ever. The offline audit must prove it.
5. **Data is ScriptableObject assets, hand-written YAML with fixed GUIDs.** The project must import
   cleanly on the very first open, with no "script missing" rows. Every asset field name is checked
   against its C# declaration by a script.
6. **Pure logic lives in static classes with no `UnityEngine` scene dependency**, so it can be unit
   tested outside Unity. MonoBehaviours are thin drivers.
7. **Offline harness is mandatory** (`Tools/`): a stub-based type-check plus logic tests. It is not
   proof of Unity compatibility, and the docs must say so.
8. **Placeholder art is primitives tagged `// ART REPLACE`.** Never block gameplay on art.
9. **Mechanics may echo the genre; identity may not.** No copied names, logos, characters, vehicles or
   map layout from Dude Theft Wars, Gang Beasts or GTA.
10. **No `Update()` that scans the scene.** Event-driven or interval-driven only; one static budget
    driver for ragdolls and one for audio.
11. **No allocation in hot paths.** `OverlapSphereNonAlloc`, cached arrays, no `GetComponentsInParent`
    per shot, hash animator parameters once.
12. **Numbers live in assets, not in code.** Movement, ragdoll, combat, vehicle, wanted settings are
    ScriptableObjects; changing a default requires a written decision.

### 1.4 Repository layout (exact)

```
Assets/
  Scripts/
    Data/           14 ScriptableObject contracts + enums + interfaces   (DudeChaos.Data;
                    the 15th, DistrictDefinition, lives in City/)
    Core/           GameManager, PrefabPool, HitStopDriver                (DudeChaos.Core)
    Player/         state machine, motor, health, ragdoll, input interface (DudeChaos.Player)
      Input/        Input System adapter                                  (DudeChaos.Player.Input)
      Ragdoll/      controller, budget, budget model, target source
    Camera/         Cinemachine rig, look controller, recoil, pelvis follow (DudeChaos.CameraRig)
    Combat/         10-step shot chain, melee, explosion, attachments, support (DudeChaos.Combat)
    Vehicles/       arcade controller, health, interaction, lock, nitro, tuning, armour (DudeChaos.Vehicles)
    Wanted/         heat/stars, police unit, police car, spawner, chase math (DudeChaos.Wanted)
    City/           districts, day/night, interiors, hotel, props, side jobs, district heat (DudeChaos.City)
    Npc/            pedestrians, animals, steering, avoidance, spawn budgets (DudeChaos.Npc)
    World/          cell streaming                                    (DudeChaos.World)
    Progression/    wallet, shop, missions, grades, dailies, achievements, stunts, bounty (DudeChaos.Progression)
    GameFlow/       save coordinator, mission director, triggers, side jobs (DudeChaos.GameFlow)
    Save/           payload + integrity + slots                       (DudeChaos.Save)
    Settings/       settings model, store, quality rules/applier, scene contract (DudeChaos.Settings)
    UI/             HUD, joystick, pause, mission/stunt/daily/achievement HUD (DudeChaos.UI)
    Audio/          fixed voice pool, drift audio                      (DudeChaos.Audio)
    Visuals/        procedural walk, wheel spin, siren, neon, lights   (DudeChaos.Visuals)
    Localization/   EN/VI table                                       (DudeChaos.Localization)
    Net/            command format, authority validation, transport   (DudeChaos.Net)
  Editor/           SceneBootstrap, ProjectSetup, BuildScript          (DudeChaos.Editor)
  Tests/PlayMode/   smoke tests                                        (DudeChaos.Tests.PlayMode)
  Data/             Weapons, Vehicles, Missions, Equipment, Shop, Districts (182 assets)
  Settings/         MovementSettings, RagdollSettings, CombatSettings, VehicleSettings,
                    WantedSettings + URP renderer/pipeline assets
  Scenes/           Test.unity (clean), TestSlice.unity (combat/vehicle props)
Packages/manifest.json
ProjectSettings/    every *.asset, Packages/, ProjectVersion.txt  (must be committed)
Tools/              CompileCheck, LogicTests, audit, verify, content, art  (never inside Assets/)
design/ production/ docs
```

**Scale:** 153 scripts / 22,598 lines (measured: `find Assets -name '*.cs' | wc -l`, plus a line count
over the same set), 22 assemblies, 196 assets, 20 missions, 48 weapons,
18 vehicles, 59 equipment items, 25 shop items, 12 districts, 78 prop kinds, 13 achievements.

### 1.5 Assembly graph (copy exactly; verified acyclic)

| Assembly | References |
|---|---|
| `DudeChaos.Data` | — |
| `DudeChaos.Core` | Data |
| `DudeChaos.Player` | Data |
| `DudeChaos.Player.Input` | Data, Player, Unity.InputSystem |
| `DudeChaos.CameraRig` | Data, Player, Combat, Unity.Cinemachine |
| `DudeChaos.Combat` | Data, Core, Player |
| `DudeChaos.Vehicles` | Data, Core, Player, Combat |
| `DudeChaos.Wanted` | Data, Core, Player, Combat |
| `DudeChaos.Progression` | Data, Core, Player, Combat, Wanted |
| `DudeChaos.Save` | Data |
| `DudeChaos.Settings` | Data |
| `DudeChaos.World` | Data, Core |
| `DudeChaos.Npc` | Data, Core, World, City |
| `DudeChaos.City` | Data, Core, Player, Progression, Wanted |
| `DudeChaos.GameFlow` | Data, Core, Player, Save, Wanted, Progression, UI, Combat, City |
| `DudeChaos.UI` | Data, Core, Player, Player.Input, Combat, Wanted, Settings, Save, Progression, City, Vehicles, UnityEngine.UI, Unity.TextMeshPro |
| `DudeChaos.Audio` | — |
| `DudeChaos.Visuals` | Data, Core, City |
| `DudeChaos.Localization` | Data |
| `DudeChaos.Net` | Data, Core |
| `DudeChaos.Editor` | Data, Player, Player.Input, CameraRig, Npc, Combat, Vehicles, UI, Settings, Cinemachine, TMP, uGUI, InputSystem, URP core+runtime |
| `DudeChaos.Tests.PlayMode` | Data, Player, Combat, TestRunner |

**Two cycles that already happened here — avoid both:**
- `City → Progression → Wanted → City`: keep district-heat files **inside City** (`DistrictHeatRules`,
  `DistrictHeatBinder`) and let City reference Wanted, never the reverse.
- `Progression → GameFlow → Progression` (achievements needing side-job counts): GameFlow **calls into**
  Progression (`NotifySideJobs(int)`), Progression never knows GameFlow exists.

### 1.6 Data contracts

15 ScriptableObject types. Fourteen live in `Assets/Scripts/Data` (`DudeChaos.Data`); the fifteenth,
`DistrictDefinition`, lives in `Assets/Scripts/City` (`DudeChaos.City`) because the district bounds code
that reads it is there too:

| Type | Purpose | Key fields |
|---|---|---|
| `MovementSettings` | all locomotion tuning | deadZone 0.18, walk 3.2, run 6.0, sprint 8.8, sprintFovKick 5, coyote 0.12, jumpBuffer 0.10, gravity −22, jumpHeight ~1.35, landSoft > −8, landMedium −8..−14, landHard < −14, crouch height; `ClassifyLanding()` |
| `RagdollSettings` | ragdoll tuning | fallRagdollThreshold −16, highForceImpulseThreshold 55, recoveryVelocity 1.1, recoveryHoldTime 1.8, getUpDuration 0.9, totalMass 78, maxFullSimRagdolls 12, fullSimRadius 55, sleepVelocity 0.5, pelvisFollowLag 0.08; `ShouldRagdollOnLanding/Impulse` |
| `CombatSettings` | melee/shot/explosion feel | light 1200 force / 0.04 stop, heavy 3800 / 0.11, ragdolled ×1.6, backstab ×1.8 @ dot −0.35, muzzle light 0.055 s, recoil recovery 0.18, point blank 2.5 m, base shot impulse 45, up bias 0.25, explosion radius 6 / impulse 260 / damage 140 / buffer 64 |
| `VehicleSettings` | enter/exit/destruction | enter/exit 0.45 s, eject threshold 9 m/s, eject 900 forward + 420 up, exit offset 1.4 m, critical 25 %, fire 50 %, destruction delay 2.8 s, steering falloff 0.35, downforce 12 |
| `WantedSettings` | heat and response | thresholds {1,4,9,16,25,36}, cooldowns {6,9,12,16,20,25}, searchRadius 48, memory 9 s, 8 crime weights (below), units {2,3,4,5,6,6}, heli from 3, roadblock from 3, SWAT from 4, open fire from 2 |
| `WeaponData` | one asset per gun | damage, headMultiplier 3.2, limbMultiplier 0.75, fireRate, spread, recoil curve, range, magazine, reload, ragdoll multiplier, sounds/prefabs, weaponClass, isAutomatic, mechanicName, burstCount, chargeSeconds, projectileSpeed, selfDamage, heatPerShot |
| `VehicleData` | one asset per vehicle | name, handlingProfile, maxHealth, seats, animations, sounds, centerOfMassOffset, maxSpeed, acceleration, brakeForce, steerAngle |
| `MissionData` | one asset per mission | missionId, title, objectives[], rewardsMoney, rewardsReputation, startTrigger, completionConditions[], dialogueLines[3], timeLimitSeconds, failAtWantedLevel, retryable, timeBonusPerSecond |
| `EquipmentData` | attachment/consumable/support/upgrade/ammo | equipmentId, displayName, kind (0..4), slot (0..6), price, unlockLevel, valueA..D, allowedClasses[] |
| `ShopItem` | shop row | itemId, itemType (Weapon/Vehicle/Skin/Emote), price, unlockLevel, prefabReference |
| `DistrictDefinition` | one per district | districtId, displayNameKey, kind, centre (XZ), size (XZ), requiresPass, cameraHeightHint, densityMultiplier |
| `ImpactMaterialData` | surface VFX/audio | material type, impact prefab, decal, sound |
| `CameraShakeProfile` | shake curves | amplitude/frequency/duration |
| `EmoteData` | emote clip | id, clip, duration |
| `QualitySettingsPreset` | device tiers | shadow distance, particle budget, draw distance, streamer unload |

Interfaces in Data (never in a higher assembly): `IDamageable` (`BodyPart`, `DamageInfo`),
`ICameraPunch`. `IPlayerInput` and `IRagdollTargetSource` live in `DudeChaos.Player` so lower
assemblies can implement them without a cycle.

`CrimeType` weights (locked): AssaultCivilian 1, AssaultPolice 2, KillPolice 3, WeaponDischarge 1,
VehicleTheft 2, RunOver 2, Explosion 2, DestroyPoliceVehicle 3.

### 1.7 Locked gameplay numbers (do not "improve" silently)

**Movement:** dead zone 0.18 · walk 3.2 · run 6.0 · sprint 8.8 m/s · sprint FOV +5 · coyote 0.12 s ·
jump buffer 0.10 s · gravity −22 · land soft > −8, medium −8..−14, hard < −14 (hard = shake + fall
damage + forced ragdoll). Bound check: landing at exactly −8 is **medium**, not soft.

**Ragdoll:** total mass 78 kg · ≤ 12 full-sim bodies · full sim inside 55 m · recovery at average
velocity < 1.1 m/s for 1.8 s · get-up 0.9 s · camera follows pelvis with 0.08 s lag · activation
reasons: Bullet, Explosion, VehicleImpact, EjectedFromVehicle, HighFall, Melee, Scripted.
`ExitingVehicle` must be an allowed ragdoll source — ejecting while the state machine is still
`InVehicle` is rejected by the transition table and softlocks the player.

**Combat:** 10-step shot chain in order: input gate → fire-rate gate → ammo → spread ray →
hit resolve → damage by body part (head 3.2 / torso 1.0 / limb 0.75) → impulse + ragdoll →
VFX/audio pool → recoil/camera punch → hit-stop/UI event. Melee light 1200, heavy 3800, ×1.6 on a
ragdolled target, ×1.8 backstab. Explosion: one `OverlapSphereNonAlloc`, linear falloff.

**Vehicle:** clean exit < 9 m/s, eject ragdoll ≥ 9 m/s. Enter/exit are 0.45 s timer windows (animation
slots exist but no clips are required). Smoke → fire → explosion after 2.8 s. Arcade model: yaw
steering + forward force, no WheelCollider. `centerOfMassOffset` must be a real number in [−5, 5];
NaN/inf/larger is rejected at `Awake` and replaced with (0, −0.35, 0), because NaN into PhysX does not
throw — it kills `Simulate` and takes the editor with it.

**Wanted:** heat from the 8 crime weights; level from cumulative thresholds; cooldown only ticks when
out of sight **and** beyond 48 m; police memory 9 s; helicopter and roadblocks from level 3; SWAT from
4; police open fire from 2.

**Mission grading:** 100 points = 40 completion + 40 clock (full at par = 70 % of the limit, 0 at the
limit) + 20 clean (−10 per retry, −3 per wanted level, max −8 for health lost). 1 star = finish;
2 stars ≥ 70; 3 stars ≥ 85 **and** no retry **and** no wanted **and** inside par. Payout bonus: 2 stars
+10 %, 3 stars +25 % of the base reward only. A mission with no time limit tops out at 2 stars — that is
intentional (test: "no clock tops out at two stars").

**Day/night:** one game day = 24 real minutes (`hoursPerSecond = 1/60`). Sun peak 68° at 12:00.
Ambient 1.1 day → 0.28 at 03:00, never 0. Street lamps fade in 18:36, out 06:12 over half an hour.

**World:** 12 districts, 2.29 km², 64 m streaming cells (~560 cells), only 9 alive at once.

**Prop rendering:** ≤ 40 m full + shadows · 40–85 m simplified, no shadows · 85–130 m decals only ·
> 130 m hidden. Chunk cap 46 props; `BatchDrawCalls` ≤ 8 material groups per district.

### 1.8 System specs

**Player.** `PlayerState` has 14 states; `PlayerStateMachine` is plain C# with an explicit transition
table, ragdoll override rules and a vehicle lockout. `PlayerMotor` drives a `CharacterController`:
dead zone, walk/run/sprint blend, coyote time, jump buffer, gravity, landing tiers, crouch collider.
`HealthSystem` burns armour before health and raises `Depleted` (→ death ragdoll). Input goes through
`IPlayerInput`; the Input System adapter polls `Keyboard.current`/`Mouse.current`/`Gamepad.current`
directly (no `.inputactions` asset) and exposes `SetVirtualMove/SetVirtualLook` for the touch HUD.
`PlayerMotor.Awake` must self-resolve `GetComponent<IPlayerInput>()` — a runtime `Bind()` is not saved
into a scene, and a scene loaded from disk would otherwise have `_input == null` and never move.

**Ragdoll.** `RagdollController` activates on 7 reasons, toggles bone simulation, spreads mass, puts
bodies to sleep, runs the recovery timer, hands back to locomotion and handles death.
`RagdollBudgetModel` is the pure selection logic; `RagdollBudget` is one static per-frame driver (no
per-ragdoll `Update`). Budget reads each ragdoll's `Position` (pelvis), never `ReadBonePositions()[0]`
— a prefab without bones returns an empty array and throws every frame.
`PelvisFollowTarget` smooth-damps at 0.08 s.

**Camera.** `ThirdPersonCameraRig`: yaw pivot + clamped pitch, Cinemachine FOV kick (+5 sprint, blended),
`Punch` as an FOV pulse (Cinemachine Impulse is a later upgrade). `PlayerLookController` turns the camera
from mouse/stick. `RecoilApplier` puts the published recoil offset on the aim pivot with alternating yaw.

**Combat.** `WeaponController` runs the 10 steps and publishes events only (VFX/UI never referenced
directly). `WeaponMechanics` implements nine mechanics: Auto, Semi, Burst, Pump, Bolt, Charge, Beam,
Projectile, Throwable — burst 2..6 per pull, pump/bolt add cycle time, charge adds `chargeSeconds`,
throwables floor at 0.8 s, beam has no shots-per-pull and continuous heat, projectile/throwable do
self-damage at point blank, move-speed and noise multipliers per mechanic.
`AttachmentMount` + `AttachmentRules`: optic zoom, spread/recoil/magazine/noise modifiers, slot and
class restrictions. `BodyPartTag` maps bones → body part + damage target + impact material; the damage
target must be an assigned inspector reference (Unity cannot `GetComponentInParent<TInterface>()`).
`Explosion`, `MeleeController`, `DamageDummy`, `PrefabPool` (casings/muzzle flash), `HitStopDriver`
(unscaled time, yields to pause), `SupportRules` (UAV, strike, gunship, mortar, minefield, turret).

**Vehicles.** `VehicleController`: enter/exit timer windows, arcade drive, downforce, eject at speed,
input fallback chain `driverInput → motor → self → warning` (an empty `driverInput` must not leave the
car unable to accelerate). `VehicleHealth` implements `IDamageable` with smoke/fire/explosion stages.
`VehicleInteraction` binds E within 3.5 m using a static non-alloc buffer; it clears `_current` **only**
when `TryExit()` returns true. `VehicleLock`/`VehicleLockRules`: lock roll, hotwire duration, break-in
heat, `TryEnter` refuses while locked. `ArmourRules`: Unarmoured/Light/Heavy/Air with bullet/explosion/
ram multipliers, hotwire ban, speed penalty, police deterrence, heat while driving, effective health.
`NitroRules/System`, `TuningRules`.

**Wanted + police.** `WantedMath` is pure: level from heat (skip zero thresholds), cooldown gate,
cooling, response table. `WantedSystem` owns heat, stars, reporting, cooldown, events
`LevelChanged`/`CrimeReported`. `PoliceUnit` has LOS, straight-line chase and `MayOpenFire` from level 2.
`PoliceCar` is a Rigidbody arcade pursuer: yaw to target, brake when close, ram at speed, back out when
wedged, damage on contact. `PoliceSpawner` keeps unit and car counts matched to the level, despawns on
level drop. No NavMesh (editor-baked data cannot be produced headless).

**City.** `DistrictRules` (day/night density, police weight, restricted zones, interiors),
`DistrictBounds` (XZ rectangles, smallest containing wins so Secret sits inside Downtown),
`DistrictRegistry` (resolve on interval, `DistrictChanged`), `DayNightRules/Cycle` (clock, sun pitch,
ambient, one loop for all lamps), `InteriorPortal` (teleport to a separate interior island, pass locks,
disable `CharacterController` during the move), `HotelRules/Service` (4 tiers, 7 nights, 50 % refund,
bed spawn), `PropRules` (78 kinds, 8 groups, surface/footprint/height, per-chunk density by district,
chunk cap 46, render tiers, `BatchDrawCalls`), `SideMissionRules` (weighted by district and hour, night
pay, stable seeds), `DistrictHeatRules/Binder`.

**NPC + world.** `NpcSteering` (arrive braking, waypoints, separation, view cone, wander),
`NpcAvoidance` (two probes, turn to the clearer side, no jitter when both blocked), `NpcTickLod`
(tick rates 60/20/6/0 per second, `TickMultiplier`), `NpcSpawnMath` (density by distance, despawn
distance, deterministic ring, hard cap), `NpcSpawner` (follows streamer cells, pooled), `NpcAgent`
(walking `IDamageable`, dies at zero, resettable). `AnimalRules/Spawner/Agent` (species per district,
budgets, wander/flee speeds, fear radius, day/night, birds hover). `WorldStreamMath/Streamer` (cell
grid, resident ring, Chebyshev distance, load player cell + radius, unload rest on an interval).

**Progression.** `Wallet` (never negative) + `ShopRules`; `ShopSystem` (price/level/ownership gates,
persistence); `MissionRun` (Idle/Running/Completed/Failed, ordered objectives, clock, fail reasons
Timeout/PlayerDied/WantedTooHigh/Abandoned, 3 retries, time bonus capped at 34 %); `MissionSystem`
(uses `MissionRun`, counts the clock and death/wanted, events, `TryRetry`, `Clear`, `TimeLeft`,
`CurrentObjective`, `Retries`, grading, best-star persistence); `MissionModifierRules` (Time Attack
−40 % clock +15 %, Hot Zone fail one level earlier +30 %, Fragile double damage weight +20 %, only on a
mission already completed); `MissionGradeRules`; `MissionSelectRules` (unlock chain, progress %);
`MissionBanterRules` (line 0 intro, line 1 mid-run and per fail reason, line 2 payoff, remembers the
last three); `ProgressionCurve` (rep → level, 100 rep per level); `WeaponInventory`; `ConsumableRules`
(heal/armour, use time, carry limit, buffs, adrenaline crash); `DailyChallengeRules/System`
(3 deterministic dailies from 5 activity kinds, target scales with level ±10 %, streak 1.0→1.5 capped
at 5 days, 30 % full-set bonus, day override hook, midnight rollover); `AchievementRules/System`
(13 achievements, 10 read from existing save state, toast queue max 3, `Restore` then rescore);
`StuntRules/System` (slip angle, drift/airtime/near-miss points, combo multiplier, bank on settle,
lose on crash, points→money); `BountyRules/System` (pays on `CrimeReported`, diminishing inside 60 s,
floor 25 %, 1.5 s anti-double-pay).

`MissionRun` must count progress with its own `_nextIndex`, **not** `_done.Count` — a `HashSet` does
not grow when two objectives share a key, and the mission then never completes.

**Save.** `SaveData` format **4** (see fields below), `SaveIntegrity` (FNV-1a checksum, version gate,
validity, `Sanitise` post-load repair), `SaveSystem` (3 manual slots + autosave in `PlayerPrefs`,
corrupt-slot rejection, events). `SaveCoordinator` captures/applies the run and autosaves on pause/quit
(the only signal Android reliably delivers). A half-finished mission is deliberately not resumed.

Format-4 payload: position xyz + rotationY, health, armour, money, reputation, wantedHeat,
wantedLevel, unlockedItemIds[], inventoryItemIds[], currentMissionId, currentMissionProgress,
completedMissionIds[], hourOfDay, hotelNightsLeft, stuntsEarned, sideJobsCompleted, sideJobsFailed,
missionStars[] (encoded "id:stars"), dailyDayKey, dailyLastDayKey, dailyStreak, dailyProgress[3],
dailyBonusPaid, achievementIds[], dailiesCompleted, bestStreak, variantsCleared. Envelope:
`formatVersion`, `checksum`, `payload` (JSON string). Migrate v1→v2→v3→v4 by clamping every new field;
sanitise array lengths; `int.TryParse` everywhere (never `int.Parse` on save data — overflow throws
during load).

**Settings + UI + a11y.** `GameSettings` (quality preset, shadows, particles, draw distance 180, vSync,
target fps 60, master/music/sfx/ui volume, mute, joystick size/opacity/hand, look sensitivity, invert Y,
language EN/VI, haptics, show fps, textScale, colour-blind mode None/Protan/Deutan/Tritan, high
contrast) with `Clamp()` on every field; `SettingsStore` (load/save/apply/reset, `Changed`);
`QualityRules/Applier` (device tier from memory and cores, three presets, rolling frame-time adaptation).
UI: `HudController` (health/armour, stars, ammo, money, objective, hit marker, joystick), `MobileJoystick`,
`SafeAreaFitter` (anchors to `Screen.safeArea`, recompute on resize), `PauseMenuController`,
`MissionHud` (name, current objective, 2/3 progress, clock turns red ≤ 30 s, fail banner + retry hint,
star banner + missing-star hint for 5 s), `DailyChallengeHud`, `StuntHud`, `NitroHud`, `ConsumableHud`,
`SideMissionBoard`, `TuningPanel`, `AchievementToast`, `SettingsApplier`, `AccessibilityPalette`.

**Audio + visuals.** `AudioBudgetMath` (12 near / 8 mid / 4 far / 0 past 90 m, steal rule, falloff,
pitch jitter), `AudioDirector` (fixed pool that never grows, refuses silent voices, steals only barely
audible ones, one 2D UI source), `DriftAudio` (loop volume from slip angle + crash one-shots with
cooldown). `AnimationMath` (walk pose, wheel rotation, siren timing, neon hash, headlight brightness),
`ProceduralWalker` (humanoid from real speed, no clips, skips missing bones), `WheelSpin`, `SirenController`,
`NeonFlicker` (per-sign deterministic), `VehicleLights` (headlights at night, brake, hazards).

**Net (scaffold only).** `NetworkCommand` (fixed 32-byte client command, hand-rolled serialisation),
`AuthorityValidator` (speed hack, teleport, fire-rate abuse, damage claims, stale ticks),
`INetworkTransport` + `LoopbackTransport`, `FusionNetworkTransport` behind `#if PHOTON_FUSION` and a
documented "never compiled against the real package" status.

### 1.9 Render pipeline, lighting, camera and GPU budget

Rendering is where a rebuild fails silently: a pink scene, a flat city at night, a phone on its knees.
Every value below was read out of the shipped project (`Assets/Settings/Mobile_URP.asset`,
`Mobile_Renderer.asset`, `Assets/UniversalRenderPipelineGlobalSettings.asset`,
`ProjectSettings/QualitySettings.asset`, `ProjectSettings/GraphicsSettings.asset`,
`Assets/Editor/ProjectSetup.cs`, `Assets/Editor/SceneBootstrap.cs`, `Assets/Scripts/City/DayNightCycle.cs`,
`DayNightRules.cs`, `PropRules.cs`, `Assets/Scripts/Settings/QualityRules.cs`, `QualityApplier.cs`,
`SettingsStore.cs`, `GameSettings.cs`, `Assets/Scripts/Camera/ThirdPersonCameraRig.cs`). None of it was
measured on a device:
these are the contract's numbers, not a benchmark.

#### 1.9.1 Pipeline assets (the pink-screen gate)

Two committed assets: `Assets/Settings/Mobile_Renderer.asset` (`UniversalRendererData`) and
`Assets/Settings/Mobile_URP.asset` (`UniversalRenderPipelineAsset`). `ProjectSetup.CreatePipeline()`
(menu `DudeChaos → Setup → 1`) creates them if missing, assigns the pipeline to
`GraphicsSettings.defaultRenderPipeline` **and** to `QualitySettings.renderPipeline` for every quality
level, then logs: *"Anything pink is now a missing material, not a missing pipeline."* That sentence is
the fastest way to triage a pink scene.

| Key | Shipped asset value | Meaning |
|---|---|---|
| Render mode | `m_RenderingMode: 0` (in `Mobile_Renderer`) | Forward, not Forward+ (2) and not Deferred (1) |
| Renderer features | none | no SSAO, no decals, no render-object passes |
| MSAA | `m_MSAA: 1` | 1× in the file, even though `CreatePipeline` writes 4× |
| HDR | `m_SupportsHDR: 1` | on in the file, even though `CreatePipeline` writes off |
| Render scale | `m_RenderScale: 1` | no dynamic resolution, no FSR |
| Main light | shadows on, 2048 map, `m_MainLightRenderingMode: 1` | one shadow-casting sun |
| Additional lights | 4 per object, `m_AdditionalLightShadowsSupported: 0` | lamps light geometry, never cast shadows |
| Shadows | distance 50 m, 1 cascade, soft shadows off, depth/normal bias 1 | cheapest mobile shadow path |
| Depth / opaque texture | both off | nothing that needs them may be added silently |
| Batching | SRP Batcher on, dynamic batching off | bad materials break batching, not the code |
| Volume | pipeline `m_VolumeProfile` is null | the global default profile registered in `UniversalRenderPipelineGlobalSettings.asset` is the only one that runs, see 1.9.6 |

**Rule:** the committed asset is the authority. Rebuild, run `Setup → 1`, then diff MSAA, HDR, shadow
distance and render scale against this table before touching gameplay. In URP the MSAA sample count is
read from the pipeline asset, so `QualitySettings.antiAliasing` (which `QualityApplier` writes per tier)
does nothing unless the rebuild also writes `pipeline.msaaSampleCount` — either make that write or drop
the per-tier MSAA claim.

`ProjectSettings/QualitySettings.asset` ships **6 editor levels** (Very Low, Low, Medium, High, Very High,
Ultra; current level 5), all six with `customRenderPipeline` pointing at `Mobile_URP`. Those levels are
not the runtime tiers in 1.9.2 — do not wire one to the other and call it done.

#### 1.9.2 Runtime quality tiers (3, decided by the phone)

`QualityRules` picks a tier from RAM and core count, never from the device name (names lie).

| Rule | Value |
|---|---|
| High tier | memory ≥ 6000 MB **and** cores ≥ 8 |
| Low tier | memory < 3000 MB **or** cores < 4 |
| Medium | everything else |
| Frame targets | low/mid 30 fps, high 60 fps; 30 fps is the floor before dropping a tier |

| Field | Tier 0 (low) | Tier 1 (mid) | Tier 2 (high) |
|---|---|---|---|
| Shadow distance | 22 m, shadows **off** | 45 m, shadows on | 70 m, shadows on |
| LOD bias | 0.6 | 1.2 | 2.0 |
| Anti-aliasing | 0× | 2× | 4× |
| Target fps | 30 | 30 | 60 |
| Fog density | 0.012 | 0.009 | 0.006 |
| NPC budget | 12 | 24 | 40 |

`QualityApplier` measures an exponential moving average of frame time and steps **one tier** at a time:
down after 4 s of slow frames, up after 20 s of held frames, with 15 % tolerance above the
`1000 / targetFps` budget so one hitch cannot demote a device. It writes `QualitySettings.shadows`,
`shadowDistance`, `lodBias`, `antiAliasing`, `targetFrameRate` — and nothing else (see 1.9.3). Those two
globals are owned **exclusively** by it: `SettingsStore.Apply` and `SettingsApplier.Apply` call
`QualityApplier.SetPlayerSettings(shadows, shadowDistance, targetFps)` instead of writing them, and
`QualityRules.ShadowDistanceFor` / `TargetFrameRateFor` clamp the player's choice so it can only lower
cost, never raise it above the tier (R6). Every playable scene therefore needs **exactly one**
`QualityApplier`, on a `Systems` object the scene builder creates (`SceneBootstrap.CreateSystems`) next to
the `SettingsStore` that feeds it and the `SettingsApplier` (wired to that store) that pushes the rest of a
snapshot — audio, vSync, camera far clip, HUD, joystick — and re-applies on every change; a scene without
the applier writes no quality globals at all, so it is part of the scene contract, not an optional extra.

The contract is **checked, not assumed**. `SceneBootstrap.CheckSystems` runs on every scene the builder
saves and logs one error per fault; `DudeChaos → Verify Systems In Built Scenes` re-checks every built
scene; and the batch entry point exits non-zero so CI cannot ship a scene whose settings do nothing:

```
Unity -batchmode -quit -projectPath . \
  -executeMethod DudeChaos.EditorTools.SceneBootstrap.VerifyBuiltScenesBatch
```

The rule itself — exactly one of each component, all on one GameObject named `Systems`, `store` wired to
the scene's store — lives in plain C# in `Assets/Scripts/Settings/SceneSystemsContract.cs`, so LogicTests
cover it without an editor. A run that finds no built scene at all also fails: a verifier that passes by
checking nothing is worse than none.

#### 1.9.3 Fog is computed and never applied

`QualityPreset.FogDensity` exists per tier (0.012 / 0.009 / 0.006) and **no code writes it to
`RenderSettings`** — a grep for `RenderSettings.fog` in the shipped project returns nothing. The rebuild
must apply it in `QualityApplier.Apply` (`RenderSettings.fog = true`, one fog mode picked once,
`RenderSettings.fogDensity = preset.FogDensity`) and assert it in LogicTests; otherwise the far city pops
in at the 130 m tier cut-off with no atmospheric falloff. Fog must stay consistent with the prop render
tiers in `PropRules.RenderTier` (full ≤ 40 m, simplified 40–85 m, flat decals ≤ 130 m, hidden past 130 m).

#### 1.9.4 Lighting (day/night is code, not baked)

| Thing | Value |
|---|---|
| Bootstrap scene sun | directional, intensity 1.1, **soft** shadows, rotation Euler(50, −35, 0) |
| Runtime clock | starts 09:00, `hoursPerSecond = 1/60` → a full day every 24 real minutes |
| Sun pitch | `DayNightRules.SunPitch(hour)`, noon elevation 68° |
| Sun rotation | Euler(pitch, **35**, 0) — the yaw sign differs from the bootstrap's −35; cosmetic, pick one |
| Sun intensity | `clamp01(pitch / 68) * 1.2`, disabled when pitch ≤ −2 |
| Sun colour | lerp dusk (1, 0.55, 0.3) → day (1, 0.96, 0.88) over `pitch / 30` |
| Ambient | `RenderSettings.ambientLight = lerp(nightAmbient (0.08, 0.09, 0.14), white, AmbientFor(hour))` |
| Street lamps | one component owns the `Light[]`, sets intensity `2.4 * LightIntensity(hour)`, disables each lamp below 0.01 |

Two guards. First, `RenderSettings.ambientLight` only has an effect with **Ambient Mode = Color** (or a
gradient); leave the bootstrap scene's Environment → Ambient Source at Color, otherwise the night ambient
silently does nothing and the city stays daylight-bright at 23:00. Second, no lamp may carry its own
`Update` — one array in one component is the whole reason the night city is affordable.

Additional-light shadows are off in the pipeline, so lamps light geometry without casting shadows. That is
a deliberate mobile trade, not a bug to "fix" with 20 shadow-casting point lights.

#### 1.9.5 Camera

| Item | Value |
|---|---|
| Main Camera | FOV 60, near 0.05, far 500, `CinemachineBrain`, `AudioListener` on the same object |
| Virtual camera | `CinemachineCamera` child of the player's pitch arm at local (0, 0, −4.5) |
| Base FOV | read from the lens in `Awake`; Cinemachine 3 `Lens` is a struct — copy, edit, write back |
| Sprint kick + punch | 1.6° per unit of punch strength, 0.09 s decay, strength capped at 3 |
| Shadow distance (settings) | `GameSettings.shadowDistance`, metres, default **0 = the tier's own distance**, clamped 0–100; a value can only shorten the tier's 22/45/70 m, never lengthen it. It is a different setting from draw distance and must not be fed from it |
| Draw distance (settings) | `GameSettings.drawDistance` defaults to 180 m, clamped 60–400; the `QualitySettingsPreset.drawDistance` tooltip lists Low 120 / Medium 180 / High 250 / Ultra 350 — a four-row sheet, not the three runtime tiers. It is the camera far clip / streaming radius and has **nothing** to do with shadow distance |

The bootstrap scene sets the far plane to 500 m and `SettingsApplier.Apply` lowers it to the draw
distance setting through `QualityRules.FarClipFor`, clamped to 60–500 m, so the slider can never push past
the 500 m the scene was built for. Fog and the prop tiers still do the hiding: at a 120 m draw distance the
props beyond that are already gone (`PropRules`), so the shorter far plane clips nothing that is drawn. `virtualCamera.Lens.FieldOfView = x` compiles and does nothing — always assign a
modified copy back to `.Lens`.

#### 1.9.6 Materials, shaders, post-processing

- The project ships **no gameplay `.mat` and no custom `.shader`**: the only ones on disk are TextMesh
  Pro's packaged font materials and shaders, and every world renderer uses Unity's default material.
  Art replacement is phase P11; gameplay code must never depend on a named material or shader.
- Neon, sirens and brake lights drive `_EmissionColor` through a `MaterialPropertyBlock`. On a default,
  non-emissive material that produces **no visible glow** — URP/Lit needs the `_EMISSION` keyword and an
  emission-capable material. This is why the night city looks flat until real materials exist: the code is
  correct, the material is missing.
- Post-processing: the URP global settings point at `Assets/DefaultVolumeProfile.asset`, which has **zero**
  volume components. No bloom, no tonemapping, no vignette, no colour grading — the shipped game is the raw
  forward render. If the rebuild adds effects: one global `Volume`, one profile, bloom and SSAO off on tier
  0, and a note in `PERFORMANCE_BUDGET.md` with a before/after capture. Do not add a `Volume` per scene
  and call it art direction.

#### 1.9.7 GPU budget the renderer must hold

From `design/PERFORMANCE_BUDGET.md` (budgets, not measurements): frame ≤ 33 ms · draw calls ≤ 150 ·
set-pass calls ≤ 200 · triangles ≤ 250k · texture memory ≤ 350 MB · GC allocation 0 B/frame after warmup ·
cold start ≤ 6 s. What makes the draw-call number reachable is prop dressing, and those caps live in
`PropRules`, not in that document: 40 m chunk size, hard cap 46 props per chunk, 8 `PropGroup` values with
one batch per group per district, and render tiers of full/shadowed ≤ 40 m, simplified without shadows
40–85 m, flat decals ≤ 130 m, hidden past that. Check SRP Batcher compatibility in the Frame Debugger, not
by faith: the usual cause of 400 draw calls is 40 one-off materials.

#### 1.9.8 Known mismatches in the shipped project (accept or fix, do not rediscover)

| # | Mismatch | Consequence | Decision for the rebuild |
|---|---|---|---|
| R1 | `CreatePipeline` writes MSAA 4× / HDR off; the committed asset has 1× / HDR on | the setup log lies about what ships | treat the asset as truth, log the diff, keep 1× MSAA |
| R2 | `QualityApplier` writes `QualitySettings.antiAliasing`, URP reads `msaaSampleCount` | per-tier AA is a no-op | write `pipeline.msaaSampleCount` too, or stop claiming per-tier AA |
| R3 | `FogDensity` computed per tier, never applied | no atmospheric distance cue | apply it in `QualityApplier.Apply` (1.9.3) |
| R4 | Day-night yaw +35, bootstrap sun yaw −35 | shadows shift a few degrees on the first frame | pick one number |
| R5 | Emissive `_EmissionColor` on default materials | neon and sirens do not glow | supply emissive materials in P11; keep the code |
| R6 | four scripts wrote `shadowDistance` / `targetFrameRate` (`QualityApplier.Apply`, `SettingsStore.Apply`, `SettingsApplier.Apply`, `GameManager.Awake`/`ApplyQuality`); the last to run won | the tier was silently replaced by 180 m shadows and a 60 fps target on every device | **already fixed in this project, rebuild it this way**: `QualityApplier` is the only writer, settings push through `QualityApplier.SetPlayerSettings`, and `QualityRules.ShadowDistanceFor` / `TargetFrameRateFor` allow the player to lower cost but never raise it above the tier |

#### 1.9.9 How to prove it (needs the editor; the handoff machine cannot do this)

1. `DudeChaos → Setup → 1`, then open any scene: nothing pink, the console shows the pipeline log line.
2. Frame Debugger: SRP Batcher compatible shaders on the props; draw calls ≤ 150 in the slice.
3. Call `QualityApplier.Apply(0/1/2)` and watch `shadowDistance`, `lodBias`, `antiAliasing` and
   `Application.targetFrameRate` change in the inspector; fog density only after the 1.9.3 fix exists.
   Then change the settings (shadows off, draw distance 400, 144 fps): shadows go to 0, the shadow
   distance may only fall, and the frame target may only fall — the tier is not replaced.
4. `DayNightCycle.SetHour(9)` then `SetHour(23)`: sun intensity and colour, ambient colour and lamp
   intensity all move; the clock text reaches the HUD.
5. `DudeChaos → Verify Systems In Built Scenes` (or the batch method in 4.7): every built scene passes.
   It fails, by design, for a scene built before the systems object existed — rebuild first. Then delete
   `SettingsStore` from a copy of the scene and run it again: it must fail and name the missing piece.
6. On a device only: Profiler frame time, draw calls, set-pass, triangles, texture memory against 1.9.7.

Everything in 1.9.9 is **NOT RUN** until someone watches it. A rebuilt project that compiles proves none of
this.

### 1.10 Definition of done

A phase is done when **all four** are true:
1. `dotnet build Tools/CompileCheck/CompileCheck.csproj` → 0 errors, 0 warnings. This covers
   `Assets/Scripts` only; editor scripts stay unproven until Unity opens the project (4.1).
2. `dotnet run --project Tools/LogicTests/LogicTests.csproj` → all pass. The harness held **1028
   assertions** when this was written (`PASSED 1028  FAILED 0`); the number only ever grows, so treat the
   current run's own count as the truth, not this line.
3. `python3 Tools/audit/audit.py`, `balance.py`, `reference.py` → 0 findings.
4. `CHECKPOINT.md` updated with what was added, what was proven, and what is `NOT RUN`.

And the final report must separate: **proven offline**, **proven in Unity**, **not run**. A green
offline build is not a Unity compile; a stub can be more permissive than the real API.
## 2. CONTENT PACK (real numbers, extracted from the shipped assets)

The receiving agent must generate these as ScriptableObject `.asset` files with **fixed GUIDs**, not as
runtime constants. Tables below are the source of truth; do not re-balance them.

### 2.1 Weapons — 48 assets, `Assets/Data/Weapons/WPN_*.asset`

`headMultiplier 3.2`, `limbMultiplier 0.75` on all. Empty mechanic = class default (Pistol Semi, SMG
Auto, Rifle Auto, Shotgun Pump, Sniper Bolt, Heavy Auto, Melee Semi). Damage is per pellet/hit at torso.

| Name | Class | Dmg | Fire rate (s) | Mag | Range (m) | Mechanic | Auto | Charge | Burst |
|---|---|---|---|---|---|---|---|---|---|
| Anti-Tank Rifle | Sniper | 300 | 2.8 | 1 | 350 | Bolt | no | 0 | 1 |
| C4 Charge | Heavy | 300 | 1.8 | 1 | 6 | Throwable | no | 0.6 | 1 |
| Railgun | Heavy | 260 | 2.4 | 1 | 300 | Charge | no | 1.8 | 1 |
| Rocket Launcher | Heavy | 200 | 1.6 | 1 | 120 | class | no | None | None |
| Mortar Tube | Heavy | 190 | 2.2 | 1 | 180 | Projectile | no | 0.9 | 1 |
| Frag Grenade | Heavy | 160 | 1.6 | 1 | 22 | Throwable | no | 0.5 | 1 |
| Anti-Materiel | Sniper | 120 | 1.4 | 5 | 400 | class | no | None | None |
| Grenade Launcher | Heavy | 120 | 1 | 6 | 80 | class | no | None | None |
| Slug Shotgun | Shotgun | 96 | 0.8 | 5 | 35 | Pump | no | 0 | 1 |
| Charge Rifle | Rifle | 95 | 1.2 | 5 | 200 | Charge | no | 1.1 | 1 |
| Bolt Sniper | Sniper | 95 | 1.15 | 5 | 300 | class | no | None | None |
| Pump Shotgun | Shotgun | 88 | 0.85 | 8 | 22 | class | no | None | None |
| Semi Sniper | Sniper | 78 | 0.65 | 10 | 250 | class | no | None | None |
| Arc Caster | Heavy | 70 | 0.9 | 8 | 60 | Charge | no | 0.7 | 1 |
| Molotov | Heavy | 70 | 1.4 | 1 | 18 | Throwable | no | 0.4 | 1 |
| Auto Shotgun | Shotgun | 62 | 0.45 | 10 | 20 | class | no | None | None |
| Sledgehammer | Melee | 62 | 0.75 | 0 | 2.2 | class | no | None | None |
| Revolver .44 | Pistol | 45 | 0.55 | 6 | 55 | class | no | None | None |
| Double Barrel | Shotgun | 44 | 0.35 | 2 | 16 | Semi | no | 0 | 1 |
| Katana | Melee | 40 | 0.38 | 0 | 2.4 | class | no | None | None |
| Airburst Rifle | Rifle | 40 | 1.1 | 12 | 150 | Burst | no | 0 | 2 |
| DMR | Rifle | 38 | 0.28 | 20 | 180 | class | no | None | None |
| Battle Rifle | Rifle | 34 | 0.12 | 25 | 150 | class | yes | None | None |
| Crowbar | Melee | 34 | 0.42 | 0 | 2.2 | class | no | None | None |
| Laser Rifle | Rifle | 34 | 0.09 | 60 | 90 | Beam | yes | 0 | 0 |
| Heavy Pistol | Pistol | 32 | 0.28 | 10 | 45 | class | no | None | None |
| AK Rifle | Rifle | 32 | 0.11 | 30 | 120 | class | yes | None | None |
| Sawn-off | Shotgun | 30 | 0.7 | 2 | 18 | class | no | None | None |
| LMG | Heavy | 26 | 0.09 | 100 | 90 | class | yes | None | None |
| Knife | Melee | 26 | 0.32 | 0 | 1.8 | class | no | None | None |
| Carbine | Rifle | 26 | 0.09 | 30 | 130 | class | yes | None | None |
| Chainsaw | Melee | 24 | 0.12 | 0 | 2 | Beam | yes | 0 | 0 |
| Burst Carbine | Rifle | 22 | 0.85 | 30 | 120 | Burst | no | 0 | 3 |
| 9mm Pistol | Pistol | 22 | 0.22 | 12 | 60 | class | no | None | None |
| Nailgun | Heavy | 20 | 0.07 | 50 | 40 | Projectile | yes | 0 | 1 |
| Flare Gun | Pistol | 18 | 1.1 | 1 | 45 | Projectile | no | 0 | 1 |
| PDW | SMG | 18 | 0.075 | 35 | 60 | class | yes | None | None |
| Baseball Bat | Melee | 18 | 0.45 | 0 | 2.5 | class | no | None | None |
| Compact Pistol | Pistol | 16 | 0.16 | 15 | 35 | class | no | None | None |
| Combat Shotgun | Shotgun | 16 | 0.22 | 8 | 30 | class | yes | None | None |
| Flamethrower | Heavy | 16 | 0.06 | 120 | 14 | Projectile | yes | 0 | 1 |
| Vector SMG | SMG | 14 | 0.075 | 30 | 45 | class | yes | None | None |
| Stun Baton | Melee | 14 | 0.55 | 0 | 2 | Semi | no | 0 | 1 |
| Minigun | Heavy | 14 | 0.045 | 150 | 90 | class | yes | None | None |
| Machine Pistol | Pistol | 12 | 0.07 | 18 | 30 | class | yes | None | None |
| Uzi | SMG | 11 | 0.065 | 25 | 35 | class | yes | None | None |
| Micro SMG | SMG | 9 | 0.05 | 20 | 30 | class | yes | None | None |
| Taser | Pistol | 6 | 0.9 | 2 | 9 | Semi | no | 0 | 1 |

Melee assets carry `magazineSize 0` and must never reload.

### 2.2 Vehicles — 18 assets, `Assets/Data/Vehicles/VEH_*.asset`

`handlingProfile`: 0 Sports, 1 Muscle, 2 Truck, 3 Bike. `centerOfMassOffset` is a **number** in
[−5, 5] and differs by body type: bikes −0.25, sports −0.30, sedans −0.35, muscle −0.45, vans/trucks
−0.50, APC −0.55. Aircraft (helicopter, light jet) carry the **Bike** profile (index 3) in the shipped
data — keep it unless you rebalance handling for all three at once.

| Name | Profile | HP | Seats | Top (m/s) | Accel | Brake | Steer |
|---|---|---|---|---|---|---|---|
| Armoured Carrier | Truck | 4800 | 6 | 26 | 8 | 30 | 24 |
| Ambulance | Truck | 1800 | 4 | 28 | 9 | 30 | 27 |
| Anti-Air Truck | Truck | 3600 | 4 | 24 | 8 | 30 | 26 |
| Sport Bike | Bike | 400 | 2 | 45 | 22 | 32 | 25 |
| Street Bike | Bike | 450 | 2 | 42 | 20 | 30 | 26 |
| City Bus | Truck | 2200 | 8 | 22 | 7 | 26 | 22 |
| Helicopter | Bike | 2600 | 3 | 46 | 14 | 30 | 30 |
| Light Jet | Bike | 1800 | 1 | 55 | 22 | 32 | 28 |
| Rocket Truck | Truck | 3200 | 3 | 22 | 7 | 28 | 22 |
| Monster Truck | Truck | 2000 | 4 | 30 | 12 | 30 | 34 |
| Muscle GT | Muscle | 1000 | 2 | 36 | 16 | 24 | 34 |
| Police Cruiser | Sports | 1100 | 4 | 38 | 16 | 30 | 36 |
| Sedan | Sports | 900 | 4 | 33 | 14 | 26 | 38 |
| Sport Coupe | Sports | 850 | 2 | 39 | 17 | 28 | 36 |
| Tank | Truck | 12000 | 3 | 18 | 5 | 28 | 18 |
| Taxi | Sports | 850 | 4 | 30 | 13 | 26 | 36 |
| Pickup Truck | Truck | 1400 | 4 | 25 | 10 | 30 | 30 |
| Cargo Van | Truck | 1600 | 6 | 22 | 8 | 28 | 28 |

Speed bands that the balance checker enforces: cars 25–45, trucks/vans 18–32, bikes 38–60, APC 18–30,
tank 12–21, helicopter 40–58, light jet 45–60. Nothing exceeds 60 m/s — that is the streaming-chunk
ceiling, not a real-world figure.

### 2.3 Equipment — 59 assets, `Assets/Data/Equipment/EQ_*.asset`

`kind`: 0 Attachment, 1 Consumable, 2 Support, 3 Upgrade, 4 Ammo. `slot` (attachments only):
1 Optic, 2 Muzzle, 3 Underbarrel, 4 Magazine, 5 Stock, 6 Laser. `valueA..D` meaning depends on kind:

| Kind | valueA | valueB | valueC | valueD |
|---|---|---|---|---|
| Optic | zoom | — | — | — |
| Muzzle | recoil multiplier | noise multiplier | — | — |
| Underbarrel | — | recoil/spread multiplier | grenade count | — |
| Magazine | extra rounds | — | reload multiplier | — |
| Stock | — | recoil multiplier | — | — |
| Laser | — | spread multiplier | — | — |
| Consumable | heal | armour | use seconds | carry limit |
| Support | support number 1 | 2 | 3 | 4 (mirrors `SupportRules`) |
| Upgrade | +5 per level | — | — | — |
| Ammo | damage multiplier | pierce count | — | — |

Convention used by the shipped assets: only the optic's `valueA` is read by `AttachmentRules`; the rule
classes (`ConsumableRules`, `SupportRules`, `TuningRules`, ammo handling in `WeaponController`) are the
authority, so mirror them instead of inventing a second meaning.

| Asset id | Display name | Kind | Slot | Price | Level | valueA / B / C / D |
|---|---|---|---|---|---|---|
| `eq_ammo_12g` | 12 Gauge Shells | Ammo | - | 240 | 1 | 1.25 / 1 / 0 / 0 |
| `eq_ammo_338` | 338 Rounds | Ammo | - | 600 | 3 | 1.6 / 1 / 0 / 0 |
| `eq_ammo_45` | 45 ACP Rounds | Ammo | - | 180 | 1 | 1.15 / 1 / 0 / 0 |
| `eq_ammo_556` | 5.56 Rounds | Ammo | - | 260 | 2 | 1.2 / 1 / 0 / 0 |
| `eq_ammo_762` | 7.62 Rounds | Ammo | - | 320 | 2 | 1.35 / 1 / 0 / 0 |
| `eq_ammo_9mm` | 9mm Rounds | Ammo | - | 120 | 1 | 1 / 1 / 0 / 0 |
| `eq_ammo_ap` | AP Rounds | Ammo | - | 800 | 3 | 1.3 / 2 / 0 / 0 |
| `eq_ammo_hp` | Hollow Point | Ammo | - | 700 | 2 | 1.45 / 0.9 / 0 / 0 |
| `eq_ammo_rubber` | Rubber Rounds | Ammo | - | 200 | 1 | 0.4 / 1 / 0 / 0 |
| `eq_ammo_tracer` | Tracer Rounds | Ammo | - | 450 | 2 | 1 / 1.1 / 0 / 0 |
| `eq_con_adrenaline` | Adrenaline | Consumable | - | 300 | 2 | 0 / 1.25 / 8 / 2 |
| `eq_con_antibiotic` | Antibiotics | Consumable | - | 260 | 2 | 25 / 15 / 2.2 / 3 |
| `eq_con_bandage` | Bandage | Consumable | - | 60 | 1 | 20 / 0 / 1.2 / 8 |
| `eq_con_energy` | Energy Drink | Consumable | - | 120 | 1 | 0 / 1.15 / 12 / 4 |
| `eq_con_food` | Street Food | Consumable | - | 80 | 1 | 15 / 0 / 1.8 / 6 |
| `eq_con_medkit` | Medkit | Consumable | - | 350 | 1 | 75 / 25 / 2.8 / 3 |
| `eq_con_painkiller` | Painkillers | Consumable | - | 150 | 1 | 30 / 0 / 0.8 / 4 |
| `eq_las_flashlight` | Weapon Light | Attachment | Laser | 350 | 1 | 0 / 1 / 0 / 0 |
| `eq_las_laser` | Laser Sight | Attachment | Laser | 500 | 1 | 0 / 0.9 / 0 / 0 |
| `eq_mag_drum` | Drum Magazine | Attachment | Magazine | 1500 | 3 | 35 / 0 / 0 / 0 |
| `eq_mag_extended` | Extended Magazine | Attachment | Magazine | 600 | 1 | 10 / 0 / 0 / 0 |
| `eq_mag_fast` | Fast Magazine | Attachment | Magazine | 900 | 2 | 0 / 0 / 0.75 / 0 |
| `eq_muz_brake` | Muzzle Brake | Attachment | Muzzle | 600 | 1 | 0.7 / 1 / 0 / 0 |
| `eq_muz_compensator` | Compensator | Attachment | Muzzle | 700 | 1 | 0.75 / 1 / 0 / 0 |
| `eq_muz_flash_hider` | Flash Hider | Attachment | Muzzle | 400 | 1 | 0.95 / 1 / 0 / 0 |
| `eq_muz_heavy_suppressor` | Heavy Suppressor | Attachment | Muzzle | 2600 | 3 | 0.4 / 0.2 / 0 / 0 |
| `eq_muz_suppressor` | Suppressor | Attachment | Muzzle | 1800 | 2 | 0.45 / 0.35 / 0 / 0 |
| `eq_opt_18x` | 18x Scope | Attachment | Optic | 5200 | 4 | 18 / 0 / 0 / 0 |
| `eq_opt_3x` | 3x Scope | Attachment | Optic | 900 | 1 | 3 / 0 / 0 / 0 |
| `eq_opt_4x` | 4x Scope | Attachment | Optic | 1400 | 2 | 4 / 0 / 0 / 0 |
| `eq_opt_8x` | 8x Scope | Attachment | Optic | 2600 | 3 | 8 / 0 / 0 / 0 |
| `eq_opt_red_dot` | Red Dot Sight | Attachment | Optic | 350 | 1 | 1.2 / 0 / 0 / 0 |
| `eq_opt_thermal` | Thermal 4x | Attachment | Optic | 6500 | 4 | 4 / 0 / 0 / 0 |
| `eq_stk_heavy` | Heavy Stock | Attachment | Stock | 1100 | 2 | 0 / 0.65 / 0 / 0 |
| `eq_stk_light` | Light Stock | Attachment | Stock | 450 | 1 | 0 / 0.85 / 0 / 0 |
| `eq_stk_tactical` | Tactical Stock | Attachment | Stock | 700 | 1 | 0 / 0.75 / 0 / 0 |
| `eq_sup_airstrike` | Airstrike | Support | - | 4200 | 2 | 0 / 180 / 18 / 0 |
| `eq_sup_ammo_drop` | Ammo Drop | Support | - | 400 | 1 | 0 / 0 / 30 / 0 |
| `eq_sup_anti_air` | Anti-Air Gun | Support | - | 5000 | 3 | 60 / 150 / 0 / 0 |
| `eq_sup_armoured_car` | Armoured Car | Support | - | 3500 | 3 | 75 / 0 / 0 / 0 |
| `eq_sup_gunship` | Gunship | Support | - | 7500 | 4 | 60 / 90 / 40 / 0 |
| `eq_sup_minefield` | Minefield | Support | - | 1200 | 1 | 180 / 220 / 16 / 0 |
| `eq_sup_mortar` | Mortar Team | Support | - | 3800 | 3 | 0 / 100 / 12 / 0 |
| `eq_sup_rocket_artillery` | Rocket Artillery | Support | - | 6500 | 4 | 0 / 240 / 22 / 0 |
| `eq_sup_tank` | Tank Support | Support | - | 9000 | 5 | 90 / 200 / 0 / 0 |
| `eq_sup_turret` | Auto Turret | Support | - | 2600 | 2 | 120 / 45 / 30 / 0 |
| `eq_sup_uav` | UAV Scan | Support | - | 800 | 1 | 25 / 120 / 0 / 0 |
| `eq_sup_uav_jammer` | Radio Jammer Drone | Support | - | 1600 | 2 | 0 / 0 / 20 / 0 |
| `eq_sup_uav_strike` | Strike Drone | Support | - | 2200 | 2 | 0 / 0 / 0 / 0 |
| `eq_und_angled_grip` | Angled Grip | Attachment | Underbarrel | 450 | 1 | 0 / 0.85 / 0 / 0 |
| `eq_und_bipod` | Bipod | Attachment | Underbarrel | 800 | 2 | 0 / 0.7 / 0 / 0 |
| `eq_und_grenade_launcher` | Underbarrel Launcher | Attachment | Underbarrel | 3200 | 4 | 0 / 1 / 1 / 0 |
| `eq_und_vertical_grip` | Vertical Grip | Attachment | Underbarrel | 500 | 1 | 0 / 0.8 / 0 / 0 |
| `eq_upg_armour` | Body Armour | Upgrade | - | 1200 | 2 | 5 / 0 / 0 / 0 |
| `eq_upg_brakes` | Brake Upgrade | Upgrade | - | 1200 | 1 | 5 / 0 / 0 / 0 |
| `eq_upg_engine` | Engine Tuning | Upgrade | - | 1200 | 1 | 5 / 0 / 0 / 0 |
| `eq_upg_nitro` | Nitro Kit | Upgrade | - | 1200 | 2 | 5 / 0 / 0 / 0 |
| `eq_upg_tires` | Tire Upgrade | Upgrade | - | 1200 | 1 | 5 / 0 / 0 / 0 |
| `eq_upg_weight` | Weight Reduction | Upgrade | - | 1200 | 3 | 5 / 0 / 0 / 0 |

### 2.4 Shop — 25 assets, `Assets/Data/Shop/`

| itemId | Type | Price | Unlock level |
|---|---|---|---|
| `emote_dance` | Emote | 600 | 1 |
| `emote_laugh` | Emote | 400 | 1 |
| `skin_gold` | Skin | 3000 | 2 |
| `skin_shadow` | Skin | 3500 | 2 |
| `skin_military` | Skin | 30000 | 19 |
| `vehicle_apc` | Vehicle | 55000 | 18 |
| `vehicle_bike` | Vehicle | 5000 | 2 |
| `vehicle_sport_bike` | Vehicle | 6500 | 3 |
| `vehicle_bus` | Vehicle | 9000 | 3 |
| `vehicle_helicopter` | Vehicle | 45000 | 19 |
| `vehicle_monster_truck` | Vehicle | 12000 | 4 |
| `vehicle_muscle` | Vehicle | 6000 | 2 |
| `vehicle_taxi` | Vehicle | 4000 | 2 |
| `weapon_ak_rifle` | Weapon | 2400 | 2 |
| `weapon_crowbar` | Weapon | 900 | 1 |
| `weapon_katana` | Weapon | 3000 | 3 |
| `weapon_knife` | Weapon | 700 | 1 |
| `weapon_lmg` | Weapon | 9000 | 4 |
| `weapon_railgun` | Weapon | 40000 | 17 |
| `weapon_revolver` | Weapon | 1500 | 2 |
| `weapon_vector_smg` | Weapon | 1200 | 1 |
| `weapon_shotgun` | Weapon | 1800 | 1 |
| `weapon_sledgehammer` | Weapon | 2600 | 3 |
| `weapon_sniper` | Weapon | 4500 | 2 |
| `weapon_uzi` | Weapon | 2000 | 2 |

`ItemType` order is **Weapon, Skin, Vehicle, Emote** (index 1 is Skin, index 2 is Vehicle) — do not
assume the order from the ids. Preserve every id exactly; saves reference them. Price/level rule: an item
unlocked at level ≥ 10 must cost ≥ 1,200 per unlock level.

### 2.5 Districts — 12 assets, `Assets/Data/Districts/`, 2.29 km² total

`centre` and `size` are XZ metres. `requiresPass` = needs a ticket/pass to enter. Smallest containing
district wins (Secret sits inside Downtown).

| District | Name key | Centre | Size | Pass | Density |
|---|---|---|---|---|---|
| `district_airport` | ui.district.airport | 1100, 700 | 900, 500 | yes | 1 |
| `district_downtown` | ui.district.downtown | 0, 0 | 600, 600 | no | 1 |
| `district_farm` | ui.district.farm | -1200, 900 | 600, 600 | no | 1 |
| `district_hotel` | ui.district.hotel | 150, 150 | 150, 150 | no | 1 |
| `district_market` | ui.district.market | 400, -500 | 300, 300 | no | 1.1 |
| `district_military` | ui.district.military | 600, 1200 | 500, 400 | yes | 1 |
| `district_port` | ui.district.port | 900, -900 | 700, 400 | no | 0.9 |
| `district_residential` | ui.district.residential | -700, 300 | 500, 500 | no | 1 |
| `district_restaurant` | ui.district.restaurant | 350, 250 | 200, 200 | no | 1 |
| `district_secret` | ui.district.secret | 100, -100 | 120, 120 | yes | 1 |
| `district_showroom` | ui.district.showroom | -250, -350 | 250, 250 | no | 1 |
| `district_slums` | ui.district.slums | -800, -600 | 400, 400 | no | 1 |

Day/night roles: Slums is busiest at night and most dangerous; Market empties late; Restaurant is
busier at night than in the day; Airport and Farm are empty at night; Downtown is dense with lights;
Hotel operates 24 h; Military is guarded; Secret is hidden.

### 2.6 Missions — 20 assets, `Assets/Data/Missions/*.asset`

Linear chain. Every mission has exactly **3 dialogue lines** (intro / mid-run and fail / payoff),
`retryable = 1`, and pays on completion only. `timeBonusPerSecond` = 20 % of the money reward
divided by the limit, capped so the total time bonus never exceeds 34 % of the base reward.

| # | missionId | Title | Money | Rep | Limit (s) | Fail at wanted | Trigger | Objectives |
|---|---|---|---|---|---|---|---|---|
| 1 | `mis_01_wake_up` | Wake Up, Dude | 750 | 10 | 360 | 3 | `trigger_start_apartment` | Get out of bed → Walk to the street → Steal a car |
| 2 | `mis_02_delivery` | Special Delivery | 900 | 20 | 300 | 3 | `trigger_delivery_call` | Pick up the package → Deliver the package to the pier → Lose the cops |
| 3 | `mis_03_rooftop` | Rooftop Run | 1200 | 30 | 240 | 3 | `trigger_rooftop_call` | Reach the rooftop → Escape before the timer ends |
| 4 | `mis_04_car_meet` | Car Meet | 1500 | 40 | 240 | 3 | `trigger_car_meet` | Win the race → Bring the car back in one piece |
| 5 | `mis_05_showdown` | Showdown | 2500 | 60 | 300 | 3 | `trigger_showdown` | Survive the ambush → Take out the last wave |
| 6 | `mis_06_chop_shop` | Chop Shop | 3800 | 55 | 300 | 3 | `trigger_chop_shop` | Steal the marked car and deliver it to the chop shop → Lose the cops |
| 7 | `mis_07_heist` | The Heist | 6000 | 70 | 300 | 4 | `trigger_heist` | Get inside → Grab the cash → Escape the alarm |
| 8 | `mis_08_getaway` | Getaway | 6500 | 90 | 360 | 4 | `trigger_getaway` | Reach the parking lot → Survive the roadblock → Lose the helicopter |
| 9 | `mis_09_night_run` | Night Run | 7500 | 110 | 420 | 4 | `trigger_night_run` | Wait for nightfall → Load the cargo at the port → Deliver it before sunrise |
| 10 | `mis_10_slum_deal` | Slum Deal | 8500 | 120 | 360 | 4 | `trigger_slum_deal` | Find the contact in the slums → Survive the ambush → Get the package out |
| 11 | `mis_11_airport_heist` | Airport Heist | 9500 | 130 | 360 | 4 | `trigger_airport_heist` | Get past the terminal gate → Take the cargo from the hangar → Escape the runway |
| 12 | `mis_12_military_run` | Military Run | 10500 | 140 | 420 | 4 | `trigger_military_run` | Sneak past the checkpoint → Steal the truck → Lose the patrol |
| 13 | `mis_13_room_302` | Room 302 | 11500 | 150 | 360 | 4 | `trigger_room_302` | Book a room at the hotel → Search room 302 → Leave before the call ends |
| 14 | `mis_14_rooftop_chase` | Rooftop Chase | 13000 | 160 | 300 | 4 | `trigger_rooftop_chase` | Follow the target to the roof → Catch him before he jumps → Get down alive |
| 15 | `mis_15_showroom_hit` | Showroom Hit | 14500 | 170 | 360 | 4 | `trigger_showroom_hit` | Walk into the showroom → Take the keys → Drive through the window |
| 16 | `mis_16_final_chaos` | Final Chaos | 16000 | 190 | 480 | 4 | `trigger_final_chaos` | Reach the downtown tower → Hold the lobby → Survive the last wave |
| 17 | `mis_17_prison_break` | Prison Break | 18000 | 60 | 420 | 4 | `trigger_prison_break` | Tail the transport van → Free the contact → Lose the pursuit |
| 18 | `mis_18_army_depot` | Army Depot | 20000 | 65 | 420 | 4 | `trigger_army_depot` | Cut the fence → Load the crates → Drive out before the alarm |
| 19 | `mis_19_hotel_siege` | Hotel Siege | 22500 | 70 | 360 | 4 | `trigger_hotel_siege` | Rent the top floor → Hold the stairwell → Escape over the roof |
| 20 | `mis_20_endgame` | Endgame | 26000 | 75 | 540 | 4 | `trigger_endgame` | Call in support → Take the airfield → Walk away clean |

Totals: **200650 $**, **1815 reputation** → level 19 (100 rep per level). Perfect (3-star) run with time
bonuses pays **319,033 $**. Nothing may pay less than the mission before it; money per objective
must stay inside 200–9,000; time per objective inside 60–300 s; par = 70 % of the limit.

#### Completion conditions (objective keys reported by gameplay)

- `mis_01_wake_up`: car_stolen
- `mis_02_delivery`: package_delivered / not_wanted
- `mis_03_rooftop`: rooftop_cleared
- `mis_04_car_meet`: race_won / vehicle_alive
- `mis_05_showdown`: ambush_survived / final_wave_cleared
- `mis_06_chop_shop`: car_delivered / not_wanted
- `mis_07_heist`: cash_taken / alarm_escaped
- `mis_08_getaway`: parking_reached / roadblock_survived / heli_lost
- `mis_09_night_run`: night_fallen / cargo_loaded / cargo_delivered
- `mis_10_slum_deal`: contact_met / ambush_survived / package_secured
- `mis_11_airport_heist`: terminal_passed / hangar_looted / runway_cleared
- `mis_12_military_run`: checkpoint_passed / truck_stolen / patrol_lost
- `mis_13_room_302`: room_booked / room_searched / call_ended
- `mis_14_rooftop_chase`: roof_reached / target_caught / descent_survived
- `mis_15_showroom_hit`: showroom_entered / keys_taken / window_crashed
- `mis_16_final_chaos`: tower_reached / lobby_held / wave_survived
- `mis_17_prison_break`: van_tailed / contact_freed / pursuit_lost
- `mis_18_army_depot`: fence_cut / crates_loaded / alarm_beaten
- `mis_19_hotel_siege`: floor_rented / stairwell_held / roof_escaped
- `mis_20_endgame`: support_called / airfield_taken / clean_exit

#### Dialogue (Vietnamese, shipped in the assets; EN translation is a localization row)

**Act I. The box**

1. **Wake Up, Dude** — mis_01_wake_up
   - "Dậy đi. Thành phố này không chờ ai đâu."
   - "Ra đường đi, đứng nhìn xe mãi à."
   - "Ừ, giờ mày có xe. Đừng làm hỏng nó ngay."
2. **Special Delivery** — mis_02_delivery
   - "Giao cái hộp này tới quán bà Bảy. Đừng mở."
   - "Đừng dừng giữa đường, không ai thích người đứng giữa đường."
   - "Xong. Bà Bảy nhớ mặt mày rồi đấy."
3. **Rooftop Run** — mis_03_rooftop
   - "Thằng đó chạy lên mái. Lên theo."
   - "Đừng để nó nhảy trước mày."
   - "Nhảy giỏi đấy. Giờ xuống đi."
4. **Car Meet** — mis_04_car_meet
   - "Tụi nó họp ở bãi sau. Vào đi."
   - "Đừng khoe xe. Khoe là mất."
   - "Mày vừa ghi tên vào danh sách. Không rút ra được đâu."
5. **Showdown** — mis_05_showdown
   - "Tới giờ thanh toán rồi."
   - "Đừng nói nhiều, nó không nghe đâu."
   - "Xong vụ này mày không còn là thằng giao hàng nữa."

**Act II. The errand boy**

6. **Chop Shop** — mis_06_chop_shop
   - "Cái xe đó đáng giá hơn khi tháo ra."
   - "Chạy tới xưởng trước khi cảnh sát tới."
   - "Tiền đây. Đừng tiêu hết trong một đêm."
7. **The Heist** — mis_07_heist
   - "Ba phút trước khi chuông báo động kêu. Ba."
   - "Lấy tiền, đừng lấy gì khác."
   - "Ra được khỏi đây là mày khác rồi."
8. **Getaway** — mis_08_getaway
   - "Cả thành phố đang đuổi theo mày."
   - "Đừng để chúng vây. Cứ chạy."
   - "Mất con trực thăng là mày sống."
9. **Night Run** — mis_09_night_run
   - "Cảng chỉ mở ban đêm. Buồn cười thật."
   - "Chất hàng lên, đừng hỏi hàng gì."
   - "Về trước khi trời sáng."
10. **Slum Deal** — mis_10_slum_deal
   - "Gặp người ở khu ổ chuột. Mang theo tiền."
   - "Nó phục kích mày đấy. Chuẩn bị đi."
   - "Không ai phản bội tao hai lần."

**Act III. The climb**

11. **Airport Heist** — mis_11_airport_heist
   - "Máy bay đi trong bốn phút. Bốn."
   - "Qua cổng, vào hangar, đừng chạm chuông."
   - "Đường băng trống rồi. Chạy đi."
12. **Military Run** — mis_12_military_run
   - "Cái xe đó đáng hơn cả căn nhà mày."
   - "Đừng để chạm ụ súng."
   - "Mất dấu tuần tra là mày về được."
13. **Room 302** — mis_13_room_302
   - "Phòng 302. Đừng động vào minibar."
   - "Tìm thứ trong két. Không phải tiền."
   - "Cúp máy là mày phải ra khỏi đó."
14. **Rooftop Chase** — mis_14_rooftop_chase
   - "Nó lên mái. Tất nhiên rồi."
   - "Bắt được trước khi nó nhảy."
   - "Xuống được là mày còn sống."
15. **Showroom Hit** — mis_15_showroom_hit
   - "Bảo hiểm là cho người xin phép."
   - "Lấy chìa, đừng lấy xe trưng bày."
   - "Đạp ga qua cửa kính. Đừng nhìn lại."

**Act IV. Collapse**

16. **Final Chaos** — mis_16_final_chaos
   - "Cả thành phố đang tới. Làm cho nó hoành tráng vào."
   - "Giữ sảnh. Đừng để thằng nào lọt."
   - "Sóng cuối cùng. Mày vẫn đứng."
17. **Prison Break** — mis_17_prison_break
   - "Đừng hỏi tại sao nó ở trong đó. Đưa nó ra."
   - "Cắt hàng rào, đừng cắt dây điện."
   - "Mất dấu xe là mất luôn người."

**Act V. Endgame**

18. **Army Depot** — mis_18_army_depot
   - "Kho đó có thứ không mua được ở đâu."
   - "Chất thùng lên, đừng mở."
   - "Ra trước khi chuông kêu."
19. **Hotel Siege** — mis_19_hotel_siege
   - "Chúng biết phòng rồi. Để chúng lên."
   - "Giữ cầu thang. Đừng bỏ tầng."
   - "Qua mái mà đi. Thang máy là bẫy."
20. **Endgame** — mis_20_endgame
   - "Một vụ cuối. Rồi thành phố này là của mình."
   - "Gọi hỗ trợ đi, lần này không cần giấu."
   - "Đi bộ ra khỏi đó. Không ai cản."

### 2.7 Story, tone and writing rules

**Logline.** A delivery kid in Bến Cảng takes a box he is not allowed to open, and in three weeks climbs
from courier to the person the whole city avoids.

**Tone.** Comedy comes from escalation, not from jokes: a small job fails small, a big job takes the
block with it. The Dude never speaks — he nods, punches, drives. Others speak for him and are always
wrong about him. No moral lessons; nobody in this game learns anything, including the police. Violence
is impact, not gore: people get thrown, not cut open. The city is a character and is trying to make
money off the man running from it.

| Character | Role | Voice | Sample line |
|---|---|---|---|
| **Dude** | player | no dialogue, acts only | — |
| **Bà Bảy** | noodle-stall owner at the market, gives the first job, knows everything first | short, sober, impatient | "Đừng mở. Tao không nói lần hai." |
| **Tí Còi** | Dude's cousin, driver, loud, bad at driving | talks a lot, confidently wrong | "Tao biết đường tắt! …chắc vậy." |
| **Vinh "Baba"** | showroom owner, the city's real boss, pays half the police | polite to the point of chilling | "Tao không giận. Tao chỉ ghi sổ." |
| **Inspector Lâm** | the only cop who still wants to do it right, and is stuck because of it | tired, sardonic | "Mày nghĩ tao thích bắt mày à?" |
| **Captain Khang** | military depot commander, sells weapons to both sides | boastful, greedy | "Hàng của tao không có hoá đơn." |
| **Mai** | hotel receptionist, side business selling information | sweet, charges per sentence | "Phòng 302 à? Anh hỏi giá chưa?" |
| **Sóng FM** | radio DJ narrating the city between jobs | unrelated to you, and that is the point | "Sáu giờ sáng, kẹt xe ở cầu Bến. Như mọi ngày." |

**Writing rules.** One sentence, one idea. Bà Bảy and Vinh never explain a plan, only the outcome.
Tí Còi is the only character allowed to ramble (pressure valve). Lâm never threatens, only states
facts. Nobody calls Dude by a real name — "mày", "thằng đó", "tài xế", "cậu em".

**Five acts.** I (1–5) The box: does he open it? He does not, but someone now knows he carries it.
II (6–10) The errand boy: what can he do with money? He can, and starts to like it.
III (11–15) The climb: which side is he on? None; he becomes his own side.
IV (16–17) Collapse: what breaks first, the city or him? Both, and he has to go pick up the pieces.
V (18–20) Endgame: what is left to keep? Nothing to lose, so he cleans house.

**Three endings**, chosen by what the player actually did:
- **Clean** — 20 missions done, final wanted level < 3: hands the ledger to Lâm, takes the bounty, leaves.
- **King** — 20 missions done, owns ≥ 3 armoured vehicles: sits in Vinh's chair; the city changes owner,
  nothing else changes.
- **Ash** — 20 missions done, destroyed ≥ 20 civilian vehicles: nobody wins, Downtown loses power for
  three days, Sóng FM keeps broadcasting.

**Radio — 10 segments, played between jobs:**
1. "Sáu giờ sáng, kẹt xe ở cầu Bến. Như mọi ngày."
2. "Cảnh sát cho biết tình hình an ninh đã được cải thiện. Cảnh sát cũng cho biết không có vụ nào."
3. "Hôm nay 32 độ, độ ẩm 80, và không ai nhớ lần cuối thành phố này mất điện."
4. "Một tài xế gọi tới nói rằng hắn đang bị đuổi. Chúng tôi không xác minh được. Chúc may mắn."
5. "Giá xăng tăng. Giá vàng tăng. Giá người thì giảm."
6. "Đêm nay ở khu ổ chuột có chợ. Không ai quảng cáo, nhưng ai cũng biết."
7. "Sân bay thông báo hoãn bốn chuyến. Không có lý do. Có lý do."
8. "Khách sạn Hải Đăng vừa đổi chủ lần thứ ba trong năm. Chủ mới cũng chưa thấy mặt."
9. "Có người hỏi vì sao tôi không đọc tin tội phạm. Vì tội phạm là bạn của tôi."
10. "Bản tin tiếp theo lúc 7 giờ. Nếu thành phố còn."

**Continuity rules — never break:** Dude never speaks, not even in text. Bà Bảy never lies to him,
only omits. Vinh never kills anyone himself. Lâm never takes money in any scene (that is why he
survives). Tí Còi never dies. The box is never opened, not even in an ending, and nobody mentions it
again. Every mission has exactly three lines.

### 2.8 Progression numbers

**Grading** — 100 points: completion 40, clock 40 (full at par = 70 % of the limit, 0 at the limit),
clean 20 (−10 per retry, −3 per wanted level, max −8 for health lost). 1 star = finish; 2 stars ≥ 70;
3 stars ≥ 85 and no retry and no wanted and inside par. Payout +10 % for 2 stars, +25 % for 3 stars,
applied to the base reward only. `NextStarHint` must name the actual blocker (e.g. "3 stars: finish
under 3:30 — this run 4:00").

**Variants** (only on a mission already completed at least once): Time Attack (clock 60 %, +15 %),
Hot Zone (fails one wanted level earlier, +30 %), Fragile (damage counted double for grading, +20 %).
The multiplier applies to the whole payout, so a 3-star Time Attack is the biggest single payout.

**13 achievements**, each reading an existing system: story completion, star counts, daily challenges,
stunt money, side jobs, money earned, variants cleared. Ten of them store nothing: they read the save.
Only three need counters: full daily sets won, best streak, variants cleared. Rewards are deliberately
small (500–30,000 $) — achievements are badges first, not a farming route.

**Daily challenges** — 3 per day, deterministic from the date, chosen from the five activity kinds the
game already has (drift/stunt score, bounty money, side jobs, missions, evading police). Target scales
with level with ±10 % daily jitter (rounded to 10). Streak: +1 if yesterday was played, back to 1 after
a missed day; multiplier 1.0→1.5 capped at 5 days. Completing all three pays a one-time 30 % bonus.
Progress is event-driven (`StuntBanked`, `Awarded`, `JobResolved`, `MissionCompleted`, `LevelChanged`) —
no per-frame scan.

**Hotels** — Basic 150/night, 60 % heal; Standard 400, 80 %; Deluxe 900, 100 %; Penthouse 2,500, 100 %
and requires a pass or an existing rental. Max 7 nights, sleep until 07:00, early checkout refunds 50 %
of unused nights, rented room becomes the death spawn.

**Bounty** — pays on the same `CrimeReported` event that adds heat, diminishing returns inside a 60 s
window, floor 25 %, 1.5 s gap so one action cannot pay twice.

### 2.9 City props and streaming detail

`PropRules` holds **78 prop kinds** in **8 groups**: StreetFurniture, StreetClutter, WallDetail,
RoofAndUtility, ShopFront, Nature, GroundDecal, NightSign. Each kind carries surface, footprint, height,
behaviour flags (blocks navigation, breakable, grabbable, must stay off road, night only, emits light,
flat decal) and a per-chunk density. Per-group density: furniture 4, clutter 6, wall detail 7, roof 3,
shop front 3, nature 5, ground decal 5, night sign 2. Chunk cap **46** props. District multiplier
applies on top. Render tiers cut shadows first, then mesh, then everything.

### 2.10 Balance rules the checkers enforce

- Weapon DPS (damage × pellets ÷ cycle) inside **55–620**.
- Within a family, a heavier round (more muzzle energy) must deal more damage.
- Shots-to-kill ordering: the lightest sniper must still need fewer torso hits than the heaviest pistol.
- Muzzle energy reference (joules): 9×19 482, .45 ACP 500, .357 Mag 750, 5.56 1,793, 7.62×39 2,045,
  12 ga slug 3,376, .308 3,592, 25 mm airburst 9,000, .50 BMG 17,296.
- Shotguns (pellets), flamethrower (damage over time), launchers/C4/grenades (area), taser (stun),
  railgun/charge rifle (charge time) are excluded from the DPS ordering on purpose.
- Every gun needs range > 0 and magazine > 0 unless melee (magazine 0, no reload). Cars must brake
  harder than they accelerate. Every shop item must be affordable and unlocked by the content that
  exists (2-star perfect chain pays 319,033 $ and reaches level 19).

## 3. ART PACK

The game ships with primitives tagged `// ART REPLACE` until real art arrives. The art below is a
separate workstream: **image prompts first, 3D models second, import third**. Never block code on art.

### 3.1 Brand

| Item | Value |
|---|---|
| Name | **DUDE CHAOS WARS** |
| Tagline | *"No rules. Just dude."* |
| Logo | "DUDE" heavy broken-stroke type, 4 px black outline, "CHAOS WARS" small underneath, a crack through the O. Yellow `#FFC812` on `#14161C`, white `#FFFFFF` outline. 1024×512 PNG, transparent. |
| App icon | smirking character face + "DCW", 512×512 |

Palette: yellow `#FFC812` (logo, stars, money) · near-black `#14161C` (UI panels, signs) · white
`#FFFFFF` (body text) · danger red `#EA3838` · police blue `#2C4BD6` · money green `#63DB4C` ·
neon orange `#FF7A2F` · neon pink `#FF3D9E` · neon cyan `#38E1FF` · concrete grey `#8A8D94` ·
brick brown `#7A4A32`.

Fonts: Bebas Neue or Anton (headings), Rajdhani SemiBold (HUD numbers, tabular), Inter Regular (body).
Signage may use any heavy uppercase face — a real city is not consistent.

Fictional in-game brands (avoid real trademarks): Dude Cola (red `#E02020`), ChaoMart (yellow/black),
Saigon Slick taxi (green/white), Ba Cây Xăng (orange/white), Nhậu 24/7 (red/yellow neon), Bến Thành
Fresh (green/white), Dude Motors (black/silver), Khách Hồng Phát (pink neon), Cảng Sài Gòn 7
(industrial blue).

### 3.2 Scale and pivots (all models)

**1 unit = 1 metre. Y up. Z forward.** Character 1.8 m tall, shoulders 0.45 m, arms 0.72 m, legs
0.9 m, head 0.23 m. Door 2.1 m, floor height 3.0 m, sidewalk 3 m, lane 3.5 m, window 1.1×1.4 m at
0.9 m, balcony depth 1.2 m.

Pivots: character between the feet · vehicle at ground centre (front +Z, four wheels separate meshes) ·
handheld weapon at the grip (+Z along the barrel) · building at the front-bottom corner (facade +Z) ·
props at ground centre.

File naming: `CHR_`, `VEH_`, `BLD_`, `WPN_`, `PRP_`, `VFX_`, then a clear name, e.g. `WPN_AK.fbx`,
`VEH_Sedan.fbx`, `BLD_Office_24x24.fbx`.

### 3.3 3D delivery spec

| Item | Requirement |
|---|---|
| Format | `.fbx` preferred, `.glb` accepted |
| Scale | 1.0 on import, metres |
| Poly budget | character ≤ 12k tris, vehicle ≤ 18k, building ≤ 25k, small prop ≤ 1k |
| LOD | 3 levels: 100 % / 40 % / 10 % |
| UV | no overlap; one atlas 1024 (small), 2048 (vehicle/building) |
| Textures | Albedo + Normal + Mask (R metallic, G roughness) + separate Emission |
| Materials | at most 4 slots: body / glass / chrome / emissive |
| Rig | humanoid (Unity Mecanim), min 15 bones |
| Animation | not required — procedural systems animate walk, wheels, lights, sirens, neon |
| Collision | not required — built from primitives in engine |
| Wheel pivot | wheel centre, X axis |

### 3.4 Animation budget (if hand animation is ever supplied)

Per asset type: characters 18 clips / 588 frames · guns 8 / 260 · melee 5 / 122 · throwables 4 / 93 ·
vehicles 6 · vehicle interiors 6 · cockpits 7 · animals 8 · nature 3 · interiors 4 · buildings 2 ·
props 2 · VFX = sprite frame count · UI = 0. Total target **792 clips / 23,511 frames** for 185 assets.

### 3.5 Image catalogue

Working set: **185 prompts in 5 step files** (`02`–`06`), one standalone prompt per item, in
`prompts/` (measured: `grep -h '^### ' prompts/0[2-6]_*.md | wc -l` → 185; the old
`design/qwen_render_manifest.json` said `asset_count: 185` and now lives only in git history).

| File | Step | Area | Prompts |
|---|---|---|---|
| `02_buoc1_nhan_vat_phuong_tien.md` | 1 | Characters, vehicles and their interiors | 31 |
| `03_buoc2_sung_kien_truc_thien_nhien.md` | 2 | Guns and attachments, architecture and interiors, nature and animals | 72 |
| `04_buoc3_do_dac_hieu_ung_khi_tai.md` | 3 | Street props, VFX, military hardware | 30 |
| `05_buoc4_y_te_ui.md` | 4 | Medical and consumer, UI and textures | 10 |
| `06_buoc5_chi_tiet_noi_that.md` | 5 | Small details, enterable interiors and cockpits | 42 |
| **Total** | | | **185** |

The first batch of renders came from an earlier **683-prompt catalogue in 11 files** (that layout is no
longer on disk; it is preserved in git history at `d332e04`). 682 images were reviewed: **563 keep, 113
redo, 5 missing** (126, 176, 177, 182, 518). Reasons for redo: baked checkerboard/dot-grid (71), baked
captions/dimension lines (41), wrong content (14 — gizmo artifacts, wall behind a prop, wrong object,
invented brand). Those prompts were rewritten into the 185-item working set above, which keeps the 16
vehicle interiors (684–699), the 29 cockpit/interior/enterable spaces (700–728) and the 22 rebuilt
structures (729–750).

### 3.6 Global prompt rules (already embedded in every block)

- **Flat pure white `#FFFFFF`, single tone.** Not "transparent background" — generators do not return
  real alpha; a clean white is what keys to alpha reliably.
- **No text at all:** no captions, labels, arrows, dimension lines, numbers on canvas, watermark,
  signature, UI overlay, viewport axis gizmo.
- **No checkerboard, no dot grid, no floor, no horizon, no cast shadow, no gradient.**
- **Exact canvas:** characters 3-view `1536×1024`; vehicles/guns/interiors `2048×1024`; single objects
  `1024×1024`; buildings `2048×2048`; sprites are a horizontal strip (sheet = frame count × cell size);
  camera backdrop `2048×1152`.
- Each prompt lists parts, materials, hex colours, wear and per-part dimensions. Do not shorten it back
  to a one-line description.
- File naming: `NNN Name.webp` (or `.png`) matching the item number, zipped per upload.

### 3.7 Two complete sample prompts (copy verbatim)

**Character reference (item 001 style):**
```
Character reference sheet, 3D game asset, stylized realistic cartoon proportions (Dude Theft Wars
style), adult male, 1.8 m tall, white t-shirt #F2F2F2, blue jeans #2E4A7D, black sneakers #1A1A1A,
neutral standing pose, front view + side view + back view in one row, correct 1:1 scale, flat studio
lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard,
no dot grid, no floor, no horizon, 1536x1024, no text, no labels, no arrows, no measurement lines, no
numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

**Weapon (item 122, shipped prompt):**
```
weapon 3D game asset, compact semi-automatic pistol, matte black slide with rear serrations, white
three-dot sights, polymer frame with textured grip, trigger guard, 15-round magazine as a separate mesh,
barrel as a separate mesh, 0,20 m long, full side view, muzzle pointing right, pivot at the grip, correct
1:1 scale, crisp hard-surface topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF
background, single tone, no gradient, no checkerboard, no dot grid, no floor, no horizon, 2048x1024, no
text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature,
no UI overlay, no viewport axis gizmo
```

### 3.8 Background keying (white → alpha)

Key by **border-connected region**, not by brightness threshold, so a white shirt or a steel prop stays
opaque. Neutral light grey (min channel ≥ 230, channel spread ≤ 8) also counts as background to remove
soft contact shadows. 2 px soft edge + un-matte against white so scaling does not leave a white fringe.
`--fill-holes 0.5` removes enclosed background pockets (bike frames, spokes, fan cages) without punching
the clothing. **Never trim** sprite strips or atlases — the grid must stay. 59 VFX sprites are skipped
(they need a dark background), and 51 checkerboard images are skipped (they belong to the redo set).

### 3.9 Art priority (when only part of the set exists)

1. Dude + 4 NPC outfits — unlocks all gameplay readability.
2. Sedan + police cruiser — unlocks driving and wanted.
3. 3 Downtown buildings + 1 interior — unlocks "walk inside".
4. Street lamp + neon signs — makes the night look finished.
5. Everything else by category.

Procedural animation covers the gap: `ProceduralWalker` (sin-driven hips/thighs/calves/arms from real
speed), `WheelSpin` (distance ÷ radius), `VehicleLights` (headlights at night, brake, hazards),
`SirenController` (red/blue alternating), `NeonFlicker` (per-sign deterministic), `DayNightCycle`
(all lamps fade in one loop). If hand-authored clips arrive later, the Animator clip wins and the
procedural driver disables itself.
## 4. VERIFICATION HARNESS (build this before gameplay)

`Tools/` sits outside `Assets/` so Unity never compiles it. It exists because the build machine has no
editor. It proves internal consistency and pure logic — **not** Unity compatibility.

### 4.1 `Tools/CompileCheck/`

- `CompileCheck.csproj` targets `net8.0` and compiles **every** `.cs` under `Assets/Scripts` — and
  **nothing under `Assets/Editor/`**. `SceneBootstrap`, `ProjectSetup` and `BuildScript` are therefore
  compiled by no tool on this machine, and nothing here proves they are valid C#. Do not report an editor
  script as "compiles" from this build; it never saw the file.
- The cheapest partial check for an editor script is a throwaway project that includes `UnityStubs.cs`
  plus `Assets/Editor/**`. Expect only `CS0246`/`CS0234` (the `UnityEditor`, URP and TMP packages are not
  stubbed); **any other error code is a real defect** — wrong member, wrong arity, missing brace. Syntax
  and arity are provable this way, Unity API compatibility is not.
- `UnityStubs.cs` hand-writes only the API surface the project touches. Rules learned the hard way:
  - Stubs that a test depends on must do **real math**: `Vector3.normalized`, `Vector3.Dot`,
    `Mathf.Clamp`, `Mathf.Clamp01`, `Mathf.Lerp`, `Mathf.MoveTowards`.
  - `CharacterController` must derive from `Collider` (or `enabled` does not exist).
  - `JsonUtility` must do a real round trip, not return `"{}"`.
  - `Rigidbody` must expose `linearVelocity` and **must not** expose `velocity` (Unity 6.3 removed it).
  - `Keyboard` has no `mouse` member; mouse state lives on `Mouse.current` (`leftButton`,
    `rightButton`, `middleButton`, `delta`).
  - `Animator.SetTrigger(int)` and `Animator.StringToHash(string)` exist (hashed parameters).
  - Never invent an API to make the build green. If Unity disagrees, the stub is wrong — fix the stub.
- Acceptance: `dotnet build` → **0 errors, 0 warnings**.

### 4.2 `Tools/LogicTests/`

Console harness (no test framework) with one assertion helper that prints `PASS`/`FAIL` and exits
non-zero on any failure. **1028 assertions** as of this writing (`dotnet run --project
Tools/LogicTests/LogicTests.csproj` prints `PASSED 1028  FAILED 0`) covering at least:

| Area | Must-have assertions |
|---|---|
| Movement | walk/run/sprint speeds, FOV kick, coyote, jump buffer, landing tiers incl. exactly −8 = medium |
| State machine | every legal transition, every illegal one rejected, `InVehicle` cannot ragdoll, `ExitingVehicle` can |
| Ragdoll | budget selection by distance/count, NaN/inf/huge center of mass rejected, `SafeCenterOfMass` fallback |
| Combat | body-part multipliers, impulse direction, blast falloff, backstab dot, magazine, fire-rate gate, all nine mechanics' cycle/shot count/heat |
| Vehicles | eject rule at 9 m/s, eject impulse, steering falloff, destruction stages, armour multipliers, tuning costs |
| Wanted | level from heat with zero thresholds skipped, cooldown gate, cooling, response table |
| Missions | ordered objectives, duplicate objective keys still complete, timeout/death/wanted fail reasons, 3-retry cap, time bonus 34 % cap, grading 40/40/20, 3-star conditions, no-clock tops at 2 stars, variant multipliers |
| Dates/dailies | invalid dates (31 April, 29 Feb non-leap, year > 9999) do not throw, 400 consecutive days produce 3 distinct kinds each, streak reset, deterministic same-day set |
| Save | checksum, version gate, sanitise, JSON round trip, migration v1→v4, `int.TryParse` on overflow, array-length repair |
| Progression | wallet never negative, shop gates, level curve, achievement rescore after restore, bounty diminishing floor 25 % |
| World/NPC | cell coord/ring/Chebyshev, spawn budget, tick rates 60/20/6/0, steering arrive/waypoints/separation, avoidance no jitter |
| Audio | voice budget 12/8/4/0 at 90 m, steal rule, falloff |
| Net | 32-byte command round trip, speed hack/teleport/fire-rate/stale-tick rejection |
| Scene contract | clean scene passes, each missing/duplicated component named, wrong object name, split objects, unwired store, two faults reported at once |

### 4.3 `Tools/audit/audit.py` (pre-import audit)

Exit 1 on findings. Checks:
`ASMDEF_PARSE`, duplicate assembly names, dangling references, scripts not covered by any asmdef,
duplicate type names, package assemblies referenced without the package in `manifest.json`,
asset `m_Script` GUID that resolves to no script `.meta` (scan `Library/PackageCache` when present, or
the check reports false positives for TMP/URP), duplicate GUIDs, project-version drift,
`ASSET_VECTOR` (every `{x:…, y:…, z:…}` must be numeric), `ASMDEF_CYCLE` (DFS over the reference
graph), `USING_NOT_REFERENCED` (every `using DudeChaos.X` must be in that assembly's references).
Baseline: **0 findings** on 22 assemblies / 221 types / 196 assets (the script prints those three
counts; `types` is every declared type, not only scripts).

### 4.4 `Tools/audit/balance.py` and `reference.py`

`balance.py` parses the asset YAML and prints weapon DPS and shots-to-kill, vehicle feel, mission
payouts and shop affordability, and fails on: gun with no range or empty magazine, melee with a
magazine, car that brakes weaker than it accelerates, shop item that is neither affordable nor
unlocked by existing content. This tool already caught a real generator bug where every weapon had a
shifted argument list (`weaponClass` garbage, AK read as a pistol). Run it after **every** content
change. `reference.py` enforces the real-world relationships in 2.10 plus mission pacing (money never
drops along the chain, 200–9,000 per objective, 60–300 s per objective) and the par table.

### 4.5 `Tools/verify/checklist.py` and `production/VERIFY_BACKLOG.md`

A markdown table of every unproven item with an id (`V-001` …), grouped by phase, status
`TODO / PASS / FAIL / BLOCKED`, and a one-line note. The CLI rewrites the file in place:
`summary`, `list --phase N`, `list --status FAIL`, `pass V-101 --note "walks at 3.2"`,
`fail V-105 --note "jumps 1.9 m"`, `block V-1101 --note "no device"`, `reset V-101`, `next`.
**A PASS means a human watched it happen.** Never mark PASS from code reading.

### 4.6 Play Mode tests (`Assets/Tests/PlayMode/PlayModeSmokeTest.cs`)

Six baseline tests. They passed 6/6 on a real editor in Phase 31; a re-run after the scene-builder
refactor is still pending (tracked as V-4591), so treat that number as historical, not current:
`Player_Falls_And_Settles_On_Ground`, `Player_Moves_Forward_With_Input`,
`Player_Sprints_Faster_Than_It_Runs` (asserts 8.8 vs 6.0), `Player_Stops_When_Input_Released`,
`HealthSystem_Burns_Armour_Before_Health`, `Built_Test_Scene_Plays` (loads `Test.unity`, asserts
player on ground, main camera, virtual camera, HUD, 8 NPCs). Tests set private fields by reflection so
the test assembly needs no `UnityEditor` reference. Use `WaitForSeconds`, not frame counts — a batch
editor runs frames far faster than 60 Hz, so "90 frames" is 0.06 s. The slice-props test is
`[Explicit]` because that scene crashed PhysX in `Simulate`; keep it explicit until the bisect scenes
identify the culprit (`TestSlice_CarOnly`, `TestSlice_TargetsOnly`, `TestSlice_WeaponOnly`, all built
from the same builder).

### 4.7 Unity commands (on a machine with the editor)

```bash
UNITY="/c/Program Files/Unity/Hub/Editor/6000.3.28f1/Editor/Unity.exe"
PROJ="C:/path/to/project"
# build scenes
"$UNITY" -batchmode -quit -projectPath "$PROJ" \
  -executeMethod DudeChaos.EditorTools.SceneBootstrap.BuildAllTestScenes -logFile build.log
# verify every built scene has the systems object (non-zero exit when one does not)
"$UNITY" -batchmode -quit -projectPath "$PROJ" \
  -executeMethod DudeChaos.EditorTools.SceneBootstrap.VerifyBuiltScenesBatch -logFile verify.log
# play mode tests (batchmode + graphics hangs when a scene with a camera enters Play)
"$UNITY" -batchmode -nographics -projectPath "$PROJ" -runTests -testPlatform PlayMode \
  -testResults results.xml -logFile tests.log
```

`ProjectSetup` menu: `DudeChaos → Setup → 0` imports TMP Essentials (required, or TMP opens an
importer window mid-Play), `→ 1` creates and assigns the URP renderer + pipeline, `→ 2` configures
Android (IL2CPP, ARM64 only, min API 25, landscape, linear, bundle id), `→ 3` reports state.
`BuildScript` builds the APK from the menu and from batch mode; signing comes from
`DUDECHAOS_KEYSTORE*` env vars only, and a release build without them fails rather than emitting an
unsigned APK.

---

## 5. PHASE PROMPTS

Each block is self-contained: paste it with the master prompt already in context. Do not skip a gate.

### P0 — Foundation, data layer, tooling

**Goal:** an empty repo becomes a Unity project that would import cleanly on first open.
**Deliver:** folder tree; `Packages/manifest.json` (URP, Input System, Cinemachine, uGUI, TMP, Test
Framework); `ProjectSettings/ProjectVersion.txt`; all 15 ScriptableObject contracts + enums +
`IDamageable`/`ICameraPunch`; `GameManager`; 22 asmdefs with the exact graph in 1.5; `Tools/CompileCheck`
with real-math stubs; `Tools/LogicTests` skeleton; `Tools/audit/audit.py`; `CHECKPOINT.md`,
`production/ROADMAP.md`, `production/STATUS.md`, `production/DECISIONS.md`; the URP pipeline and
render settings exactly as in `1.9` (`Mobile_Renderer` + `Mobile_URP`, assigned to Graphics and every
quality level, ambient mode Color).
**Gate:** `dotnet build` 0/0; audit 0 findings; every asset type has at least one valid hand-written
asset with a fixed GUID; no duplicate type names; nothing pink on first open.

### P1 — Player, camera, input, state machine

**Goal:** a capsule that walks, runs, sprints, crouches, jumps, lands and turns the camera.
**Deliver:** `MovementSettings` with the locked numbers; `PlayerState` (14 states); transition-table
state machine; `IPlayerInput`; `PlayerMotor` (dead zone, blend, coyote, jump buffer, gravity, landing
tiers, crouch collider, `Awake` self-resolve of input); `InputSystemPlayerInput` (keyboard, mouse and
gamepad polled directly, virtual move/look hooks); `ThirdPersonCameraRig` (yaw/pitch, FOV kick);
`PlayerLookController`; `HealthSystem`; `SafeAreaFitter`.
**Gate:** 45+ logic assertions; offline build 0/0; document movement feel as NOT RUN until a human
presses Play.

### P2 — Ragdoll (mandatory gate)

**Goal:** bodies that react, settle, recover and die correctly.
**Deliver:** `RagdollSettings`; `RagdollActivationReason` (7 reasons); `RagdollBudgetModel` (pure);
`RagdollBudget` (one static per-frame driver, reads `Position`); `RagdollController` (bone toggle, mass
spread, sleep, recovery, get-up, dead); `IRagdollTargetSource`; `PelvisFollowTarget`;
`production/PHASE2_RAGDOLL_VALIDATION.md`.

**Validation gate — six answers, all required, all `NOT RUN` until watched:** ragdoll from bullet,
explosion, vehicle and high fall? recovery only when velocity low and not dead? force on the correct
body parts? max ragdoll count respected? distant ragdolls simplified? camera pelvis follow with 0.08 s
lag? If the gate cannot be run, say so and record it — never claim it passed.

### P3 — Combat

**Goal:** the full 10-step shot chain plus melee, explosions and hit feedback.
**Deliver:** `CombatSettings`; `CombatMath` (pure); `WeaponController` (chain in order, events only,
no per-shot `GetComponentsInParent`, animator hashes cached); `WeaponMechanics` (nine mechanics);
`BodyPartTag`; `MeleeController`; `Explosion`; `DamageDummy`; `PrefabPool`; `HitStopDriver`;
`ICameraPunch` punch; `PlayerWeaponInput` glue (left mouse/stick → `TryFire`, auto-reload).
**Gate:** 100+ assertions; body-part multipliers exactly 3.2 / 1.0 / 0.75; no allocation in the fire
path; build 0/0.

### P4 — One vehicle

**Goal:** enter, drive, exit, eject at speed, take damage, explode.
**Deliver:** `VehicleSettings`; `VehicleMath` (pure, incl. `IsUsableCenterOfMass`, `SafeCenterOfMass`,
`MaxCenterOfMassOffset = 5`); `VehicleController` (timer windows, arcade drive, downforce, eject,
input fallback chain, center-of-mass guard in `Awake`); `VehicleHealth` (smoke/fire/explosion 2.8 s);
`VehicleInteraction` (E within 3.5 m, non-alloc buffer, clear reference only on successful exit);
`VehicleLock`/`VehicleLockRules`; `ArmourRules`.
**Gate:** 123+ assertions; every vehicle asset has numeric `centerOfMassOffset`; audit `ASSET_VECTOR`
0 findings; driving feel NOT RUN until played.

### P5 — Wanted and police

**Goal:** heat rises and falls, stars match, police respond and cool down correctly.
**Deliver:** `WantedSettings` (weights, thresholds, cooldowns, response table); `WantedMath` (pure);
`WantedSystem` (events `LevelChanged`/`CrimeReported`); `PoliceUnit` (LOS, chase, fire from level 2);
`PoliceChaseMath`; `PoliceCar` (Rigidbody arcade pursuer); `PoliceSpawner` (units + cars matched to
level, despawn on drop).
**Gate:** 157+ assertions; cooldown only when out of sight **and** > 48 m; memory 9 s; no NavMesh.

### P6 — Vertical slice integration

**Goal:** the 11-point slice is complete and playable.
**Deliver:** `GameSettings` + `SettingsStore` (clamped); `SaveData` v1 + `SaveIntegrity` + `SaveSystem`
(3 slots + autosave); `GameFlowController`; `HudController`; `MobileJoystick`; `PauseMenuController`;
`SafeAreaFitter`; `SceneBootstrap` (sun, ground, blocks, player with all components, yaw/pitch pivots,
Cinemachine camera + brain, 1920×1080 canvas, HUD, joystick, settings assets); `ProjectSetup` (URP,
Android, TMP); `PlayModeSmokeTest` (6 tests); `design/SETUP_GUIDE.md`.
**Gate:** slice checklist item by item, each with a V-id; Play Mode 6/6 only if actually run. Scene
builder must log `"<Type> has no serialized field '<name>'"` instead of silently producing a dead
scene, and must load settings assets **after** creating the scene.

### P7 — Content, economy, missions, retention

**Goal:** the 20-mission chain, shop, grading, dailies, achievements and variants.
**Deliver:** `Wallet`/`ShopRules`/`ShopSystem`; `MissionRun` (own `_nextIndex`, ordered objectives,
clock, fail reasons, retries, 34 % bonus cap); `MissionSystem` (grading, best stars, events);
`MissionModifierRules`; `MissionGradeRules` (`int.TryParse`); `MissionSelectRules`;
`MissionBanterRules`; `ProgressionCurve`; `WeaponInventory`; `ConsumableRules`; `DailyChallengeRules`
(safe `DateOf`, distinct kinds, 400-day test) + `DailyChallengeSystem`; `AchievementRules/System`;
`StuntRules/System`; `BountyRules/System`; `MissionHud`, `DailyChallengeHud`, `StuntHud`,
`AchievementToast`, `SideMissionBoard`, `TuningPanel`; `ChallengeBinder` (event-only, no `Update`);
`SideJobDirector`; 20 mission assets with 3 dialogue lines each; `Tools/content/story_lines.py` +
`gen_content.py`; save format upgrade.
**Gate:** 190+ assertions; audit + balance + reference 0 findings; chain pays 200,650 $ / 1,815 rep →
level 19; perfect run 319,033 $; no mission pays less than the one before.

### P8 — City, NPCs, animals, streaming

**Goal:** a city that streams, walks, drives and changes with the clock.
**Deliver:** 12 district assets; `DistrictRules`, `DistrictBounds`, `DistrictDefinition`,
`DistrictRegistry`, `DayNightRules/Cycle`, `InteriorPortal`, `HotelRules/Service`, `PropRules`
(78 kinds, 8 groups, 46 cap, render tiers, batch count), `SideMissionRules`, `DistrictHeatRules/Binder`;
`WorldStreamMath/Streamer`; `NpcSteering`, `NpcAvoidance`, `NpcTickLod` (60/20/6/0), `NpcSpawnMath`,
`NpcSpawner`, `NpcAgent`; `AnimalRules/Spawner/Agent`; `design/CITY_PLAN.md`.
**Gate:** 240+ assertions; audit 0 findings; 12 districts total 2.29 km²; no per-frame scene scan.

### P9 — Presentation: UI, a11y, audio, visuals

**Goal:** the game looks and sounds like a game, not a test scene.
**Deliver:** `AccessibilityPalette`; text scale, colour-blind palette, high contrast in settings;
`LocalizationTable` (EN/VI, fallback); `AudioBudgetMath/Director`, `DriftAudio`; `AnimationMath`,
`ProceduralWalker`, `WheelSpin`, `SirenController`, `NeonFlicker`, `VehicleLights`; `NitroHud`,
`ConsumableHud`, `SettingsApplier`; `design/PERFORMANCE_BUDGET.md`; the render pass of `1.9` (day/night
lighting, fog applied per tier, tier-driven shadow distance and MSAA, camera punch values).
**Gate:** 270+ assertions; audio pool never grows; no `Update` per lamp/neon; fog written to
`RenderSettings`; UI wiring and every visual check marked NOT RUN until seen.

### P10 — Multiplayer scaffold (never runtime netcode without the package)

**Goal:** prove the authority model, not ship online play.
**Deliver:** `NetworkCommand` (fixed 32 bytes), `AuthorityValidator` (speed hack, teleport, fire-rate,
damage claims, stale ticks), `INetworkTransport` + `LoopbackTransport`, `FusionNetworkTransport` behind
`#if PHOTON_FUSION` with a docstring stating it has never been compiled against the real package.
**Gate:** 273+ assertions; local two-player loopback test; every net claim tagged NOT RUN against
Fusion; do not add Photon before the slice is green.

### P11 — Hardening, art pipeline, release

**Goal:** make it shippable and keep the evidence honest.
**Deliver:** `Tools/art/setup_tools.sh` (Blender portable headless, ffmpeg, gltf-transform, Pillow),
`make_props.py` (pipeline smoke test only — not shipping art), `optimize_art.sh`,
`optimize_textures.py`, `build_prompts.py`, `review_images.py`, `key_alpha.py`; `design/` bibles and
prompt files; `BuildScript` (APK, signed from env vars only); `production/RELEASE_CHECKLIST.md`;
`production/PERFORMANCE_BUDGET.md`; `production/VERIFY_BACKLOG.md` filled and walked.
**Gate:** Android build only claimed with an APK in hand; device test only claimed with a device;
every unrun item has a V-id and status.

---

## 6. LANDMINES (real failures from this project — do not rediscover them)

| # | Symptom | Cause | Fix / guard |
|---|---|---|---|
| 1 | Editor dies inside `PhysX Simulate`, no exception | `centerOfMassOffset: {x: 0, y: VEH_Muscle_GT1, z: 0}` — a string where a number belongs, in 6 vehicle assets | audit rule `ASSET_VECTOR`; runtime `IsUsableCenterOfMass` guard with (0, −0.35, 0) fallback and an error naming the asset |
| 2 | `Rigidbody.velocity` compile error on Unity 6.3 | property removed | use `linearVelocity`; remove `velocity` from stubs |
| 3 | `Keyboard does not contain a definition for 'mouse'` | wrong API family | `Mouse.current.leftButton/rightButton/delta`; never let a stub invent `Keyboard.mouse` |
| 4 | `One or more cyclic dependencies detected between assemblies` | `City → Progression → Wanted → City` | keep district heat in City; City may reference Wanted, not the reverse |
| 5 | Same class of cycle in achievements | Progression needed a GameFlow type | GameFlow calls `NotifySideJobs`; Progression never references GameFlow |
| 6 | `CS0234: 'Input' does not exist in namespace 'DudeChaos.Player'` | `CameraRig` used `using DudeChaos.Player.Input` without referencing that assembly | it only needed `IPlayerInput` (namespace `DudeChaos.Player`); delete the using, do not widen references |
| 7 | `CS0234: 'Universal' does not exist in 'UnityEngine.Rendering'` | Editor asmdef missing URP runtime references | add `Unity.RenderPipelines.Core.Runtime` + `Unity.RenderPipelines.Universal.Runtime` |
| 8 | Whole scene renders pink | no URP asset assigned; `Assets/Settings` empty | `ProjectSetup.CreatePipeline` must create `Mobile_Renderer` + `Mobile_URP`, MSAA 4×, no HDR, assign to Graphics + all quality levels, commit the generated assets |
| 9 | TMP importer window opens during Play | TMP Essential Resources not imported | `DudeChaos → Setup → 0` imports them; works in batch mode; keep the editor alive until import finishes |
| 10 | Scene's `settings` fields are `{fileID: 0}` | assets created **before** `EditorSceneManager.NewScene`, then unloaded → fake-null | create scene first, then load assets, then wire |
| 11 | Player in a loaded scene does not fall or move | `Bind()` is runtime-only; `_input` null on scene load | `Awake` self-resolves `GetComponent<IPlayerInput>()` |
| 12 | Shooting and entering vehicles do nothing | nobody called `TryFire()` / `TryEnter()` | add `PlayerWeaponInput` and `VehicleInteraction` glue components |
| 13 | Character leaves the car but is stuck forever | eject called `Activate()` from `InVehicle`; table rejects `InVehicle → Ragdoll` | route through `ExitingVehicle`, add it to `RagdollSources`, plus a pending-exit timer fallback |
| 14 | Mission never completes | progress read `_done.Count` on a `HashSet`; duplicate keys do not grow it | own `_nextIndex`, reset in `Retry()` |
| 15 | Exception every frame in `Update` | `new DateTime(...)` from a save key like 31 April / 29 Feb non-leap | validate `DaysInMonth` and `year ≤ 9999` before constructing |
| 16 | `OverflowException` while loading a save | `int.Parse` on a long digit string | `int.TryParse` everywhere on save data |
| 17 | Daily shows the same challenge three times | guard ran out and slots defaulted to kind 0 | fill remaining slots with unused kinds |
| 18 | Error every frame, budget blocked | `RagdollBudget` used `ReadBonePositions()[0]` on a boneless prefab | read `Position` (pelvis) |
| 19 | Player inside a car but cannot exit | `_current` was nulled even when `TryExit()` refused | clear only on success |
| 20 | Car never accelerates | `driverInput` left empty → `_input` null → throttle 0 | fallback `driverInput → motor → self → warning` |
| 21 | Editor hangs starting Play in batch mode | graphics batch mode + camera in Play | run tests with `-batchmode -nographics`; visual checks are human-only |
| 22 | Slice scene crashes PhysX | unknown; three suspects arrive together | build `TestSlice_CarOnly/_TargetsOnly/_WeaponOnly` from one builder and bisect; keep the slice test `[Explicit]` until then |
| 23 | 600 files show as modified with no content change | worktree checked out CRLF while index is LF | `git ls-files --eol` filter `i/lf w/crlf`, then strip CR; never commit the churn |
| 24 | Six weapons had a string in a float field | generator `%s1` bug on a recoil curve (`value: WPN_Rifle_AK3`) | asset type audit; `balance.py` prints enum names so garbage classes are visible |
| 25 | Every weapon had the wrong class/auto flag | generated with a one-slot-shifted argument list | regenerate from a named-field generator, not positional args; balance checker validates |
| 26 | Tests passed for the wrong reason | stubs were passthrough (`Clamp`, `normalized`) | stubs must compute; a test that cannot fail is worse than no test |
| 27 | `AndroidApiLevel24` warning that will become an error | obsolete API level | min API 25 |
| 28 | Mission with no time limit can never reach 3 stars | deliberate design (`NoClockScore = 20`) | keep it; the test documents "can be good, never perfect" |
| 29 | Scene is not pink but the night city is flat and the far buildings pop in hard | per-tier `FogDensity` is computed and never written to `RenderSettings` | apply `RenderSettings.fog/fogMode/fogDensity` in `QualityApplier.Apply` (1.9.3) |
| 30 | Per-tier anti-aliasing "applies" but nothing changes on screen | URP reads MSAA from the pipeline asset, `QualitySettings.antiAliasing` is ignored under SRP | write `pipeline.msaaSampleCount` from the tier, or drop the claim (1.9.8 R2) |
| 31 | Neon signs, sirens and brake lights do not glow | `_EmissionColor` through a `MaterialPropertyBlock` on a non-emissive default material | emissive materials in P11; code is already correct (1.9.6) |
| 32 | Pixel-perfect setup values do not match the asset file | `CreatePipeline` was never re-run after inspector tuning | treat the `.asset` as authority, diff after setup (1.9.1) |

---

## 7. REPORT FORMAT THE RECEIVING AGENT MUST USE

```
PHASE <n> — <name>
Built: <files/systems>
Changed: <what and why>
Checks actually run:
  - dotnet build Tools/CompileCheck        -> PASS (0 errors, 0 warnings)
  - dotnet run Tools/LogicTests            -> PASS (1028 assertions, 0 failed)
  - python3 Tools/audit/audit.py           -> PASS (0 findings, 22 asm / 196 assets)
  - python3 Tools/audit/balance.py         -> PASS (0 findings)
  - python3 Tools/audit/reference.py       -> PASS (0 findings)
  - Unity batch compile                    -> NOT RUN (no editor on this machine)
  - Play Mode                              -> NOT RUN (needs editor + human)
  - Android build / device                 -> BLOCKED (no SDK/device)
Not done / known gaps: <explicit list, each with a V-id if it belongs in the backlog>
Next action: <one concrete step for the human or the next phase>
```

Rules for that report: never upgrade `NOT RUN` to `PASS` because the code "looks right"; never omit a
failed check; if a gate is skipped, record it as a deliberate deviation with the reason and who
approved it (as `production/DECISIONS.md` does with D-013).

---

## 8. WHAT "DONE" LOOKS LIKE

- 22 assemblies, 0 cycles, 153 scripts, 22,598 lines, 196 assets, 0 audit findings.
- 1028 logic assertions passing offline (`dotnet run --project Tools/LogicTests/LogicTests.csproj`);
  the six Play Mode tests re-run green in a real editor
  (the 6/6 from Phase 31 must be re-proven after any scene-builder change).
- 20 missions playable in order with clock, failure, retry, stars and variants.
- 12 districts streaming, day/night, interiors, traffic/pedestrians/animals.
- 48 weapons, 18 vehicles, 59 equipment items, 25 shop items, 13 achievements, 3 dailies per day.
- Save format 4 with migration, corrupt-slot rejection and autosave on pause/quit.
- Art: 185 prompts in 5 step files (`grep -h '^### ' prompts/0[2-6]_*.md | wc -l`), 792 clips / 23,511 frames for
  185 assets, white-to-alpha pipeline, 3D delivery spec.
- A backlog where every unproven item is visible, and not one PASS that nobody watched.
