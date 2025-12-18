set SCRIPT_DIR=%~dp0
for %%I in ("%SCRIPT_DIR%..") do set PROJECT_ROOT=%%~fI
cd /d "%PROJECT_ROOT%"

echo [serve] Ensuring dependencies are installed...
if not exist node_modules (
  call npm install
) else (
  echo [serve] node_modules directory exists; skipping npm install.
)

echo [serve] Starting the local server for development...
call npm run dev