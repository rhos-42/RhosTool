# Site web RhosTool

Le dossier `web/` contient un site statique (accueil, compte, téléchargements) branché sur le **même projet Supabase** que l’exe.

## URL publique

**https://rhos-42.github.io/RhosTool/**

Déploiement automatique via [`.github/workflows/deploy-pages.yml`](../.github/workflows/deploy-pages.yml) (GitHub Pages / Actions).

### Prérequis GitHub Pages

Le dépôt est actuellement **privé**. Pages ne peut être activé par l’agent (droits insuffisants). Une fois :

1. soit le dépôt rendu **public**,  
2. soit un plan GitHub avec Pages sur dépôt privé,

allez dans **Settings → Pages → Build and deployment → Source = GitHub Actions**, puis relancez le workflow *Deploy GitHub Pages*.

## État

| Élément | Statut |
|---------|--------|
| App WPF (ADB / MTP / packs / auth) | Prêt |
| ZIP portable `dist/RhosTool-Portable.zip` | Prêt (via `publish-portable.ps1`) |
| Site `web/` | Prêt (statique) |
| Clé anon dans `web/js/config.js` | Renseignée (clé publishable) |
| Hébergement GitHub Pages | Workflow Actions |

## Démarrage local

```powershell
cd web
python -m http.server 8080
```

Ouvrir http://localhost:8080

## Auth

Voir [AUTH-SUPABASE.md](AUTH-SUPABASE.md).  
Dans Supabase → Authentication → URL Configuration, ajoutez `https://rhos-42.github.io/RhosTool/**` aux Redirect URLs si besoin.

## Publication du ZIP

```powershell
.\installer\publish-portable.ps1
# le script copie aussi vers web/downloads/ si présent
```

Le site sert `web/downloads/RhosTool-Portable.zip`. Pour alléger le dépôt, publiez le ZIP en **GitHub Release** et mettez à jour `downloadUrl` dans `web/js/config.js`.

## Lien depuis l’app

Bouton **CONFIG** → Annuler ouvre https://rhos-42.github.io/RhosTool/
