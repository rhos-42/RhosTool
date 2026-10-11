<#
.SYNOPSIS
  Désinstalle RhosTool (raccourcis + fichiers + entrée Registre).
#>
$ErrorActionPreference = "Stop"
$AppName = "RhosTool"
$InstallRoot = Join-Path $env:LOCALAPPDATA "Programs\RhosTool"
$UninstallRegPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\RhosTool"

Get-Process -Name "RhosTool" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 400

if (Test-Path $InstallRoot) {
  Remove-Item -Recurse -Force $InstallRoot
}

$sm = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs\RhosTool"
if (Test-Path $sm) { Remove-Item -Recurse -Force $sm }

$desk = Join-Path ([Environment]::GetFolderPath("Desktop")) "$AppName.lnk"
if (Test-Path $desk) { Remove-Item -Force $desk }

if (Test-Path $UninstallRegPath) { Remove-Item -Recurse -Force $UninstallRegPath }

Write-Host "RhosTool désinstallé." -ForegroundColor Green
