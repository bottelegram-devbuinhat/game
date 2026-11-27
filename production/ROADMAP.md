# Dude Chaos Wars — Phase Roadmap (P0–P11)

Each phase enforces four non-negotiable gates (§1.10):
1. `dotnet build Tools/CompileCheck/CompileCheck.csproj` -> **0 errors, 0 warnings**
2. `dotnet run --project Tools/LogicTests/LogicTests.csproj` -> **0 failed assertions**
3. `python3 Tools/audit/audit.py`, `balance.py`, `reference.py` -> **0 findings**
4. Every runtime / visual / hardware check not watched live is logged as **`NOT RUN`** or **`BLOCKED`** in `production/VERIFY_BACKLOG.md`.

| Phase | Name | Core Deliverables | Offline Gate | Unity / Device Gate |
|---|---|---|---|---|
| **P0** | Foundation, Data & Tooling | 22 `.asmdef` acyclic graph, 15 ScriptableObject contracts, `GameManager`, URP assets (`Mobile_URP` + `Mobile_Renderer`), `CompileCheck`, `LogicTests`, `audit.py` | Compile 0/0 + `audit.py` 0 findings | `V-601`, `V-901` (`NOT RUN`) |
| **P1** | Player, Camera, Input & FSM | `MovementSettings`, 14-state `PlayerStateMachine`, `PlayerMotor` (`Awake` input self-resolve), `InputSystemPlayerInput`, `ThirdPersonCameraRig`, `HealthSystem` | 45+ logic assertions | `V-101`..`V-106` (`NOT RUN`) |
| **P2** | Ragdoll (Mandatory Gate) | `RagdollSettings`, 7 activation reasons, `RagdollBudgetModel`/`RagdollBudget` (<=12 full-sim, 55 m), `RagdollController`, `PelvisFollowTarget`, `PHASE2_RAGDOLL_VALIDATION.md` | Pure budget & CoM math assertions | `V-201`..`V-206` (`NOT RUN` — D-013) |
| **P3** | Combat & Shot Chain | `CombatSettings`, 10-step `WeaponController`, 9 `WeaponMechanics`, `AttachmentRules`, `BodyPartTag` (3.2 / 1.0 / 0.75), `MeleeController`, `Explosion`, `PrefabPool`, `HitStopDriver` | 100+ assertions | `V-301`..`V-305` (`NOT RUN`) |
| **P4** | Arcade Vehicles | `VehicleSettings`, `VehicleController` (0.45 s enter/exit, >=9 m/s eject via `ExitingVehicle`, `SafeCenterOfMass`), `VehicleHealth` (2.8 s boom), `VehicleLock`, `ArmourRules` | 123+ assertions + `ASSET_VECTOR` 0 findings | `V-401`..`V-405` (`NOT RUN`) |
| **P5** | Wanted & Police | `WantedSettings` (8 crime weights, `{1,4,9,16,25,36}`), `WantedSystem`, `PoliceUnit` (LOS, fire >= star 2), `PoliceCar` (no NavMesh), `PoliceSpawner` | 157+ assertions | `V-501`..`V-504` (`NOT RUN`) |
| **P6** | Vertical Slice | `GameSettings`/`SettingsStore`, `SaveSystem` (3 slots + pause autosave), `HudController`, `MobileJoystick`, `SceneBootstrap`, `ProjectSetup`, 6 `PlayModeSmokeTest` tests | Offline harness green | `V-601`..`V-611`, `V-4591` (`NOT RUN`) |
| **P7** | Content, Economy & Retention | 48 weapons, 18 vehicles, 59 equipment, 25 shop items, 20 missions (`$200,650` / `1,815` rep -> Lvl 19 / `$319,033` 3-star), grading, variants, dailies, 13 achievements, stunts, bounty | `balance.py` & `reference.py` 0 findings | `V-701`..`V-705` (`NOT RUN`) |
| **P8** | City, NPCs, Animals & Streaming | 12 districts (`2.29 km²`), 64 m `WorldStreamer` (9 active cells), `DayNightCycle`, `InteriorPortal`, `HotelService`, `PropRules` (78 kinds, 46 cap), `NpcSpawner` (60/20/6/0 Hz), `AnimalSpawner` | 240+ assertions + area check | `V-801`..`V-805` (`NOT RUN`) |
| **P9** | Presentation, UI, Audio & A11y | `QualityApplier` (single writer + per-tier fog), `AccessibilityPalette`, `LocalizationTable` (EN/VI), `AudioDirector` (12/8/4/0 fixed pool), `ProceduralWalker`, `WheelSpin`, `SirenController`, `NeonFlicker` | 270+ assertions | `V-901`..`V-906` (`NOT RUN`) |
| **P10** | Multiplayer Authority Scaffold | Fixed 32-byte `NetworkCommand`, `AuthorityValidator`, `LoopbackTransport`, `#if PHOTON_FUSION` `FusionNetworkTransport` stub | 273+ assertions | `V-1001` (`NOT RUN`), `V-1002` (`BLOCKED`) |
| **P11** | Art Pipeline, Hardening & Release | `Tools/art/*` (`key_alpha.py`, `review_images.py`, `optimize_textures.py`, `build_prompts.py`, `make_props.py`), `BuildScript`, `RELEASE_CHECKLIST.md`, `VERIFY_BACKLOG.md` | All offline tools verified | `V-1101`..`V-1102` (`BLOCKED`), `V-1103` (`TODO`) |
