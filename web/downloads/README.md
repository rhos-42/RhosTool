# Artefacts de téléchargement

Placez ici `RhosTool-Portable.zip` (généré par `installer\publish-portable.ps1`).

```powershell
# Depuis la racine du dépôt
Copy-Item .\dist\RhosTool-Portable.zip .\web\downloads\RhosTool-Portable.zip -Force
```

Ou publiez le ZIP sur **GitHub Releases** et mettez à jour `releasesUrl` / `downloadUrl` dans `web/js/config.js`.
