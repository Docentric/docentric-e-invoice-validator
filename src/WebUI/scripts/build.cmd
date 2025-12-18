@echo off
setlocal enabledelayedexpansion

set SCRIPT_DIR=%~dp0
for %%I in ("%SCRIPT_DIR%..") do set PROJECT_ROOT=%%~fI
cd /d "%PROJECT_ROOT%"

echo [build] Ensuring dependencies are installed...
if not exist node_modules (
  call npm install
) else (
  echo [build] node_modules directory exists; skipping npm install.
)

echo [build] Building the production bundle...
call npm run build

echo [build] Build complete. Final HTML and assets are available in the dist/ directory.
endlocal
