# Dude Chaos Wars — Bến Cảng City Plan (§2.5, §2.9, P8)

## 1. World Scale & Streaming Topology

- **Total Footprint:** **12 districts**, **2,289,400 m² (`2.29 km²`)**.
- **Coordinate Convention:** `1 unit = 1 metre`, `Y` up, `+Z` north/forward. District bounds are axis-aligned XZ rectangles (`DistrictBounds`).
- **Containment Rule:** Smallest containing XZ rectangle wins (`district_secret` at `(100, -100)` size `120x120` resolves cleanly inside `district_downtown` at `(0, 0)` size `600x600`).
- **Cell Streaming (`WorldStreamMath` / `WorldStreamer`):**
  - Cell size: **`64 m x 64 m`** (~560 cells across the playable envelope).
  - Active resident window: **Chebyshev radius 1 (`3 x 3 = 9` cells alive simultaneously)**.
  - Unload interval: driven by `QualitySettingsPreset.streamerUnloadInterval` (no per-frame scene scan).

---

## 2. District Catalogue (`Assets/Data/Districts/district_*.asset`)

| District Asset | Localization Key | Kind | Centre XZ (m) | Size XZ (m) | Area (m²) | Pass Required | Prop Density | Day / Night Role |
|---|---|---|---|---|---|---|---|---|
| `district_downtown` | `ui.district.downtown` | Downtown (0) | `(0, 0)` | `600 x 600` | 360,000 | No | 1.0 | Dense traffic & neon lights; power-outage stage in Ash ending |
| `district_slums` | `ui.district.slums` | Slums (1) | `(-800, -600)` | `400 x 400` | 160,000 | No | 1.0 | Busiest & most dangerous at night; underground night market |
| `district_market` | `ui.district.market` | Market (2) | `(400, -500)` | `300 x 300` | 90,000 | No | 1.1 | Bà Bảy's noodle stall; high daytime pedestrian count, empties late |
| `district_restaurant` | `ui.district.restaurant` | Restaurant (3) | `(350, 250)` | `200 x 200` | 40,000 | No | 1.0 | Busier at night than daytime (`Nhậu 24/7` corridor) |
| `district_hotel` | `ui.district.hotel` | Hotel (4) | `(150, 150)` | `150 x 150` | 22,500 | No | 1.0 | Operates 24 h; Hải Đăng Hotel (`Room 302`, 4 rental tiers, death respawn) |
| `district_showroom` | `ui.district.showroom` | Showroom (5) | `(-250, -350)` | `250 x 250` | 62,500 | No | 1.0 | Vinh "Baba"'s Dude Motors showroom (`Showroom Hit`) |
| `district_residential` | `ui.district.residential` | Residential (6) | `(-700, 300)` | `500 x 500` | 250,000 | No | 1.0 | Starting apartment (`Wake Up, Dude`), mid-density shophouses |
| `district_port` | `ui.district.port` | Port (7) | `(900, -900)` | `700 x 400` | 280,000 | No | 0.9 | Industrial piers (`Cảng Sài Gòn 7`), night cargo runs |
| `district_airport` | `ui.district.airport` | Airport (8) | `(1100, 700)` | `900 x 500` | 450,000 | **Yes** | 1.0 | Guarded terminal & hangar runway; empty at night (`Airport Heist`, `Endgame`) |
| `district_farm` | `ui.district.farm` | Farm (9) | `(-1200, 900)` | `600 x 600` | 360,000 | No | 1.0 | Rural perimeter & livestock; empty at night |
| `district_military` | `ui.district.military` | Military (10) | `(600, 1200)` | `500 x 400` | 200,000 | **Yes** | 1.0 | Captain Khang's depot; heavy patrol & turrets (`Military Run`, `Army Depot`) |
| `district_secret` | `ui.district.secret` | Secret (11) | `(100, -100)` | `120 x 120` | 14,400 | **Yes** | 1.0 | Hidden enclave nested inside Downtown |
| **TOTAL (12)** | | | | | **2,289,400** | | | **`2.29 km²`** |

---

## 3. Walkable Interiors & Portals (`InteriorPortal` + `HotelService`)

- Walkable interiors (apartment, noodle shopfront, hotel lobby & Room 302, showroom floor, hangar office) sit on isolated interior islands outside camera clip range and connect via `InteriorPortal`.
- `InteriorPortal` temporarily disables `CharacterController` before writing `transform.position` so Unity does not snap the player back to the street collider.
- Restricted portals (`Airport`, `Military`, `Secret`, `Penthouse`) check pass/rental ownership and trigger district heat when breached without authorization.

---

## 4. Street Dressing, Day/Night & Living Population

- **78 Prop Kinds in 8 `PropGroup` Batches (`PropRules`):**
  - Base per-chunk (40 m x 40 m) densities: `StreetFurniture` (4), `StreetClutter` (6), `WallDetail` (7), `RoofAndUtility` (3), `ShopFront` (3), `Nature` (5), `GroundDecal` (5), `NightSign` (2).
  - Hard chunk cap: **46 props per chunk** after applying `densityMultiplier`.
  - Material batching ceiling: **`BatchDrawCalls <= 8` groups per district**.
- **Render Tiers (`PropRules.RenderTier` + Per-Tier Fog):**
  - `<= 40 m`: Full mesh + shadows
  - `40–85 m`: Simplified mesh, shadows off
  - `85–130 m`: Flat ground/wall decals only
  - `> 130 m`: Hidden (masked by `RenderSettings.fogDensity` `0.012 / 0.009 / 0.006`)
- **Day/Night Clock (`DayNightRules` / `DayNightCycle`):**
  - 1 game day = **24 real minutes** (`hoursPerSecond = 1/60`), starting at `09:00`.
  - Noon sun elevation `68°`; ambient `1.1` day down to `0.28` at `03:00` (never `0`, requires `Ambient Mode = Color`).
  - Street lamps fade in at `18:36` and out at `06:12` over 30 game minutes inside **one** array loop (zero per-lamp `Update()`).
- **Pedestrians & Animals (`NpcSpawner`, `AnimalSpawner`):**
  - Pooled agents spawned along active streamer cells; tick rates `60 / 20 / 6 / 0 Hz` by distance (`NpcTickLod`).
  - District-appropriate animals (street dogs, cats, chickens/cows in Farm, seagulls/pigeons hovering in Port/Downtown) with flee radius and day/night schedules.
