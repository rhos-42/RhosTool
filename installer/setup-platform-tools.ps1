<#
.SYNOPSIS
  Télécharge Android platform-tools (adb) officiels dans tools/platform-tools.
#>
[CmdletBinding()]
param(
    [string]$RepoRoot = ""
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
$toolsDir = Join-Path $RepoRoot "tools"
$zipPath = Join-Path $toolsDir "platform-tools.zip"
$destDir = Join-Path $toolsDir "platform-tools"
$url = "https://dl.google.com/android/repository/platform-tools-latest-windows.zip"

New-Item -ItemType Directory -Force -Path $toolsDir | Out-Null

Write-Host "Téléchargement platform-tools…" -ForegroundColor Cyan
Invoke-WebRequest -Uri $url -OutFile $zipPath -UseBasicParsing

if (Test-Path $destDir) {
    Remove-Item -Recurse -Force $destDir
}

Write-Host "Extraction…" -ForegroundColor Cyan
Expand-Archive -Path $zipPath -DestinationPath $toolsDir -Force
Remove-Item -Force $zipPath

$adb = Join-Path $destDir "adb.exe"
if (-not (Test-Path $adb)) {
    throw "adb.exe introuvable après extraction."
}

Write-Host "OK : $adb" -ForegroundColor Green
Write-Host "Vous pouvez lancer l'app avec : installer\run-rhostool.ps1"
