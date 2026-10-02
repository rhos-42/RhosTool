# Lie RhosTool a votre projet Supabase (URL + cle anon).
# Usage: .\installer\link-supabase.ps1

$ErrorActionPreference = "Stop"
$projectRef = "hoczacluoagcncwohglw"
$defaultUrl = "https://$projectRef.supabase.co"

Write-Host ""
Write-Host "=== RhosTool <-> Supabase ===" -ForegroundColor Cyan
Write-Host "1) Ouvrez: https://supabase.com/dashboard/project/$projectRef"
Write-Host "2) Bouton Connect (en haut) -> onglet .env.local"
Write-Host "3) Copiez la valeur de NEXT_PUBLIC_SUPABASE_ANON_KEY (ou anon key legacy)"
Write-Host "   Ne copiez JAMAIS service_role."
Write-Host ""

$url = Read-Host "Supabase URL [$defaultUrl]"
if ([string]::IsNullOrWhiteSpace($url)) { $url = $defaultUrl }

$anon = Read-Host "Supabase anon / publishable key"
if ([string]::IsNullOrWhiteSpace($anon)) {
    Write-Host "Cle vide — annule." -ForegroundColor Red
    exit 1
}

$dir = Join-Path $env:APPDATA "RhosTool"
New-Item -ItemType Directory -Force -Path $dir | Out-Null
$path = Join-Path $dir "config.json"

$obj = [ordered]@{
    SupabaseUrl     = $url.Trim().TrimEnd('/')
    SupabaseAnonKey = $anon.Trim()
}
$json = $obj | ConvertTo-Json
Set-Content -Path $path -Value $json -Encoding UTF8

Write-Host ""
Write-Host "OK — config enregistree:" -ForegroundColor Green
Write-Host "  $path"
Write-Host "Relancez RhosTool, puis Creer un compte / Connexion."
Write-Host ""
