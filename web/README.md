# Site RhosTool (`web/`)

Site statique (accueil, compte, téléchargements) branché sur le **même projet Supabase** que l’application Windows.

## Lancer en local

```powershell
# PowerShell — depuis la racine du dépôt
cd web
python -m http.server 8080
# puis ouvrir http://localhost:8080
```

Ou avec l’extension « Live Server » de VS Code / Cursor.

## Config Supabase

1. Copiez `js/config.example.js` → `js/config.js` si besoin.
2. Collez la clé **anon** (Dashboard → Connect → `.env.local`).
3. Ne mettez jamais `service_role` dans le site.

L’URL projet par défaut est `https://hoczacluoagcncwohglw.supabase.co` (voir `docs/AUTH-SUPABASE.md`).

## Téléchargement portable

```powershell
.\installer\publish-portable.ps1
Copy-Item .\dist\RhosTool-Portable.zip .\web\downloads\RhosTool-Portable.zip -Force
```

## Pages

| Fichier | Rôle |
|---------|------|
| `index.html` | Accueil |
| `account.html` | Login / inscription / crédits |
| `downloads.html` | ZIP RhosTool + liens GitHub |
