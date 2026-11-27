# Dude Chaos Wars — Android Release Checklist (P11)

No release candidate APK may be declared shippable until **every** gate below is genuinely `PASS` with recorded command output or device evidence. Never upgrade `NOT RUN` or `BLOCKED` to `PASS` without running the step (§1.3).

---

## 1. Offline Engineering & Content Gates

- [x] `dotnet build Tools/CompileCheck/CompileCheck.csproj` -> `0 Warning(s)`, `0 Error(s)`
- [x] `dotnet run --project Tools/LogicTests/LogicTests.csproj` -> `PASSED 1065  FAILED 0`
- [x] `python3 Tools/audit/audit.py` -> `0 findings (22 assemblies / 206 types / 196 assets)`
- [x] `python3 Tools/audit/balance.py` -> `0 findings`
- [x] `python3 Tools/audit/reference.py` -> `0 findings` (`$200,650` base, `$319,033` 3-star, `1,815` rep -> Lvl 19, `2.29 km²`)
- [x] `git ls-files --eol` shows zero `i/lf w/crlf` churn (Landmine #23)

---

## 2. Unity `6000.3.28f1` Editor & Scene Contract Gates

- [ ] Project opens in Unity `6000.3.28f1` with zero Console compiler errors (`Assets/Editor/**` + `Assets/Scripts/**` + `Assets/Tests/**`)
- [ ] `DudeChaos -> Setup -> 0` (TMP Essential Resources imported, Landmine #9)
- [ ] `DudeChaos -> Setup -> 1` (`Mobile_Renderer` + `Mobile_URP` verified against §1.9.1; zero pink materials — `V-901`)
- [ ] `DudeChaos -> Setup -> 2` (Android `com.dudechaos.wars`, IL2CPP, ARM64 only, min SDK 25, Linear colour, Landscape)
- [ ] `SceneBootstrap.BuildAllTestScenes` builds `Test.unity` and `TestSlice.unity` cleanly with zero `"has no serialized field"` warnings
- [ ] `SceneBootstrap.VerifyBuiltScenesBatch` exits `0` on all built scenes and exits non-zero on a stripped test copy (`V-905`)
- [ ] `PlayModeSmokeTest` passes all 6 tests in `-batchmode -nographics` (`V-4591`)

---

## 3. Mandatory Gameplay & Presentation Gates

- [ ] **Phase 2 Ragdoll Gate (`V-201`..`V-206`):** All 6 questions in `production/PHASE2_RAGDOLL_VALIDATION.md` watched and passed in Play Mode
- [ ] **Vertical Slice (`V-601`..`V-611`):** All 11 slice items walked end-to-end by a human tester
- [ ] **20-Mission Story Chain (`V-701`..`V-705`):** Playable 1..20 with Vietnamese dialogue, par grading, variants, 3 daily challenges, and 13 achievements
- [ ] **12-District City (`V-801`..`V-805`):** 64 m streaming (9 active cells), `DayNightCycle`, `InteriorPortal`, `HotelService`, NPCs, and animals verified
- [ ] **Rendering & Audio (`V-902`..`V-906`):** SRP Batcher verified in Frame Debugger (`<= 150` draw calls), per-tier `QualityApplier` + `RenderSettings.fogDensity` verified, EN/VI localization & colour-blind modes verified
- [ ] **Art Replacement (`V-1103`):** Keyed 2D UI/VFX assets, `.fbx`/`.glb` meshes within §3.3 tri/material budgets, and emissive URP/Lit materials imported

---

## 4. Android Signing, Build & Physical Device Gates

- [ ] Environment secrets exported (`DUDECHAOS_KEYSTORE_PATH`, `DUDECHAOS_KEYSTORE_PASS`, `DUDECHAOS_KEYALIAS_NAME`, `DUDECHAOS_KEYALIAS_PASS`) — never committed to git
- [ ] Signed release APK built via `DudeChaos.EditorTools.BuildScript` (`V-1101`), size inside **200–400 MB**
- [ ] Installed and profiled on a mid-tier Android phone (`V-1102`):
  - Frame time `<= 33.3 ms` sustained (60 fps on Tier 2 hardware)
  - Draw calls `<= 150`, SetPass `<= 200`, Triangles `<= 250k`, Textures `<= 350 MB`
  - Steady-state managed GC allocation `0 B/frame`
  - Cold boot to playable scene `<= 6.0 s`
  - Pause/home-button autosave (`OnApplicationPause`) restores Format-4 save cleanly after process kill
- [ ] `python3 Tools/verify/checklist.py summary` reports `TODO: 0 | FAIL: 0` (excluding documented `V-1002` Photon scaffold)
