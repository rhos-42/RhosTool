# Build RhosTool C++ (Qt 6 / MinGW via MSYS2)
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$src = Join-Path $root "cpp"
$build = Join-Path $src "build"
$ucrt = "C:\msys64\ucrt64"
if (-not (Test-Path "$ucrt\bin\cmake.exe")) {
  throw "MSYS2/Qt manquant. Installe MSYS2 puis: pacman -S mingw-w64-ucrt-x86_64-toolchain mingw-w64-ucrt-x86_64-cmake mingw-w64-ucrt-x86_64-ninja mingw-w64-ucrt-x86_64-qt6-base"
}

$env:PATH = "$ucrt\bin;C:\msys64\usr\bin;" + $env:PATH
cmake -S $src -B $build -G Ninja -DCMAKE_BUILD_TYPE=Release "-DCMAKE_PREFIX_PATH=$ucrt"
if ($LASTEXITCODE -ne 0) { throw "cmake configure failed" }
cmake --build $build
if ($LASTEXITCODE -ne 0) { throw "cmake build failed" }

& "$ucrt\bin\windeployqt6.exe" --release (Join-Path $build "RhosTool.exe")

# Runtime MinGW (windeployqt ne les copie pas)
$mingwDlls = @(
  "libgcc_s_seh-1.dll","libstdc++-6.dll","libwinpthread-1.dll",
  "libicuuc78.dll","libicuin78.dll","libicudt78.dll","libpcre2-16-0.dll",
  "libzstd.dll","zlib1.dll","libharfbuzz-0.dll","libfreetype-6.dll",
  "libpng16-16.dll","libbrotlidec.dll","libbrotlicommon.dll",
  "libglib-2.0-0.dll","libintl-8.dll","libiconv-2.dll","libpcre2-8-0.dll",
  "libgraphite2.dll","libbz2-1.dll","libdouble-conversion.dll","libmd4c.dll"
)
foreach ($d in $mingwDlls) {
  $src = Join-Path "$ucrt\bin" $d
  if (Test-Path $src) { Copy-Item $src $build -Force }
}

$dataSrc = Join-Path $root "cpp\data"
if (Test-Path $dataSrc) {
  $dataDst = Join-Path $build "data"
  New-Item -ItemType Directory -Force -Path $dataDst | Out-Null
  Copy-Item (Join-Path $dataSrc "*") $dataDst -Force
}

$ptSrc = Join-Path $root "tools\platform-tools"
$ptDst = Join-Path $build "platform-tools"
if (Test-Path (Join-Path $ptSrc "adb.exe")) {
  New-Item -ItemType Directory -Force -Path $ptDst | Out-Null
  Copy-Item (Join-Path $ptSrc "*") $ptDst -Force
}

$dist = Join-Path $root "dist\cpp"
Remove-Item -Recurse -Force $dist -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $dist | Out-Null
robocopy $build $dist /E /NFL /NDL /NJH /NJS /NP | Out-Null

Write-Host "OK: $(Join-Path $build 'RhosTool.exe')" -ForegroundColor Green
Write-Host "OK: $(Join-Path $dist 'RhosTool.exe')" -ForegroundColor Green
