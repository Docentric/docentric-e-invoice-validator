# Set strict mode equivalent to bash's set -euo pipefail
$ErrorActionPreference = "Stop"

# Get script directory and project root
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir

# Change to project root
Set-Location $ProjectRoot

function Test-Command {
    param(
        [string]$Command,
        [string]$FriendlyName
    )
    
    if (-not (Get-Command $Command -ErrorAction SilentlyContinue)) {
        Write-Error "[init] ERROR: $FriendlyName ('$Command') is not available on PATH."
        Write-Error "[init] Please install Node.js 18+ (which bundles npm) before continuing."
        exit 1
    }
}

# Check for required commands
Test-Command "node" "Node.js"
Test-Command "npm" "npm"

# Get versions
$nodeVersion = & node --version
$npmVersion = & npm --version

Write-Host "[init] Using Node.js $nodeVersion and npm $npmVersion."

Write-Host "[init] Installing project dependencies..."
& npm install

if ($LASTEXITCODE -ne 0) {
    Write-Error "[init] ERROR: Failed to install dependencies."
    exit 1
}

Write-Host "[init] Tooling successfully initialised. You can now run one of the build scripts in scripts/."