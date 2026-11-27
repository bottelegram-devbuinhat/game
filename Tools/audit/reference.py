#!/usr/bin/env python3
"""Real-world ballistic reference, mission chain pacing, and city area checker (§2.5-2.6, §2.10, §4.4).

Enforces:
  1. Within each weapon family, higher muzzleEnergyJoules deals strictly more damage.
  2. Reference calibres from §2.10 (482, 500, 750, 1793, 2045, 3376, 3592, 9000, 17296 J) are represented.
  3. Lightest sniper needs fewer torso hits than heaviest pistol.
  4. 20-mission chain totals $200,650 base, 1,815 rep -> level 19, $319,033 3-star perfect run;
     rewards never drop along chain; $200-$9,000 per objective; 60-300 s per objective;
     3 Vietnamese dialogue lines per mission; par = 70% of limit.
  5. 12 districts total 2.29 km².
"""

from __future__ import annotations

import math
import sys
from pathlib import Path

SCRIPT_DIR = Path(__file__).resolve().parent
REPO_ROOT = SCRIPT_DIR.parent.parent
sys.path.insert(0, str(SCRIPT_DIR))

from balance import WEAPON_CLASS_NAMES, parse_unity_asset  # noqa: E402

REFERENCE_JOULES = (482.0, 500.0, 750.0, 1793.0, 2045.0, 3376.0, 3592.0, 9000.0, 17296.0)


def check_ballistics(findings: list[str]) -> None:
    wpn_dir = REPO_ROOT / "Assets" / "Data" / "Weapons"
    files = sorted(wpn_dir.glob("WPN_*.asset"))
    by_family: dict[str, list[tuple[str, float, float]]] = {}
    all_energies: set[float] = set()

    for f in files:
        d = parse_unity_asset(f)
        name = str(d.get("displayName", f.stem))
        wclass = int(str(d.get("weaponClass", -1)))
        cname = WEAPON_CLASS_NAMES.get(wclass, "Unknown")
        dmg = float(str(d.get("damage", 0)))
        energy = float(str(d.get("muzzleEnergyJoules", 0)))
        all_energies.add(round(energy, 2))
        if cname != "Melee":
            by_family.setdefault(cname, []).append((name, energy, dmg))

    print("=== BALLISTIC ORDERING BY FAMILY ===")
    for cname, items in sorted(by_family.items()):
        items_sorted = sorted(items, key=lambda x: (x[1], x[2]))
        summary = ", ".join(f"{n}({e:.0f}J->{dm:.0f})" for n, e, dm in items_sorted)
        print(f"  {cname:<8}: {summary}")
        for i in range(len(items_sorted)):
            n1, e1, d1 = items_sorted[i]
            if e1 <= 0:
                findings.append(f"MUZZLE_ENERGY_ZERO: {cname} '{n1}' has muzzleEnergyJoules={e1} <= 0")
            for j in range(i + 1, len(items_sorted)):
                n2, e2, d2 = items_sorted[j]
                if e2 > e1 and d2 <= d1:
                    findings.append(
                        f"MUZZLE_ENERGY_ORDER: in {cname}, '{n2}' ({e2} J, {d2} dmg) "
                        f"has more muzzle energy than '{n1}' ({e1} J, {d1} dmg) but <= damage"
                    )

    for ref_j in REFERENCE_JOULES:
        if round(ref_j, 2) not in all_energies:
            findings.append(f"REFERENCE_CALIBRE_MISSING: {ref_j} J not present on any weapon asset")

    snipers = by_family.get("Sniper", [])
    pistols = by_family.get("Pistol", [])
    if not snipers or not pistols:
        findings.append("STK_ORDER: missing Sniper or Pistol family")
    else:
        lightest_sniper = min(snipers, key=lambda x: x[2])
        heaviest_pistol = max(pistols, key=lambda x: x[2])
        sniper_stk = math.ceil(100.0 / lightest_sniper[2])
        pistol_stk = math.ceil(100.0 / heaviest_pistol[2])
        print(
            f"\nShots-to-kill check (100 HP torso): lightest sniper '{lightest_sniper[0]}' "
            f"({lightest_sniper[2]:.0f} dmg -> {sniper_stk} hits) vs heaviest pistol "
            f"'{heaviest_pistol[0]}' ({heaviest_pistol[2]:.0f} dmg -> {pistol_stk} hits)"
        )
        if sniper_stk >= pistol_stk:
            findings.append(
                f"STK_ORDER: lightest sniper '{lightest_sniper[0]}' ({sniper_stk} hits) "
                f">= heaviest pistol '{heaviest_pistol[0]}' ({pistol_stk} hits)"
            )


def check_missions(findings: list[str]) -> None:
    mis_dir = REPO_ROOT / "Assets" / "Data" / "Missions"
    files = sorted(mis_dir.glob("MIS_*.asset"))
    if len(files) != 20:
        findings.append(f"MISSION_COUNT: expected 20 missions, found {len(files)}")
        return

    print("\n=== MISSION PACING & PAR TABLE (20) ===")
    print(f"{'#':>2} {'MissionId':<22} {'Money':>7} {'Rep':>4} {'Limit':>6} {'Par(70%)':>8} {'Obj':>3} {'$/Obj':>7} {'s/Obj':>6}")

    prev_money = -1
    total_money = 0
    total_rep = 0
    perfect_total = 0

    for idx, f in enumerate(files, start=1):
        d = parse_unity_asset(f)
        mid = str(d.get("missionId", f.stem))
        money = int(str(d.get("rewardsMoney", 0)))
        rep = int(str(d.get("rewardsReputation", 0)))
        limit_s = float(str(d.get("timeLimitSeconds", 0)))
        retryable = int(str(d.get("retryable", 0)))
        tbs = float(str(d.get("timeBonusPerSecond", 0)))
        objectives = d.get("objectives")
        dialogue = d.get("dialogueLines")

        nobj = len(objectives) if isinstance(objectives, list) else 0
        ndlg = len(dialogue) if isinstance(dialogue, list) else 0
        par_s = 0.70 * limit_s
        mpo = money / nobj if nobj > 0 else 0.0
        tpo = limit_s / nobj if nobj > 0 else 0.0

        print(f"{idx:>2d} {mid:<22} {money:>7d} {rep:>4d} {limit_s:>6.0f} {par_s:>8.1f} {nobj:>3d} {mpo:>7.1f} {tpo:>6.1f}")

        if money < prev_money:
            findings.append(f"MISSION_DROP: {mid} pays ${money} < previous ${prev_money}")
        prev_money = money

        if nobj == 0:
            findings.append(f"MISSION_OBJECTIVES: {mid} has 0 objectives")
        else:
            if not (200.0 <= mpo <= 9000.0):
                findings.append(f"MISSION_MONEY_PER_OBJ: {mid} $/obj={mpo:.1f} outside [200, 9000]")
            if not (60.0 <= tpo <= 300.0):
                findings.append(f"MISSION_TIME_PER_OBJ: {mid} s/obj={tpo:.1f} outside [60, 300]")

        if ndlg != 3 or any(not str(line).strip() for line in (dialogue or [])):
            findings.append(f"MISSION_DIALOGUE: {mid} must have exactly 3 non-empty dialogueLines, got {ndlg}")

        if retryable != 1:
            findings.append(f"MISSION_RETRYABLE: {mid} must have retryable=1, got {retryable}")

        expected_tbs = round(0.20 * money / limit_s, 4) if limit_s > 0 else 0.0
        if abs(tbs - expected_tbs) > 1e-3:
            findings.append(
                f"MISSION_TIME_BONUS: {mid} timeBonusPerSecond={tbs} != expected {expected_tbs}"
            )

        total_money += money
        total_rep += rep
        perfect_total += money + int(money * 0.25) + int(money * 0.34)

    level = 1 + (total_rep // 100)
    print(f"Totals: base=${total_money}, 3-star perfect=${perfect_total}, rep={total_rep} (level {level})")

    if total_money != 200650:
        findings.append(f"MISSION_TOTAL_MONEY: expected 200650, got {total_money}")
    if total_rep != 1815:
        findings.append(f"MISSION_TOTAL_REP: expected 1815, got {total_rep}")
    if level != 19:
        findings.append(f"MISSION_FINAL_LEVEL: expected level 19, got {level}")
    if perfect_total != 319033:
        findings.append(f"MISSION_PERFECT_TOTAL: expected 319033, got {perfect_total}")


def check_districts(findings: list[str]) -> None:
    dist_dir = REPO_ROOT / "Assets" / "Data" / "Districts"
    files = sorted(dist_dir.glob("district_*.asset"))
    if len(files) != 12:
        findings.append(f"DISTRICT_COUNT: expected 12 districts, found {len(files)}")

    total_m2 = 0.0
    for f in files:
        d = parse_unity_asset(f)
        sz = d.get("size")
        if not isinstance(sz, tuple) or len(sz) != 2:
            findings.append(f"DISTRICT_SIZE: {f.name} invalid size={sz!r}")
            continue
        sx, sy = float(sz[0]), float(sz[1])
        if sx <= 0 or sy <= 0:
            findings.append(f"DISTRICT_SIZE: {f.name} non-positive size=({sx}, {sy})")
        total_m2 += sx * sy

    area_km2 = round(total_m2 / 1_000_000.0, 2)
    print(f"\n=== DISTRICTS ({len(files)}) ===")
    print(f"Total area: {total_m2:,.0f} m² ({area_km2:.2f} km²)")
    if area_km2 != 2.29:
        findings.append(f"DISTRICT_AREA: expected 2.29 km², got {area_km2:.2f} km²")


def main() -> int:
    findings: list[str] = []
    check_ballistics(findings)
    check_missions(findings)
    check_districts(findings)

    print()
    for f in findings:
        print(f"FINDING: {f}")
    print(f"{len(findings)} findings")
    return 0 if len(findings) == 0 else 1


if __name__ == "__main__":
    raise SystemExit(main())
