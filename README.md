# RhosTool

Application Windows (WPF / .NET 8) style atelier pour diagnostics Android & iOS : détection USB, lecture d’infos appareil, redémarrage ADB, modules par marque.

## Périmètre

Inclus :
- Interface type outil atelier (sidebar marques, opérations, journal)
- Détection Android via `adb`
- Infos iOS via `ideviceinfo` si présent
- Stubs pour opérations partenaires licenciées (réseau / FRP / iCloud)

Non inclus (volontairement) :
- Contournement FRP, Activation Lock, crack de codes

## Authentification en ligne

RhosTool utilise **Supabase** (gratuit) pour login + crédits.

Voir le guide : [docs/AUTH-SUPABASE.md](docs/AUTH-SUPABASE.md)

Config locale : `%AppData%\RhosTool\config.json`

## Site web

Dossier [`web/`](web/) : accueil, compte (même Supabase que l’exe) et téléchargement de **RhosTool-Portable.zip**.

```bash
python3 -m http.server 8080
# → http://localhost:8080/web/
```

Collez la clé anon dans `web/assets/config.js` (identiques à `config.json`).

## Prérequis

- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (ou un SDK portable dans `%LOCALAPPDATA%\dotnet-sdk-portable`)
- Câble USB + débogage USB activé (Android)

## Setup rapide

Dans PowerShell, depuis la racine du dépôt :

```powershell
# 1) Télécharger adb (platform-tools Google)
.\installer\setup-platform-tools.ps1

# 2) Compiler et lancer
.\installer\run-rhostool.ps1
```

Ou manuellement :

```powershell
dotnet build .\RhosTool.sln -c Release
dotnet run --project .\src\RhosTool.App\RhosTool.App.csproj -c Release
```

## Structure

```
RhosTool.sln
src/RhosTool.App/     # UI WPF
src/RhosTool.Core/    # ADB, iOS, modules, logs
web/                  # site (compte Supabase + téléchargement)
tools/platform-tools/ # adb (généré par le script)
installer/            # scripts setup / run
docs/                 # AUTH-SUPABASE, etc.
```

## iOS (optionnel)

Placez une build Windows de `libimobiledevice` (avec `ideviceinfo.exe`) dans `tools/libimobiledevice/`, ou ajoutez-la au `PATH`.

## Licence d’usage

Utilisez uniquement sur des appareils dont vous êtes propriétaire ou pour lesquels vous avez une autorisation explicite, via des canaux légitimes.
