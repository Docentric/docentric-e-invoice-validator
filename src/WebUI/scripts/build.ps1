#!/usr/bin/env pwsh
$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$projectRoot = Resolve-Path (Join-Path $scriptDir "..")
Set-Location $projectRoot

Write-Host "[build] Ensuring dependencies are installed..."
if (-not (Test-Path "node_modules")) {
  npm install
} else {
  Write-Host "[build] node_modules directory exists; skipping npm install."
}

Write-Host "[build] Building the production bundle..."
npm run build

Write-Host "[build] Build complete. Final HTML and assets are available in the dist/ directory."
