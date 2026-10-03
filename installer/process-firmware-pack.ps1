<#
.SYNOPSIS
  Classe, extrait et valide un paquet firmware mobile (MTK / Odin / Qualcomm / OTA ZIP).
  Écrit un manifest.json sous %AppData%\RhosTool\packages\{brand}\{model}\{packId}\.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$InputPath,

    [string]$Brand = "unknown",
    [string]$ModelCode = "generic",
    [string]$OutRoot = "",
    [switch]$Extract,
    [switch]$ValidateOnly
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($OutRoot)) {
    $OutRoot = Join-Path $env:APPDATA "RhosTool\packages"
}

function Get-Sha256([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return $null }
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Get-RelativePath([string]$Root, [string]$Full) {
    $rootFull = [IO.Path]::GetFullPath($Root).TrimEnd('\', '/')
    $fullPath = [IO.Path]::GetFullPath($Full)
    if ($fullPath.StartsWith($rootFull, [StringComparison]::OrdinalIgnoreCase)) {
        return $fullPath.Substring($rootFull.Length).TrimStart('\', '/').Replace('\', '/')
    }
    return [IO.Path]::GetFileName($Full)
}

function Find-Files([string]$Root, [string]$Pattern) {
    if (-not (Test-Path -LiteralPath $Root)) { return @() }
    return @(Get-ChildItem -LiteralPath $Root -Recurse -File -Filter $Pattern -ErrorAction SilentlyContinue)
}

function Detect-Format([string]$Path, [string]$ScanRoot) {
    $name = [IO.Path]::GetFileName($Path).ToLowerInvariant()
    if ($name.EndsWith(".tar.md5") -or $name.EndsWith(".tar")) { return "odin_tar" }

    $scatter = @(Find-Files $ScanRoot "*scatter*.txt")
    if ($scatter.Count -gt 0) { return "mtk_scatter" }

    $fh = @()
    $fh += Find-Files $ScanRoot "prog_firehose*"
    $fh += Find-Files $ScanRoot "*firehose*"
    $fh += Find-Files $ScanRoot "rawprogram*.xml"
    if ($fh.Count -gt 0) { return "qualcomm" }

    if ($name.EndsWith(".zip") -or (Test-Path -LiteralPath $Path -PathType Container)) {
        $meta = Find-Files $ScanRoot "META-INF"
        $payload = Find-Files $ScanRoot "payload.bin"
        if ($meta.Count -gt 0 -or $payload.Count -gt 0) { return "ota_zip" }
        # ZIP without META still often OTA-like
        if ($name.EndsWith(".zip")) { return "ota_zip" }
    }

    return "unknown"
}

function Parse-ScatterPartitions([string]$ScatterPath, [string]$BaseDir) {
    $parts = New-Object System.Collections.Generic.List[object]
    $currentName = $null
    $currentFile = $null
    foreach ($line in Get-Content -LiteralPath $ScatterPath -ErrorAction SilentlyContinue) {
        if ($line -match '^\s*partition_name\s*:\s*(.+)\s*$') {
            if ($currentName -and $currentFile -and $currentFile -ne "NONE") {
                $full = Join-Path $BaseDir $currentFile
                $size = if (Test-Path -LiteralPath $full) { (Get-Item -LiteralPath $full).Length } else { 0 }
                $parts.Add([pscustomobject]@{
                    name   = $currentName.Trim()
                    file   = $currentFile.Trim()
                    size   = [int64]$size
                    sha256 = if ($size -gt 0) { Get-Sha256 $full } else { $null }
                })
            }
            $currentName = $Matches[1]
            $currentFile = $null
        }
        elseif ($line -match '^\s*file_name\s*:\s*(.+)\s*$') {
            $currentFile = $Matches[1]
        }
    }
    if ($currentName -and $currentFile -and $currentFile -ne "NONE") {
        $full = Join-Path $BaseDir $currentFile
        $size = if (Test-Path -LiteralPath $full) { (Get-Item -LiteralPath $full).Length } else { 0 }
        $parts.Add([pscustomobject]@{
            name   = $currentName.Trim()
            file   = $currentFile.Trim()
            size   = [int64]$size
            sha256 = if ($size -gt 0) { Get-Sha256 $full } else { $null }
        })
    }
    return $parts
}

function Inventory-Files([string]$Root) {
    $parts = New-Object System.Collections.Generic.List[object]
    if (-not (Test-Path -LiteralPath $Root)) { return $parts }
    foreach ($f in Get-ChildItem -LiteralPath $Root -Recurse -File -ErrorAction SilentlyContinue) {
        # Skip hashing huge trees beyond 64 MB for speed in inventory mode
        $hash = $null
        if ($f.Length -le 64MB) { $hash = Get-Sha256 $f.FullName }
        $parts.Add([pscustomobject]@{
            name   = [IO.Path]::GetFileNameWithoutExtension($f.Name)
            file   = Get-RelativePath $Root $f.FullName
            size   = [int64]$f.Length
            sha256 = $hash
        })
    }
    return $parts
}

function Validate-Pack([string]$Format, [string]$ScanRoot, $Partitions) {
    $missing = New-Object System.Collections.Generic.List[string]
    $ok = $false

    switch ($Format) {
        "mtk_scatter" {
            $scatter = Find-Files $ScanRoot "*scatter*.txt"
            if ($scatter.Count -eq 0) { $missing.Add("scatter.txt") }
            $withFile = @($Partitions | Where-Object { $_.file -and $_.file -ne "NONE" -and $_.size -gt 0 })
            if ($withFile.Count -eq 0) { $missing.Add("referenced_image") }
            $ok = ($scatter.Count -gt 0 -and $withFile.Count -gt 0)
        }
        "odin_tar" {
            $tar = @(Get-ChildItem -LiteralPath $ScanRoot -Recurse -File -ErrorAction SilentlyContinue |
                Where-Object { $_.Name -match '\.tar(\.md5)?$' })
            if ($tar.Count -eq 0) { $missing.Add("tar_archive") }
            $ok = $tar.Count -gt 0
            # Optional MD5 sidecar check
            foreach ($t in $tar) {
                if ($t.Name -like "*.tar.md5") {
                    $first = Get-Content -LiteralPath $t.FullName -TotalCount 1 -ErrorAction SilentlyContinue
                    if (-not $first) { $missing.Add("tar_md5_header"); $ok = $false }
                }
            }
        }
        "qualcomm" {
            $fh = @()
            $fh += Find-Files $ScanRoot "prog_firehose*"
            $fh += Find-Files $ScanRoot "*firehose*"
            if ($fh.Count -eq 0) { $missing.Add("firehose_programmer") }
            $ok = $fh.Count -gt 0
        }
        "ota_zip" {
            $zipOk = (Find-Files $ScanRoot "META-INF").Count -gt 0 -or
                     (Find-Files $ScanRoot "payload.bin").Count -gt 0 -or
                     (Find-Files $ScanRoot "*.img").Count -gt 0
            if (-not $zipOk) { $missing.Add("ota_artifact") }
            $ok = $zipOk
        }
        default {
            $ok = ($Partitions.Count -gt 0)
            if (-not $ok) { $missing.Add("any_file") }
        }
    }

    return @{ requiredOk = [bool]$ok; missing = @($missing) }
}

# --- main ---
if (-not (Test-Path -LiteralPath $InputPath)) {
    throw "Chemin introuvable: $InputPath"
}

$Brand = ($Brand -replace '[^\w\-]+', '_').ToLowerInvariant()
if ([string]::IsNullOrWhiteSpace($Brand)) { $Brand = "unknown" }
$ModelCode = ($ModelCode -replace '[^\w\-]+', '_')
if ([string]::IsNullOrWhiteSpace($ModelCode)) { $ModelCode = "generic" }

$packId = (Get-Date -Format "yyyyMMdd-HHmmss") + "-" + ([Guid]::NewGuid().ToString("N").Substring(0, 8))
$packDir = Join-Path $OutRoot (Join-Path $Brand (Join-Path $ModelCode $packId))
$sourceDir = Join-Path $packDir "source"
$extractedDir = Join-Path $packDir "extracted"

if ($ValidateOnly) {
    $scanRoot = if (Test-Path -LiteralPath $InputPath -PathType Container) { $InputPath } else { Split-Path -Parent $InputPath }
    $format = Detect-Format $InputPath $scanRoot
    $scatterFiles = Find-Files $scanRoot "*scatter*.txt"
    if ($format -eq "mtk_scatter" -and $scatterFiles.Count -gt 0) {
        $partitions = Parse-ScatterPartitions $scatterFiles[0].FullName $scanRoot
    }
    else {
        $partitions = Inventory-Files $scanRoot
    }
    $validation = Validate-Pack $format $scanRoot $partitions
    $manifest = [ordered]@{
        packId       = "validate-only"
        brand        = $Brand
        modelHint    = $ModelCode
        chipsetHint  = ""
        format       = $format
        sourcePath   = [IO.Path]::GetFullPath($InputPath)
        packDir      = $null
        partitions   = @($partitions)
        requiredOk   = $validation.requiredOk
        missing      = @($validation.missing)
        createdUtc   = [DateTime]::UtcNow.ToString("o")
    }
    $manifest | ConvertTo-Json -Depth 6
    if (-not $validation.requiredOk) { exit 2 }
    exit 0
}

New-Item -ItemType Directory -Force -Path $sourceDir | Out-Null
New-Item -ItemType Directory -Force -Path $extractedDir | Out-Null

$isDir = Test-Path -LiteralPath $InputPath -PathType Container
if ($isDir) {
    Copy-Item -LiteralPath $InputPath -Destination (Join-Path $sourceDir ([IO.Path]::GetFileName($InputPath))) -Recurse -Force
}
else {
    Copy-Item -LiteralPath $InputPath -Destination $sourceDir -Force
}

$scanRoot = $sourceDir
$inputName = [IO.Path]::GetFileName($InputPath).ToLowerInvariant()

if ($Extract) {
    if (-not $isDir -and $inputName.EndsWith(".zip")) {
        $zipFile = Join-Path $sourceDir ([IO.Path]::GetFileName($InputPath))
        Write-Host "Extraction ZIP…" -ForegroundColor Cyan
        Expand-Archive -LiteralPath $zipFile -DestinationPath $extractedDir -Force
        $scanRoot = $extractedDir
    }
    elseif ($isDir) {
        # Work on copied tree
        $scanRoot = Join-Path $sourceDir ([IO.Path]::GetFileName($InputPath))
    }
}

$format = Detect-Format $InputPath $scanRoot
Write-Host "Format détecté: $format" -ForegroundColor Green

$scatterFiles = Find-Files $scanRoot "*scatter*.txt"
if ($format -eq "mtk_scatter" -and $scatterFiles.Count -gt 0) {
    $base = Split-Path -Parent $scatterFiles[0].FullName
    $partitions = Parse-ScatterPartitions $scatterFiles[0].FullName $base
}
else {
    $partitions = Inventory-Files $scanRoot
}

$validation = Validate-Pack $format $scanRoot $partitions

$chipHint = ""
if ($format -eq "mtk_scatter") { $chipHint = "MediaTek" }
elseif ($format -eq "odin_tar") { $chipHint = "Exynos/Snapdragon" }
elseif ($format -eq "qualcomm") { $chipHint = "Snapdragon" }

$manifest = [ordered]@{
    packId      = $packId
    brand       = $Brand
    modelHint   = $ModelCode
    chipsetHint = $chipHint
    format      = $format
    sourcePath  = [IO.Path]::GetFullPath($InputPath)
    packDir     = $packDir
    partitions  = @($partitions)
    requiredOk  = $validation.requiredOk
    missing     = @($validation.missing)
    createdUtc  = [DateTime]::UtcNow.ToString("o")
}

$manifestPath = Join-Path $packDir "manifest.json"
$manifest | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $manifestPath -Encoding UTF8

Write-Host "Pack: $packDir"
Write-Host "Manifest: $manifestPath"
Write-Host "Partitions/fichiers: $($partitions.Count)"
Write-Host "requiredOk: $($validation.requiredOk)"
if ($validation.missing.Count -gt 0) {
    Write-Host ("missing: " + ($validation.missing -join ", ")) -ForegroundColor Yellow
}

Write-Output $manifestPath
if (-not $validation.requiredOk) { exit 2 }
