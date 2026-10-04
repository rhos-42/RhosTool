# Authentification Supabase (gratuit)

Guide rapide pour brancher RhosTool à un projet Supabase.

## Site + app = même projet

Le site statique [`web/`](../web/) et l’application Windows utilisent **le même projet Supabase** (URL + clé **anon**).

| Client | Config |
|--------|--------|
| Exe RhosTool | `%AppData%\RhosTool\config.json` (via `installer\link-supabase.ps1`) |
| Site `web/` | [`web/assets/config.js`](../web/assets/config.js) |

Un compte créé sur `web/account.html` fonctionne dans l’exe, et inversement. Les crédits sont lus dans la table `profiles`.

Téléchargement du binaire : page [`web/downloads.html`](../web/downloads.html) → `RhosTool-Portable.zip` (GitHub Releases, pas de catalogue UnlockTool).

Dans l’exe : bouton **SITE** ou **CONFIG** → *Ouvrir le site* (`https://rhos-42.github.io/RhosTool/`).

## 1. Créer le projet

1. Allez sur [https://supabase.com](https://supabase.com) et créez un compte (gratuit).
2. **New project** → choisissez un nom, un mot de passe DB, une région.
3. Attendez que le projet soit prêt.

## 2. Auth email

1. **Authentication** → **Providers** → **Email** activé.
2. Pour démarrer sans friction : **Authentication** → **Providers** → Email → désactivez *Confirm email* (vous pourrez le réactiver plus tard).

## 3. Table `profiles` + trigger

Dans **SQL Editor**, exécutez :

```sql
create table if not exists public.profiles (
  id uuid primary key references auth.users(id) on delete cascade,
  username text unique,
  credits numeric(12,2) not null default 0,
  updated_at timestamptz default now()
);

alter table public.profiles enable row level security;

create policy "read own profile"
  on public.profiles for select
  using (auth.uid() = id);

create policy "update own profile"
  on public.profiles for update
  using (auth.uid() = id);

create or replace function public.handle_new_user()
returns trigger
language plpgsql
security definer set search_path = public
as $$
begin
  insert into public.profiles (id, username, credits)
  values (
    new.id,
    coalesce(new.raw_user_meta_data->>'username', split_part(new.email, '@', 1)),
    0
  );
  return new;
end;
$$;

drop trigger if exists on_auth_user_created on auth.users;
create trigger on_auth_user_created
  after insert on auth.users
  for each row execute procedure public.handle_new_user();
```

## 4. Clés API

1. **Project Settings** → **API**.
2. Copiez :
   - **Project URL** (ex. `https://xxxx.supabase.co`)
   - **anon public** key
3. **Ne mettez jamais** la clé `service_role` dans RhosTool.

## 5. Configurer RhosTool (rapide)

Projet déjà créé : `hoczacluoagcncwohglw`  
URL : `https://hoczacluoagcncwohglw.supabase.co`

Dans PowerShell, à la racine du dépôt :

```powershell
.\installer\link-supabase.ps1
```

Ou manuellement : éditez `%AppData%\RhosTool\config.json` avec l’URL + la clé **anon** (bouton **Connect** → `.env.local` sur le dashboard Supabase).

**Ne mettez jamais** la clé `service_role` dans RhosTool.

Relancez RhosTool → fenêtre de connexion → **Créer un compte** ou **Connexion**.

## 6. Crédits

Dans **Table Editor** → `profiles`, modifiez la colonne `credits` pour un utilisateur.

Le footer affiche `Credits: X.XX` après connexion.

## Dépannage

| Message | Cause |
|--------|--------|
| Config Supabase manquante | URL / anon key vides dans `config.json` |
| Invalid login credentials | Mauvais email/mot de passe |
| Email not confirmed | Réactivez Confirm email ou confirmez le mail |
| profiles vide / credits 0 | Vérifiez le trigger `on_auth_user_created` |
