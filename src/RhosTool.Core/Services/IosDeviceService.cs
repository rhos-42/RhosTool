using System.Diagnostics;
using System.Text;
using RhosTool.Core.Models;

namespace RhosTool.Core.Services;

public sealed class IosDeviceService
{
    private readonly LogService _log;
    private string? _ideviceinfoPath;

    public IosDeviceService(LogService log)
    {
        _log = log;
    }

    public bool IsAvailable
    {
        get
        {
            _ideviceinfoPath ??= ResolveIdeviceinfoPath();
            return !string.IsNullOrEmpty(_ideviceinfoPath) && File.Exists(_ideviceinfoPath);
        }
    }

    public async Task<IReadOnlyList<DeviceInfo>> GetDevicesAsync(CancellationToken ct = default)
    {
        if (!IsAvailable)
        {
            return Array.Empty<DeviceInfo>();
        }

        try
        {
            var output = await RunAsync(string.Empty, ct).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(output) ||
                output.Contains("No device found", StringComparison.OrdinalIgnoreCase))
            {
                return Array.Empty<DeviceInfo>();
            }

            var map = ParseKeyValues(output);
            var udid = map.GetValueOrDefault("UniqueDeviceID")
                       ?? map.GetValueOrDefault("Unique Chip ID")
                       ?? "ios-device";

            var info = new DeviceInfo
            {
                Serial = udid,
                Platform = DevicePlatform.Ios,
                IsAuthorized = true,
                State = "device",
                Model = map.GetValueOrDefault("ProductType") ?? map.GetValueOrDefault("DeviceName") ?? "iPhone",
                Manufacturer = "Apple",
                AndroidVersion = map.GetValueOrDefault("ProductVersion") ?? "-",
                BuildId = map.GetValueOrDefault("BuildVersion") ?? "-",
                Product = map.GetValueOrDefault("ProductName") ?? "iOS"
            };

            return new[] { info };
        }
        catch (Exception ex)
        {
            _log.Warn($"iOS: {ex.Message}");
            return Array.Empty<DeviceInfo>();
        }
    }

    private async Task<string> RunAsync(string arguments, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(_ideviceinfoPath))
        {
            throw new InvalidOperationException("ideviceinfo n'est pas disponible.");
        }

        var psi = new ProcessStartInfo
        {
            FileName = _ideviceinfoPath,
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
        var stdout = await process.StandardOutput.ReadToEndAsync(ct).ConfigureAwait(false);
        var stderr = await process.StandardError.ReadToEndAsync(ct).ConfigureAwait(false);
        await process.WaitForExitAsync(ct).ConfigureAwait(false);

        if (process.ExitCode != 0 && !string.IsNullOrWhiteSpace(stderr))
        {
            return stderr;
        }

        return stdout;
    }

    private static Dictionary<string, string> ParseKeyValues(string output)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var idx = line.IndexOf(':');
            if (idx <= 0)
            {
                continue;
            }

            var key = line[..idx].Trim();
            var value = line[(idx + 1)..].Trim();
            map[key] = value;
        }

        return map;
    }

    private static string? ResolveIdeviceinfoPath()
    {
        var candidates = new List<string>();
        var baseDir = AppContext.BaseDirectory;
        candidates.Add(Path.Combine(baseDir, "libimobiledevice", "ideviceinfo.exe"));
        candidates.Add(Path.Combine(baseDir, "tools", "libimobiledevice", "ideviceinfo.exe"));

        var current = new DirectoryInfo(baseDir);
        for (var i = 0; i < 6 && current != null; i++)
        {
            candidates.Add(Path.Combine(current.FullName, "tools", "libimobiledevice", "ideviceinfo.exe"));
            current = current.Parent;
        }

        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var segment in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            candidates.Add(Path.Combine(segment.Trim('"'), "ideviceinfo.exe"));
        }

        return candidates.FirstOrDefault(File.Exists);
    }
}
