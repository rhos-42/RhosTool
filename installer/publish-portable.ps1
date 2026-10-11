<#
.SYNOPSIS
  Compile RhosTool en un seul RhosTool.exe (self-contained) + ZIP à 1 fichier.
#>
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$srcPt = Join-Path $root "tools\platform-tools"
$resDir = Join-Path $root "src\RhosTool.App\Resources"
$bundle = Join-Path $resDir "platform-tools.zip"
$publishDir = Join-Path $root "dist\publish-single"
$zipOut = Join-Path $root "dist\RhosTool-Portable.zip"
$proj = Join-Path $root "src\RhosTool.App\RhosTool.App.csproj"

New-Item -ItemType Directory -Force -Path $resDir | Out-Null
$stage = Join-Path $env:TEMP "rhostool-pt-bundle"
Remove-Item -Recurse -Force $stage -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $stage | Out-Null
foreach ($f in @("adb.exe","AdbWinApi.dll","AdbWinUsbApi.dll","fastboot.exe","libwinpthread-1.dll","NOTICE.txt","source.properties")) {
  Copy-Item (Join-Path $srcPt $f) $stage -Force
}
Remove-Item $bundle -Force -ErrorAction SilentlyContinue
Compress-Archive -Path (Join-Path $stage "*") -DestinationPath $bundle -Force

Get-Process -Name "RhosTool" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 500
Remove-Item -Recurse -Force $publishDir -ErrorAction SilentlyContinue
dotnet publish $proj -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true `
  -p:UseAppHost=true `
  -p:DebugType=None `
  -p:DebugSymbols=false `
  -o $publishDir

$exe = Join-Path $publishDir "RhosTool.exe"
if (-not (Test-Path $exe)) { throw "RhosTool.exe manquant" }

Add-Type -AssemblyName System.IO.Compression.FileSystem
if (Test-Path $zipOut) { Remove-Item $zipOut -Force }
$z = [IO.Compression.ZipFile]::Open($zipOut, [IO.Compression.ZipArchiveMode]::Create)
[IO.Compression.ZipFileExtensions]::CreateEntryFromFile($z, $exe, "RhosTool.exe") | Out-Null
$z.Dispose()

Write-Host "OK: $zipOut" -ForegroundColor Green
Write-Host "EXE: $exe ($([math]::Round((Get-Item $exe).Length/1MB,2)) MB)"
