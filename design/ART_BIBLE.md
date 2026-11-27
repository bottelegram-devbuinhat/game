# Dude Chaos Wars — Art Bible & Asset Delivery Spec (§3)

## 1. Visual Identity & Brand (§3.1)

- **Title:** **DUDE CHAOS WARS** — *"No rules. Just dude."*
- **Style:** Stylized semi-realistic low-poly cartoon proportions (clean readable silhouettes, tactile street wear, Vietnamese urban character, slapstick impact without gore).
- **Placeholder Rule (Engineering Rule 8):** Gameplay ships on primitive meshes tagged `// ART REPLACE` and never blocks on art. Code must never hard-depend on named `.mat` or `.shader` assets.

### Core Colour Palette

| Swatch | Hex | Usage |
|---|---|---|
| Chaos Yellow | `#FFC812` | Primary logo, wanted stars, cash pickups |
| Panel Charcoal | `#14161C` | HUD cards, street backing boards, outlines |
| Clean White | `#FFFFFF` | Body copy, studio render key background |
| Danger Red | `#EA3838` | Low health, grenade warnings, mission timer <= 30 s |
| Police Blue | `#2C4BD6` | Cruisers, tactical vests, siren strobe |
| Cash Green | `#63DB4C` | Wallet gain, completion banners |
| Neon Orange / Pink / Cyan | `#FF7A2F` / `#FF3D9E` / `#38E1FF` | Night signage (`Nhậu 24/7`, `Khách Hồng Phát`, Downtown) |
| Concrete / Brick | `#8A8D94` / `#7A4A32` | Sidewalks, shophouse facades |

### Typography & Fictional In-World Brands

- **Fonts:** `Bebas Neue` / `Anton` (display headers), `Rajdhani SemiBold` (tabular HUD counters), `Inter Regular` (body & subtitles).
- **Fictional Vietnamese Brands (Zero Real Trademarks):** *Dude Cola* (`#E02020`), *ChaoMart*, *Saigon Slick* taxi, *Ba Cây Xăng*, *Nhậu 24/7*, *Bến Thành Fresh*, *Dude Motors*, *Khách Hồng Phát*, *Cảng Sài Gòn 7*.

---

## 2. World Scale, Pivots & Naming (§3.2)

- **Units:** `1 Unity unit = 1.0 metre`, `Y` up, `+Z` forward.
- **Human Proportions:** `1.80 m` standing height, `0.45 m` shoulders, `0.72 m` arms, `0.90 m` legs, `0.23 m` head.
- **Architecture Metrics:** Door `2.1 m`, storey height `3.0 m`, sidewalk `3.0 m`, traffic lane `3.5 m`, window `1.1 x 1.4 m` at `0.9 m` sill, balcony depth `1.2 m`.
- **Pivot Rules:**
  - `CHR_*` (Character): Ground centre directly between the feet (`+Z` forward).
  - `VEH_*` (Vehicle): Ground centre under chassis (`+Z` forward; 4 separate wheel meshes pivoted at each wheel's centre along X axis).
  - `WPN_*` (Weapon): Primary hand grip (`+Z` along barrel).
  - `BLD_*` (Building): Front-bottom-left facade corner (`+Z` facade normal).
  - `PRP_*` (Prop): Ground centre (`min Y = 0`).

---

## 3. 3D Mesh & Material Delivery Contract (§3.3)

| Property | Requirement |
|---|---|
| File format | `.fbx` preferred, `.glb` / `.gltf` accepted (scale `1.0`) |
| Triangle caps (LOD0) | Character `<= 12,000` · Vehicle `<= 18,000` · Building `<= 25,000` · Small Prop `<= 1,000` |
| LOD chain | 3 levels: `LOD0 = 100%`, `LOD1 = 40%`, `LOD2 = 10%` |
| UVs & Atlases | Non-overlapping UVs; `1024x1024` (props/weapons/characters), `2048x2048` (vehicles/buildings) |
| Texture maps | `Albedo` + `Normal` + `Mask` (`R`=Metallic, `G`=Roughness) + `Emission` (where needed) |
| Material slots | Maximum **4** per asset (`Body`, `Glass`, `Chrome`, `Emissive`) — URP/Lit SRP-Batcher compatible |
| Rigging | Unity Mecanim Humanoid, minimum 15 bones (procedural walk/ragdoll compatible) |
| Colliders | Built from Unity primitives (`BoxCollider`/`CapsuleCollider`), never high-poly `MeshCollider` |

> **Emissive Note (§1.9.6 R5):** `NeonFlicker`, `SirenController`, and `VehicleLights` drive `_EmissionColor` via `MaterialPropertyBlock`. Shipped night props/vehicles must enable `_EMISSION` on their emissive URP/Lit material slot to glow visibly.

---

## 4. 2D Reference & Sprite Generation Pipeline (§3.5–3.8)

- **185-Prompt Working Catalogue (5 Steps, `02`–`06`):**
  1. Step 1 (`02`): Characters, vehicles & interiors — **31** items
  2. Step 2 (`03`): Guns/attachments, architecture/interiors, nature/animals — **72** items
  3. Step 3 (`04`): Street props, VFX, military hardware — **30** items
  4. Step 4 (`05`): Medical/consumer, UI & textures — **10** items
  5. Step 5 (`06`): Small details, cockpits & enterable spaces — **42** items
- **Mandatory Prompt Rules:** Pure flat `#FFFFFF` single-tone background, zero text/labels/watermarks/gizmos, zero checkerboard/dot-grid, zero cast floor shadow, exact canvas (`1536x1024` 3-view character, `2048x1024` vehicle/weapon, `2048x2048` building, `1024x1024` prop/interior, `2048x1152` backdrop).
- **White-to-Alpha Keying (`Tools/art/key_alpha.py`):**
  - Border-connected BFS flood fill (`min >= 230`, `spread <= 8`) strips background and soft contact shadows while preserving `#F2F2F2` clothing and metallic highlights.
  - `--fill-holes 0.5` clears enclosed pure-white pockets (spokes, cages, trigger guards <= 0.5% area).
  - 2 px soft edge + mathematical white un-matte eliminates scaling fringes; `--no-trim` preserves sprite strip grids.

---

## 5. Replacement Priority Order (§3.9)

1. **Dude + 4 NPC outfits** (core combat & ragdoll silhouette readability)
2. **Sedan + Police Cruiser** (core driving & Wanted loop)
3. **3 Downtown shophouses/buildings + 1 walkable interior** (`InteriorPortal` loop)
4. **Street lamp + Vietnamese neon signs** (night city identity)
5. Remaining weapons, vehicles, props, and animals by district density.
