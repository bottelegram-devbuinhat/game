#!/usr/bin/env python3
"""Offline gameplay balance checker for Dude Chaos Wars (§2.1-2.4, §2.6, §2.10, §4.4).

Parses ScriptableObject YAML assets in Assets/Data/ without external dependencies,
prints weapon DPS & shots-to-kill, vehicle handling & speed bands, mission payouts,
and shop affordability, and exits non-zero on any balance violation.
"""

from __future__ import annotations

import math
import re
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent.parent

WEAPON_CLASS_NAMES = {
    0: "Pistol",
    1: "SMG",
    2: "Rifle",
    3: "Shotgun",
    4: "Sniper",
    5: "Heavy",
    6: "Melee",
}

HANDLING_NAMES = {
    0: "Sports",
    1: "Muscle",
    2: "Truck",
    3: "Bike",
}

ARMOUR_NAMES = {
    0: "Unarmoured",
    1: "Light",
    2: "Heavy",
    3: "Air",
}

SHOP_TYPE_NAMES = {
    0: "Weapon",
    1: "Skin",
    2: "Vehicle",
    3: "Emote",
}

DPS_EXCLUDED_NAMES = {
    "C4 Charge",
    "Railgun",
    "Rocket Launcher",
    "Mortar Tube",
    "Frag Grenade",
    "Grenade Launcher",
    "Charge Rifle",
    "Arc Caster",
    "Molotov",
    "Flare Gun",
    "Flamethrower",
    "Taser",
}


def unquote_yaml(val: str) -> str:
    s = val.strip()
    if len(s) >= 2 and s[0] == '"' and s[-1] == '"':
        return s[1:-1].replace('\\"', '"').replace("\\\\", "\\")
    if len(s) >= 2 and s[0] == "'" and s[-1] == "'":
        return s[1:-1]
    return s


def parse_unity_asset(path: Path) -> dict[str, object]:
    """Parses 2-space-indented MonoBehaviour fields (scalars, inline vectors, string/float lists)."""
    data: dict[str, object] = {}
    lines = path.read_text(encoding="utf-8", errors="replace").splitlines()
    in_mb = False
    current_list_key: str | None = None

    vec_re = re.compile(
        r"^\{\s*x:\s*([^,}]+),\s*y:\s*([^,}]+)(?:,\s*z:\s*([^,}]+))?\s*\}$"
    )

    for line in lines:
        if line.startswith("MonoBehaviour:"):
            in_mb = True
            continue
        if not in_mb:
            continue
        if line.startswith("--- "):
            break

        if current_list_key is not None:
            lm = re.match(r"^  -\s+(.*)$", line)
            if lm:
                lst = data[current_list_key]
                assert isinstance(lst, list)
                lst.append(unquote_yaml(lm.group(1)))
                continue
            else:
                current_list_key = None

        fm = re.match(r"^  ([A-Za-z_][A-Za-z0-9_]*):(.*)$", line)
        if not fm:
            continue
        key = fm.group(1)
        raw_val = fm.group(2).strip()
        if raw_val == "":
            data[key] = []
            current_list_key = key
            continue
        if raw_val == "[]":
            data[key] = []
            continue

        vm = vec_re.match(raw_val)
        if vm:
            data[key] = tuple(v.strip() for v in vm.groups() if v is not None)
            continue

        data[key] = unquote_yaml(raw_val)

    return data


def resolve_mechanic(wclass: int, mechanic_name: str, is_auto: int, fire_rate: float) -> str:
    m = mechanic_name.strip()
    if m and m.lower() != "class":
        return m
    if wclass == 0:
        return "Auto" if is_auto else "Semi"
    if wclass == 1:
        return "Auto"
    if wclass == 2:
        return "Auto" if is_auto else "Semi"
    if wclass == 3:
        return "Auto" if is_auto else "Pump"
    if wclass == 4:
        return "Bolt" if fire_rate >= 1.0 else "Semi"
    if wclass == 5:
        return "Auto" if is_auto else "Projectile"
    return "Semi"


def effective_cycle(mechanic: str, fire_rate: float, charge_seconds: float) -> float:
    base = max(0.02, fire_rate)
    if mechanic == "Pump":
        return base + 0.12
    if mechanic == "Bolt":
        return base + 0.20
    if mechanic == "Charge":
        return base + max(0.0, charge_seconds)
    if mechanic == "Throwable":
        return max(0.8, base + max(0.0, charge_seconds))
    return base


def check_weapons(findings: list[str]) -> None:
    wpn_dir = REPO_ROOT / "Assets" / "Data" / "Weapons"
    files = sorted(wpn_dir.glob("WPN_*.asset"))
    if len(files) != 48:
        findings.append(f"WEAPON_COUNT: expected 48 weapon assets, found {len(files)}")

    print(f"=== WEAPONS ({len(files)}) ===")
    print(f"{'Name':<20} {'Class':<8} {'Mech':<10} {'Dmg':>6} {'Pel':>3} {'Cycle':>6} {'DPS':>7} {'STK':>4}")
    for f in files:
        d = parse_unity_asset(f)
        name = str(d.get("displayName", f.stem))
        try:
            wclass = int(str(d.get("weaponClass", -1)))
            dmg = float(str(d.get("damage", 0)))
            pellets = int(str(d.get("pellets", 1)))
            fire_rate = float(str(d.get("fireRate", 0)))
            rng = float(str(d.get("range", 0)))
            mag = int(str(d.get("magazineSize", 0)))
            reload_t = float(str(d.get("reloadTime", 0)))
            is_auto = int(str(d.get("isAutomatic", 0)))
            burst = int(str(d.get("burstCount", 1)))
            charge = float(str(d.get("chargeSeconds", 0)))
        except ValueError as exc:
            findings.append(f"WEAPON_FIELD_TYPE: {f.name}: {exc}")
            continue

        if wclass not in WEAPON_CLASS_NAMES:
            findings.append(f"WEAPON_CLASS: {f.name} ({name}): invalid weaponClass={wclass}")
            cname = f"INVALID({wclass})"
        else:
            cname = WEAPON_CLASS_NAMES[wclass]

        mech_raw = str(d.get("mechanicName", ""))
        mech = resolve_mechanic(wclass, mech_raw, is_auto, fire_rate)
        cycle = effective_cycle(mech, fire_rate, charge)
        shots = max(2, min(6, burst)) if mech == "Burst" else 1
        dps = (dmg * pellets * shots) / cycle if cycle > 0 else 0.0
        stk = math.ceil(100.0 / max(0.01, dmg * pellets))

        print(f"{name:<20} {cname:<8} {mech:<10} {dmg:>6.1f} {pellets:>3d} {cycle:>6.3f} {dps:>7.1f} {stk:>4d}")

        if rng <= 0:
            findings.append(f"WEAPON_RANGE: {f.name} ({name}) has range={rng} <= 0")
        if dmg <= 0:
            findings.append(f"WEAPON_DAMAGE: {f.name} ({name}) has damage={dmg} <= 0")
        if fire_rate <= 0:
            findings.append(f"WEAPON_FIRERATE: {f.name} ({name}) has fireRate={fire_rate} <= 0")
        if pellets < 1:
            findings.append(f"WEAPON_PELLETS: {f.name} ({name}) has pellets={pellets} < 1")

        if cname == "Melee":
            if mag != 0:
                findings.append(f"MELEE_MAGAZINE: {f.name} ({name}) melee must have magazineSize=0, got {mag}")
            if reload_t != 0:
                findings.append(f"MELEE_RELOAD: {f.name} ({name}) melee must have reloadTime=0, got {reload_t}")
        else:
            if mag <= 0:
                findings.append(f"GUN_MAGAZINE: {f.name} ({name}) gun must have magazineSize > 0, got {mag}")
            if reload_t <= 0:
                findings.append(f"GUN_RELOAD: {f.name} ({name}) gun must have reloadTime > 0, got {reload_t}")

            # §2.10 DPS band 55..620 for non-excluded guns
            excluded = (
                cname == "Shotgun"
                or mech in ("Charge", "Throwable")
                or name in DPS_EXCLUDED_NAMES
            )
            if not excluded and not (55.0 <= dps <= 620.0):
                findings.append(
                    f"WEAPON_DPS_BAND: {f.name} ({name}) DPS={dps:.1f} outside [55, 620]"
                )


def check_vehicles(findings: list[str]) -> None:
    veh_dir = REPO_ROOT / "Assets" / "Data" / "Vehicles"
    files = sorted(veh_dir.glob("VEH_*.asset"))
    if len(files) != 18:
        findings.append(f"VEHICLE_COUNT: expected 18 vehicle assets, found {len(files)}")

    print(f"\n=== VEHICLES ({len(files)}) ===")
    print(f"{'Name':<18} {'Profile':<8} {'Armour':<11} {'HP':>6} {'Top':>5} {'Acc':>5} {'Brk':>5} {'CoM.y':>6}")
    for f in files:
        d = parse_unity_asset(f)
        name = str(d.get("vehicleName", f.stem))
        try:
            prof = int(str(d.get("handlingProfile", -1)))
            armour = int(str(d.get("armourClass", -1)))
            hp = float(str(d.get("maxHealth", 0)))
            top = float(str(d.get("maxSpeed", 0)))
            acc = float(str(d.get("acceleration", 0)))
            brk = float(str(d.get("brakeForce", 0)))
            com = d.get("centerOfMassOffset")
            if not isinstance(com, tuple) or len(com) != 3:
                raise ValueError(f"invalid centerOfMassOffset={com!r}")
            com_xyz = tuple(float(c) for c in com)
        except ValueError as exc:
            findings.append(f"VEHICLE_FIELD_TYPE: {f.name}: {exc}")
            continue

        pname = HANDLING_NAMES.get(prof, f"INVALID({prof})")
        aname = ARMOUR_NAMES.get(armour, f"INVALID({armour})")
        print(f"{name:<18} {pname:<8} {aname:<11} {hp:>6.0f} {top:>5.1f} {acc:>5.1f} {brk:>5.1f} {com_xyz[1]:>6.2f}")

        if prof not in HANDLING_NAMES:
            findings.append(f"VEHICLE_PROFILE: {f.name} ({name}) invalid handlingProfile={prof}")
        if armour not in ARMOUR_NAMES:
            findings.append(f"VEHICLE_ARMOUR: {f.name} ({name}) invalid armourClass={armour}")
        if any(not math.isfinite(c) or abs(c) > 5.0 for c in com_xyz):
            findings.append(f"VEHICLE_COM: {f.name} ({name}) centerOfMassOffset={com_xyz} out of [-5, 5]")
        if brk <= acc:
            findings.append(f"VEHICLE_BRAKE: {f.name} ({name}) brakeForce={brk} <= acceleration={acc}")
        if top > 60.0 or top <= 0.0:
            findings.append(f"VEHICLE_CEILING: {f.name} ({name}) maxSpeed={top} outside (0, 60]")

        # §2.2 speed bands
        if name == "Tank":
            lo, hi = 12.0, 21.0
        elif name == "Armoured Carrier":
            lo, hi = 18.0, 30.0
        elif name == "Helicopter":
            lo, hi = 40.0, 58.0
        elif name == "Light Jet":
            lo, hi = 45.0, 60.0
        elif pname == "Bike":
            lo, hi = 38.0, 60.0
        elif pname == "Truck":
            lo, hi = 18.0, 32.0
        else:
            lo, hi = 25.0, 45.0

        if not (lo <= top <= hi):
            findings.append(f"VEHICLE_SPEED_BAND: {f.name} ({name}) maxSpeed={top} outside [{lo}, {hi}]")


def check_economy_and_shop(findings: list[str]) -> None:
    mis_dir = REPO_ROOT / "Assets" / "Data" / "Missions"
    mis_files = sorted(mis_dir.glob("MIS_*.asset"))
    if len(mis_files) != 20:
        findings.append(f"MISSION_COUNT: expected 20 mission assets, found {len(mis_files)}")

    base_money = 0
    perfect_money = 0
    total_rep = 0
    for f in mis_files:
        d = parse_unity_asset(f)
        money = int(str(d.get("rewardsMoney", 0)))
        rep = int(str(d.get("rewardsReputation", 0)))
        base_money += money
        perfect_money += money + int(money * 0.25) + int(money * 0.34)
        total_rep += rep

    max_level = 1 + (total_rep // 100)
    print(f"\n=== ECONOMY SUMMARY ===")
    print(
        f"Missions: {len(mis_files)} | Base payout: ${base_money} | "
        f"3-star perfect payout: ${perfect_money} | Total rep: {total_rep} -> Level {max_level}"
    )

    shp_dir = REPO_ROOT / "Assets" / "Data" / "Shop"
    shp_files = sorted(shp_dir.glob("SHP_*.asset"))
    if len(shp_files) != 25:
        findings.append(f"SHOP_COUNT: expected 25 shop assets, found {len(shp_files)}")

    print(f"\n=== SHOP ({len(shp_files)}) ===")
    print(f"{'ItemId':<24} {'Type':<8} {'Price':>8} {'UnlockLvl':>9}")
    for f in shp_files:
        d = parse_unity_asset(f)
        item_id = str(d.get("itemId", f.stem))
        itype = int(str(d.get("itemType", -1)))
        price = int(str(d.get("price", 0)))
        lvl = int(str(d.get("unlockLevel", 0)))
        tname = SHOP_TYPE_NAMES.get(itype, f"INVALID({itype})")
        print(f"{item_id:<24} {tname:<8} {price:>8d} {lvl:>9d}")

        if itype not in SHOP_TYPE_NAMES:
            findings.append(f"SHOP_TYPE: {f.name} ({item_id}) invalid itemType={itype}")
        if price <= 0 or price > perfect_money:
            findings.append(f"SHOP_AFFORDABILITY: {f.name} ({item_id}) price={price} unaffordable (budget={perfect_money})")
        if lvl < 1 or lvl > max_level:
            findings.append(f"SHOP_UNLOCK: {f.name} ({item_id}) unlockLevel={lvl} unreachable (max={max_level})")
        if lvl >= 10 and price < 1200 * lvl:
            findings.append(
                f"SHOP_HIGH_LEVEL_PRICE: {f.name} ({item_id}) level={lvl} requires price >= {1200 * lvl}, got {price}"
            )

    eq_dir = REPO_ROOT / "Assets" / "Data" / "Equipment"
    eq_files = sorted(eq_dir.glob("EQ_*.asset"))
    if len(eq_files) != 59:
        findings.append(f"EQUIPMENT_COUNT: expected 59 equipment assets, found {len(eq_files)}")
    for f in eq_files:
        d = parse_unity_asset(f)
        eq_id = str(d.get("equipmentId", f.stem))
        price = int(str(d.get("price", 0)))
        lvl = int(str(d.get("unlockLevel", 0)))
        if price <= 0 or price > perfect_money:
            findings.append(f"EQUIPMENT_PRICE: {f.name} ({eq_id}) invalid price={price}")
        if lvl < 1 or lvl > max_level:
            findings.append(f"EQUIPMENT_UNLOCK: {f.name} ({eq_id}) unlockLevel={lvl} outside [1, {max_level}]")


def main() -> int:
    findings: list[str] = []
    check_weapons(findings)
    check_vehicles(findings)
    check_economy_and_shop(findings)

    print()
    for f in findings:
        print(f"FINDING: {f}")
    print(f"{len(findings)} findings")
    return 0 if len(findings) == 0 else 1


if __name__ == "__main__":
    raise SystemExit(main())
