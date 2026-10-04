(() => {
  const cfg = window.RHOSToolConfig || {};
  const AUTH_STORAGE_KEY = "rhostool.supabase.auth";
  const PROFILE_CACHE_KEY = "rhostool.profile.cache";

  function $(id) {
    return document.getElementById(id);
  }

  function isConfigured() {
    const url = (cfg.supabaseUrl || "").trim();
    const key = (cfg.supabaseAnonKey || "").trim();
    return (
      url.length > 0 &&
      key.length > 0 &&
      !url.includes("VOTRE_REF") &&
      !key.startsWith("eyJhbGciOi...REPLACE")
    );
  }

  function setStatus(el, message, kind) {
    if (!el) return;
    el.textContent = message || "";
    el.classList.remove("ok", "err");
    if (kind) el.classList.add(kind);
  }

  function loadProfileCache() {
    try {
      const raw = localStorage.getItem(PROFILE_CACHE_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }

  function saveProfileCache(profile) {
    if (!profile) {
      localStorage.removeItem(PROFILE_CACHE_KEY);
      return;
    }
    localStorage.setItem(PROFILE_CACHE_KEY, JSON.stringify(profile));
  }

  function createClient() {
    if (!isConfigured()) {
      throw new Error(
        "Config Supabase manquante. Éditez web/assets/config.js (même URL + anon key que %AppData%\\RhosTool\\config.json)."
      );
    }
    if (!window.supabase || typeof window.supabase.createClient !== "function") {
      throw new Error("Client Supabase JS non chargé.");
    }
    return window.supabase.createClient(cfg.supabaseUrl, cfg.supabaseAnonKey, {
      auth: {
        persistSession: true,
        autoRefreshToken: true,
        detectSessionInUrl: true,
        storageKey: AUTH_STORAGE_KEY,
      },
    });
  }

  async function fetchCredits(client, userId) {
    const { data, error } = await client
      .from("profiles")
      .select("username,credits")
      .eq("id", userId)
      .maybeSingle();
    if (error) throw error;
    return data || { username: null, credits: 0 };
  }

  function renderSignedOut(ui) {
    ui.guest?.classList.remove("hidden");
    ui.user?.classList.add("hidden");
    if (ui.credits) ui.credits.textContent = "—";
    if (ui.email) ui.email.textContent = "";
    if (ui.username) ui.username.textContent = "";
  }

  async function renderSignedIn(client, ui, session) {
    ui.guest?.classList.add("hidden");
    ui.user?.classList.remove("hidden");
    const email = session.user?.email || "—";
    if (ui.email) ui.email.textContent = email;
    try {
      const profile = await fetchCredits(client, session.user.id);
      if (ui.username) {
        ui.username.textContent = profile.username || email.split("@")[0];
      }
      if (ui.credits) {
        const n = Number(profile.credits ?? 0);
        ui.credits.textContent = Number.isFinite(n) ? n.toFixed(2) : "0.00";
      }
      saveProfileCache({
        email,
        username: profile.username || email.split("@")[0],
        credits: profile.credits ?? 0,
      });
    } catch (err) {
      if (ui.username) ui.username.textContent = email.split("@")[0];
      if (ui.credits) ui.credits.textContent = "indisponible";
      setStatus(ui.status, `Profil : ${err.message || err}`, "err");
    }
  }

  async function initAccountPage() {
    const ui = {
      guest: $("auth-guest"),
      user: $("auth-user"),
      status: $("auth-status"),
      email: $("user-email"),
      username: $("user-username"),
      credits: $("user-credits"),
      loginForm: $("login-form"),
      registerForm: $("register-form"),
      recoverForm: $("recover-form"),
      logoutBtn: $("logout-btn"),
      refreshBtn: $("refresh-btn"),
    };

    if (!isConfigured()) {
      setStatus(
        ui.status,
        "Config Supabase manquante dans web/assets/config.js (même projet que l’app).",
        "err"
      );
      return;
    }

    let client;
    try {
      client = createClient();
    } catch (err) {
      setStatus(ui.status, err.message, "err");
      return;
    }

    const {
      data: { session },
    } = await client.auth.getSession();
    if (session) {
      await renderSignedIn(client, ui, session);
      setStatus(ui.status, "Connecté — même compte que RhosTool.exe", "ok");
    } else {
      renderSignedOut(ui);
    }

    client.auth.onAuthStateChange(async (_event, next) => {
      if (next) {
        await renderSignedIn(client, ui, next);
      } else {
        renderSignedOut(ui);
        saveProfileCache(null);
      }
    });

    ui.loginForm?.addEventListener("submit", async (e) => {
      e.preventDefault();
      const email = $("login-email")?.value?.trim() || "";
      const password = $("login-password")?.value || "";
      setStatus(ui.status, "Connexion…");
      const { error } = await client.auth.signInWithPassword({ email, password });
      if (error) {
        setStatus(ui.status, error.message || "Échec connexion.", "err");
        return;
      }
      setStatus(ui.status, "Connexion réussie.", "ok");
    });

    ui.registerForm?.addEventListener("submit", async (e) => {
      e.preventDefault();
      const email = $("register-email")?.value?.trim() || "";
      const password = $("register-password")?.value || "";
      const username = $("register-username")?.value?.trim() || email.split("@")[0];
      if (password.length < 6) {
        setStatus(ui.status, "Mot de passe : 6 caractères minimum.", "err");
        return;
      }
      setStatus(ui.status, "Création du compte…");
      const { data, error } = await client.auth.signUp({
        email,
        password,
        options: { data: { username } },
      });
      if (error) {
        setStatus(ui.status, error.message || "Échec inscription.", "err");
        return;
      }
      if (data.session) {
        setStatus(ui.status, "Compte créé — utilisable aussi dans l’exe.", "ok");
      } else {
        setStatus(
          ui.status,
          "Compte créé. Si la confirmation email est active, validez votre boîte mail puis connectez-vous.",
          "ok"
        );
      }
    });

    ui.recoverForm?.addEventListener("submit", async (e) => {
      e.preventDefault();
      const email = $("recover-email")?.value?.trim() || "";
      setStatus(ui.status, "Envoi du mail de réinitialisation…");
      const { error } = await client.auth.resetPasswordForEmail(email, {
        redirectTo: window.location.href,
      });
      if (error) {
        setStatus(ui.status, error.message || "Échec récupération.", "err");
        return;
      }
      setStatus(ui.status, "Email de réinitialisation envoyé (si le compte existe).", "ok");
    });

    ui.logoutBtn?.addEventListener("click", async () => {
      await client.auth.signOut();
      renderSignedOut(ui);
      setStatus(ui.status, "Déconnecté.", "ok");
    });

    ui.refreshBtn?.addEventListener("click", async () => {
      const {
        data: { session: current },
      } = await client.auth.getSession();
      if (!current) {
        setStatus(ui.status, "Non connecté.", "err");
        return;
      }
      setStatus(ui.status, "Actualisation des crédits…");
      await renderSignedIn(client, ui, current);
      setStatus(ui.status, "Crédits à jour.", "ok");
    });
  }

  function initDownloadPage() {
    const primary = $("download-primary");
    const fallback = $("download-fallback");
    const meta = $("download-meta");
    if (primary && cfg.downloadUrl) {
      primary.href = cfg.downloadUrl;
    }
    if (fallback && cfg.downloadFallbackUrl) {
      fallback.href = cfg.downloadFallbackUrl;
    }
    if (meta) {
      meta.textContent = "RhosTool-Portable.zip · Windows · single-file";
    }
  }

  function initNavAuthHint() {
    const chip = $("nav-auth-chip");
    if (!chip) return;
    const cached = loadProfileCache();
    if (cached?.email) {
      chip.textContent = cached.email;
      chip.classList.remove("hidden");
    }
  }

  document.addEventListener("DOMContentLoaded", () => {
    const page = document.body?.dataset?.page;
    initNavAuthHint();
    if (page === "account") {
      initAccountPage();
    }
    if (page === "downloads") {
      initDownloadPage();
    }
  });
})();
