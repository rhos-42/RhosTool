using System.Diagnostics;
using System.Text;
using RhosTool.Core.Models;

namespace RhosTool.Core.Services;

public sealed class FastbootService
{
    private readonly LogService _log;
    private string? _fastbootPath;

    public FastbootService(LogService log)
    {
        _log = log;
    }

    public string? FastbootPath => _fastbootPath ??= ResolveFastbootPath();

    public bool IsAvailable => !string.IsNullOrEmpty(FastbootPath) && File.Exists(FastbootPath);

    public async Task<IReadOnlyList<string>> GetSerialsAsync(CancellationToken ct = default)
    {
        if (!IsAvailable)
        {
            return Array.Empty<string>();
        }

        var output = await RunAsync("devices", ct).ConfigureAwait(false);
        var serials = new List<string>();
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (line.StartsWith("List of devices", StringComparison.OrdinalIgnoreCase))
                continue;

            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2 &&
                parts[1].Equals("fastboot", StringComparison.OrdinalIgnoreCase))
            {
                serials.Add(parts[0]);
            }
            else if (parts.Length >= 1 &&
                     !line.Contains('?', StringComparison.Ordinal) &&
                     parts[0].Length > 2 &&
                     !parts[0].Equals("fastboot", StringComparison.OrdinalIgnoreCase))
            {
                // Certaines builds affichent juste "SERIAL\tfastboot" déjà couvert ;
                // ignore les lignes vides / bannières.
                if (parts.Length >= 2)
                    serials.Add(parts[0]);
            }
        }

        return serials.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<string> DevicesAsync(CancellationToken ct = default)
    {
        if (!IsAvailable)
        {
            return "fastboot introuvable. Placez platform-tools (fastboot.exe) à côté d'adb.";
        }

        var serials = await GetSerialsAsync(ct).ConfigureAwait(false);
        if (serials.Count == 0)
        {
            return "Aucun appareil fastboot détecté.\nAstuce : REBOOT BOOTLOADER puis reconnectez le câble.";
        }

        _log.Success($"{serials.Count} appareil(s) fastboot.");
        return string.Join(Environment.NewLine, serials.Select(s => $"{s}\tfastboot"));
    }

    public async Task<string> GetVarAsync(string variable, CancellationToken ct = default)
    {
        if (!IsAvailable)
        {
            return "fastboot introuvable.";
        }

        return (await RunAsync($"getvar {variable}", ct).ConfigureAwait(false)).Trim();
    }

    private async Task<string> RunAsync(string arguments, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(FastbootPath))
        {
            throw new InvalidOperationException("fastboot n'est pas disponible.");
        }

        var psi = new ProcessStartInfo
        {
            FileName = FastbootPath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = new Process { StartInfo = psi };
        process.Start();
        var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
        var stderrTask = process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct).ConfigureAwait(false);
        var stdout = await stdoutTask.ConfigureAwait(false);
        var stderr = await stderrTask.ConfigureAwait(false);
        // fastboot often prints to stderr
        var combined = string.Join(Environment.NewLine, new[] { stdout, stderr }.Where(s => !string.IsNullOrWhiteSpace(s)));
        return combined.Trim();
    }

    private static string? ResolveFastbootPath()
    {
        var candidates = new List<string>();
        candidates.Add(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RhosTool", "platform-tools", "fastboot.exe"));

        var baseDir = AppContext.BaseDirectory;
        candidates.Add(Path.Combine(baseDir, "platform-tools", "fastboot.exe"));
        candidates.Add(Path.Combine(baseDir, "tools", "platform-tools", "fastboot.exe"));

        var current = new DirectoryInfo(baseDir);
        for (var i = 0; i < 6 && current != null; i++)
        {
            candidates.Add(Path.Combine(current.FullName, "tools", "platform-tools", "fastboot.exe"));
            current = current.Parent;
        }

        var env = Environment.GetEnvironmentVariable("ANDROID_HOME")
                  ?? Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");
        if (!string.IsNullOrWhiteSpace(env))
        {
            candidates.Add(Path.Combine(env, "platform-tools", "fastboot.exe"));
        }

        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var segment in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            candidates.Add(Path.Combine(segment.Trim('"'), "fastboot.exe"));
        }

        return candidates.FirstOrDefault(File.Exists);
    }
}
