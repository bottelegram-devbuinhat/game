#!/usr/bin/env python3
"""Mobile texture budget optimizer for Dude Chaos Wars (§1.9.7 & §3.3).

Clamps oversized textures to mobile atlas caps (default 2048 for vehicles/buildings,
1024 for props/small assets), aligns dimensions to multiples of 4 for ASTC/ETC2 block
compression, strips non-essential metadata, and reports byte savings.
"""

from __future__ import annotations

import argparse
import sys
from pathlib import Path
from PIL import Image

LARGE_PREFIXES = ("VEH_", "BLD_", "veh_", "bld_")


def target_cap_for(filename: str, default_max: int) -> int:
    if filename.startswith(LARGE_PREFIXES):
        return max(default_max, 2048)
    return default_max


def align_multiple_of_4(val: int) -> int:
    return max(4, (val // 4) * 4)


def process_texture(src: Path, dst: Path, max_dim: int, dry_run: bool) -> tuple[int, int, bool]:
    orig_bytes = src.stat().st_size
    cap = target_cap_for(src.name, max_dim)

    with Image.open(src) as img:
        w, h = img.size
        scale = min(1.0, cap / max(w, h))
        new_w = align_multiple_of_4(int(round(w * scale)))
        new_h = align_multiple_of_4(int(round(h * scale)))
        resized = (new_w, new_h) != (w, h)

        if dry_run:
            return orig_bytes, orig_bytes, resized

        work = img.resize((new_w, new_h), Image.Resampling.LANCZOS) if resized else img.copy()
        dst.parent.mkdir(parents=True, exist_ok=True)
        suffix = dst.suffix.lower()
        if suffix == ".webp":
            work.save(dst, format="WEBP", lossless=True, method=4)
        else:
            work.save(dst, format="PNG", optimize=True)

    new_bytes = dst.stat().st_size
    return orig_bytes, new_bytes, resized


def main() -> int:
    parser = argparse.ArgumentParser(description="Optimize textures for Android URP budget")
    parser.add_argument("input_dir", type=Path, help="Input directory containing PNG/WebP textures")
    parser.add_argument("--out", type=Path, default=None, help="Output directory (defaults to in-place)")
    parser.add_argument("--max-size", type=int, default=1024, help="Default max dimension for props (default: 1024)")
    parser.add_argument("--dry-run", action="store_true", help="Inspect without writing files")
    args = parser.parse_args()

    in_dir: Path = args.input_dir
    out_dir: Path = args.out or in_dir
    if not in_dir.is_dir():
        print(f"ERROR: directory not found: {in_dir}", file=sys.stderr)
        return 1

    files = sorted(
        p for p in in_dir.rglob("*") if p.is_file() and p.suffix.lower() in (".png", ".webp")
    )
    total_before = 0
    total_after = 0
    resized_count = 0

    for src in files:
        rel = src.relative_to(in_dir)
        dst = out_dir / rel
        before, after, resized = process_texture(src, dst, args.max_size, args.dry_run)
        total_before += before
        total_after += after
        if resized:
            resized_count += 1

    saved = total_before - total_after
    print(
        f"Textures scanned: {len(files)} | Resized/aligned: {resized_count} | "
        f"Before: {total_before:,} B | After: {total_after:,} B | Saved: {saved:,} B"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
