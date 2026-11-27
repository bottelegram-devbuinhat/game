#!/usr/bin/env python3
"""3D prop pipeline smoke test ONLY — NOT shipping art (P11 §5 & §3.2-3.3).

Validates the 3D export contract:
  - 1 unit = 1 metre, Y up, +Z forward
  - Ground-centre pivot (min Y == 0.0, X/Z centred at 0.0)
  - Small prop triangle budget <= 1,000 tris

Modes:
  1. Inside headless Blender:
       blender --background --python Tools/art/make_props.py -- --out /tmp/props_smoke
  2. Standalone offline self-test (no Blender required):
       python3 Tools/art/make_props.py --self-test --out /tmp/props_smoke
"""

from __future__ import annotations

import argparse
import base64
import json
import math
import struct
import sys
from pathlib import Path

PROP_SPECS = (
    # (name, width_x, height_y, depth_z)
    ("PRP_TrafficCone", 0.36, 0.72, 0.36),
    ("PRP_StreetCrate", 0.60, 0.50, 0.60),
    ("PRP_TrashBin", 0.48, 0.85, 0.48),
)


def make_box_mesh(wx: float, hy: float, dz: float) -> tuple[list[tuple[float, float, float]], list[tuple[int, int, int]]]:
    """Builds a ground-pivoted box: X in [-wx/2, wx/2], Y in [0, hy], Z in [-dz/2, dz/2]."""
    hx, hz = wx * 0.5, dz * 0.5
    verts = [
        (-hx, 0.0, -hz),
        (hx, 0.0, -hz),
        (hx, hy, -hz),
        (-hx, hy, -hz),
        (-hx, 0.0, hz),
        (hx, 0.0, hz),
        (hx, hy, hz),
        (-hx, hy, hz),
    ]
    tris = [
        (0, 2, 1), (0, 3, 2),  # back -Z
        (4, 5, 6), (4, 6, 7),  # front +Z
        (0, 1, 5), (0, 5, 4),  # bottom Y=0
        (3, 6, 2), (3, 7, 6),  # top Y=hy
        (0, 4, 7), (0, 7, 3),  # left -X
        (1, 2, 6), (1, 6, 5),  # right +X
    ]
    return verts, tris


def write_minimal_gltf(out_path: Path, name: str, verts: list[tuple[float, float, float]], tris: list[tuple[int, int, int]]) -> None:
    pos_bytes = b"".join(struct.pack("<fff", *v) for v in verts)
    idx_bytes = b"".join(struct.pack("<HHH", *t) for t in tris)
    while len(idx_bytes) % 4 != 0:
        idx_bytes += b"\x00"
    raw_buf = pos_bytes + idx_bytes
    uri = "data:application/octet-stream;base64," + base64.b64encode(raw_buf).decode("ascii")

    min_xyz = [min(v[i] for v in verts) for i in range(3)]
    max_xyz = [max(v[i] for v in verts) for i in range(3)]

    gltf = {
        "asset": {"version": "2.0", "generator": "DudeChaos make_props.py (smoke test only)"},
        "scene": 0,
        "scenes": [{"name": "SmokeScene", "nodes": [0]}],
        "nodes": [{"name": name, "mesh": 0}],
        "meshes": [
            {
                "name": name,
                "primitives": [{"attributes": {"POSITION": 0}, "indices": 1}],
            }
        ],
        "buffers": [{"byteLength": len(raw_buf), "uri": uri}],
        "bufferViews": [
            {"buffer": 0, "byteOffset": 0, "byteLength": len(pos_bytes), "target": 34962},
            {"buffer": 0, "byteOffset": len(pos_bytes), "byteLength": len(tris) * 6, "target": 34963},
        ],
        "accessors": [
            {
                "bufferView": 0,
                "componentType": 5126,
                "count": len(verts),
                "type": "VEC3",
                "min": min_xyz,
                "max": max_xyz,
            },
            {
                "bufferView": 1,
                "componentType": 5123,
                "count": len(tris) * 3,
                "type": "SCALAR",
            },
        ],
    }
    out_path.write_text(json.dumps(gltf, indent=2) + "\n", encoding="utf-8")


def run_blender_export(out_dir: Path) -> int:
    import bpy  # type: ignore

    out_dir.mkdir(parents=True, exist_ok=True)
    for name, wx, hy, dz in PROP_SPECS:
        bpy.ops.wm.read_factory_settings(use_empty=True)
        verts, tris = make_box_mesh(wx, hy, dz)
        mesh = bpy.data.meshes.new(name)
        mesh.from_pydata(verts, [], tris)
        mesh.update()
        obj = bpy.data.objects.new(name, mesh)
        bpy.context.collection.objects.link(obj)
        glb_path = out_dir / f"{name}.glb"
        bpy.ops.export_scene.gltf(filepath=str(glb_path), export_format="GLB", export_yup=True)
        print(f"Exported smoke prop (Blender): {glb_path}")
    return 0


def run_self_test(out_dir: Path) -> int:
    out_dir.mkdir(parents=True, exist_ok=True)
    for name, wx, hy, dz in PROP_SPECS:
        verts, tris = make_box_mesh(wx, hy, dz)
        min_y = min(v[1] for v in verts)
        cx = 0.5 * (min(v[0] for v in verts) + max(v[0] for v in verts))
        cz = 0.5 * (min(v[2] for v in verts) + max(v[2] for v in verts))
        if abs(min_y) > 1e-6 or abs(cx) > 1e-6 or abs(cz) > 1e-6:
            print(f"FAIL: {name} ground-centre pivot violated (min_y={min_y}, cx={cx}, cz={cz})", file=sys.stderr)
            return 1
        if len(tris) > 1000:
            print(f"FAIL: {name} exceeds 1000-tri small prop budget ({len(tris)} tris)", file=sys.stderr)
            return 1
        gltf_path = out_dir / f"{name}.gltf"
        write_minimal_gltf(gltf_path, name, verts, tris)
        print(f"Smoke prop OK: {name} ({wx}x{hy}x{dz} m, {len(tris)} tris, ground pivot) -> {gltf_path}")
    return 0


def main() -> int:
    argv = sys.argv[1:]
    if "--" in argv:
        argv = argv[argv.index("--") + 1 :]

    parser = argparse.ArgumentParser(description="Pipeline smoke-test prop generator (NOT shipping art)")
    parser.add_argument("--out", type=Path, default=Path("/tmp/dudechaos_smoke_props"), help="Output folder")
    parser.add_argument("--self-test", action="store_true", help="Run standalone glTF smoke test without Blender")
    args = parser.parse_args(argv)

    if args.self_test:
        return run_self_test(args.out)

    try:
        import bpy  # noqa: F401  # type: ignore
    except ImportError:
        print("Blender (bpy) not active; running standalone glTF smoke test instead.")
        return run_self_test(args.out)

    return run_blender_export(args.out)


if __name__ == "__main__":
    raise SystemExit(main())
