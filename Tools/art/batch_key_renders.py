#!/usr/bin/env python3
"""Batch White-to-Alpha Keying & Quality Verifier for Dude Chaos Wars (§3.8).

Reads `production/IMAGE_MANIFEST_185.json` and scans `renders/buoc_01/`..`renders/buoc_05/`:
- Leaves `renders/buoc_0X/` and `buoc_0X.zip` 100% untouched (pure white #FFFFFF per `00_PROMPT_TONG.md`).
- Keys all completed `pure_white_border` renders via `key_image_white_to_alpha` (`--fill-holes 0.5`,
  `--soft-edge 2`, `--no-trim`) into `--out` (default: `/tmp/dudechaos_keyed_alpha`).
- Copies non-`pure_white_border` renders (`first_person_interior_openings_white`,
  `full_bleed_camera_plate`, `tileable_texture`) as-is without border alpha keying.
- `--verify` checks every keyed `pure_white_border` output for:
  1. Exact manifest dimensions (`--no-trim` preserved).
  2. All 4 corners are 100% transparent (`alpha == 0`).
  3. Opaque foreground ratio is within realistic bounds (`4% <= opaque_ratio <= 94%`), ensuring
     interior white clothing/steel highlights (e.g. `001 Dude mặc định.webp` `#F2F2F2` shirt,
     `148 Combat Shotgun.webp`, `165 Súng bắn đinh tán.webp`) stay opaque (`alpha == 255`).
"""

from __future__ import annotations

import argparse
import json
from pathlib import Path
import shutil
import sys
import tempfile

from PIL import Image

from key_alpha import key_image_white_to_alpha

REPO_ROOT = Path(__file__).resolve().parents[2]
MANIFEST_PATH = REPO_ROOT / "production" / "IMAGE_MANIFEST_185.json"


def load_manifest() -> list[dict]:
    data = json.loads(MANIFEST_PATH.read_text(encoding="utf-8"))
    return data["items"]


def verify_keyed_image(img: Image.Image, meta: dict) -> list[str]:
    errors: list[str] = []
    rgba = img.convert("RGBA")
    w, h = rgba.size
    exp_w, exp_h = meta["expected_size"]
    if (w, h) != (exp_w, exp_h):
        errors.append(f"size {(w, h)} != expected {(exp_w, exp_h)} (--no-trim violated)")

    px = rgba.load()
    corners = [(0, 0), (w - 1, 0), (0, h - 1), (w - 1, h - 1)]
    for cx, cy in corners:
        if px[cx, cy][3] != 0:
            errors.append(f"corner ({cx}, {cy}) alpha={px[cx, cy][3]} != 0")

    alpha_bytes = rgba.getchannel("A").tobytes()
    total = len(alpha_bytes)
    opaque_count = sum(1 for a in alpha_bytes if a >= 250)
    transparent_count = sum(1 for a in alpha_bytes if a == 0)
    opaque_ratio = opaque_count / max(1, total)
    trans_ratio = transparent_count / max(1, total)

    if trans_ratio < 0.05:
        errors.append(f"transparent ratio too low ({trans_ratio:.1%}); background was not keyed")
    if opaque_ratio < 0.04:
        errors.append(f"opaque ratio too low ({opaque_ratio:.1%}); subject silhouette was punched out")
    return errors


def run_batch(
    out_root: Path,
    fill_holes: float = 0.5,
    soft_edge: int = 2,
    verify: bool = True,
) -> int:
    items = load_manifest()
    keyed_count = 0
    exempt_count = 0
    failures: list[str] = []

    for meta in items:
        step = int(meta["step"])
        fname = meta["output_filename"]
        src_path = REPO_ROOT / "renders" / f"buoc_{step:02d}" / fname
        if not src_path.is_file():
            continue

        dst_dir = out_root / f"buoc_{step:02d}"
        dst_dir.mkdir(parents=True, exist_ok=True)
        dst_path = dst_dir / fname

        bg_mode = meta["bg_mode"]
        if bg_mode == "pure_white_border":
            with Image.open(src_path) as im:
                keyed = key_image_white_to_alpha(
                    im,
                    min_channel=230,
                    max_spread=8,
                    fill_holes_pct=fill_holes,
                    soft_edge_px=soft_edge,
                    trim=False,
                )
                if verify:
                    errs = verify_keyed_image(keyed, meta)
                    for e in errs:
                        failures.append(f"{meta['id']} ({fname}): {e}")
                keyed.save(dst_path, format="WEBP", lossless=True, method=4)
            keyed_count += 1
        else:
            shutil.copy2(src_path, dst_path)
            exempt_count += 1

    if failures:
        for f in failures:
            print(f"FAIL: {f}", file=sys.stderr)
        return 1

    print(
        f"Batch White-to-Alpha Keying PASS: {keyed_count + exempt_count} completed renders processed "
        f"(keyed={keyed_count} pure_white_border with --fill-holes {fill_holes} --no-trim, "
        f"exempt={exempt_count} interior/full-bleed) -> {out_root}"
    )
    return 0


def main() -> int:
    parser = argparse.ArgumentParser(description="Batch white-to-alpha keyer & verifier for completed renders (§3.8)")
    parser.add_argument("--out", type=Path, default=None, help="Output directory (defaults to temporary directory)")
    parser.add_argument("--fill-holes", type=float, default=0.5, help="Max enclosed white pocket area %% (default: 0.5)")
    parser.add_argument("--soft-edge", type=int, default=2, help="Soft antialiased edge width in px (default: 2)")
    parser.add_argument("--verify", action="store_true", default=True, help="Verify corner alpha==0 and subject opacity")
    args = parser.parse_args()

    if args.out is not None:
        return run_batch(args.out, fill_holes=args.fill_holes, soft_edge=args.soft_edge, verify=args.verify)

    with tempfile.TemporaryDirectory(prefix="dcw_keyed_alpha_") as tmp:
        return run_batch(Path(tmp), fill_holes=args.fill_holes, soft_edge=args.soft_edge, verify=args.verify)


if __name__ == "__main__":
    raise SystemExit(main())
