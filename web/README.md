# Site RhosTool

```powershell
cd web
python -m http.server 8080
```

→ http://localhost:8080

## Structure

| Fichier | Rôle |
|---------|------|
| `index.html` | Accueil + raccourcis catégories |
| `downloads.html` | App (GitHub Release) + catalogue ressources |
| `account.html` | Login / crédits Supabase |
| `js/config.js` | URL app + `resourceUrls` |
| `js/resources.js` | Catalogue ordonné (noms / groupes) |
| `js/downloads.js` | Rendu page téléchargements |

## Brancher un fichier

Dans `js/config.js` :

```js
resourceUrls: {
  "drv-apple-usb": "https://www.mediafire.com/file/XXXX/fichier.zip/file"
}
```

L’app portable est déjà en ligne :  
https://github.com/rhos-42/RhosTool/releases/download/v1.0.0/RhosTool.zip
