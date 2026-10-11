<#
.SYNOPSIS
  Installe RhosTool sur le PC (Menu Démarrer, Bureau, Désinstaller).
.EXAMPLE
  .\install.ps1
  .\install.ps1 -SourceDir "..\dist\publish-single"
  .\install.ps1 -NoDesktopShortcut
#>
param(
  [string]$SourceDir = "",
  [switch]$NoDesktopShortcut
)

$ErrorActionPreference = "Stop"
$AppName = "RhosTool"
$Publisher = "RhosTool"
$Version = "1.0.0"
$InstallRoot = Join-Path $env:LOCALAPPDATA "Programs\RhosTool"
$ExeName = "RhosTool.exe"
$UninstallRegPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\RhosTool"
$ScriptDir = $PSScriptRoot

function New-Shortcut {
  param([string]$Path, [string]$Target, [string]$WorkingDir, [string]$Description, [string]$Arguments = "")
  $w = New-Object -ComObject WScript.Shell
  $s = $w.CreateShortcut($Path)
  $s.TargetPath = $Target
  $s.WorkingDirectory = $WorkingDir
  $s.Description = $Description
  if ($Arguments) { $s.Arguments = $Arguments }
  $s.Save()
}

if (-not $SourceDir) {
  foreach ($c in @(
      (Join-Path $ScriptDir "payload"),
      (Join-Path (Split-Path -Parent $ScriptDir) "dist\publish-single"),
      $ScriptDir
    )) {
    if (Test-Path (Join-Path $c $ExeName)) { $SourceDir = $c; break }
  }
}

$srcExe = Join-Path $SourceDir $ExeName
if (-not (Test-Path $srcExe)) {
  throw "RhosTool.exe introuvable. Lance d'abord .\installer\publish-installer.ps1"
}

Get-Process -Name "RhosTool" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 300

New-Item -ItemType Directory -Force -Path $InstallRoot | Out-Null
Copy-Item -Path (Join-Path $SourceDir "*") -Destination $InstallRoot -Recurse -Force
Copy-Item -Force (Join-Path $ScriptDir "uninstall.ps1") (Join-Path $InstallRoot "uninstall.ps1")

$exePath = Join-Path $InstallRoot $ExeName
$uninstallPs1 = Join-Path $InstallRoot "uninstall.ps1"

$smDir = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs\RhosTool"
New-Item -ItemType Directory -Force -Path $smDir | Out-Null
New-Shortcut -Path (Join-Path $smDir "$AppName.lnk") -Target $exePath -WorkingDir $InstallRoot -Description $AppName
New-Shortcut -Path (Join-Path $smDir "Désinstaller RhosTool.lnk") `
  -Target "powershell.exe" `
  -WorkingDir $InstallRoot `
  -Description "Désinstaller RhosTool" `
  -Arguments "-NoProfile -ExecutionPolicy Bypass -File `"$uninstallPs1`""

if (-not $NoDesktopShortcut) {
  $desk = Join-Path ([Environment]::GetFolderPath("Desktop")) "$AppName.lnk"
  New-Shortcut -Path $desk -Target $exePath -WorkingDir $InstallRoot -Description $AppName
}

New-Item -Path $UninstallRegPath -Force | Out-Null
Set-ItemProperty -Path $UninstallRegPath -Name "DisplayName" -Value $AppName
Set-ItemProperty -Path $UninstallRegPath -Name "DisplayVersion" -Value $Version
Set-ItemProperty -Path $UninstallRegPath -Name "Publisher" -Value $Publisher
Set-ItemProperty -Path $UninstallRegPath -Name "InstallLocation" -Value $InstallRoot
Set-ItemProperty -Path $UninstallRegPath -Name "DisplayIcon" -Value $exePath
Set-ItemProperty -Path $UninstallRegPath -Name "UninstallString" -Value "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"$uninstallPs1`""
Set-ItemProperty -Path $UninstallRegPath -Name "QuietUninstallString" -Value "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"$uninstallPs1`""
Set-ItemProperty -Path $UninstallRegPath -Name "NoModify" -Value 1 -Type DWord
Set-ItemProperty -Path $UninstallRegPath -Name "NoRepair" -Value 1 -Type DWord
try {
  $sizeKb = [int]((Get-ChildItem $InstallRoot -Recurse -File | Measure-Object Length -Sum).Sum / 1KB)
  Set-ItemProperty -Path $UninstallRegPath -Name "EstimatedSize" -Value $sizeKb -Type DWord
} catch {}

Write-Host ""
Write-Host "Installation OK" -ForegroundColor Green
Write-Host "  Dossier      : $InstallRoot"
Write-Host "  Menu Démarrer: RhosTool"
if (-not $NoDesktopShortcut) { Write-Host "  Bureau       : raccourci créé" }
Write-Host "  Désinstaller : Paramètres → Applications → RhosTool"
Write-Host ""
Start-Process $exePath
