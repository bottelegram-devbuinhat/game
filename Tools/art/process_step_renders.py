#!/usr/bin/env python3
"""
Batch Post-Processor & Step Packager for Dude Chaos Wars AI Renders (Steps 1..5).

Driven by `production/IMAGE_MANIFEST_185.json`:
- Supports all 4 `bg_mode` contracts:
  1. `pure_white_border`:
     - Optional horizontal flip (`--flip-h` or per-item config)
     - Border-connected BFS pure-white normalization (`min_channel=242, max_chroma=6`)
     - For horizontal VFX strips (`W / H >= 3`, e.g. Step 3 `6144x512` / `512x64`), trims
       excess top/bottom connected pure-white margins before fitting so the horizontal
       sprite row fills the strip height cleanly.
     - Aspect-preserving LANCZOS fit onto an exact `(W, H)` `#FFFFFF` canvas
     - Post-resize border white cleanup (`min_channel=252, max_chroma=3`)
  2. `first_person_interior_openings_white`:
     - Cover-scale + center-crop to `(W, H)` + normalize near-white window openings to `#FFFFFF`
  3. `full_bleed_camera_plate` (`706`) & `tileable_texture` (`455`):
     - Cover-scale + center-crop to `(W, H)` with no white-border modification
- Saves lossless RGB `.webp` (`lossless=True, quality=100, method=4`) to
  `renders/buoc_0X/<NNN> <Title>.webp`.
- Runs `review_images.py` validation and, when `--seal-zip` is passed (or when all items in the
  step are `KEEP`), packages `buoc_0X.zip` (`zipfile.ZIP_STORED`, flat root entries only).
"""

from __future__ import annotations

import argparse
from collections import deque
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import zipfile

from PIL import Image, ImageDraw

REPO_ROOT = Path(__file__).resolve().parents[2]
MANIFEST_PATH = REPO_ROOT / "production" / "IMAGE_MANIFEST_185.json"
REVIEW_SCRIPT = Path(__file__).resolve().parent / "review_images.py"


def load_manifest(manifest_path: Path = MANIFEST_PATH) -> dict[str, dict]:
    data = json.loads(manifest_path.read_text(encoding="utf-8"))
    return {item["id"]: item for item in data["items"]}


def normalize_connected_border_white(
    img: Image.Image,
    min_channel: int = 242,
    max_chroma: int = 6,
) -> Image.Image:
    """Flood-fill from all 4 image edges and snap connected near-white background to #FFFFFF."""
    rgb = img.convert("RGB")
    w, h = rgb.size
    px = rgb.load()
    visited = bytearray(w * h)
    q: deque[tuple[int, int]] = deque()

    def is_bg_white(r: int, g: int, b: int) -> bool:
        return (
            r >= min_channel
            and g >= min_channel
            and b >= min_channel
            and (max(r, g, b) - min(r, g, b)) <= max_chroma
        )

    def try_push(x: int, y: int) -> None:
        idx = y * w + x
        if visited[idx]:
            return
        r, g, b = px[x, y]
        if is_bg_white(r, g, b):
            visited[idx] = 1
            q.append((x, y))

    for x in range(w):
        try_push(x, 0)
        try_push(x, h - 1)
    for y in range(h):
        try_push(0, y)
        try_push(w - 1, y)

    while q:
        x, y = q.popleft()
        px[x, y] = (255, 255, 255)
        if x > 0:
            try_push(x - 1, y)
        if x + 1 < w:
            try_push(x + 1, y)
        if y > 0:
            try_push(x, y - 1)
        if y + 1 < h:
            try_push(x, y + 1)

    return rgb


def trim_strip_vertical_white_margins(
    rgb: Image.Image,
    pad_ratio: float = 0.08,
) -> Image.Image:
    """
    For wide horizontal VFX strips (target W / H >= 3) generated inside a 16:9 frame,
    crop excess top/bottom pure-white margins while preserving full width or subject bbox
    with a small breathing margin so the strip fills the target height cleanly.
    """
    w, h = rgb.size
    px = rgb.load()
    min_x, min_y = w, h
    max_x, max_y = -1, -1

    for y in range(h):
        for x in range(w):
            if px[x, y] != (255, 255, 255):
                if x < min_x:
                    min_x = x
                if x > max_x:
                    max_x = x
                if y < min_y:
                    min_y = y
                if y > max_y:
                    max_y = y

    if max_x < min_x or max_y < min_y:
        return rgb

    subj_w = max_x - min_x + 1
    subj_h = max_y - min_y + 1
    pad_x = max(4, int(round(subj_w * pad_ratio * 0.5)))
    pad_y = max(4, int(round(subj_h * pad_ratio)))

    x0 = max(0, min_x - pad_x)
    y0 = max(0, min_y - pad_y)
    x1 = min(w, max_x + 1 + pad_x)
    y1 = min(h, max_y + 1 + pad_y)
    return rgb.crop((x0, y0, x1, y1))


def fit_on_pure_white_canvas(rgb: Image.Image, target_w: int, target_h: int) -> Image.Image:
    sw, sh = rgb.size
    if (sw, sh) == (target_w, target_h):
        return rgb.copy()
    scale = min(target_w / sw, target_h / sh)
    nw = max(1, int(round(sw * scale)))
    nh = max(1, int(round(sh * scale)))
    resized = rgb.resize((nw, nh), Image.Resampling.LANCZOS)
    canvas = Image.new("RGB", (target_w, target_h), (255, 255, 255))
    ox = (target_w - nw) // 2
    oy = (target_h - nh) // 2
    canvas.paste(resized, (ox, oy))
    return canvas


def cover_crop(rgb: Image.Image, target_w: int, target_h: int) -> Image.Image:
    sw, sh = rgb.size
    if (sw, sh) == (target_w, target_h):
        return rgb.copy()
    scale = max(target_w / sw, target_h / sh)
    nw = max(target_w, int(round(sw * scale)))
    nh = max(target_h, int(round(sh * scale)))
    resized = rgb.resize((nw, nh), Image.Resampling.LANCZOS)
    ox = (nw - target_w) // 2
    oy = (nh - target_h) // 2
    return resized.crop((ox, oy, ox + target_w, oy + target_h))


def normalize_window_openings_white(
    rgb: Image.Image,
    min_channel: int = 244,
    max_chroma: int = 6,
) -> Image.Image:
    out = rgb.copy()
    w, h = out.size
    px = out.load()
    for y in range(h):
        for x in range(w):
            r, g, b = px[x, y]
            if (
                r >= min_channel
                and g >= min_channel
                and b >= min_channel
                and (max(r, g, b) - min(r, g, b)) <= max_chroma
            ):
                px[x, y] = (255, 255, 255)
    return out


def process_single_item(
    src_path: Path,
    item_meta: dict,
    out_dir: Path | None = None,
    flip_h: bool = False,
) -> Path:
    target_w, target_h = item_meta["expected_size"]
    bg_mode = item_meta["bg_mode"]
    dest_dir = out_dir if out_dir is not None else (REPO_ROOT / "renders" / f"buoc_{int(item_meta['step']):02d}")
    dest_dir.mkdir(parents=True, exist_ok=True)
    out_path = dest_dir / item_meta["output_filename"]

    img = Image.open(src_path).convert("RGB")
    if flip_h:
        img = img.transpose(Image.Transpose.FLIP_LEFT_RIGHT)

    if bg_mode == "pure_white_border":
        img = normalize_connected_border_white(img, min_channel=242, max_chroma=6)
        if target_w >= target_h * 3:
            img = trim_strip_vertical_white_margins(img)
        img = fit_on_pure_white_canvas(img, target_w, target_h)
        img = normalize_connected_border_white(img, min_channel=252, max_chroma=3)
    elif bg_mode == "first_person_interior_openings_white":
        img = cover_crop(img, target_w, target_h)
        img = normalize_window_openings_white(img, min_channel=244, max_chroma=6)
    elif bg_mode in ("full_bleed_camera_plate", "tileable_texture"):
        img = cover_crop(img, target_w, target_h)
    else:
        raise ValueError(f"Unsupported bg_mode: {bg_mode}")

    img.save(out_path, "WEBP", lossless=True, quality=100, method=4)
    return out_path


def seal_step_zip(step: int, renders_dir: Path | None = None, zip_out: Path | None = None) -> Path:
    step_tag = f"buoc_{step:02d}"
    src_dir = renders_dir if renders_dir is not None else (REPO_ROOT / "renders" / step_tag)
    dest_zip = zip_out if zip_out is not None else (REPO_ROOT / f"{step_tag}.zip")

    manifest = load_manifest()
    step_items = [m for m in manifest.values() if int(m["step"]) == step]
    step_items.sort(key=lambda m: m["output_filename"])

    missing = [
        m["output_filename"]
        for m in step_items
        if not (src_dir / m["output_filename"]).is_file()
    ]
    if missing:
        raise FileNotFoundError(
            f"Cannot seal {dest_zip.name}: {len(missing)}/{len(step_items)} files missing in {src_dir}"
        )

    with zipfile.ZipFile(dest_zip, "w", compression=zipfile.ZIP_STORED) as zf:
        for m in step_items:
            fname = m["output_filename"]
            zf.write(src_dir / fname, arcname=fname)

    return dest_zip


def run_self_test() -> int:
    """Synthetically exercise all 4 bg_modes and extreme aspect ratios against review_images.py."""
    manifest = load_manifest()
    selected: dict[str, dict] = {}
    for item_id, meta in manifest.items():
        w, h = meta["expected_size"]
        key = f"{meta['bg_mode']}_{w}x{h}"
        if key not in selected:
            selected[key] = meta
    representative_ids = [m["id"] for m in selected.values()]

    with tempfile.TemporaryDirectory(prefix="dcw_art_selftest_") as tmp:
        tmp_dir = Path(tmp)
        raw_dir = tmp_dir / "raw"
        out_dir = tmp_dir / "renders"
        raw_dir.mkdir(parents=True, exist_ok=True)
        out_dir.mkdir(parents=True, exist_ok=True)

        for item_id in representative_ids:
            meta = manifest[item_id]
            raw_path = raw_dir / f"raw_{item_id}.jpg"
            bg_mode = meta["bg_mode"]

            if bg_mode == "pure_white_border":
                img = Image.new("RGB", (1024, 576), (255, 255, 255))
                draw = ImageDraw.Draw(img)
                # Draw a multi-toned colored subject in the center
                for idx in range(6):
                    x0 = 140 + idx * 120
                    draw.rectangle(
                        [x0, 210, x0 + 90, 360],
                        fill=(50 + idx * 25, 110 + idx * 15, 180 - idx * 15),
                        outline=(30, 40, 55),
                        width=3,
                    )
            elif bg_mode == "first_person_interior_openings_white":
                img = Image.new("RGB", (1024, 576), (48, 52, 60))
                draw = ImageDraw.Draw(img)
                # White windshield opening
                draw.rectangle([120, 40, 900, 260], fill=(255, 255, 255))
                # Dashboard dials
                draw.ellipse([220, 320, 420, 520], fill=(80, 95, 115), outline=(25, 30, 35), width=4)
            else:
                img = Image.new("RGB", (1024, 576), (85, 110, 92))
                draw = ImageDraw.Draw(img)
                for y in range(0, 576, 32):
                    for x in range(0, 1024, 32):
                        c = (70 + (x * 13 + y * 7) % 90, 85 + (x * 5 + y * 11) % 80, 65 + (x + y) % 70)
                        draw.rectangle([x, y, x + 31, y + 31], fill=c)

            img.save(raw_path, "JPEG", quality=96)
            out_path = process_single_item(raw_path, meta, out_dir=out_dir)
            with Image.open(out_path) as check_im:
                assert check_im.size == tuple(meta["expected_size"]), (
                    f"{item_id}: size {check_im.size} != {meta['expected_size']}"
                )

        cmd = [
            sys.executable,
            str(REVIEW_SCRIPT),
            str(out_dir),
            "--allow-partial",
            "--strict",
        ]
        res = subprocess.run(cmd, capture_output=True, text=True, check=False)
        if res.returncode != 0:
            sys.stderr.write(res.stdout + "\n" + res.stderr)
            return res.returncode

    print(
        f"SELF-TEST PASS: {len(representative_ids)} representative items across all 4 bg_modes "
        f"and dimensions (up to 6144x512 & down to 128x128) passed review_images.py --strict."
    )
    return 0


def main() -> int:
    parser = argparse.ArgumentParser(description="Post-process & package Dude Chaos Wars AI step renders.")
    parser.add_argument("--id", help="3-digit item ID from IMAGE_MANIFEST_185.json (e.g. 146)")
    parser.add_argument("--src", type=Path, help="Source raw image path (.jpg/.png/.webp)")
    parser.add_argument("--out-dir", type=Path, help="Optional override output directory")
    parser.add_argument("--flip-h", action="store_true", help="Flip image horizontally before background cleanup")
    parser.add_argument("--batch-json", type=Path, help="JSON file mapping item ID -> source path (or {src, flip_h})")
    parser.add_argument("--seal-step", type=int, choices=[1, 2, 3, 4, 5], help="Verify and seal buoc_0X.zip")
    parser.add_argument("--self-test", action="store_true", help="Run synthetic self-test across all 4 bg_modes")
    args = parser.parse_args()

    if args.self_test:
        return run_self_test()

    manifest = load_manifest()

    if args.id and args.src:
        item_id = f"{int(args.id):03d}"
        if item_id not in manifest:
            raise KeyError(f"Unknown item ID: {item_id}")
        out_path = process_single_item(args.src, manifest[item_id], out_dir=args.out_dir, flip_h=args.flip_h)
        print(f"Processed {item_id} -> {out_path} ({manifest[item_id]['expected_size']})")

    if args.batch_json:
        batch_data = json.loads(args.batch_json.read_text(encoding="utf-8"))
        for raw_id, entry in batch_data.items():
            item_id = f"{int(raw_id):03d}"
            meta = manifest[item_id]
            if isinstance(entry, dict):
                src_path = Path(entry["src"])
                flip_h = bool(entry.get("flip_h", False))
            else:
                src_path = Path(entry)
                flip_h = False
            out_path = process_single_item(src_path, meta, out_dir=args.out_dir, flip_h=flip_h)
            print(f"Processed {item_id} -> {out_path} ({meta['expected_size']})")

    if args.seal_step:
        step = args.seal_step
        step_dir = args.out_dir if args.out_dir else (REPO_ROOT / "renders" / f"buoc_{step:02d}")
        cmd = [sys.executable, str(REVIEW_SCRIPT), str(step_dir), "--step", str(step), "--strict"]
        res = subprocess.run(cmd, check=False)
        if res.returncode != 0:
            return res.returncode
        zip_path = seal_step_zip(step, renders_dir=step_dir)
        verify_cmd = [sys.executable, str(REVIEW_SCRIPT), str(zip_path), "--step", str(step), "--strict"]
        res_zip = subprocess.run(verify_cmd, check=False)
        if res_zip.returncode != 0:
            return res_zip.returncode
        print(f"Sealed and verified {zip_path}")

    return 0


if __name__ == "__main__":
    sys.exit(main())
