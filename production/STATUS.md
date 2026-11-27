# Dude Chaos Wars — Project Status Report

**Target Stack:** Unity `6000.3.28f1` (6.3 LTS) · URP `17.3.0` · Input System `1.20.1` · Cinemachine `3.1.7` · Android ARM64 IL2CPP (min API 25)

---

## 1. Evidence Separation (§1.10 & §7)

### A. Proven Offline (Executed on Linux Build Environment)

| Check / Deliverable | Status | Measured Result |
|---|---|---|
| `dotnet build Tools/CompileCheck/CompileCheck.csproj` | **PASS** | **`0 Warning(s), 0 Error(s)`** across all 162 runtime C# scripts (`Assets/Scripts/**/*.cs`, `TreatWarningsAsErrors=true`) |
| `dotnet run --project Tools/LogicTests/LogicTests.csproj` | **PASS** | **`PASSED 1065  FAILED 0`** across all 15 pure-logic test suites (§4.2) |
| `python3 Tools/audit/audit.py` | **PASS** | **`0 findings (22 assemblies / 206 types / 196 assets)`** (5 `.unity` scenes including 3 PhysX bisect scenes, official Unity URP/CoreRP/TMP GUIDs) |
| `python3 Tools/content/story_lines.py` | **PASS** | 20 missions x 3 Vietnamese dialogue lines, 10 Sóng FM radio segments, 0 continuity violations |
| `python3 Tools/content/gen_content.py` | **PASS** | **196 `.asset` files + 196 `.asset.meta` files** generated deterministically (48 weapons, 18 vehicles, 59 equipment, 25 shop, 20 missions, 12 districts, 10 gameplay settings/misc, 4 URP/Volume) |
| `python3 Tools/audit/balance.py` | **PASS** | **0 findings** (all 48 weapons valid class/mag/range & non-excluded DPS in `55..620`; all 18 vehicles `brake > accel` & within speed bands; all 25 shop & 59 equipment items affordable within `$319,033` / Lvl 19) |
| `python3 Tools/audit/reference.py` | **PASS** | **0 findings** (strict ballistic muzzle-energy ordering per family, Semi Sniper 2 hits < Revolver .44 3 hits, 20 missions = `$200,650` base / `$319,033` 3-star / `1,815` rep -> Lvl 19, 12 districts = `2.29 km²`) |
| `python3 Tools/verify/checklist.py summary` | **PASS** | 59 verification items tracked (`56 TODO`, `0 PASS`, `0 FAIL`, `3 BLOCKED`) |
| `Tools/art/*` (`build_prompts.py`, `review_images.py`, `process_step_renders.py`, `key_alpha.py`, `optimize_textures.py`, `make_props.py`) | **PASS** | **`185/185` prompts** (`31+72+30+10+42`) extracted into `production/IMAGE_MANIFEST_185.json` (`0` warnings); **`buoc_01.zip` (`31/31 KEEP, 0 REDO`)** sealed; **`renders/buoc_02/` (`22/72 KEEP, 0 REDO`)** verified (`50` remaining queued for next `gemini-3.1-flash-image` window) |
| `ProjectSettings/*.asset` | **PASS** | All 9 Unity manager YAML files + `ProjectVersion.txt` (`6000.3.28f1`) committed with `Mobile_URP` and built scene GUIDs wired |

### B. Proven in Unity Editor

- **NONE (`NOT RUN` on this Linux container — Unity `6000.3.28f1` is not installed on the build VM).**
- Editor C# scripts under `Assets/Editor/**` (`SceneBootstrap`, `ProjectSetup`, `BuildScript`), URP material rendering (`V-901`..`V-905`), Phase 2 Ragdoll 6-question gate (`V-201`..`V-206`), and PlayMode smoke tests (`V-4591`) require opening the project in Unity `6000.3.28f1`.

### C. Blocked on External Dependencies / Hardware

- **`V-1002` (`BLOCKED`):** `FusionNetworkTransport` (`#if PHOTON_FUSION`) — Photon Fusion SDK intentionally excluded from `Packages/manifest.json` (§1.2 & D-011).
- **`V-1101` (`BLOCKED`):** Signed Android ARM64 IL2CPP APK build — requires Unity `6000.3.28f1` + Android SDK/NDK + `DUDECHAOS_KEYSTORE*` secrets.
- **`V-1102` (`BLOCKED`):** Mid-tier Android hardware GPU/CPU/memory profiling against §1.9.7 budgets — requires physical device.

---

## 2. Next Concrete Action for Human Operator

1. Open the repository on a workstation with **Unity `6000.3.28f1`**, run `DudeChaos -> Setup -> 0 / 1 / 2 / 3`, then run `BuildAllTestScenes` and `VerifyBuiltScenesBatch`.
2. Execute the Play Mode smoke suite (`-batchmode -nographics -runTests -testPlatform PlayMode`) to resolve `V-4591`, then walk `V-201`..`V-206` (`production/PHASE2_RAGDOLL_VALIDATION.md`) in interactive Play Mode.
