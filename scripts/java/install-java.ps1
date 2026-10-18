<#
.SYNOPSIS
    Checks for Java 11+ and installs Eclipse Temurin 21 JRE if it is missing (Windows).

.DESCRIPTION
    The e-Invoice Validator runs the Mustang CLI, which requires a Java runtime (JRE 11+).
    The script does nothing if a suitable Java is found via JAVA_HOME or PATH.
    Otherwise it tries winget and falls back to a checksum-verified Temurin ZIP
    installed per user (no administrator rights required).

.PARAMETER Force
    Non-interactive mode: install without asking for confirmation.

.NOTES
    Exit codes: 0 = Java 11+ available, 1 = installation failed, 2 = cancelled by the user.
#>
[CmdletBinding()]
param(
    [switch]$Force
)

$ErrorActionPreference = 'Stop'
$MinimumMajorVersion = 11
$TemurinFeatureVersion = 21

function Get-JavaMajorVersion([string]$JavaExecutable) {
    try {
        $output = & $JavaExecutable -version 2>&1 | Out-String
    }
    catch {
        return $null
    }

    if ($output -match 'version "(?<v>[^"]+)"') {
        $parts = $Matches['v'].Split('.', '-', '_', '+')
        if ($parts[0] -eq '1' -and $parts.Length -gt 1) { return [int]$parts[1] }
        return [int]$parts[0]
    }
    return $null
}

function Find-SuitableJava {
    $candidates = @()
    if ($env:JAVA_HOME) { $candidates += (Join-Path $env:JAVA_HOME 'bin\java.exe') }
    $onPath = Get-Command java.exe -ErrorAction SilentlyContinue
    if ($onPath) { $candidates += $onPath.Source }

    foreach ($candidate in $candidates) {
        if (-not (Test-Path -LiteralPath $candidate)) { continue }
        $major = Get-JavaMajorVersion $candidate
        if ($major -and $major -ge $MinimumMajorVersion) {
            return [pscustomobject]@{ Path = $candidate; Major = $major }
        }
    }
    return $null
}

function Install-WithWinget {
    $winget = Get-Command winget.exe -ErrorAction SilentlyContinue
    if (-not $winget) { return $false }

    Write-Host "Installing Temurin $TemurinFeatureVersion JRE with winget..."
    & $winget.Source install --exact --id "EclipseAdoptium.Temurin.$TemurinFeatureVersion.JRE" --silent --accept-package-agreements --accept-source-agreements
    return ($LASTEXITCODE -eq 0)
}

function Install-FromZip {
    $architecture = switch ($env:PROCESSOR_ARCHITECTURE) {
        'ARM64' { 'aarch64' }
        'AMD64' { 'x64' }
        default { throw "Unsupported processor architecture: $($env:PROCESSOR_ARCHITECTURE)" }
    }

    $url = "https://api.adoptium.net/v3/binary/latest/$TemurinFeatureVersion/ga/windows/$architecture/jre/hotspot/normal/eclipse"
    $temp = Join-Path ([IO.Path]::GetTempPath()) ([Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $temp | Out-Null

    try {
        $zipPath = Join-Path $temp 'temurin-jre.zip'
        Write-Host "Downloading Temurin $TemurinFeatureVersion JRE ($architecture)..."
        Invoke-WebRequest -Uri $url -OutFile $zipPath -UseBasicParsing

        # The API redirects to the GitHub release asset, which has a .sha256.txt next to it.
        $response = Invoke-WebRequest -Uri $url -Method Head -UseBasicParsing
        $finalUrl = $response.BaseResponse.RequestMessage.RequestUri.AbsoluteUri
        $checksumText = (Invoke-WebRequest -Uri "$finalUrl.sha256.txt" -UseBasicParsing).Content
        if ($checksumText -is [byte[]]) { $checksumText = [Text.Encoding]::ASCII.GetString($checksumText) }
        $expected = ($checksumText.Trim() -split '\s+')[0].ToLowerInvariant()
        $actual = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($expected -ne $actual) {
            throw "Checksum mismatch for the downloaded JRE (expected $expected, got $actual)."
        }

        $installRoot = Join-Path $env:LOCALAPPDATA 'Programs\Temurin-JRE'
        if (Test-Path -LiteralPath $installRoot) { Remove-Item -LiteralPath $installRoot -Recurse -Force }
        $extract = Join-Path $temp 'extract'
        Expand-Archive -LiteralPath $zipPath -DestinationPath $extract
        $jreFolder = Get-ChildItem -LiteralPath $extract -Directory | Select-Object -First 1
        New-Item -ItemType Directory -Path (Split-Path $installRoot) -Force | Out-Null
        Move-Item -LiteralPath $jreFolder.FullName -Destination $installRoot

        [Environment]::SetEnvironmentVariable('JAVA_HOME', $installRoot, 'User')
        $userPath = [Environment]::GetEnvironmentVariable('Path', 'User')
        $binPath = Join-Path $installRoot 'bin'
        if (($userPath -split ';') -notcontains $binPath) {
            [Environment]::SetEnvironmentVariable('Path', ($userPath.TrimEnd(';') + ";$binPath").TrimStart(';'), 'User')
        }
        Write-Host "Installed to $installRoot. Open a new terminal for the PATH change to apply."
    }
    finally {
        Remove-Item -LiteralPath $temp -Recurse -Force -ErrorAction SilentlyContinue
    }
}

$existing = Find-SuitableJava
if ($existing) {
    Write-Host "Java $($existing.Major) found at $($existing.Path). Nothing to do."
    exit 0
}

Write-Host "Java $MinimumMajorVersion or newer was not found."
if (-not $Force) {
    $answer = Read-Host "Install Temurin $TemurinFeatureVersion JRE now? [y/N]"
    if ($answer -notmatch '^(y|yes)$') {
        Write-Host 'Cancelled.'
        exit 2
    }
}

try {
    if (-not (Install-WithWinget)) {
        Write-Host 'winget is unavailable or failed; using direct download.'
        Install-FromZip
    }
}
catch {
    Write-Error "Java installation failed: $($_.Exception.Message)"
    exit 1
}

# winget changes the machine PATH, so refresh it for this session before verifying.
$env:Path = [Environment]::GetEnvironmentVariable('Path', 'Machine') + ';' + [Environment]::GetEnvironmentVariable('Path', 'User')
$env:JAVA_HOME = [Environment]::GetEnvironmentVariable('JAVA_HOME', 'User')
$installed = Find-SuitableJava
if (-not $installed) {
    Write-Error 'Installation finished but Java 11+ could not be verified. Open a new terminal and run "java -version".'
    exit 1
}

Write-Host "Java $($installed.Major) is available at $($installed.Path)."
exit 0
