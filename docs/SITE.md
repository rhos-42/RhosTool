# Site web RhosTool

Le dossier `web/` contient un site statique (accueil, compte, téléchargements) branché sur le **même projet Supabase** que l’exe.

## État

| Élément | Statut |
|---------|--------|
| App WPF (ADB / MTP / packs / auth) | Prêt |
| ZIP portable `dist/RhosTool-Portable.zip` | Prêt (via `publish-portable.ps1`) |
| Site `web/` | Prêt (statique) |
| Clé anon dans `web/js/config.js` | À renseigner |
| Hébergement public (Pages / domaine) | À faire |

## Démarrage local

```powershell
cd web
python -m http.server 8080
```

Ouvrir http://localhost:8080

## Auth

Voir [AUTH-SUPABASE.md](AUTH-SUPABASE.md).  
Collez la clé **anon** dans `web/js/config.js` (modèle : `web/js/config.example.js`).

## Publication du ZIP

```powershell
.\installer\publish-portable.ps1
# le script copie aussi vers web/downloads/ si présent
```

Ou uploadez le ZIP sur GitHub Releases et mettez à jour `downloadUrl` / `releasesUrl`.

## Lien depuis l’app

Bouton **CONFIG** → Annuler ouvre le dossier `web/` sur GitHub.
