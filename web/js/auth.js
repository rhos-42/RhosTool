(function () {
  const cfg = () => window.RHOS_CONFIG || {};
  const STORAGE_KEY = "rhostool.session";

  function isConfigured() {
    const c = cfg();
    return Boolean(c.supabaseUrl && c.supabaseAnonKey && !c.supabaseAnonKey.includes("COLLEZ"));
  }

  function headers(extra = {}) {
    const c = cfg();
    return {
      apikey: c.supabaseAnonKey,
      Authorization: `Bearer ${extra.accessToken || c.supabaseAnonKey}`,
      "Content-Type": "application/json",
      ...extra.headers
    };
  }

  function saveSession(session) {
    if (!session) {
      localStorage.removeItem(STORAGE_KEY);
      return;
    }
    localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
  }

  function loadSession() {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }

  async function parseError(res) {
    try {
      const body = await res.json();
      return body.error_description || body.msg || body.message || body.error || `Erreur HTTP ${res.status}`;
    } catch {
      return `Erreur HTTP ${res.status}`;
    }
  }

  async function login(email, password) {
    if (!isConfigured()) {
      return { ok: false, message: "Clé Supabase manquante dans web/js/config.js" };
    }
    const url = `${cfg().supabaseUrl.replace(/\/$/, "")}/auth/v1/token?grant_type=password`;
    const res = await fetch(url, {
      method: "POST",
      headers: headers(),
      body: JSON.stringify({ email: email.trim(), password })
    });
    if (!res.ok) {
      return { ok: false, message: await parseError(res) };
    }
    const data = await res.json();
    const session = {
      accessToken: data.access_token,
      refreshToken: data.refresh_token,
      userId: data.user?.id,
      email: data.user?.email || email.trim(),
      username: data.user?.user_metadata?.username || (email.trim().split("@")[0]),
      credits: 0
    };
    await enrichProfile(session);
    saveSession(session);
    return { ok: true, session, message: "Connecté." };
  }

  async function register(email, password, username) {
    if (!isConfigured()) {
      return { ok: false, message: "Clé Supabase manquante dans web/js/config.js" };
    }
    if ((password || "").length < 6) {
      return { ok: false, message: "Mot de passe : 6 caractères minimum." };
    }
    const cleanEmail = email.trim();
    const cleanUser = (username || "").trim() || cleanEmail.split("@")[0];
    const url = `${cfg().supabaseUrl.replace(/\/$/, "")}/auth/v1/signup`;
    const res = await fetch(url, {
      method: "POST",
      headers: headers(),
      body: JSON.stringify({
        email: cleanEmail,
        password,
        data: { username: cleanUser }
      })
    });
    if (!res.ok) {
      return { ok: false, message: await parseError(res) };
    }
    const data = await res.json();
    if (data.access_token && data.user?.id) {
      const session = {
        accessToken: data.access_token,
        refreshToken: data.refresh_token,
        userId: data.user.id,
        email: data.user.email || cleanEmail,
        username: cleanUser,
        credits: 0
      };
      await enrichProfile(session);
      saveSession(session);
      return { ok: true, session, message: "Compte créé." };
    }
    return {
      ok: true,
      session: null,
      message: "Compte créé. Si la confirmation email est active, validez votre boîte mail puis connectez-vous."
    };
  }

  async function recover(email) {
    if (!isConfigured()) {
      return { ok: false, message: "Clé Supabase manquante dans web/js/config.js" };
    }
    const url = `${cfg().supabaseUrl.replace(/\/$/, "")}/auth/v1/recover`;
    const res = await fetch(url, {
      method: "POST",
      headers: headers(),
      body: JSON.stringify({ email: email.trim() })
    });
    if (!res.ok) {
      return { ok: false, message: await parseError(res) };
    }
    return { ok: true, message: "Email de récupération envoyé (si le compte existe)." };
  }

  async function enrichProfile(session) {
    if (!session?.accessToken || !session.userId) return;
    const url =
      `${cfg().supabaseUrl.replace(/\/$/, "")}/rest/v1/profiles` +
      `?select=username,credits&id=eq.${session.userId}`;
    const res = await fetch(url, {
      headers: headers({
        accessToken: session.accessToken,
        headers: { Accept: "application/json" }
      })
    });
    if (!res.ok) return;
    const rows = await res.json();
    if (Array.isArray(rows) && rows[0]) {
      if (rows[0].username) session.username = rows[0].username;
      if (rows[0].credits != null) session.credits = Number(rows[0].credits);
    }
  }

  async function refreshSession() {
    const session = loadSession();
    if (!session?.refreshToken || !isConfigured()) return null;
    const url = `${cfg().supabaseUrl.replace(/\/$/, "")}/auth/v1/token?grant_type=refresh_token`;
    const res = await fetch(url, {
      method: "POST",
      headers: headers(),
      body: JSON.stringify({ refresh_token: session.refreshToken })
    });
    if (!res.ok) {
      saveSession(null);
      return null;
    }
    const data = await res.json();
    const next = {
      accessToken: data.access_token,
      refreshToken: data.refresh_token || session.refreshToken,
      userId: data.user?.id || session.userId,
      email: data.user?.email || session.email,
      username: session.username,
      credits: session.credits ?? 0
    };
    await enrichProfile(next);
    saveSession(next);
    return next;
  }

  function logout() {
    saveSession(null);
  }

  window.RhosAuth = {
    isConfigured,
    login,
    register,
    recover,
    refreshSession,
    loadSession,
    logout,
    enrichProfile
  };
})();
