@echo off
setlocal enabledelayedexpansion

:: Get script directory and project root
set "SCRIPT_DIR=%~dp0"
set "PROJECT_ROOT=%SCRIPT_DIR%.."

:: Change to project root
cd /d "%PROJECT_ROOT%"

:: Function to check if a command exists
:check_command
where "%~1" >nul 2>&1
if !errorlevel! neq 0 (
    echo [init] ERROR: %~2 ('%~1') is not available on PATH. >&2
    echo [init] Please install Node.js 18+ (which bundles npm) before continuing. >&2
    exit /b 1
)
goto :eof

:: Check for required commands
call :check_command node "Node.js"
if !errorlevel! neq 0 exit /b !errorlevel!

call :check_command npm "npm"
if !errorlevel! neq 0 exit /b !errorlevel!

:: Get versions
for /f "delims=" %%i in ('node --version') do set "node_version=%%i"
for /f "delims=" %%i in ('npm --version') do set "npm_version=%%i"

echo [init] Using Node.js !node_version! and npm !npm_version!.

echo [init] Installing project dependencies...
npm install
if !errorlevel! neq 0 (
    echo [init] ERROR: Failed to install dependencies. >&2
    exit /b 1
)

echo [init] Tooling successfully initialised. You can now run one of the build scripts in scripts/.