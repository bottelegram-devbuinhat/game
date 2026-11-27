#!/usr/bin/env python3
"""CLI manager for production/VERIFY_BACKLOG.md (§4.5).

Commands:
  summary
  list [--phase N] [--status S]
  pass <ID> --note "..."
  fail <ID> --note "..."
  block <ID> --note "..."
  reset <ID> [--note "..."]
  next
"""

from __future__ import annotations

import argparse
import re
import sys
from collections import Counter
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent.parent
DEFAULT_BACKLOG = REPO_ROOT / "production" / "VERIFY_BACKLOG.md"
VALID_STATUSES = ("TODO", "PASS", "FAIL", "BLOCKED")

ROW_RE = re.compile(
    r"^\|\s*(V-\d+)\s*\|\s*(P\d+)\s*\|\s*(TODO|PASS|FAIL|BLOCKED)\s*\|\s*(.*?)\s*\|\s*(.*?)\s*\|$"
)


def normalize_phase(phase_arg: str | None) -> str | None:
    if phase_arg is None:
        return None
    s = phase_arg.strip().upper()
    if not s.startswith("P"):
        s = f"P{s}"
    return s


def load_rows(path: Path) -> tuple[list[str], list[tuple[int, str, str, str, str, str]]]:
    lines = path.read_text(encoding="utf-8").splitlines()
    rows: list[tuple[int, str, str, str, str, str]] = []
    for idx, line in enumerate(lines):
        m = ROW_RE.match(line.strip())
        if m:
            rows.append((idx, m.group(1), m.group(2), m.group(3), m.group(4), m.group(5)))
    return lines, rows


def format_row(vid: str, phase: str, status: str, desc: str, note: str) -> str:
    clean_note = note.replace("|", "/").replace("\n", " ").strip()
    return f"| {vid} | {phase} | {status} | {desc} | {clean_note} |"


def cmd_summary(path: Path) -> int:
    _, rows = load_rows(path)
    counts = Counter(r[3] for r in rows)
    by_phase: dict[str, Counter[str]] = {}
    for _, _, phase, status, _, _ in rows:
        by_phase.setdefault(phase, Counter())[status] += 1

    print(
        f"Total: {len(rows)} | "
        + " | ".join(f"{st}: {counts.get(st, 0)}" for st in VALID_STATUSES)
    )
    def phase_sort_key(p: str) -> int:
        return int(p[1:]) if p[1:].isdigit() else 999

    for phase in sorted(by_phase, key=phase_sort_key):
        pc = by_phase[phase]
        ptotal = sum(pc.values())
        detail = ", ".join(f"{st}={pc.get(st, 0)}" for st in VALID_STATUSES if pc.get(st, 0) > 0)
        print(f"  {phase:<4} ({ptotal:>2d}): {detail}")
    return 0


def cmd_list(path: Path, phase_arg: str | None, status_arg: str | None) -> int:
    _, rows = load_rows(path)
    target_phase = normalize_phase(phase_arg)
    target_status = status_arg.strip().upper() if status_arg else None
    if target_status and target_status not in VALID_STATUSES:
        print(f"ERROR: invalid status '{target_status}'. Choose from {VALID_STATUSES}", file=sys.stderr)
        return 1

    matched = [
        r
        for r in rows
        if (target_phase is None or r[2] == target_phase)
        and (target_status is None or r[3] == target_status)
    ]
    for _, vid, phase, status, desc, note in matched:
        print(f"{vid:<7} {phase:<4} {status:<8} {desc} [{note}]")
    print(f"({len(matched)} items)")
    return 0


def cmd_update(path: Path, vid: str, new_status: str, note: str | None) -> int:
    target_id = vid.strip().upper()
    lines, rows = load_rows(path)
    for line_idx, row_id, phase, old_status, desc, old_note in rows:
        if row_id == target_id:
            updated_note = note if note is not None else ("Reset to TODO" if new_status == "TODO" else old_note)
            if new_status in ("PASS", "FAIL", "BLOCKED") and not (updated_note and updated_note.strip()):
                print(f"ERROR: --note is required when marking {target_id} as {new_status}", file=sys.stderr)
                return 1
            lines[line_idx] = format_row(row_id, phase, new_status, desc, updated_note)
            path.write_text("\n".join(lines) + "\n", encoding="utf-8", newline="\n")
            print(f"{row_id} ({phase}): {old_status} -> {new_status} | {updated_note}")
            return 0

    print(f"ERROR: item '{target_id}' not found in {path}", file=sys.stderr)
    return 1


def cmd_next(path: Path) -> int:
    _, rows = load_rows(path)
    for _, vid, phase, status, desc, note in rows:
        if status == "FAIL":
            print(f"NEXT (FAIL): {vid} [{phase}] — {desc} ({note})")
            return 0
    for _, vid, phase, status, desc, note in rows:
        if status == "TODO":
            print(f"NEXT (TODO): {vid} [{phase}] — {desc} ({note})")
            return 0
    print("No TODO or FAIL items remaining.")
    return 0


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Manage production/VERIFY_BACKLOG.md")
    parser.add_argument("--file", type=Path, default=DEFAULT_BACKLOG, help="Path to VERIFY_BACKLOG.md")
    sub = parser.add_subparsers(dest="command", required=True)

    sub.add_parser("summary", help="Print status counts overall and per phase")

    p_list = sub.add_parser("list", help="List backlog items with optional filters")
    p_list.add_argument("--phase", type=str, default=None, help="Phase number or ID (e.g. 2 or P2)")
    p_list.add_argument("--status", type=str, default=None, help="Status filter (TODO/PASS/FAIL/BLOCKED)")

    for cmd_name in ("pass", "fail", "block"):
        p_up = sub.add_parser(cmd_name, help=f"Mark an item {cmd_name.upper()}")
        p_up.add_argument("item_id", type=str, help="Verification ID (e.g. V-101)")
        p_up.add_argument("--note", type=str, required=True, help="Evidence or reason note")

    p_reset = sub.add_parser("reset", help="Reset an item back to TODO")
    p_reset.add_argument("item_id", type=str, help="Verification ID (e.g. V-101)")
    p_reset.add_argument("--note", type=str, default="Reset to TODO", help="Optional note after reset")

    sub.add_parser("next", help="Show the highest-priority FAIL or TODO item")
    return parser


def main() -> int:
    parser = build_parser()
    args = parser.parse_args()
    path: Path = args.file
    if not path.is_file():
        print(f"ERROR: backlog file not found: {path}", file=sys.stderr)
        return 1

    if args.command == "summary":
        return cmd_summary(path)
    if args.command == "list":
        return cmd_list(path, args.phase, args.status)
    if args.command == "pass":
        return cmd_update(path, args.item_id, "PASS", args.note)
    if args.command == "fail":
        return cmd_update(path, args.item_id, "FAIL", args.note)
    if args.command == "block":
        return cmd_update(path, args.item_id, "BLOCKED", args.note)
    if args.command == "reset":
        return cmd_update(path, args.item_id, "TODO", args.note)
    if args.command == "next":
        return cmd_next(path)
    return 1


if __name__ == "__main__":
    raise SystemExit(main())
