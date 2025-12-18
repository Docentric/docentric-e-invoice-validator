#!/usr/bin/env pwsh
$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$projectRoot = Resolve-Path (Join-Path $scriptDir "..")
Set-Location $projectRoot

Write-Host "[serve] Ensuring dependencies are installed..."
if (-not (Test-Path "node_modules")) {
  npm install
} else {
  Write-Host "[serve] node_modules directory exists; skipping npm install."
}

Write-Host "[serve] Starting the local server for development..."
npm run dev
