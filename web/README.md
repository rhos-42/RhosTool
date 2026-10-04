# Site RhosTool (`web/`)

Site statique (HTML/CSS/JS) branché sur le **même projet Supabase** que l’application WPF.

## Pages

| Fichier | Rôle |
|---------|------|
| `index.html` | Accueil + CTA téléchargement / compte |
| `account.html` | Login, register, crédits (`profiles`) |
| `downloads.html` | Lien `RhosTool-Portable.zip` (GitHub Releases ou `public/downloads/`) |

## Config

Éditez [`assets/config.js`](assets/config.js) avec l’URL + clé **anon** (identiques à `%AppData%\RhosTool\config.json`).

Ne jamais y mettre `service_role`.

## Lancer en local

Depuis la racine du dépôt (pour que le lien vers `docs/AUTH-SUPABASE.md` fonctionne) :

```bash
python3 -m http.server 8080
```

Puis ouvrez `http://localhost:8080/web/`.

## GitHub Pages

Publiez le dossier `web/` (Settings → Pages → Deploy from branch → `/web`), ou copiez le contenu de `web/` à la racine d’une branche `gh-pages`.
