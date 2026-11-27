#!/usr/bin/env python3
"""Border-connected white-to-alpha background keyer for Dude Chaos Wars (§3.8).

Implements the exact §3.8 specification using Pillow + BFS flood fill:
  1. Keys by border-connected region (NOT global brightness), so white shirts (#F2F2F2)
     and steel highlights inside silhouettes remain 100% opaque.
  2. Treats pure white AND neutral light grey (min channel >= 230, channel spread <= 8)
     as background to strip soft contact shadows cleanly.
  3. Optional `--fill-holes <pct>` (e.g. `--fill-holes 0.5`) removes small enclosed pure-white
     background pockets (bike frames, wheel spokes, fan cages) up to `<pct>%` of canvas area
     without punching shaded clothing.
  4. Applies a 2 px soft antialiased edge + mathematical white un-matte on boundary pixels so
     scaling down in Unity leaves zero white halo.
  5. `--no-trim` (default) preserves exact canvas and grid alignment for sprite strips/atlases.
"""

from __future__ import annotations

import argparse
from collections import deque
import sys
from pathlib import Path
from PIL import Image, ImageFilter


def key_image_white_to_alpha(
    img: Image.Image,
    min_channel: int = 230,
    max_spread: int = 8,
    fill_holes_pct: float = 0.0,
    soft_edge_px: int = 2,
    trim: bool = False,
) -> Image.Image:
    rgba = img.convert("RGBA")
    w, h = rgba.size
    total_px = w * h
    if total_px == 0:
        return rgba

    raw = bytearray(rgba.tobytes())

    # 1. Classify candidate background pixels:
    #    0 = foreground
    #    1 = neutral light-grey / white candidate (min >= min_channel, spread <= max_spread)
    #    2 = pure white candidate (min >= 248, spread <= 4) or already alpha == 0
    candidate = bytearray(total_px)
    for p in range(total_px):
        off = p * 4
        r, g, b, a = raw[off], raw[off + 1], raw[off + 2], raw[off + 3]
        if a == 0:
            candidate[p] = 2
            continue
        mn = r if r < g else g
        if b < mn:
            mn = b
        mx = r if r > g else g
        if b > mx:
            mx = b
        spread = mx - mn
        if mn >= 248 and spread <= 4:
            candidate[p] = 2
        elif mn >= min_channel and spread <= max_spread:
            candidate[p] = 1

    # 2. 4-connected BFS flood fill seeded from all 4 image borders
    keyed = bytearray(total_px)
    q: deque[int] = deque()

    def try_seed(idx: int) -> None:
        if candidate[idx] and not keyed[idx]:
            keyed[idx] = 1
            q.append(idx)

    for x in range(w):
        try_seed(x)
        try_seed((h - 1) * w + x)
    for y in range(1, h - 1):
        row = y * w
        try_seed(row)
        try_seed(row + w - 1)

    while q:
        cur = q.popleft()
        cy, cx = divmod(cur, w)
        if cx > 0:
            nb = cur - 1
            if candidate[nb] and not keyed[nb]:
                keyed[nb] = 1
                q.append(nb)
        if cx + 1 < w:
            nb = cur + 1
            if candidate[nb] and not keyed[nb]:
                keyed[nb] = 1
                q.append(nb)
        if cy > 0:
            nb = cur - w
            if candidate[nb] and not keyed[nb]:
                keyed[nb] = 1
                q.append(nb)
        if cy + 1 < h:
            nb = cur + w
            if candidate[nb] and not keyed[nb]:
                keyed[nb] = 1
                q.append(nb)

    # 3. Optional enclosed pocket keying (--fill-holes <pct>, e.g. 0.5 = 0.5% of image area)
    if fill_holes_pct > 0.0:
        max_pocket = max(1, int(round((fill_holes_pct / 100.0) * total_px)))
        visited_inner = bytearray(total_px)
        for start in range(total_px):
            if keyed[start] or visited_inner[start] or not candidate[start]:
                continue
            comp: list[int] = [start]
            visited_inner[start] = 1
            has_pure_white = candidate[start] == 2
            cq: deque[int] = deque([start])
            while cq:
                cur = cq.popleft()
                cy, cx = divmod(cur, w)
                for nb in (
                    cur - 1 if cx > 0 else -1,
                    cur + 1 if cx + 1 < w else -1,
                    cur - w if cy > 0 else -1,
                    cur + w if cy + 1 < h else -1,
                ):
                    if nb >= 0 and not keyed[nb] and not visited_inner[nb] and candidate[nb]:
                        visited_inner[nb] = 1
                        if candidate[nb] == 2:
                            has_pure_white = True
                        comp.append(nb)
                        cq.append(nb)

            if has_pure_white and len(comp) <= max_pocket:
                for p in comp:
                    keyed[p] = 1

    # 4. 2px soft edge + mathematical white un-matte
    hard_bytes = bytes(0 if keyed[p] else 255 for p in range(total_px))
    hard_mask = Image.frombytes("L", (w, h), hard_bytes)

    if soft_edge_px > 0:
        core_mask = hard_mask
        for _ in range(soft_edge_px):
            core_mask = core_mask.filter(ImageFilter.MinFilter(3))
        blur_mask = hard_mask.filter(ImageFilter.GaussianBlur(radius=max(0.5, soft_edge_px * 0.5)))
        core_data = core_mask.tobytes()
        blur_data = blur_mask.tobytes()
    else:
        core_data = hard_bytes
        blur_data = hard_bytes

    for p in range(total_px):
        off = p * 4
        if keyed[p]:
            raw[off] = 0
            raw[off + 1] = 0
            raw[off + 2] = 0
            raw[off + 3] = 0
        elif core_data[p] == 255:
            # Strictly interior foreground (> soft_edge_px from keyed region): keep full opacity & original RGB
            pass
        else:
            # Within the 2px silhouette edge band: apply soft alpha + un-matte against white (#FFFFFF)
            orig_a = raw[off + 3]
            soft_a = min(orig_a, max(1, blur_data[p]))
            af = soft_a / 255.0
            bg_contrib = (1.0 - af) * 255.0
            for ch in range(3):
                c = raw[off + ch]
                unmatted = int(round((c - bg_contrib) / af))
                raw[off + ch] = 0 if unmatted < 0 else (255 if unmatted > 255 else unmatted)
            raw[off + 3] = soft_a

    out = Image.frombytes("RGBA", (w, h), bytes(raw))
    if trim:
        bbox = out.getbbox()
        if bbox:
            out = out.crop(bbox)
    return out


def process_path(
    src: Path,
    dst: Path,
    min_channel: int,
    max_spread: int,
    fill_holes: float,
    soft_edge: int,
    trim: bool,
) -> None:
    with Image.open(src) as img:
        keyed = key_image_white_to_alpha(
            img,
            min_channel=min_channel,
            max_spread=max_spread,
            fill_holes_pct=fill_holes,
            soft_edge_px=soft_edge,
            trim=trim,
        )
        dst.parent.mkdir(parents=True, exist_ok=True)
        if dst.suffix.lower() == ".webp":
            keyed.save(dst, format="WEBP", lossless=True, method=4)
        else:
            keyed.save(dst, format="PNG")


def main() -> int:
    parser = argparse.ArgumentParser(description="Border-connected white-to-alpha keyer (§3.8)")
    parser.add_argument("input", type=Path, help="Input image file or directory")
    parser.add_argument("--out", type=Path, required=True, help="Output file or directory")
    parser.add_argument("--min-channel", type=int, default=230, help="Min RGB channel for neutral grey/white bg (default: 230)")
    parser.add_argument("--max-spread", type=int, default=8, help="Max RGB channel spread for neutral grey/white bg (default: 8)")
    parser.add_argument("--fill-holes", type=float, default=0.0, help="Max enclosed white pocket area %% to key (e.g. 0.5)")
    parser.add_argument("--soft-edge", type=int, default=2, help="Soft edge width in pixels (default: 2)")
    trim_group = parser.add_mutually_exclusive_group()
    trim_group.add_argument("--no-trim", dest="trim", action="store_false", help="Keep original canvas/grid (default; mandatory for sprite strips)")
    trim_group.add_argument("--trim", dest="trim", action="store_true", help="Crop transparent margins (single icons only)")
    parser.set_defaults(trim=False)

    args = parser.parse_args()
    inp: Path = args.input
    out: Path = args.out

    if inp.is_file():
        target_file = out if out.suffix else (out / f"{inp.stem}.png")
        process_path(inp, target_file, args.min_channel, args.max_spread, args.fill_holes, args.soft_edge, args.trim)
        print(f"Keyed 1 image -> {target_file} (trim={args.trim}, fill_holes={args.fill_holes}%)")
        return 0

    if inp.is_dir():
        files = sorted(p for p in inp.rglob("*") if p.is_file() and p.suffix.lower() in (".png", ".webp"))
        for src in files:
            rel = src.relative_to(inp)
            dst = out / rel
            process_path(src, dst, args.min_channel, args.max_spread, args.fill_holes, args.soft_edge, args.trim)
        print(f"Keyed {len(files)} image(s) -> {out} (trim={args.trim}, fill_holes={args.fill_holes}%)")
        return 0

    print(f"ERROR: input not found: {inp}", file=sys.stderr)
    return 1


if __name__ == "__main__":
    raise SystemExit(main())
