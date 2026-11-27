# Dude Chaos Wars — Production Performance Sign-Off Ledger (P11)

This ledger tracks real hardware measurements against the architectural budgets defined in `design/PERFORMANCE_BUDGET.md` (§1.9.7).

> **Honesty Lock (§1.3 & §1.9):** Every `"Measured on Device"` cell below is **`NOT RUN / BLOCKED`** (`V-1102`) because no physical Android device or Unity `6000.3.28f1` runtime is attached to this Linux build VM. Do NOT fill numbers here from estimates or Editor play-in-window FPS.

---

## 1. Mid-Tier Android Hardware Sign-Off Table (`V-1102`)

- **Test Device:** `NOT RUN` *(target: 4–6 GB RAM, 6–8 core ARM64 SoC, Android 8.0+ / API >= 25)*
- **Build Artifact:** `BLOCKED` (`V-1101` — signed IL2CPP ARM64 APK)

| Budget Metric | Contract Ceiling (§1.9.7) | Tier 1 (Mid) Measured | Tier 2 (High) Measured | Status |
|---|---|---|---|---|
| Frame Time (p95 Downtown + Traffic) | `<= 33.3 ms` (Tier 1) / `<= 16.6 ms` (Tier 2) | `NOT RUN` | `NOT RUN` | `BLOCKED` (`V-1102`) |
| URP Draw Calls (Frame Debugger) | `<= 150` | `NOT RUN` | `NOT RUN` | `TODO` (`V-902`) |
| SetPass Calls | `<= 200` | `NOT RUN` | `NOT RUN` | `BLOCKED` (`V-1102`) |
| Visible Triangles | `<= 250,000` | `NOT RUN` | `NOT RUN` | `BLOCKED` (`V-1102`) |
| Texture Memory Resident | `<= 350 MB` | `NOT RUN` | `NOT RUN` | `BLOCKED` (`V-1102`) |
| Steady-State Managed GC Alloc | `0 B / frame` after warmup | `NOT RUN` | `NOT RUN` | `TODO` (`V-301` / `V-1102`) |
| Cold Start to Playable Control | `<= 6.0 s` | `NOT RUN` | `NOT RUN` | `BLOCKED` (`V-1102`) |
| Signed APK Size | `200–400 MB` | `NOT RUN` | `NOT RUN` | `BLOCKED` (`V-1101`) |

---

## 2. Hard Subsystem Caps Verified in Code / Assets

| Subsystem | Hard Cap | Source of Truth | Offline Proof |
|---|---|---|---|
| Full-sim Ragdolls | `<= 12` within `55 m` (`78 kg`) | `Assets/Settings/RagdollSettings.asset` + `RagdollBudgetModel` | `audit.py` + `LogicTests` |
| Explosion Overlap Buffer | `64` colliders (`OverlapSphereNonAlloc`) | `Assets/Settings/CombatSettings.asset` | `audit.py` + `LogicTests` |
| Active Streaming Cells | `9` cells (`3x3` x `64 m`) | `WorldStreamMath` | `LogicTests` |
| Props per 40 m Chunk | `<= 46` props, `<= 8` batches/district | `PropRules` | `LogicTests` |
| Prop Render Cutoff + Fog | `<=40 m` shadow · `40–85 m` mesh · `85–130 m` decal · `>130 m` culled | `PropRules` + `QualityApplier` (`fogDensity` `0.012/0.009/0.006`) | `LogicTests` |
| NPC Budget & Tick LOD | `12 / 24 / 40` NPCs at `60 / 20 / 6 / 0 Hz` | `QualityRules` + `NpcTickLod` | `LogicTests` |
| 3D Audio Voice Pool | `12` near + `8` mid + `4` far + `1` UI (`0` > `90 m`) | `AudioBudgetMath` + `ProjectSettings/AudioManager.asset` (`32` real voices) | `LogicTests` |
| Vehicle Speed Ceiling | `<= 60 m/s` (streaming safety cap) | `Assets/Data/Vehicles/VEH_*.asset` | `balance.py` (`0 findings`) |

---

## 3. Post-Processing Policy (§1.9.6)

`Assets/DefaultVolumeProfile.asset` ships with **`m_Components: []`** (zero bloom, zero SSAO, zero tonemapping, zero vignette). If any post-process effect is ever proposed:
1. Keep **one** global `Volume` and **one** shared profile; never add per-scene volumes.
2. Force Bloom and SSAO **off** on Tier 0 (`Low`).
3. Record a real Android GPU before/after frame-time capture in Section 1 above before merging.
