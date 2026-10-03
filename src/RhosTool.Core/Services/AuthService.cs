using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using RhosTool.Core.Models;

namespace RhosTool.Core.Services;

public sealed class AuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _http;
    private AppAuthConfig _config;

    public AuthService(AppAuthConfig config, HttpClient? httpClient = null)
    {
        _config = config;
        _http = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    }

    public AuthSession? CurrentSession { get; private set; }

    public bool IsAuthenticated => CurrentSession is not null &&
                                   !string.IsNullOrWhiteSpace(CurrentSession.AccessToken);

    public bool IsConfigured => _config.IsConfigured;

    public void UpdateConfig(AppAuthConfig config) => _config = config;

    public void SetSession(AuthSession? session) => CurrentSession = session;

    public void Logout() => CurrentSession = null;

    public async Task<AuthResult> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        if (!EnsureConfigured(out var fail))
        {
            return fail;
        }

        email = email.Trim();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return AuthResult.Fail("Email et mot de passe requis.");
        }

        try
        {
            var url = $"{NormalizeUrl(_config.SupabaseUrl)}/auth/v1/token?grant_type=password";
            using var req = CreateRequest(HttpMethod.Post, url);
            req.Content = new StringContent(
                JsonSerializer.Serialize(new { email, password }, JsonOptions),
                Encoding.UTF8,
                "application/json");

            using var res = await _http.SendAsync(req, ct).ConfigureAwait(false);
            var body = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if (!res.IsSuccessStatusCode)
            {
                return AuthResult.Fail(ExtractError(body) ?? $"Échec connexion ({(int)res.StatusCode}).");
            }

            var token = JsonSerializer.Deserialize<TokenResponse>(body, JsonOptions);
            if (token?.AccessToken is null || token.User?.Id is null)
            {
                return AuthResult.Fail("Réponse auth invalide.");
            }

            var session = new AuthSession
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken ?? "",
                UserId = token.User.Id,
                Email = token.User.Email ?? email,
                ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn > 0 ? token.ExpiresIn : 3600)
            };

            CurrentSession = session;
            await RefreshProfileAsync(ct).ConfigureAwait(false);
            return AuthResult.Ok(CurrentSession!, "Connexion réussie.");
        }
        catch (TaskCanceledException)
        {
            return AuthResult.Fail("Délai dépassé — vérifiez votre connexion.");
        }
        catch (Exception ex)
        {
            return AuthResult.Fail($"Erreur réseau : {ex.Message}");
        }
    }

    public async Task<AuthResult> RegisterAsync(
        string email,
        string password,
        string? username = null,
        CancellationToken ct = default)
    {
        if (!EnsureConfigured(out var fail))
        {
            return fail;
        }

        email = email.Trim();
        username = string.IsNullOrWhiteSpace(username)
            ? email.Split('@')[0]
            : username.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return AuthResult.Fail("Email et mot de passe requis.");
        }

        if (password.Length < 6)
        {
            return AuthResult.Fail("Mot de passe : 6 caractères minimum.");
        }

        try
        {
            var url = $"{NormalizeUrl(_config.SupabaseUrl)}/auth/v1/signup";
            using var req = CreateRequest(HttpMethod.Post, url);
            var payload = new
            {
                email,
                password,
                data = new { username }
            };
            req.Content = new StringContent(
                JsonSerializer.Serialize(payload, JsonOptions),
                Encoding.UTF8,
                "application/json");

            using var res = await _http.SendAsync(req, ct).ConfigureAwait(false);
            var body = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if (!res.IsSuccessStatusCode)
            {
                return AuthResult.Fail(ExtractError(body) ?? $"Échec inscription ({(int)res.StatusCode}).");
            }

            // Certains projets exigent la confirmation email : pas de session immédiate
            var token = JsonSerializer.Deserialize<TokenResponse>(body, JsonOptions);
            if (!string.IsNullOrWhiteSpace(token?.AccessToken) && token.User?.Id is not null)
            {
                CurrentSession = new AuthSession
                {
                    AccessToken = token.AccessToken,
                    RefreshToken = token.RefreshToken ?? "",
                    UserId = token.User.Id,
                    Email = token.User.Email ?? email,
                    Username = username,
                    ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn > 0 ? token.ExpiresIn : 3600)
                };
                await RefreshProfileAsync(ct).ConfigureAwait(false);
                return AuthResult.Ok(CurrentSession!, "Compte créé.");
            }

            return AuthResult.Succeed(
                "Compte créé. Si la confirmation email est active, validez votre boîte mail puis connectez-vous.");
        }
        catch (TaskCanceledException)
        {
            return AuthResult.Fail("Délai dépassé — vérifiez votre connexion.");
        }
        catch (Exception ex)
        {
            return AuthResult.Fail($"Erreur réseau : {ex.Message}");
        }
    }

    public async Task<AuthResult> RecoverPasswordAsync(string email, CancellationToken ct = default)
    {
        if (!EnsureConfigured(out var fail))
        {
            return fail;
        }

        email = email.Trim();
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return AuthResult.Fail("Email requis pour réinitialiser le mot de passe.");
        }

        try
        {
            var url = $"{NormalizeUrl(_config.SupabaseUrl)}/auth/v1/recover";
            using var req = CreateRequest(HttpMethod.Post, url);
            req.Content = new StringContent(
                JsonSerializer.Serialize(new { email }, JsonOptions),
                Encoding.UTF8,
                "application/json");

            using var res = await _http.SendAsync(req, ct).ConfigureAwait(false);
            var body = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            if (!res.IsSuccessStatusCode)
            {
                return AuthResult.Fail(ExtractError(body) ?? $"Échec récupération ({(int)res.StatusCode}).");
            }

            return AuthResult.Succeed(
                "Email de réinitialisation envoyé. Vérifiez votre boîte mail.");
        }
        catch (TaskCanceledException)
        {
            return AuthResult.Fail("Délai dépassé — vérifiez votre connexion.");
        }
        catch (Exception ex)
        {
            return AuthResult.Fail($"Erreur réseau : {ex.Message}");
        }
    }

    public async Task<AuthResult> RestoreSessionAsync(AuthSession session, CancellationToken ct = default)
    {
        if (!EnsureConfigured(out var fail))
        {
            return fail;
        }

        CurrentSession = session;

        if (!string.IsNullOrWhiteSpace(session.RefreshToken) &&
            session.ExpiresAt <= DateTimeOffset.UtcNow.AddMinutes(2))
        {
            var refreshed = await RefreshTokenAsync(session.RefreshToken, ct).ConfigureAwait(false);
            if (!refreshed.Success)
            {
                CurrentSession = null;
                return refreshed;
            }
        }

        try
        {
            var url = $"{NormalizeUrl(_config.SupabaseUrl)}/auth/v1/user";
            using var req = CreateRequest(HttpMethod.Get, url, CurrentSession!.AccessToken);
            using var res = await _http.SendAsync(req, ct).ConfigureAwait(false);
            if (!res.IsSuccessStatusCode)
            {
                CurrentSession = null;
                return AuthResult.Fail("Session expirée — reconnectez-vous.");
            }

            await RefreshProfileAsync(ct).ConfigureAwait(false);
            return AuthResult.Ok(CurrentSession!, "Session restaurée.");
        }
        catch (Exception ex)
        {
            CurrentSession = null;
            return AuthResult.Fail($"Impossible de restaurer la session : {ex.Message}");
        }
    }

    public async Task RefreshProfileAsync(CancellationToken ct = default)
    {
        if (CurrentSession is null || !IsConfigured)
        {
            return;
        }

        try
        {
            var url =
                $"{NormalizeUrl(_config.SupabaseUrl)}/rest/v1/profiles?select=username,credits&id=eq.{CurrentSession.UserId}";
            using var req = CreateRequest(HttpMethod.Get, url, CurrentSession.AccessToken);
            req.Headers.TryAddWithoutValidation("Prefer", "return=representation");

            using var res = await _http.SendAsync(req, ct).ConfigureAwait(false);
            var body = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            if (!res.IsSuccessStatusCode)
            {
                return;
            }

            var rows = JsonSerializer.Deserialize<List<ProfileRow>>(body, JsonOptions);
            var row = rows?.FirstOrDefault();
            if (row is null)
            {
                return;
            }

            CurrentSession.Username = string.IsNullOrWhiteSpace(row.Username)
                ? CurrentSession.Email.Split('@')[0]
                : row.Username;
            CurrentSession.Credits = row.Credits;
        }
        catch
        {
            // profil optionnel au démarrage
        }
    }

    private async Task<AuthResult> RefreshTokenAsync(string refreshToken, CancellationToken ct)
    {
        try
        {
            var url = $"{NormalizeUrl(_config.SupabaseUrl)}/auth/v1/token?grant_type=refresh_token";
            using var req = CreateRequest(HttpMethod.Post, url);
            req.Content = new StringContent(
                JsonSerializer.Serialize(new { refresh_token = refreshToken }, JsonOptions),
                Encoding.UTF8,
                "application/json");

            using var res = await _http.SendAsync(req, ct).ConfigureAwait(false);
            var body = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            if (!res.IsSuccessStatusCode)
            {
                return AuthResult.Fail(ExtractError(body) ?? "Refresh token invalide.");
            }

            var token = JsonSerializer.Deserialize<TokenResponse>(body, JsonOptions);
            if (token?.AccessToken is null || token.User?.Id is null)
            {
                return AuthResult.Fail("Refresh auth invalide.");
            }

            CurrentSession = new AuthSession
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken ?? refreshToken,
                UserId = token.User.Id,
                Email = token.User.Email ?? CurrentSession?.Email ?? "",
                Username = CurrentSession?.Username ?? "",
                Credits = CurrentSession?.Credits ?? 0,
                ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn > 0 ? token.ExpiresIn : 3600)
            };

            return AuthResult.Ok(CurrentSession);
        }
        catch (Exception ex)
        {
            return AuthResult.Fail(ex.Message);
        }
    }

    private bool EnsureConfigured(out AuthResult fail)
    {
        if (_config.IsConfigured)
        {
            fail = AuthResult.Fail("");
            return true;
        }

        fail = AuthResult.Fail(
            "Config Supabase manquante. Éditez %AppData%\\RhosTool\\config.json (voir docs/AUTH-SUPABASE.md).");
        return false;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string url, string? bearer = null)
    {
        var req = new HttpRequestMessage(method, url);
        req.Headers.TryAddWithoutValidation("apikey", _config.SupabaseAnonKey);
        if (!string.IsNullOrWhiteSpace(bearer))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
        }
        else
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.SupabaseAnonKey);
        }

        return req;
    }

    private static string NormalizeUrl(string url) => url.Trim().TrimEnd('/');

    private static string? ExtractError(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error_description", out var desc))
            {
                return desc.GetString();
            }

            if (doc.RootElement.TryGetProperty("msg", out var msg))
            {
                return msg.GetString();
            }

            if (doc.RootElement.TryGetProperty("error", out var err))
            {
                return err.ValueKind == JsonValueKind.String ? err.GetString() : err.ToString();
            }

            if (doc.RootElement.TryGetProperty("message", out var message))
            {
                return message.GetString();
            }
        }
        catch
        {
            // ignore
        }

        return string.IsNullOrWhiteSpace(body) ? null : body.Trim();
    }

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("user")]
        public UserDto? User { get; set; }
    }

    private sealed class UserDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }
    }

    private sealed class ProfileRow
    {
        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("credits")]
        public decimal Credits { get; set; }
    }
}
