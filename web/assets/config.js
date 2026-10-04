/**
 * Config site RhosTool — mêmes valeurs Supabase que %AppData%\RhosTool\config.json
 * (URL + clé anon). Ne jamais y mettre la clé service_role.
 *
 * Remplacez supabaseAnonKey par la clé anon du projet
 * (Connect → .env.local → NEXT_PUBLIC_SUPABASE_ANON_KEY).
 */
window.RHOSToolConfig = {
  supabaseUrl: "https://hoczacluoagcncwohglw.supabase.co",
  supabaseAnonKey: "eyJhbGciOi...REPLACE_WITH_ANON_KEY",
  downloadUrl:
    "https://github.com/rhos-42/RhosTool/releases/latest/download/RhosTool-Portable.zip",
  downloadFallbackUrl: "public/downloads/RhosTool-Portable.zip",
  websiteUrl: "https://rhos-42.github.io/RhosTool/",
  unlockToolSite: "https://file.unlocktool.net/",
  authGuideUrl: "https://github.com/rhos-42/RhosTool/blob/main/docs/AUTH-SUPABASE.md",
};
