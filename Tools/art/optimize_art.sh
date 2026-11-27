#!/usr/bin/env bash
# Optimize 3D GLB/GLTF meshes and PNG/WebP textures for mobile APK budget (§1.9.7 & §3.3).
# Usage:
#   bash Tools/art/optimize_art.sh <input_dir> [output_dir] [--dry-run]
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if [ "${1:-}" = "--help" ] || [ $# -lt 1 ]; then
  echo "Usage: $0 <input_dir> [output_dir] [--dry-run]"
  exit 0
fi

IN_DIR="$1"
OUT_DIR="${2:-$IN_DIR}"
DRY_RUN="${3:-}"

if [ ! -d "$IN_DIR" ]; then
  echo "ERROR: input directory '$IN_DIR' does not exist." >&2
  exit 1
fi

mkdir -p "$OUT_DIR"

mesh_count=0
while IFS= read -r -d '' glb; do
  rel="${glb#"$IN_DIR"/}"
  dest="$OUT_DIR/$rel"
  mkdir -p "$(dirname "$dest")"
  mesh_count=$((mesh_count + 1))
  if [ "$DRY_RUN" = "--dry-run" ]; then
    echo "[DRY-RUN] mesh: $glb -> $dest"
    continue
  fi
  if command -v gltf-transform >/dev/null 2>&1; then
    gltf-transform optimize "$glb" "$dest" --compress false >/dev/null
    echo "[OK] optimized mesh: $rel"
  elif [ "$glb" != "$dest" ]; then
    cp "$glb" "$dest"
    echo "[WARN] gltf-transform absent; copied mesh unchanged: $rel"
  fi
done < <(find "$IN_DIR" -type f \( -iname '*.glb' -o -iname '*.gltf' \) -print0)

TEX_ARGS=("$IN_DIR" --out "$OUT_DIR")
if [ "$DRY_RUN" = "--dry-run" ]; then
  TEX_ARGS+=(--dry-run)
fi
python3 "$SCRIPT_DIR/optimize_textures.py" "${TEX_ARGS[@]}"
echo "optimize_art complete ($mesh_count mesh file(s) scanned)."
