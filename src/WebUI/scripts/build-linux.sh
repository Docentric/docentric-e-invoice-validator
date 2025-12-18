#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

cd "${PROJECT_ROOT}"

echo "[build] Ensuring dependencies are installed..."
if [ ! -d "node_modules" ]; then
  npm install
else
  echo "[build] node_modules directory exists; skipping npm install."
fi

echo "[build] Building the production bundle..."
npm run build

echo "[build] Build complete. Final HTML and assets are available in the dist/ directory."
