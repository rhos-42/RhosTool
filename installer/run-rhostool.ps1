<#
.SYNOPSIS
  Compile et lance RhosTool (Debug).
#>
[CmdletBinding()]
param(
    [string]$RepoRoot = "",
    [switch]$SkipBuild
)

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $scriptDir = if (-not [string]::IsNullOrWhiteSpace($PSScriptRoot)) {
        $PSScriptRoot
    } else {
        Split-Path -Parent $MyInvocation.MyCommand.Path
    }
    $RepoRoot = (Resolve-Path (Join-Path $scriptDir "..")).Path
}

$ErrorActionPreference = "Stop"
$sln = Join-Path $RepoRoot "RhosTool.sln"
$adb = Join-Path $RepoRoot "tools\platform-tools\adb.exe"

$dotnet = $null
$cmd = Get-Command dotnet -ErrorAction SilentlyContinue
$candidates = @()
if ($cmd) { $candidates += $cmd.Source }
$candidates += @(
    "$env:LOCALAPPDATA\dotnet-sdk-portable\dotnet.exe",
    "$env:ProgramFiles\dotnet\dotnet.exe"
)
foreach ($candidate in $candidates) {
    if (-not [string]::IsNullOrWhiteSpace($candidate) -and (Test-Path $candidate)) {
        $dotnet = $candidate
        break
    }
}

if (-not $dotnet) {
    throw "SDK .NET introuvable. Installez .NET 8 SDK : https://dotnet.microsoft.com/download/dotnet/8.0"
}

if (-not (Test-Path $adb)) {
    Write-Host "ADB absent — lancement setup-platform-tools.ps1…" -ForegroundColor Yellow
    & (Join-Path $PSScriptRoot "setup-platform-tools.ps1") -RepoRoot $RepoRoot
}

if (-not $SkipBuild) {
    Write-Host "Build…" -ForegroundColor Cyan
    & $dotnet build $sln -c Debug
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

$exe = Join-Path $RepoRoot "src\RhosTool.App\bin\Debug\net8.0-windows\RhosTool.exe"
if (-not (Test-Path $exe)) {
    throw "Exécutable introuvable : $exe"
}

# Copier platform-tools à côté de l'exe si besoin
$outPt = Join-Path (Split-Path $exe) "platform-tools"
if (-not (Test-Path (Join-Path $outPt "adb.exe"))) {
    New-Item -ItemType Directory -Force -Path $outPt | Out-Null
    Copy-Item -Path (Join-Path $RepoRoot "tools\platform-tools\*") -Destination $outPt -Recurse -Force
}

Write-Host "Lancement RhosTool…" -ForegroundColor Green
Start-Process $exe
