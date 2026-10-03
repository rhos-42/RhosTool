using System.Text.Json.Serialization;

namespace RhosTool.Core.Models;

public sealed class AppAuthConfig
{
    public string SupabaseUrl { get; set; } = "";
    public string SupabaseAnonKey { get; set; } = "";

    [JsonIgnore]
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(SupabaseUrl) &&
        !string.IsNullOrWhiteSpace(SupabaseAnonKey) &&
        !SupabaseUrl.Contains("VOTRE_REF", StringComparison.OrdinalIgnoreCase) &&
        !SupabaseAnonKey.StartsWith("eyJhbGciOi...REPLACE", StringComparison.Ordinal);
}

public sealed class AuthSession
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required string UserId { get; init; }
    public required string Email { get; init; }
    public string Username { get; set; } = "";
    public decimal Credits { get; set; }
    public DateTimeOffset ExpiresAt { get; init; }
}

public sealed class AuthResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = "";
    public AuthSession? Session { get; init; }

    public static AuthResult Ok(AuthSession session, string message = "OK") =>
        new() { Success = true, Message = message, Session = session };

    public static AuthResult Succeed(string message) =>
        new() { Success = true, Message = message };

    public static AuthResult Fail(string message) =>
        new() { Success = false, Message = message };
}
