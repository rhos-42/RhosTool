# Compile RhosTool + pack installateur
# Sorties:
#   dist\publish-single\RhosTool.exe
#   dist\RhosTool-Installer.zip   (toujours)
#   dist\RhosTool-Setup.exe       (si Inno Setup / ISCC)
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$portableScript = Join-Path $PSScriptRoot "publish-portable.ps1"
$iss = Join-Path $PSScriptRoot "RhosTool.iss"
$publishDir = Join-Path $root "dist\publish-single"
$zipOut = Join-Path $root "dist\RhosTool-Installer.zip"
$setupOut = Join-Path $root "dist\RhosTool-Setup.exe"

Write-Host "==> Publish portable (single-file)..." -ForegroundColor Cyan
& $portableScript

$exe = Join-Path $publishDir "RhosTool.exe"
if (-not (Test-Path $exe)) { throw "Publish failed - RhosTool.exe missing" }

$stage = Join-Path $root "dist\installer-stage"
Remove-Item -Recurse -Force $stage -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path (Join-Path $stage "payload") | Out-Null
Copy-Item $exe (Join-Path $stage "payload\RhosTool.exe") -Force
Copy-Item (Join-Path $PSScriptRoot "install.ps1") (Join-Path $stage "install.ps1") -Force
Copy-Item (Join-Path $PSScriptRoot "uninstall.ps1") (Join-Path $stage "uninstall.ps1") -Force

$cmdLines = @(
  '@echo off',
  'title Installation RhosTool',
  'cd /d "%~dp0"',
  'echo.',
  'echo  Installation de RhosTool...',
  'echo.',
  'powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0install.ps1" -SourceDir "%~dp0payload"',
  'if errorlevel 1 (',
  '  echo.',
  '  echo Echec installation. Appuyez sur une touche.',
  '  pause >nul',
  ')'
)
Set-Content -Path (Join-Path $stage "Installer.cmd") -Value $cmdLines -Encoding ASCII

if (Test-Path $zipOut) { Remove-Item $zipOut -Force }
Compress-Archive -Path (Join-Path $stage "*") -DestinationPath $zipOut -Force
Write-Host "OK ZIP installer: $zipOut" -ForegroundColor Green

$iscc = $null
$candidates = @(
  "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
  "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
  "${env:ProgramFiles(x86)}\Inno Setup 5\ISCC.exe",
  "$env:LocalAppData\Programs\Inno Setup 6\ISCC.exe"
)
foreach ($c in $candidates) {
  if (Test-Path $c) { $iscc = $c; break }
}
if (-not $iscc) {
  $cmdIscc = Get-Command ISCC.exe -ErrorAction SilentlyContinue
  if ($cmdIscc) { $iscc = $cmdIscc.Source }
}

if ($iscc) {
  Write-Host "==> Inno Setup ($iscc)..." -ForegroundColor Cyan
  & $iscc $iss
  if ($LASTEXITCODE -ne 0) { throw "ISCC failed ($LASTEXITCODE)" }
  if (Test-Path $setupOut) {
    $mb = [math]::Round((Get-Item $setupOut).Length / 1MB, 2)
    Write-Host "OK Setup.exe: $setupOut ($mb MB)" -ForegroundColor Green
  }
} else {
  Write-Host "Inno Setup not found - no RhosTool-Setup.exe" -ForegroundColor Yellow
  Write-Host "  Install https://jrsoftware.org/isinfo.php then re-run this script." -ForegroundColor Yellow
  Write-Host "  Meanwhile: unzip RhosTool-Installer.zip and run Installer.cmd" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Portable EXE : $exe"
Write-Host "  Install ZIP  : $zipOut"
if (Test-Path $setupOut) { Write-Host "  Setup.exe    : $setupOut" }
Write-Host ""
Write-Host "Install now on this PC:" -ForegroundColor Cyan
Write-Host "  .\installer\install.ps1"
