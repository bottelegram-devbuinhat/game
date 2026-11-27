# Phase 2 Mandatory Gate — Ragdoll Validation Report (§1.7 & §5 P2)

> **Gate Status:** **`NOT RUN`** (Deferred under `D-013` — no Unity `6000.3.28f1` Editor on the headless Linux rebuild machine).
>
> **Non-Negotiable Rule (§0 & P2):** None of the six questions below may be marked `PASS` until a human opens `TestSlice.unity` / `Test.unity` in Unity `6000.3.28f1`, enters Play Mode, watches the body physics live, and updates `V-201`..`V-206` via `python3 Tools/verify/checklist.py pass <ID> --note "..."`.

---

## Locked Ragdoll Parameters (`Assets/Settings/RagdollSettings.asset`)

| Field | Locked Value | Purpose |
|---|---|---|
| `totalMass` | `78.0 kg` | Distributed across humanoid rigidbody bones |
| `fallRagdollThreshold` | `-16.0 m/s` | Vertical landing velocity forcing HighFall ragdoll |
| `highForceImpulseThreshold` | `55.0` | Hit/explosion impulse threshold |
| `recoveryVelocity` | `1.1 m/s` | Max average bone speed before recovery timer ticks |
| `recoveryHoldTime` | `1.8 s` | Settle duration required before standing up |
| `getUpDuration` | `0.9 s` | Blend window from ragdoll pose back to locomotion |
| `maxFullSimRagdolls` | `12` | Hard simultaneous full-physics ragdoll cap |
| `fullSimRadius` | `55.0 m` | Distance beyond which ragdolls simplify/sleep |
| `sleepVelocity` | `0.5 m/s` | Velocity threshold to sleep settled bodies |
| `pelvisFollowLag` | `0.08 s` | Smooth-damp camera target lag on pelvis |

---

## Six Mandatory Validation Questions

### 1. Ragdoll Activation Across Triggers (`V-201`) — **Status: `NOT RUN`**
- **Question:** Does the character enter ragdoll cleanly from **bullet**, **explosion**, **vehicle impact / high-speed eject (`>= 9 m/s` via `ExitingVehicle`)**, and **high fall (`<= -16 m/s`)** without state-machine softlock?
- **How to Verify in Unity:** Shoot an NPC with high-impulse weapon (`WPN_Pump_Shotgun`), detonate `WPN_Frag_Grenade`, strike with `VEH_Sedan`, eject above `9 m/s`, and drop player from `> 15 m` rooftop. Confirm `ExitingVehicle -> Ragdoll` transitions cleanly (Landmine #13).
- **Observed Result:** `NOT RUN` (awaiting Unity `6000.3.28f1` Play Mode).

### 2. Velocity-Gated Recovery & Permanent Death (`V-202`) — **Status: `NOT RUN`**
- **Question:** Does recovery trigger **only** after average velocity stays `< 1.1 m/s` for `1.8 s` **and** `HealthSystem.IsAlive == true` (dead bodies never stand back up)?
- **How to Verify in Unity:** Knock a live character down a slope (must not stand up mid-slide); let it settle (`1.8 s` hold + `0.9 s` get-up); then deplete HP to `0` and confirm the corpse stays ragdolled indefinitely.
- **Observed Result:** `NOT RUN` (awaiting Unity `6000.3.28f1` Play Mode).

### 3. Per-Limb Directed Impulse (`V-203`) — **Status: `NOT RUN`**
- **Question:** Is hit impulse applied directly to the struck `BodyPartTag` bone (`Head`, `Torso`, `Limb`) rather than pushing the root uniformly?
- **How to Verify in Unity:** Shoot left leg, right arm, and head separately on a standing NPC and confirm rotational torque matches the struck limb.
- **Observed Result:** `NOT RUN` (awaiting Unity `6000.3.28f1` Play Mode).

### 4. Simultaneous Full-Simulation Cap (`V-204`) — **Status: `NOT RUN`**
- **Question:** Does `RagdollBudget` strictly respect `maxFullSimRagdolls <= 12` when 20+ bodies are knocked down at once, using one static driver with zero per-ragdoll `Update()`?
- **How to Verify in Unity:** Trigger `WPN_Rocket_Launcher` in a dense 20-NPC crowd and inspect active non-kinematic ragdoll count + Unity Profiler `BehaviourUpdate`.
- **Observed Result:** `NOT RUN` (awaiting Unity `6000.3.28f1` Play Mode).

### 5. Distance Simplification & Boneless Prefab Safety (`V-205`) — **Status: `NOT RUN`**
- **Question:** Are ragdolls beyond `55 m` simplified/slept cleanly, reading `Position` (pelvis) so boneless placeholder prefabs never throw `IndexOutOfRangeException` (Landmine #18)?
- **How to Verify in Unity:** Ragdoll NPCs, drive `> 60 m` away, verify distant bodies freeze/sleep and Console logs zero exceptions.
- **Observed Result:** `NOT RUN` (awaiting Unity `6000.3.28f1` Play Mode).

### 6. Pelvis Camera Tracking (`V-206`) — **Status: `NOT RUN`**
- **Question:** Does `PelvisFollowTarget` follow the ragdolled player's pelvis smoothly with `0.08 s` lag and hand back to the standing pivot over `0.9 s` without camera snapping?
- **How to Verify in Unity:** Eject player from `VEH_Sport_Bike` at `25 m/s` and watch `ThirdPersonCameraRig` through ragdoll tumble and stand-up transition.
- **Observed Result:** `NOT RUN` (awaiting Unity `6000.3.28f1` Play Mode).
