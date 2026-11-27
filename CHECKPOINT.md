# CHECKPOINT — DUDE CHAOS WARS REBUILD (P0 → P11)

Engine target: **Unity 6.3 LTS (`6000.3.28f1`)** · Pipeline: **URP (`17.3.0`)** · Bundle ID: `com.dudechaos.wars`

---

## Summary Verification Matrix

| Category | Check / Command | Status | Evidence |
|---|---|---|---|
| **Proven Offline** | `dotnet build Tools/CompileCheck/CompileCheck.csproj` | **PASS** | `0 Warning(s), 0 Error(s)` across all `Assets/Scripts/**/*.cs` (`net8.0`, `TreatWarningsAsErrors=true`) |
| **Proven Offline** | `dotnet run --project Tools/LogicTests/LogicTests.csproj` | **PASS** | `PASSED 1065  FAILED 0` (covers all 15 areas in §4.2, including 196 state pairs, 400 consecutive daily sets & runtime `RagdollBudget` driver) |
| **Proven Offline** | `python3 Tools/audit/audit.py` | **PASS** | `0 findings (22 assemblies / 206 types / 196 assets)` (using official Unity URP/Core RP/TMP package script GUIDs) |
| **Proven Offline** | `python3 Tools/audit/balance.py` | **PASS** | `0 findings` (48 weapons, 18 vehicles, 59 equipment, 25 shop, 20 missions) |
| **Proven Offline** | `python3 Tools/audit/reference.py` | **PASS** | `0 findings` (ballistic family ordering, STK, $200,650 base / $319,033 3-star / 1,815 rep → Lv.19, 2.29 km² city) |
| **Proven Offline** | Partial Editor/Test syntax check (§4.1) | **PASS** | Only `CS0234`/`CS0246` from unstubbed `UnityEditor`/URP/NUnit packages; `0` syntax/member/arity defects |
| **Proven Offline** | `Tools/art/*` (`build_prompts.py`, `review_images.py`, `key_alpha.py`, `make_props.py`) | **PASS** | `185/185` prompts (`31+72+30+10+42`) extracted into `production/IMAGE_MANIFEST_185.json` (`--strict` `0` warnings); border-connected flood-fill keyer & 3 glTF smoke props |
| **Not Run (Needs Unity Editor)** | Unity `6000.3.28f1` batch import & Editor compile | **NOT RUN** | No Unity Editor installed on this Linux VM |
| **Not Run (Needs Unity Editor)** | `SceneBootstrap.BuildAllTestScenes` & `VerifyBuiltScenesBatch` | **NOT RUN** | Tracked in `production/VERIFY_BACKLOG.md` (`V-601`..`V-611`, `V-905`) |
| **Not Run (Needs Unity + Human)** | `PlayModeSmokeTest` (6 smoke tests + 1 `[Explicit]` slice test) | **NOT RUN** | Tracked under `V-4591` |
| **Not Run (Needs Unity + Human)** | Phase 2 Ragdoll 6-point visual validation & §1.9.9 GPU/SRP checks | **NOT RUN** | Recorded honestly in `production/PHASE2_RAGDOLL_VALIDATION.md` (`V-201`..`V-206`, `V-901`..`V-905`) |
| **Blocked** | Photon Fusion runtime compile (`#if PHOTON_FUSION`) | **BLOCKED** | Photon Fusion SDK intentionally excluded (§1.2 & P10; `V-1002`) |
| **Blocked** | Signed Android IL2CPP ARM64 APK & mid-tier phone profiler | **BLOCKED** | Requires Unity Android Build Support + `DUDECHAOS_KEYSTORE_*` + physical Android device (`V-1101`, `V-1102`) |

---

## Per-Phase Checkpoint Log

### P0 — Foundation, Data Layer, Tooling
- **Added**: Directory tree, `Packages/manifest.json`, `ProjectSettings/*.asset` + `ProjectVersion.txt` (`6000.3.28f1`), 22 acyclic `.asmdef` files, 15 `ScriptableObject` contracts (`MovementSettings`, `RagdollSettings`, `CombatSettings`, `VehicleSettings`, `WantedSettings`, `WeaponData`, `VehicleData`, `MissionData`, `EquipmentData`, `ShopItem`, `ImpactMaterialData`, `CameraShakeProfile`, `EmoteData`, `QualitySettingsPreset`, `DistrictDefinition`), `IDamageable`, `ICameraPunch`, `GameManager`, `PrefabPool`, `HitStopDriver`, `Mobile_Renderer.asset`, `Mobile_URP.asset`, `UniversalRenderPipelineGlobalSettings.asset`, `DefaultVolumeProfile.asset`, `Tools/CompileCheck`, `Tools/LogicTests`, `Tools/audit/audit.py`.
- **Proven**: `CompileCheck` 0/0, `audit.py` 0 findings, 0 assembly cycles, fixed deterministic GUIDs across all `.meta` files.
- **NOT RUN**: First open in Unity `6000.3.28f1` (`V-901`).

### P1 — Player, Camera, Input, State Machine
- **Added**: `PlayerState` (14 states), `PlayerStateMachine` (explicit transition table, `ExitingVehicle` ragdoll source, `InVehicle` ragdoll block), `IPlayerInput`, `InputSystemPlayerInput`, `LocomotionMath`, `PlayerMotor` (`Awake` self-resolving `IPlayerInput`), `HealthSystem`, `ThirdPersonCameraRig` (Cinemachine 3 `Lens` struct copy-edit-write), `PlayerLookController`, `RecoilApplier`, `SafeAreaFitter`.
- **Proven**: 220+ movement/state/health/camera assertions in `LogicTests` (including exact `-8.0` Medium landing bound).
- **NOT RUN**: In-editor movement & camera feel (`V-101`..`V-106`).

### P2 — Ragdoll (Mandatory Gate)
- **Added**: `RagdollActivationReason` (7 reasons), `IRagdollTargetSource`, `RagdollBudgetModel`, `RagdollBudget` (reads `Position`, safe on boneless prefabs), `RagdollController` (78 kg mass distribution, `< 1.1 m/s` for `1.8 s` recovery, `0.9 s` get-up, death lock), `PelvisFollowTarget` (`0.08 s` lag), `production/PHASE2_RAGDOLL_VALIDATION.md`.
- **Proven**: Budget distance/cap selection, boneless safety, recovery timer, death non-recovery in `LogicTests`.
- **NOT RUN**: Six visual PhysX validation questions (`V-201`..`V-206` in `production/PHASE2_RAGDOLL_VALIDATION.md`).

### P3 — Combat
- **Added**: `CombatMath`, `WeaponMechanics` (all 9 mechanics: Auto, Semi, Burst, Pump, Bolt, Charge, Beam, Projectile, Throwable), `AttachmentRules`, `AttachmentMount`, `BodyPartTag`, `WeaponController` (strict 10-step shot chain, non-alloc raycast buffer, hashed animator triggers), `PlayerWeaponInput`, `MeleeController`, `Explosion` (`OverlapSphereNonAlloc` 64-slot buffer), `DamageDummy`, `SupportRules`.
- **Proven**: Body-part multipliers `3.2 / 1.0 / 0.75`, all 9 mechanics, backstab `×1.8`, ragdolled `×1.6`, explosion linear falloff, pause-aware `HitStopDriver` in `LogicTests`.
- **NOT RUN**: Live combat feel & VFX/hitstop timing in Play Mode (`V-301`..`V-305`).

### P4 — Vehicles
- **Added**: `VehicleMath` (`IsUsableCenterOfMass` & `SafeCenterOfMass` `(0, -0.35, 0)` fallback), `VehicleController` (`0.45 s` enter/exit windows, arcade yaw + `Rigidbody.linearVelocity`, `>= 9 m/s` eject via `ExitingVehicle`, input fallback chain), `VehicleHealth` (`2.8 s` burn-to-explosion), `VehicleInteraction` (`3.5 m` non-alloc overlap, clears `_current` only when `TryExit()` succeeds), `VehicleLockRules`/`VehicleLock`, `ArmourRules`, `NitroRules`/`NitroSystem`, `TuningRules`.
- **Proven**: CoM NaN/Inf/out-of-range rejection, eject routing without softlock, `ASSET_VECTOR` 0 findings on all 18 vehicle assets.
- **NOT RUN**: Arcade driving feel in Play Mode (`V-401`..`V-405`).

### P5 — Wanted & Police
- **Added**: `WantedMath`, `WantedSystem` (`LevelChanged`, `CrimeReported`), `PoliceChaseMath`, `PoliceUnit` (LOS non-alloc raycast, open fire from Lv.2), `PoliceCar` (`Rigidbody` arcade pursuit & reverse-when-wedged), `PoliceSpawner` (level-matched counts, despawn on drop, no NavMesh).
- **Proven**: Zero-threshold skipping, `> 48 m` + out-of-LOS + `>= 9 s` cooldown gate, response table in `LogicTests`.
- **NOT RUN**: Live pursuit & roadblock behaviour in Play Mode (`V-501`..`V-504`).

### P6 — Vertical Slice Integration
- **Added**: `GameSettings`, `SettingsStore`, `QualityRules`, `QualityApplier`, `SceneSystemsContract`, `SaveData`/`SaveIntegrity`/`SaveSystem`, `GameFlowController`, `HudController`, `MobileJoystick`, `PauseMenuController`, `ProjectSetup`, `SceneBootstrap`, `PlayModeSmokeTest`, `Assets/Scenes/Test.unity`, `Assets/Scenes/TestSlice.unity`, `design/SETUP_GUIDE.md`.
- **Proven**: `SceneSystemsContract` single/multi-fault detection, settings clamping, save checksum/corruption rejection in `LogicTests`.
- **NOT RUN**: 11-point vertical slice walkthrough (`V-601`..`V-611`) and `PlayModeSmokeTest` (`V-4591`).

### P7 — Content, Economy, Missions, Retention
- **Added**: `Wallet`, `ProgressionCurve`, `ShopRules`/`ShopSystem`, `MissionRun` (`_nextIndex` counter, `34%` time bonus cap, `3` retries), `MissionGradeRules` (`40/40/20`, `NoClockScore = 20`, `int.TryParse`), `MissionModifierRules`, `MissionSelectRules`, `MissionBanterRules`, `MissionSystem`, `WeaponInventory`, `ConsumableRules`, `DailyChallengeRules`/`System`, `AchievementRules`/`System`, `StuntRules`/`System`, `BountyRules`/`System`, `SideJobDirector`, `ChallengeBinder` (event-only), `SaveCoordinator`, `MissionTriggerVolume`, `Tools/content/story_lines.py`, `Tools/content/gen_content.py`, 182 content `.asset` files.
- **Proven**: 400 consecutive daily sets with 3 distinct kinds each, duplicate mission objective keys, `balance.py` 0 findings, `reference.py` 0 findings ($200,650 base / $319,033 3-star / 1,815 rep → Lv.19).
- **NOT RUN**: Full 20-mission playthrough in Unity (`V-701`..`V-705`).

### P8 — City, NPCs, Animals, Streaming
- **Added**: 12 `DistrictDefinition` assets (`2.29 km²`), `DistrictBounds` (smallest containing wins), `DistrictRules`, `DistrictRegistry`, `DayNightRules`/`Cycle` (`hoursPerSecond = 1/60`, single `Light[]` loop), `InteriorPortal`, `HotelRules`/`Service`, `PropRules` (78 kinds, 8 groups, 46 chunk cap, 4 render tiers), `SideMissionRules`, `DistrictHeatRules`/`Binder`, `WorldStreamMath`/`Streamer` (64 m cells, 9 resident), `NpcSteering`, `NpcAvoidance`, `NpcTickLod`, `NpcSpawnMath`, `NpcAgent`, `NpcSpawner`, `AnimalRules`/`Agent`/`Spawner`, `design/CITY_PLAN.md`.
- **Proven**: Secret-inside-Downtown resolution, 78 prop specs, 60/20/6/0 Hz NPC LODs, deterministic avoidance, streaming cell ring in `LogicTests` and `reference.py`.
- **NOT RUN**: Streaming & traffic observation in Editor (`V-801`..`V-805`).

### P9 — Presentation: UI, Accessibility, Audio, Visuals
- **Added**: `AccessibilityPalette`, `LocalizationTable` (EN/VI), `AudioBudgetMath`/`AudioDirector` (fixed 24-voice pool), `DriftAudio`, `AnimationMath`, `ProceduralWalker`, `WheelSpin`, `SirenController`, `NeonFlicker`, `VehicleLights`, `MissionHud`, `DailyChallengeHud`, `StuntHud`, `NitroHud`, `ConsumableHud`, `SideMissionBoard`, `TuningPanel`, `AchievementToast`, `SettingsApplier`, `design/PERFORMANCE_BUDGET.md`.
- **Proven**: Tier fog density written to `RenderSettings.fogDensity` (`0.012 / 0.009 / 0.006`), R6 shadow/FPS ceiling enforcement, fixed 24-voice pool never grows in `LogicTests`.
- **NOT RUN**: Frame Debugger SRP Batcher & visual/audio inspection (`V-901`..`V-906`).

### P10 — Multiplayer Scaffold
- **Added**: `NetworkCommand` (fixed 32-byte wire format), `AuthorityValidator`, `INetworkTransport`, `LoopbackTransport`, `FusionNetworkTransport` (`#if PHOTON_FUSION` scaffold).
- **Proven**: 32-byte round-trip, two-endpoint loopback, stale-tick/teleport/speed-hack/fire-rate/damage-claim rejection in `LogicTests`.
- **BLOCKED**: Compiling `FusionNetworkTransport` against Photon Fusion SDK (`V-1002`).

### P11 — Hardening, Art Pipeline, Release
- **Added**: `Tools/art/` (`setup_tools.sh`, `make_props.py`, `optimize_art.sh`, `optimize_textures.py`, `build_prompts.py`, `review_images.py`, `process_step_renders.py`, `key_alpha.py`), `production/IMAGE_MANIFEST_185.json` (`185` items), `BuildScript.cs` (env-var keystore gate), `Tools/verify/checklist.py`, `production/VERIFY_BACKLOG.md`, `production/RELEASE_CHECKLIST.md`, `design/ART_BIBLE.md`, **`buoc_01.zip` (`31/31` verified `.webp` assets)**, plus **Step 2 renders `122`..`165` (`22/72` verified `.webp` assets in `renders/buoc_02/`)**.
- **Proven**: `buoc_01: 31/31 ảnh, 0 lỗi` sealed in `buoc_01.zip` & verified via `review_images.py`; `buoc_02: 22/72` verified via `review_images.py --allow-partial`.
- **BLOCKED**: Android APK build & mid-tier phone profiling (`V-1101`, `V-1102`); remaining `50/72` Step 2 renders (`166`..`750`) + Steps 3–5 paused at `gemini-3.1-flash-image` weekly quota (`retry after ~120h`).

