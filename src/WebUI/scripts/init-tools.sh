#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

cd "${PROJECT_ROOT}"

check_command() {
  local cmd="$1"
  local friendly_name="$2"
  if ! command -v "$cmd" >/dev/null 2>&1; then
    echo "[init] ERROR: $friendly_name ('$cmd') is not available on PATH." >&2
    echo "[init] Please install Node.js 18+ (which bundles npm) before continuing." >&2
    exit 1
  fi
}

check_command node "Node.js"
check_command npm "npm"

node_version="$(node --version)"
npm_version="$(npm --version)"

echo "[init] Using Node.js ${node_version} and npm ${npm_version}."

echo "[init] Installing project dependencies..."
npm install

echo "[init] Tooling successfully initialised. You can now run one of the build scripts in scripts/."
