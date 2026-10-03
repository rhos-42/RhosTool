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

    public async Task<string> DevicesAsync(CancellationToken ct = default)
    {
        if (!IsAvailable)
        {
            return "fastboot introuvable. Placez platform-tools (fastboot.exe) à côté d'adb.";
        }

        var output = await RunAsync("devices", ct).ConfigureAwait(false);
        var lines = output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(l => !l.StartsWith("List of devices", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (lines.Count == 0)
        {
            return "Aucun appareil fastboot détecté.\nAstuce : REBOOT BOOTLOADER puis reconnectez le câble.";
        }

        _log.Success($"{lines.Count} appareil(s) fastboot.");
        return string.Join(Environment.NewLine, lines);
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

        // same folder as adb if found via sibling
        return candidates.FirstOrDefault(File.Exists);
    }
}
