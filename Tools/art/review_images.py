#!/usr/bin/env python3
"""Rendered image quality gate checker for Dude Chaos Wars (§3.5-3.6 & 00_PROMPT_TONG.md §4-§5).

Validates a `renders/buoc_0X/` directory or `buoc_0X.zip` archive directly against
`production/IMAGE_MANIFEST_185.json` (auto-loaded when present):
  1. Stray file check inside `.zip` (`00_PROMPT_TONG.md` §4: only `.webp` / `.png` images allowed)
  2. Exact count, duplicate check, missing-item check, and out-of-step check (`00_PROMPT_TONG.md` §5 Gates 1, 2, 5)
  3. Exact `NNN Title.<webp|png>` filename stem check (`00_PROMPT_TONG.md` §4)
  4. Exact `[W, H]` pixel canvas check (`00_PROMPT_TONG.md` §5 Gate 3)
  5. Pure `#FFFFFF` border & anti-checkerboard / dot-grid check where `requires_white_bg=True` (`00_PROMPT_TONG.md` §5 Gate 4)
"""

from __future__ import annotations

import argparse
import io
import json
import re
import sys
import zipfile
from pathlib import Path
from PIL import Image

REPO_ROOT = Path(__file__).resolve().parent.parent.parent
DEFAULT_MANIFEST_PATH = REPO_ROOT / "production" / "IMAGE_MANIFEST_185.json"

NAME_RE = re.compile(r"^(\d{3})\s+(.+)\.(webp|png)$", re.IGNORECASE)
LOOSE_ID_RE = re.compile(r"^(\d{3})(?:[\s_.-]+(.+))?\.(webp|png)$", re.IGNORECASE)
STEP_RE = re.compile(r"buoc_?0?([1-5])(?:\D|$)", re.IGNORECASE)

STANDARD_CANVASES = {
    (1536, 1024),
    (2048, 1024),
    (2048, 2048),
    (1024, 1024),
    (2048, 1152),
    (1024, 512),
    (512, 512),
    (128, 128),
}


def inspect_border(img: Image.Image) -> tuple[bool, str]:
    rgb = img.convert("RGB")
    w, h = rgb.size
    px = rgb.load()
    assert px is not None

    samples: list[tuple[int, int, int]] = []
    step_x = max(1, w // 64)
    step_y = max(1, h // 64)
    for x in range(0, w, step_x):
        samples.append(px[x, 0])
        samples.append(px[x, h - 1])
    for y in range(0, h, step_y):
        samples.append(px[0, y])
        samples.append(px[w - 1, y])

    if not samples:
        return False, "empty image"

    white_like = 0
    grey_checker = 0
    for r, g, b in samples:
        mn, mx = min(r, g, b), max(r, g, b)
        if mn >= 240 and (mx - mn) <= 8:
            white_like += 1
        elif 180 <= mn <= 235 and (mx - mn) <= 6:
            grey_checker += 1

    white_ratio = white_like / len(samples)
    checker_ratio = grey_checker / len(samples)
    if white_ratio >= 0.85 and checker_ratio >= 0.08:
        return False, f"baked checkerboard/dot-grid suspected on border (grey={checker_ratio:.0%})"
    if white_ratio < 0.80:
        if checker_ratio >= 0.15:
            return False, f"baked checkerboard/grey background (white={white_ratio:.0%}, grey={checker_ratio:.0%})"
        return False, f"non-white border ({white_ratio:.0%} white)"
    return True, "clean white border"


def infer_step_from_target(target: Path) -> int | None:
    for part in (target.name, target.stem, str(target)):
        m = STEP_RE.search(part)
        if m:
            return int(m.group(1))
    return None


def load_manifest_index(
    manifest_path: Path, step_filter: int | None
) -> tuple[dict[str, dict[str, object]], dict[str, dict[str, object]]]:
    """Returns (all_items_by_id, scoped_items_by_id)."""
    mdata = json.loads(manifest_path.read_text(encoding="utf-8"))
    all_by_id: dict[str, dict[str, object]] = {}
    scoped_by_id: dict[str, dict[str, object]] = {}

    for item in mdata.get("items", []):
        iid = str(item["id"])
        all_by_id[iid] = item
        if step_filter is None or int(item.get("step", 0)) == step_filter:
            scoped_by_id[iid] = item

    return all_by_id, scoped_by_id


def expected_dims_for(spec: dict[str, object]) -> tuple[int, int] | None:
    es = spec.get("expected_size")
    if isinstance(es, list) and len(es) == 2:
        return int(es[0]), int(es[1])
    if spec.get("width") and spec.get("height"):
        return int(spec["width"]), int(spec["height"])
    return None


def expected_stem_for(spec: dict[str, object]) -> str | None:
    out_name = spec.get("output_filename") or spec.get("expected_filename")
    if isinstance(out_name, str) and out_name:
        return Path(out_name).stem
    if "id" in spec and "title" in spec:
        return f"{spec['id']} {spec['title']}"
    return None


def review_single(
    filename: str,
    raw_bytes: bytes,
    all_manifest: dict[str, dict[str, object]],
    scoped_manifest: dict[str, dict[str, object]],
    step_filter: int | None,
) -> tuple[str, str | None, str]:
    base_name = Path(filename).name
    lm = LOOSE_ID_RE.match(base_name)
    if not lm:
        return "REDO", None, "filename does not match 'NNN Title.webp|.png'"

    item_id = lm.group(1)
    strict_m = NAME_RE.match(base_name)
    if not strict_m:
        return "REDO", item_id, f"filename must use single space after ID ('{item_id} Title.webp')"

    spec = scoped_manifest.get(item_id)
    if all_manifest:
        if item_id not in all_manifest:
            return "REDO", item_id, f"item {item_id} is not in the 185-prompt manifest (already approved or invalid ID)"
        if spec is None and step_filter is not None:
            actual_step = all_manifest[item_id].get("step")
            return "REDO", item_id, f"item {item_id} belongs to step {actual_step}, not step {step_filter}"

        exp_stem = expected_stem_for(spec) if spec else None
        actual_stem = Path(base_name).stem
        if exp_stem and actual_stem != exp_stem:
            return "REDO", item_id, f"title mismatch '{actual_stem}' (expected '{exp_stem}')"

    try:
        with Image.open(io.BytesIO(raw_bytes)) as img:
            img.load()
            w, h = img.size
            if spec is not None:
                dims = expected_dims_for(spec)
                if dims is not None and (w, h) != dims:
                    return "REDO", item_id, f"wrong size {w}x{h} (expected {dims[0]}x{dims[1]})"
                requires_white = bool(spec.get("requires_white_bg", True))
            else:
                is_strip = w > h and h >= 64 and (w % h == 0)
                if (w, h) not in STANDARD_CANVASES and not is_strip:
                    return "REDO", item_id, f"non-standard canvas {w}x{h}"
                requires_white = True

            if requires_white:
                ok_bg, bg_note = inspect_border(img)
                if not ok_bg:
                    return "REDO", item_id, bg_note
                bg_summary = "white border OK"
            else:
                bg_mode = str(spec.get("bg_mode", "exempt")) if spec else "exempt"
                bg_summary = f"bg exempt ({bg_mode})"
    except Exception as exc:
        return "REDO", item_id, f"corrupt image: {exc}"

    return "KEEP", item_id, f"{item_id} OK ({w}x{h}, {bg_summary})"


def main() -> int:
    parser = argparse.ArgumentParser(description="Review rendered art images against 185-prompt manifest & §5 gates")
    parser.add_argument("target", type=Path, help="Folder (e.g. renders/buoc_01) or .zip (e.g. buoc_01.zip)")
    parser.add_argument(
        "--manifest",
        type=Path,
        default=None,
        help="Path to IMAGE_MANIFEST_185.json (auto-discovers production/IMAGE_MANIFEST_185.json if omitted)",
    )
    parser.add_argument("--no-manifest", action="store_true", help="Disable auto-loading IMAGE_MANIFEST_185.json")
    parser.add_argument("--step", type=int, choices=[1, 2, 3, 4, 5], default=None, help="Override step number (1..5)")
    parser.add_argument("--allow-partial", action="store_true", help="Do not fail completeness gate on missing items")
    parser.add_argument("--strict", action="store_true", help="Exit 1 on any REDO, stray file, duplicate, or missing item")
    args = parser.parse_args()

    target: Path = args.target
    if not target.exists():
        print(f"ERROR: target not found: {target}", file=sys.stderr)
        return 1

    manifest_path: Path | None = None
    if not args.no_manifest:
        if args.manifest is not None:
            if not args.manifest.is_file():
                print(f"ERROR: manifest not found: {args.manifest}", file=sys.stderr)
                return 1
            manifest_path = args.manifest
        elif DEFAULT_MANIFEST_PATH.is_file():
            manifest_path = DEFAULT_MANIFEST_PATH

    step_filter = args.step if args.step is not None else infer_step_from_target(target)
    all_manifest: dict[str, dict[str, object]] = {}
    scoped_manifest: dict[str, dict[str, object]] = {}
    if manifest_path is not None:
        all_manifest, scoped_manifest = load_manifest_index(manifest_path, step_filter)

    items: list[tuple[str, bytes]] = []
    stray_files: list[str] = []

    if target.is_file() and target.suffix.lower() == ".zip":
        with zipfile.ZipFile(target, "r") as zf:
            for name in sorted(zf.namelist()):
                if name.endswith("/"):
                    continue
                if Path(name).suffix.lower() in (".webp", ".png"):
                    items.append((name, zf.read(name)))
                else:
                    stray_files.append(name)
    elif target.is_dir():
        for p in sorted(target.rglob("*")):
            if p.is_file() and p.suffix.lower() in (".webp", ".png"):
                items.append((p.name, p.read_bytes()))
    else:
        print(f"ERROR: target must be a directory or .zip archive: {target}", file=sys.stderr)
        return 1

    keep = 0
    redo = 0
    seen_ids: dict[str, str] = {}
    valid_present_ids: set[str] = set()

    for s in stray_files:
        redo += 1
        print(f"[REDO] {s:<42} stray non-image file inside zip (§4 forbids extra files)")

    for name, raw in items:
        verdict, item_id, detail = review_single(name, raw, all_manifest, scoped_manifest, step_filter)
        if item_id is not None:
            if item_id in seen_ids:
                verdict = "REDO"
                detail = f"duplicate item ID {item_id} (already seen in {seen_ids[item_id]})"
            else:
                seen_ids[item_id] = Path(name).name
                if item_id in scoped_manifest:
                    valid_present_ids.add(item_id)

        print(f"[{verdict:<4}] {Path(name).name:<42} {detail}")
        if verdict == "KEEP":
            keep += 1
        else:
            redo += 1

    missing_ids: list[str] = []
    if scoped_manifest:
        missing_ids = sorted(set(scoped_manifest.keys()) - valid_present_ids)
        if missing_ids and not args.allow_partial:
            print(f"[MISS] Missing {len(missing_ids)} item(s): {', '.join(missing_ids)}")

    step_label = f"buoc_{step_filter:02d}" if step_filter is not None else target.stem
    expected_count = len(scoped_manifest) if scoped_manifest else len(items)
    if missing_ids:
        print(
            f"Gate log: {step_label}: {keep}/{expected_count} ảnh đạt, {redo} lỗi, "
            f"thiếu {', '.join(missing_ids)}"
        )
    else:
        print(f"Gate log: {step_label}: {keep}/{expected_count} ảnh, {redo} lỗi")

    print(
        f"Summary: {len(items)} image(s) reviewed | KEEP={keep} | REDO={redo} | "
        f"MISSING={len(missing_ids)}"
    )

    failed_gate = redo > 0 or (bool(missing_ids) and not args.allow_partial)
    if args.strict and failed_gate:
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
