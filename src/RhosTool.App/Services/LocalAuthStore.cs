using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using RhosTool.Core.Models;

namespace RhosTool.App.Services;

public static class LocalAuthStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static string AppDataDir
    {
        get
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RhosTool");
            Directory.CreateDirectory(dir);
            return dir;
        }
    }

    public static string ConfigPath => Path.Combine(AppDataDir, "config.json");
    public static string SessionPath => Path.Combine(AppDataDir, "session.bin");
    public static string IdentitiesPath => Path.Combine(AppDataDir, "identities.json");

    private const string LastUsernameKey = "__last_username__";

    public static void RememberIdentity(string username, string email)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        var map = LoadIdentities();
        map[username.Trim()] = email.Trim();
        map[LastUsernameKey] = username.Trim();
        var json = JsonSerializer.Serialize(map, JsonOptions);
        File.WriteAllText(IdentitiesPath, json, Encoding.UTF8);
    }

    public static string? LastUsername()
    {
        var map = LoadIdentities();
        if (map.TryGetValue(LastUsernameKey, out var last) && !string.IsNullOrWhiteSpace(last))
        {
            return last;
        }

        return map.Keys.FirstOrDefault(k =>
            !string.Equals(k, LastUsernameKey, StringComparison.OrdinalIgnoreCase));
    }

    public static string? ResolveEmail(string usernameOrEmail)
    {
        var value = usernameOrEmail?.Trim() ?? "";
        if (value.Contains('@', StringComparison.Ordinal))
        {
            return value;
        }

        var map = LoadIdentities();
        return map.TryGetValue(value, out var email) ? email : null;
    }

    private static Dictionary<string, string> LoadIdentities()
    {
        try
        {
            if (!File.Exists(IdentitiesPath))
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            var json = File.ReadAllText(IdentitiesPath);
            var raw = JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions)
                      ?? new Dictionary<string, string>();
            return new Dictionary<string, string>(raw, StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public static AppAuthConfig LoadOrCreateConfig()
    {
        // Priorité : %AppData%, sinon appsettings.json à côté de l'exe
        if (File.Exists(ConfigPath))
        {
            var cfg = ReadConfig(ConfigPath);
            if (cfg is not null)
            {
                return cfg;
            }
        }

        var bundled = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (File.Exists(bundled))
        {
            var cfg = ReadConfig(bundled);
            if (cfg is not null)
            {
                SaveConfig(cfg);
                return cfg;
            }
        }

        var empty = new AppAuthConfig
        {
            SupabaseUrl = "https://VOTRE_REF.supabase.co",
            SupabaseAnonKey = "eyJhbGciOi...REPLACE_WITH_ANON_KEY"
        };
        SaveConfig(empty);
        return empty;
    }

    public static void SaveConfig(AppAuthConfig config)
    {
        var json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(ConfigPath, json, Encoding.UTF8);
    }

    public static void SaveSession(AuthSession session)
    {
        var json = JsonSerializer.Serialize(session, JsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);
        var protectedBytes = ProtectedData.Protect(bytes, optionalEntropy: null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(SessionPath, protectedBytes);
    }

    public static AuthSession? LoadSession()
    {
        if (!File.Exists(SessionPath))
        {
            return null;
        }

        try
        {
            var protectedBytes = File.ReadAllBytes(SessionPath);
            var bytes = ProtectedData.Unprotect(protectedBytes, optionalEntropy: null, DataProtectionScope.CurrentUser);
            var json = Encoding.UTF8.GetString(bytes);
            return JsonSerializer.Deserialize<AuthSession>(json, JsonOptions);
        }
        catch
        {
            TryDeleteSession();
            return null;
        }
    }

    public static void ClearSession() => TryDeleteSession();

    private static void TryDeleteSession()
    {
        try
        {
            if (File.Exists(SessionPath))
            {
                File.Delete(SessionPath);
            }
        }
        catch
        {
            // ignore
        }
    }

    private static AppAuthConfig? ReadConfig(string path)
    {
        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<AppAuthConfig>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
