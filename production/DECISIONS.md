# Dude Chaos Wars — Architectural & Engineering Decisions (`D-001`..`D-013`)

Every locked stack choice, trade-off, and gate deferral is recorded here so future contributors do not re-introduce known landmines (§1.2–1.9 & §6).

---

### D-001 — Lock Engine to Unity 6.3 LTS (`6000.3.28f1`) + URP Forward (`m_RenderingMode: 0`)
- **Context:** Unity 2022.3 and 6.0 are EOL; mobile GPUs suffer tile-resolve stalls on Deferred or depth/opaque prepasses.
- **Decision:** Lock `ProjectVersion.txt` to `6000.3.28f1` and commit `Mobile_URP.asset` + `Mobile_Renderer.asset` in Forward mode (`m_RenderingMode: 0`, SRP Batcher ON, 1x MSAA, HDR ON, 50 m 1-cascade sun shadow, depth/opaque textures OFF, zero renderer features), wired into `GraphicsSettings` and all 6 `QualitySettings` levels.
- **Consequence:** Zero pink screens on first open; `audit.py` enforces version lock (`PROJECT_VERSION`).

### D-002 — Single Root `GameManager` Service Registry (No Singleton Sprawl)
- **Context:** Independent `WantedManager.Instance` / `AudioManager.Instance` / `SaveManager.Instance` singletons create hidden initialization-order bugs and leak across scene reloads.
- **Decision:** Only `GameManager` acts as the root coordinator; domain subsystems register explicitly with it.
- **Consequence:** Deterministic scene teardown and isolated offline testing.

### D-003 — Strict Acyclic 22-Assembly Tree (`Assets/**/*.asmdef`)
- **Context:** Two circular dependencies previously broke Unity compilation (`City -> Progression -> Wanted -> City` and `Progression -> GameFlow -> Progression`, Landmines #4 & #5).
- **Decision:** Enforce the exact 22-assembly DAG in §1.5. Keep `DistrictHeatRules`/`DistrictHeatBinder` inside `DudeChaos.City` (referencing `Wanted`), and let `GameFlow` push `NotifySideJobs(int)` into `Progression` so `Progression` never references `GameFlow`.
- **Consequence:** Proven acyclic on every commit by `Tools/audit/audit.py` (`ASMDEF_CYCLE` + `USING_NOT_REFERENCED`).

### D-004 — Pure Static C# Logic Separated from Thin MonoBehaviour Drivers
- **Context:** Build and CI environments frequently lack a licensed Unity Editor.
- **Decision:** Put all math, state machines, grading, economy, streaming, steering, save integrity, and quality selection into pure C# classes with zero scene dependency, covered by `Tools/CompileCheck` and `Tools/LogicTests`.
- **Consequence:** 1,000+ assertions run in <2 seconds via `dotnet run`, while docs explicitly state offline stubs do not prove Unity Editor compatibility.

### D-005 — Deterministic Hand-Written YAML Assets with Named-Field Generator
- **Context:** Positional generator bugs previously shifted weapon classes/auto flags and injected strings into `Vector3`/`AnimationCurve` floats, crashing PhysX `Simulate` without an exception (Landmines #1, #24, #25).
- **Decision:** Generate all 196 `.asset` and `.asset.meta` files from `Tools/content/gen_content.py` using frozen dataclasses and deterministic MD5 GUIDs (`md5("DudeChaosUnity:" + rel_path)`), plus runtime `VehicleMath.SafeCenterOfMass` fallback `(0, -0.35, 0)`.
- **Consequence:** Verified clean by `audit.py` (`ASSET_FIELD_MISMATCH`, `ASSET_VECTOR`), `balance.py`, and `reference.py`.

### D-006 — Arcade Yaw + Forward-Force Vehicle Physics (Reject `WheelCollider`)
- **Context:** `WheelCollider` suspension tuning fights slapstick collisions, flips unpredictably on curbs, and burns mobile physics budget across traffic cars.
- **Decision:** Use a single `Rigidbody` arcade yaw + downforce (`12`) + `brakeForce > acceleration` model, paired with 0.45 s enter/exit windows and high-speed ragdoll ejection (`>= 9 m/s` routed via `ExitingVehicle -> Ragdoll`, Landmine #13).
- **Consequence:** Snappy Gang-Beasts/Dude-Theft-Wars driving feel and cheap `PoliceCar` pursuit without NavMesh.

### D-007 — Reject DOTween and `.inputactions` Asset Indirection
- **Context:** DOTween adds a manual Asset Store dependency for trivial UI/camera lerps; `.inputactions` codegen assets add fragile GUID/import state for basic controls.
- **Decision:** Use lightweight coroutines/`Mathf.MoveTowards` for tweens, and poll `Keyboard.current` / `Mouse.current` / `Gamepad.current` directly inside `InputSystemPlayerInput` alongside `SetVirtualMove`/`SetVirtualLook` for `MobileJoystick`. `PlayerMotor.Awake` self-resolves `GetComponent<IPlayerInput>()` (Landmine #11).
- **Consequence:** Zero external tween packages; disk-loaded scenes move immediately without runtime `Bind()`.

### D-008 — Single Static Budget Drivers for Ragdolls and Audio (Zero Scene-Scanning `Update()`)
- **Context:** Per-ragdoll, per-lamp, or per-frame `FindObjectsByType` loops destroy frame pacing on mid-tier Android CPUs.
- **Decision:** `RagdollBudget` runs one static selection pass reading `Position` (never `ReadBonePositions()[0]`, Landmine #18) to cap full-sim bodies at `12` within `55 m`. `AudioDirector` pre-allocates a fixed `12 near / 8 mid / 4 far` 3D pool + 1 UI source (`0` beyond `90 m`). `DayNightCycle` updates all street lamps in one array loop.
- **Consequence:** Predictable O(active) CPU cost and 0 B/frame steady-state GC allocation.

### D-009 — Single-Writer `QualityApplier` + Applied Per-Tier Fog (Fixes §1.9.8 R2, R3, R6)
- **Context:** Four scripts previously overwrote `QualitySettings.shadowDistance` and `Application.targetFrameRate` (R6), `QualityPreset.FogDensity` was never written to `RenderSettings` (R3 / Landmine #29), and URP ignored `QualitySettings.antiAliasing` unless `pipeline.msaaSampleCount` was updated (R2 / Landmine #30).
- **Decision:** `QualityApplier` exclusively owns `shadows`, `shadowDistance`, `lodBias`, `msaaSampleCount`, `targetFrameRate`, and `RenderSettings.fog`/`fogDensity`. `SettingsStore` and `SettingsApplier` route player overrides through `QualityApplier.SetPlayerSettings`, clamped by `QualityRules` so player settings can only lower cost below the hardware tier, never raise it. Enforce presence via `SceneSystemsContract`.
- **Consequence:** Hardware tiers hold under user settings changes, and 130 m prop culling blends into atmospheric fog.

### D-010 — Format-4 Checksummed Save Envelope with `int.TryParse` & Pause Autosave
- **Context:** Android reliably delivers `OnApplicationPause(true)` rather than `OnApplicationQuit`, and `int.Parse` on tampered/corrupt save strings throws `OverflowException` during boot (Landmine #16).
- **Decision:** Wrap JSON payloads in a Format-4 envelope with FNV-1a `checksum`, migrate v1->v2->v3->v4 with clamped bounds, parse all integers/dates defensively (`int.TryParse`, `DateTime.DaysInMonth`, Landmines #15–#16), reject corrupt slots, autosave on pause/quit, and intentionally do not resume half-finished missions.
- **Consequence:** Crash-safe persistence across upgrades and malformed `PlayerPrefs` data.

### D-011 — Multiplayer Kept Strictly as Offline Authority Scaffold (Reject Runtime Photon Fusion)
- **Context:** Shipping real netcode before the single-player sandbox is proven wastes months, and Photon Fusion cannot be compiled/verified without installing its proprietary SDK (§1.2 & §4.4).
- **Decision:** Ship `NetworkCommand` (fixed 32-byte binary serializer), `AuthorityValidator` (speed/teleport/fire-rate/damage/stale-tick checks), and `LoopbackTransport` for local authority tests; guard `FusionNetworkTransport` behind `#if PHOTON_FUSION` and mark `V-1002` `BLOCKED`.
- **Consequence:** Authority math is unit-tested today without polluting single-player runtime dependencies.

### D-012 — Deliberate 2-Star Ceiling on Clockless Missions (Landmine #28)
- **Context:** Mission grading awards `40` completion + `40` clock + `20` clean (`100` max), requiring `>= 85` points and finishing inside par (`70%` of limit) for 3 stars.
- **Decision:** Keep `NoClockScore = 20` on missions with `timeLimitSeconds <= 0` so clockless runs top out at `80` points (2 stars: *"can be good, never perfect"*), while all 20 story missions specify explicit limits (`240–540 s`).
- **Consequence:** Consistent grading rules with zero special-case 3-star inflation.

### D-013 — Explicit Deferral of Live Unity Play Mode & Phase 2 Ragdoll Visual Gate on Headless VM
- **Context:** Engineering Rule 1 & P2 require watching all 6 ragdoll validation questions (`V-201`..`V-206`), PlayMode smoke tests (`V-4591`), and URP rendering checks (`V-901`..`V-905`) inside a real Unity `6000.3.28f1` Editor, which does not exist on this headless Linux rebuild machine.
- **Decision:** Proceed through Phases P3–P11 for code, deterministic assets, offline logic tests, and tooling while recording **every** un-watched runtime gate as `NOT RUN` (`TODO` / `BLOCKED`) in `production/PHASE2_RAGDOLL_VALIDATION.md` and `production/VERIFY_BACKLOG.md`. Approved per §0 & §7 rebuild protocol.
- **Consequence:** Complete repository delivered with 100% honest verification telemetry and zero fabricated `PASS` entries.
