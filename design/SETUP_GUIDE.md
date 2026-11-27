# Dude Chaos Wars — Unity 6.3 LTS Setup & Verification Guide

## 1. Locked Toolchain (§1.2)

| Component | Locked Version / Value | Notes |
|---|---|---|
| Unity Editor | **`6000.3.28f1` (Unity 6.3 LTS)** | Supported through Dec 2027. Do NOT downgrade to 6.0 or 2022.3. |
| Render Pipeline | **URP 17.3.0** (`Mobile_URP.asset` + `Mobile_Renderer.asset`) | Forward mode (`m_RenderingMode: 0`), SRP Batcher on, 1x MSAA, HDR on, 50 m single-cascade shadows. |
| Input | **Input System 1.20.1** + **uGUI 2.0.0** | Polls `Keyboard.current` / `Mouse.current` / `Gamepad.current` directly + virtual touch hooks. |
| Camera | **Cinemachine 3.1.7** | Third-person rig only (`CinemachineBrain` + `CinemachineCamera`). |
| Android Target | **IL2CPP, ARM64 only, min API 25**, Linear colour, Landscape | Bundle ID: `com.dudechaos.wars`. |
| Physics | Fixed Timestep **`0.02`**, `m_AutoSyncTransforms: 0` | Non-alloc overlap/raycast buffers only. |

---

## 2. Offline Verification (No Unity Editor Needed)

The offline harness under `Tools/` verifies C# syntax/type-checking against hand-written stubs, pure static gameplay math, assembly graph acyclicity, YAML asset schemas, and economy/ballistic curves.

> **Important (§1.3 & §4.1):** Passing the offline harness is **NOT** proof of Unity Editor compatibility or Play Mode behaviour. `CompileCheck` excludes `Assets/Editor/**`.

```bash
# 1. Offline C# compile check (Assets/Scripts/** against UnityStubs.cs)
dotnet build Tools/CompileCheck/CompileCheck.csproj

# 2. Pure C# gameplay logic assertions
dotnet run --project Tools/LogicTests/LogicTests.csproj

# 3. Pre-import project, assembly graph, GUID, field-name, and vector audit
python3 Tools/audit/audit.py

# 4. Weapon DPS, vehicle speed/braking, mission payout, and shop balance audit
python3 Tools/audit/balance.py

# 5. Ballistic muzzle-energy ordering, shots-to-kill, par table, and district area audit
python3 Tools/audit/reference.py

# 6. Inspect remaining Play Mode / hardware verification items
python3 Tools/verify/checklist.py summary
```

To regenerate all 196 deterministic ScriptableObject and URP `.asset` files:

```bash
python3 Tools/content/gen_content.py
```

---

## 3. First Open in Unity `6000.3.28f1` (§1.9.1 & §4.7)

1. Open the repository root in **Unity Hub** using **`6000.3.28f1`** with Android Build Support (SDK/NDK + IL2CPP).
2. Run the editor setup menu items in order:
   - **`DudeChaos -> Setup -> 0`**: Import TextMeshPro Essential Resources (prevents TMP popup mid-Play, Landmine #9).
   - **`DudeChaos -> Setup -> 1`**: Verify/assign `Assets/Settings/Mobile_Renderer.asset` and `Assets/Settings/Mobile_URP.asset` across `GraphicsSettings` and all 6 `QualitySettings` levels (`"Anything pink is now a missing material, not a missing pipeline"`). Diff `m_MSAA`, `m_SupportsHDR`, `m_ShadowDistance`, and `m_RenderScale` against §1.9.1 so inspector edits never drift silently (Landmine #32).
   - **`DudeChaos -> Setup -> 2`**: Apply locked Android player settings (`com.dudechaos.wars`, IL2CPP, ARM64, min SDK 25, Linear colour, Landscape).
   - **`DudeChaos -> Setup -> 3`**: Print full configuration report to Console.
3. Build scenes from `SceneBootstrap` (`DudeChaos -> Build All Test Scenes`), ensuring scenes are created **before** loading ScriptableObject settings assets (Landmine #10).
4. Verify every built scene satisfies `SceneSystemsContract` (`Systems` GameObject with `SettingsStore`, `QualityApplier`, `SettingsApplier` wired).

---

## 4. Headless Unity Batch Commands (§4.7)

Run on a workstation with Unity `6000.3.28f1` installed:

```bash
UNITY="/path/to/Unity/6000.3.28f1/Editor/Unity"
PROJ="/path/to/game"

# 1. Build test scenes
"$UNITY" -batchmode -quit -projectPath "$PROJ" \
  -executeMethod DudeChaos.EditorTools.SceneBootstrap.BuildAllTestScenes -logFile build_scenes.log

# 2. Verify Systems contract in every built scene (exits non-zero on fault)
"$UNITY" -batchmode -quit -projectPath "$PROJ" \
  -executeMethod DudeChaos.EditorTools.SceneBootstrap.VerifyBuiltScenesBatch -logFile verify_scenes.log

# 3. Run PlayMode smoke tests (-nographics required so batch camera does not hang, Landmine #21)
"$UNITY" -batchmode -nographics -projectPath "$PROJ" \
  -runTests -testPlatform PlayMode -testResults results.xml -logFile playmode_tests.log
```

---

## 5. Signed Android APK Build

`BuildScript` reads signing credentials strictly from environment variables and refuses to emit an unsigned release APK:

```bash
export DUDECHAOS_KEYSTORE_PATH="/secure/path/dudechaos.keystore"
export DUDECHAOS_KEYSTORE_PASS="..."
export DUDECHAOS_KEYALIAS_NAME="dudechaos_release"
export DUDECHAOS_KEYALIAS_PASS="..."
```

Record all real editor and device verification results in `production/VERIFY_BACKLOG.md` via `python3 Tools/verify/checklist.py`.
