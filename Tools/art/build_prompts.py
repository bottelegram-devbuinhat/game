#!/usr/bin/env python3
"""185-prompt manifest builder and strict rule validator for Dude Chaos Wars (§3.5-3.6 & 00_PROMPT_TONG.md).

Parses the 5 step markdown files (`02_PROMPT_buoc1*.md` .. `06_PROMPT_buoc5*.md`, or `prompts/0[2-6]_*.md`),
extracts all 185 `### NNN. Title` sections with exact:
  - `step` (1..5)
  - `source_file`
  - `id` ("NNN")
  - `title`
  - `output_filename` ("NNN Title.webp")
  - `expected_size` [W, H] (handling sprite strip `sheet size WxH`, §3 `512x512` loose body parts,
    `1536x1024` 3-view characters, `2048x1024` vehicles/guns/cockpits, `2048x2048` buildings,
    `2048x1152` camera plates/HUDs, `1024x1024` single props/decals, and explicit UI dimensions)
  - `requires_white_bg` (True for isolated `#FFFFFF` border assets; False for intentional full-bleed
    camera backdrops like `706`, seamless tileable textures like `455`, and first-person enclosed
    cockpits/interiors `684..692`, `700..705`, `711..728` where `#FFFFFF` appears only outside windows)
  - `forbids_text` (True on 184 prompts; False on `536` game logo which intentionally spells
    `DUDE CHAOS WARS` while forbidding callout labels/watermarks)

Writes `production/IMAGE_MANIFEST_185.json` when invoked with `--write-manifest`.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent.parent
DEFAULT_MANIFEST_PATH = REPO_ROOT / "production" / "IMAGE_MANIFEST_185.json"

EXPECTED_STEP_COUNTS: dict[int, int] = {
    1: 31,
    2: 72,
    3: 30,
    4: 10,
    5: 42,
}
EXPECTED_TOTAL_COUNT = 185

STEP_FILE_RE = re.compile(r"^0([2-6])_")
HEADING_RE = re.compile(r"^###\s+(\d{3})\.\s+(.+?)\s*$")
SHEET_SIZE_RE = re.compile(r"\bsheet size\s+(\d{2,5})x(\d{2,5})\b", re.IGNORECASE)
SPRITE_CELL_RE = re.compile(
    r"\ball\s+(\d+)\s+frames\s+in\s+one\s+horizontal\s+row,\s+each\s+cell\s+(\d{2,4})x(\d{2,4})\b",
    re.IGNORECASE,
)
CANVAS_DIM_RE = re.compile(r"\b(\d{2,5})x(\d{2,5})\b")


def infer_step_number(md_path: Path) -> int:
    m = STEP_FILE_RE.match(md_path.name)
    if m:
        # 02 -> step 1, 03 -> step 2, ..., 06 -> step 5
        return int(m.group(1)) - 1
    buoc_m = re.search(r"buoc_?0?([1-5])", md_path.name, re.IGNORECASE)
    if buoc_m:
        return int(buoc_m.group(1))
    raise ValueError(f"Cannot infer step number (1..5) from filename: {md_path.name}")


def resolve_expected_size(
    step: int,
    item_id: str,
    title: str,
    prompt_text: str,
    warnings: list[str],
    loc: str,
) -> tuple[int, int]:
    low = prompt_text.lower()

    # 1. Horizontal VFX sprite strips: explicit `sheet size WxH`, cross-checked against `frames * cell`.
    sm = SHEET_SIZE_RE.search(prompt_text)
    if sm:
        sw, sh = int(sm.group(1)), int(sm.group(2))
        cm = SPRITE_CELL_RE.search(prompt_text)
        if cm:
            frames, cw, ch = int(cm.group(1)), int(cm.group(2)), int(cm.group(3))
            if (sw, sh) != (frames * cw, ch):
                warnings.append(
                    f"{loc} item {item_id} ({title}) sprite strip mismatch: "
                    f"{frames} frames * {cw}x{ch} != sheet size {sw}x{sh}"
                )
        return sw, sh

    # 2. Loose character body parts (`00_PROMPT_TONG.md` §3 & `02_PROMPT_buoc1` header:
    #    `1536x1024 cho cả ba hướng, 512x512 cho chi tiết cơ thể rời`).
    if step == 1 and "isolated single body part" in low:
        return 512, 512

    # 3. Standard / explicit canvas dimensions (`WxH`, >= 2 digits per side so `4x4 grid` is ignored).
    all_dims = [(int(m.group(1)), int(m.group(2))) for m in CANVAS_DIM_RE.finditer(prompt_text)]
    if all_dims:
        return all_dims[-1]

    warnings.append(f"{loc} item {item_id} ({title}) missing target WxH canvas size")
    return 1024, 1024


def evaluate_rule_flags(
    item_id: str,
    title: str,
    prompt_text: str,
    warnings: list[str],
    loc: str,
) -> tuple[bool, bool, str]:
    """Returns (requires_white_bg, forbids_text, bg_mode) and records warnings on unexpected omissions."""
    low = prompt_text.lower()

    # Universal anti-artifact clauses required across all 185 prompts (with intentional exceptions noted below)
    if "no checkerboard" not in low:
        warnings.append(f"{loc} item {item_id} ({title}) missing 'no checkerboard'")
    if "no dot grid" not in low:
        warnings.append(f"{loc} item {item_id} ({title}) missing 'no dot grid'")
    if "no watermark" not in low:
        warnings.append(f"{loc} item {item_id} ({title}) missing 'no watermark'")

    # Background policy:
    # - Full-bleed aerial camera plate (`706`): intentionally has no white background at all.
    # - Seamless tileable VFX/texture (`455` fog): soft turbulent tileable mist, no hard white border required.
    # - Dark-background VFX sprite sheets (if any say dark/black background): no white border required.
    # - First-person cockpits & walkable interiors (`684..692`, `700..705`, `711..728`): `#FFFFFF` is visible
    #   ONLY outside windows/openings while cabin/room walls, ceiling, and dashboard touch the outer frame border.
    # - Everything else (150 items, including `707` UAV HUD "on a pure white background" inside 10% margin):
    #   isolated on pure `#FFFFFF` outer border -> `requires_white_bg = True`.
    is_camera_plate = "camera plate" in low and "single flat image with no frame border" in low
    is_tileable = "tileable" in low and "no hard edges" in low
    is_dark_vfx = "vfx sprite" in low and ("black background" in low or "dark background" in low)
    is_first_person_enclosed = (
        "visible only outside the glass" in low
        or ("first-person interior view" in low and "no scenery outside the openings" in low)
        or ("first-person cockpit view" in low)
        or ("first-person driving view" in low)
    )
    has_explicit_white_clause = ("#ffffff" in low) or ("pure white background" in low)

    if is_camera_plate:
        requires_white_bg = False
        bg_mode = "full_bleed_camera_plate"
    elif is_dark_vfx:
        requires_white_bg = False
        bg_mode = "dark_vfx_sheet"
    elif is_tileable:
        requires_white_bg = False
        bg_mode = "tileable_texture"
    elif is_first_person_enclosed:
        if not has_explicit_white_clause:
            warnings.append(f"{loc} item {item_id} ({title}) interior prompt missing white-opening clause")
        requires_white_bg = False
        bg_mode = "first_person_interior_openings_white"
    else:
        if not has_explicit_white_clause:
            warnings.append(f"{loc} item {item_id} ({title}) missing '#ffffff' / 'pure white background'")
        requires_white_bg = True
        bg_mode = "pure_white_border"

    # Text policy:
    # - Game logo (`536`): intentionally renders `spelling exactly DUDE CHAOS WARS` with
    #   `the words are the only text on the image` and `no labels`.
    # - All other 184 prompts forbid text via `no text` or `no words, no letters, no numbers, no labels`.
    allows_logo_words = "the words are the only text on the image" in low
    has_no_text_clause = ("no text" in low) or ("no words" in low and "no letters" in low)

    if allows_logo_words:
        forbids_text = False
        if "no labels" not in low:
            warnings.append(f"{loc} item {item_id} ({title}) logo exception missing 'no labels'")
    else:
        forbids_text = True
        if not has_no_text_clause:
            warnings.append(f"{loc} item {item_id} ({title}) missing 'no text' / 'no words, no letters'")

    return requires_white_bg, forbids_text, bg_mode


def parse_prompt_markdown(md_path: Path) -> tuple[int, list[dict[str, object]], list[str]]:
    step = infer_step_number(md_path)
    lines = md_path.read_text(encoding="utf-8").splitlines()
    entries: list[dict[str, object]] = []
    warnings: list[str] = []

    cur_id: str | None = None
    cur_title: str | None = None
    in_fence = False
    fence_lines: list[str] = []

    for lineno, line in enumerate(lines, start=1):
        hm = HEADING_RE.match(line)
        if hm and not in_fence:
            if cur_id is not None:
                warnings.append(
                    f"{md_path.name}:{lineno} heading {cur_id} ({cur_title}) had no fenced prompt block before {hm.group(1)}"
                )
            cur_id = hm.group(1)
            cur_title = hm.group(2)
            continue

        if line.strip().startswith("```"):
            if not in_fence:
                in_fence = True
                fence_lines = []
            else:
                in_fence = False
                if cur_id is not None and cur_title is not None:
                    prompt_text = "\n".join(fence_lines).strip()
                    loc = f"{md_path.name}:{lineno}"
                    exp_w, exp_h = resolve_expected_size(step, cur_id, cur_title, prompt_text, warnings, loc)
                    requires_white_bg, forbids_text, bg_mode = evaluate_rule_flags(
                        cur_id, cur_title, prompt_text, warnings, loc
                    )
                    output_filename = f"{cur_id} {cur_title}.webp"
                    entries.append(
                        {
                            "step": step,
                            "source_file": md_path.name,
                            "id": cur_id,
                            "title": cur_title,
                            "output_filename": output_filename,
                            "expected_size": [exp_w, exp_h],
                            "width": exp_w,
                            "height": exp_h,
                            "requires_white_bg": requires_white_bg,
                            "forbids_text": forbids_text,
                            "bg_mode": bg_mode,
                            "prompt": prompt_text,
                        }
                    )
                    cur_id = None
                    cur_title = None
            continue

        if in_fence:
            fence_lines.append(line)

    if cur_id is not None:
        warnings.append(f"{md_path.name}:EOF heading {cur_id} ({cur_title}) had no fenced prompt block")

    return step, entries, warnings


def discover_default_files() -> list[Path]:
    prompts_dir = REPO_ROOT / "prompts"
    if prompts_dir.is_dir():
        found = sorted(prompts_dir.glob("0[2-6]_*.md"))
        if found:
            return found
    return sorted(REPO_ROOT.glob("0[2-6]_PROMPT_*.md"))


def build_manifest_payload(all_entries: list[dict[str, object]], step_counts: dict[int, int]) -> dict[str, object]:
    return {
        "schema_version": 1,
        "game": "Dude Chaos Wars",
        "total_count": len(all_entries),
        "count": len(all_entries),
        "step_counts": {str(k): step_counts.get(k, 0) for k in sorted(step_counts)},
        "items": all_entries,
    }


def main() -> int:
    parser = argparse.ArgumentParser(description="Extract and validate the 185 image generation prompts")
    parser.add_argument("files", nargs="*", type=Path, help="Prompt markdown files (auto-discovers 02..06 if omitted)")
    parser.add_argument(
        "--write-manifest",
        action="store_true",
        help=f"Write manifest to {DEFAULT_MANIFEST_PATH.relative_to(REPO_ROOT)}",
    )
    parser.add_argument("--out", type=Path, default=None, help="Optional custom JSON manifest output path")
    parser.add_argument("--strict", action="store_true", help="Exit 1 on rule or count warnings")
    args = parser.parse_args()

    using_default_discovery = not args.files
    files = args.files if args.files else discover_default_files()
    if not files:
        print("ERROR: No prompt markdown files found.", file=sys.stderr)
        return 1

    all_entries: list[dict[str, object]] = []
    all_warnings: list[str] = []
    seen_ids: dict[str, str] = {}
    step_counts: dict[int, int] = {}

    for f in files:
        if not f.is_file():
            print(f"ERROR: file not found: {f}", file=sys.stderr)
            return 1
        step, entries, warns = parse_prompt_markdown(f)
        step_counts[step] = step_counts.get(step, 0) + len(entries)
        print(f"Step {step} ({f.name}): {len(entries)} prompt(s)")

        exp_step_count = EXPECTED_STEP_COUNTS.get(step)
        if exp_step_count is not None and len(entries) != exp_step_count:
            all_warnings.append(
                f"Step {step} ({f.name}) has {len(entries)} prompts (expected {exp_step_count})"
            )

        for e in entries:
            eid = str(e["id"])
            if eid in seen_ids:
                all_warnings.append(f"Duplicate item ID {eid} in {seen_ids[eid]} and {f.name}")
            else:
                seen_ids[eid] = f.name

        all_entries.extend(entries)
        all_warnings.extend(warns)

    if using_default_discovery and len(all_entries) != EXPECTED_TOTAL_COUNT:
        all_warnings.append(f"Total prompt count is {len(all_entries)} (expected {EXPECTED_TOTAL_COUNT})")

    for w in all_warnings:
        print(f"WARN: {w}")

    out_targets: list[Path] = []
    if args.write_manifest:
        out_targets.append(DEFAULT_MANIFEST_PATH)
    if args.out and args.out not in out_targets:
        out_targets.append(args.out)

    if out_targets:
        payload = build_manifest_payload(all_entries, step_counts)
        serialized = json.dumps(payload, ensure_ascii=False, indent=2) + "\n"
        for target in out_targets:
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_text(serialized, encoding="utf-8")
            print(f"Wrote {len(all_entries)} items to {target}")

    white_true = sum(1 for e in all_entries if e["requires_white_bg"])
    forbid_text_true = sum(1 for e in all_entries if e["forbids_text"])
    print(
        f"Total extracted prompts: {len(all_entries)} "
        f"(requires_white_bg={white_true}/{len(all_entries)}, "
        f"forbids_text={forbid_text_true}/{len(all_entries)}, "
        f"{len(all_warnings)} warning(s))"
    )

    if args.strict and all_warnings:
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
