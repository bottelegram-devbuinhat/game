#!/usr/bin/env bash
# Setup / check headless art pipeline tools for Dude Chaos Wars (P11 §5).
# Usage:
#   bash Tools/art/setup_tools.sh --check
#   bash Tools/art/setup_tools.sh --install
set -euo pipefail

MODE="${1:---check}"
TOOLS_CACHE="${DUDECHAOS_TOOLS_DIR:-$HOME/.cache/dudechaos-tools}"
BLENDER_VER="4.2.3"
BLENDER_DIR="$TOOLS_CACHE/blender-${BLENDER_VER}-linux-x64"

check_tool() {
  local label="$1"
  local cmd="$2"
  if eval "$cmd" >/dev/null 2>&1; then
    echo "  [OK]      $label"
    return 0
  else
    echo "  [MISSING] $label"
    return 1
  fi
}

report_status() {
  echo "=== Dude Chaos Wars Art Toolchain Status ==="
  local missing=0
  check_tool "python3" "command -v python3" || missing=$((missing + 1))
  check_tool "Pillow (PIL)" "python3 -c 'import PIL'" || missing=$((missing + 1))
  check_tool "ffmpeg" "command -v ffmpeg" || missing=$((missing + 1))
  check_tool "node / npx" "command -v npx" || missing=$((missing + 1))
  check_tool "gltf-transform" "command -v gltf-transform" || missing=$((missing + 1))
  if command -v blender >/dev/null 2>&1; then
    echo "  [OK]      blender ($(command -v blender))"
  elif [ -x "$BLENDER_DIR/blender" ]; then
    echo "  [OK]      blender portable ($BLENDER_DIR/blender)"
  else
    echo "  [MISSING] blender headless"
    missing=$((missing + 1))
  fi
  echo "Summary: $missing tool(s) missing."
  return 0
}

if [ "$MODE" = "--check" ]; then
  report_status
  exit 0
fi

if [ "$MODE" = "--install" ]; then
  mkdir -p "$TOOLS_CACHE"
  echo "Installing Python Pillow..."
  python3 -m pip install --user --quiet Pillow

  if ! command -v ffmpeg >/dev/null 2>&1; then
    echo "Installing ffmpeg via apt-get..."
    sudo apt-get update -qq && sudo apt-get install -y -qq ffmpeg
  fi

  if ! command -v gltf-transform >/dev/null 2>&1 && command -v npm >/dev/null 2>&1; then
    echo "Installing @gltf-transform/cli..."
    npm install --prefix "$TOOLS_CACHE/npm" -g @gltf-transform/cli
  fi

  if ! command -v blender >/dev/null 2>&1 && [ ! -x "$BLENDER_DIR/blender" ]; then
    TARBALL="blender-${BLENDER_VER}-linux-x64.tar.xz"
    URL="https://download.blender.org/release/Blender4.2/${TARBALL}"
    echo "Downloading portable Blender ${BLENDER_VER} from ${URL}..."
    curl -fSL "$URL" -o "$TOOLS_CACHE/$TARBALL"
    tar -xf "$TOOLS_CACHE/$TARBALL" -C "$TOOLS_CACHE"
    rm -f "$TOOLS_CACHE/$TARBALL"
  fi

  report_status
  exit 0
fi

echo "Usage: $0 [--check | --install]" >&2
exit 2
