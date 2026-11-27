# Dude Chaos Wars — Rendering & Performance Budget (§1.9 & P9)

> **Evidence Note (§1.3 & §1.9):** Values below are **contract targets** enforced by architecture and offline rules (`PropRules`, `RagdollBudgetModel`, `AudioBudgetMath`, `QualityRules`, `NpcTickLod`). Real Android GPU/CPU timings remain **NOT RUN / BLOCKED** (`V-1102`) until profiled on physical hardware.

---

## 1. Frame, Memory & Package Targets (§1.2 & §1.9.7)

| Metric | Mid-Tier Target (Tier 1) | High-Tier Target (Tier 2) | Enforcement Mechanism |
|---|---|---|---|
| Frame time | **<= 33.3 ms (30 fps)** | **<= 16.6 ms (60 fps)** | `QualityApplier` EMA auto-step (demote after 4 s >115% budget, promote after 20 s held) |
| Draw calls | **<= 150** | **<= 150** | SRP Batcher ON, <= 4 materials/asset, <= 8 `PropGroup` batches/district |
| SetPass calls | **<= 200** | **<= 200** | Single URP/Lit shader family, zero renderer features |
| Visible triangles | **<= 250,000** | **<= 250,000** | 3-level LODs (100% / 40% / 10%) + 130 m prop cutoff + 64 m cell streaming |
| Texture memory | **<= 350 MB** | **<= 350 MB** | 1024/2048 atlas caps (`optimize_textures.py`), ASTC compression |
| Post-warmup GC alloc | **0 B / frame** | **0 B / frame** | `OverlapSphereNonAlloc`, `PrefabPool`, cached animator hashes, static `RagdollBudget` |
| Cold start time | **<= 6.0 s** | **<= 6.0 s** | IL2CPP ARM64, 9-cell initial stream window, zero synchronous `Resources.LoadAll` |
| Final APK size | **200–400 MB** | **200–400 MB** | Strip engine code, ARM64-only ABI, shared texture atlases |

---

## 2. URP Pipeline Lock (`Assets/Settings/Mobile_URP.asset` & `Mobile_Renderer.asset`)

| Parameter | Shipped Value | Rationale |
|---|---|---|
| Renderer Mode | `m_RenderingMode: 0` (Forward) | Lowest mobile tile-memory bandwidth; zero G-buffer overhead |
| Renderer Features | `[]` (None) | No SSAO, no screen-space decals, no custom full-screen passes |
| Camera Depth / Opaque Texture | Both `0` (Off) | Prevents mid-frame tile resolves on mobile GPUs |
| Main Light Shadows | `1` (On), `2048` map, `1` cascade, `50 m`, Soft Shadows `0` | Single cheapest directional sun shadow map |
| Additional Lights | Per-object limit `4`, Shadows `0` | Street lamps illuminate geometry without casting point shadows |
| SRP Batcher / Dynamic Batching | SRP Batcher `1`, Dynamic `0` | Constant-buffer batching across shared URP shaders |
| Post-Processing | `DefaultVolumeProfile` (`m_Components: []`) | Raw forward output; zero post-process cost on Tier 0 |

---

## 3. Runtime Device Tiers (`QualityRules` & `QualityApplier`)

Tier classification uses `SystemInfo.systemMemorySize` and `SystemInfo.processorCount` (never device model strings):
- **Tier 2 (High):** RAM >= 6000 MB **and** cores >= 8
- **Tier 0 (Low):** RAM < 3000 MB **or** cores < 4
- **Tier 1 (Medium):** All other devices

| Parameter | Tier 0 (Low) | Tier 1 (Medium) | Tier 2 (High) |
|---|---|---|---|
| Shadows | **Off** (`22 m` cap) | **On**, `45 m` | **On**, `70 m` |
| `lodBias` | `0.6` | `1.2` | `2.0` |
| MSAA (`pipeline.msaaSampleCount`) | `1x` (0x AA) | `2x` | `4x` |
| `Application.targetFrameRate` | `30` | `30` | `60` |
| `RenderSettings.fogDensity` | `0.012` | `0.009` | `0.006` |
| Active NPC budget | `12` | `24` | `40` |

**Single-Writer Rule (R6):** `QualityApplier` is the **sole writer** of `QualitySettings.shadows`, `shadowDistance`, and `Application.targetFrameRate`. Player preferences feed `QualityApplier.SetPlayerSettings(...)`, which may **lower** cost below the device tier but never raise it above the tier.

---

## 4. Simulation & Audio Budgets

- **Ragdolls (`RagdollBudget`):** Hard ceiling of **12 full-simulation ragdolls** within **55 m** (`totalMass = 78 kg`, sleep below `0.5 m/s`, recovery below `1.1 m/s` for `1.8 s`). One static manager updates budgets; individual ragdolls carry no `Update()`.
- **NPCs (`NpcTickLod`):** `< 25 m` -> `60 Hz`; `25–55 m` -> `20 Hz`; `55–95 m` -> `6 Hz`; `> 95 m` -> `0 Hz` (frozen/despawned).
- **Props (`PropRules`):** Max **46 props per 40 m chunk**; `<= 40 m` shadowed, `40–85 m` unshadowed simplified, `85–130 m` flat decals, `> 130 m` culled behind exponential fog.
- **Audio (`AudioDirector` + `AudioBudgetMath`):** Fixed voice pool (**12 near + 8 mid + 4 far = 24 3D voices + 1 2D UI source**, `0` voices spawned beyond `90 m`). Pool never allocates new `AudioSource` components at runtime.
