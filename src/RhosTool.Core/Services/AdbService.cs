using System.Diagnostics;
using System.Text;
using RhosTool.Core.Models;

namespace RhosTool.Core.Services;

public sealed class AdbService
{
    private readonly LogService _log;
    private string? _adbPath;

    public AdbService(LogService log)
    {
        _log = log;
    }

    public string? AdbPath => _adbPath ??= ResolveAdbPath();

    public bool IsAvailable => !string.IsNullOrEmpty(AdbPath) && File.Exists(AdbPath);

    public async Task EnsureServerAsync(CancellationToken ct = default)
    {
        if (!IsAvailable)
        {
            _log.Warn("ADB introuvable. Placez platform-tools dans tools/platform-tools.");
            return;
        }

        await RunAsync("start-server", ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<DeviceInfo>> GetDevicesAsync(CancellationToken ct = default)
    {
        if (!IsAvailable)
        {
            return Array.Empty<DeviceInfo>();
        }

        var output = await RunAsync("devices -l", ct).ConfigureAwait(false);
        var devices = new List<DeviceInfo>();

        foreach (var raw in output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (raw.StartsWith("List of devices", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var parts = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                continue;
            }

            var serial = parts[0];
            var state = parts[1];
            var authorized = state.Equals("device", StringComparison.OrdinalIgnoreCase);

            var info = new DeviceInfo
            {
                Serial = serial,
                State = state,
                IsAuthorized = authorized,
                Platform = DevicePlatform.Android
            };

            if (authorized)
            {
                info = await EnrichAsync(info, ct).ConfigureAwait(false);
            }

            devices.Add(info);
        }

        return devices;
    }

    public async Task<DeviceInfo?> RefreshDeviceAsync(string serial, CancellationToken ct = default)
    {
        var devices = await GetDevicesAsync(ct).ConfigureAwait(false);
        return devices.FirstOrDefault(d => d.Serial.Equals(serial, StringComparison.OrdinalIgnoreCase));
    }

    public async Task RebootAsync(string serial, string? mode = null, CancellationToken ct = default)
    {
        var args = string.IsNullOrWhiteSpace(mode)
            ? $"-s {serial} reboot"
            : $"-s {serial} reboot {mode}";

        _log.Info($"Redémarrage ({mode ?? "normal"}) : {serial}");
        await RunAsync(args, ct).ConfigureAwait(false);
        _log.Success("Commande reboot envoyée.");
    }

    public async Task<string> StatusAsync(CancellationToken ct = default)
    {
        if (!IsAvailable)
        {
            return "ADB introuvable. Lancez installer/setup-platform-tools.ps1 ou placez platform-tools dans le dossier de l'app.";
        }

        await EnsureServerAsync(ct).ConfigureAwait(false);
        var devices = await GetDevicesAsync(ct).ConfigureAwait(false);
        var sb = new StringBuilder();
        sb.AppendLine($"ADB: {AdbPath}");
        sb.AppendLine($"Appareils: {devices.Count}");
        foreach (var d in devices)
        {
            sb.AppendLine($"  - {d.Serial} [{d.State}] {d.Manufacturer} {d.Model}".Trim());
        }

        if (devices.Count == 0)
        {
            sb.AppendLine("Aucun appareil. Activez le débogage USB et acceptez l'empreinte RSA.");
        }

        return sb.ToString().TrimEnd();
    }

    public async Task<string> GetDetailedInfoAsync(string serial, CancellationToken ct = default)
    {
        var props = new (string Key, string Prop)[]
        {
            ("Fabricant", "ro.product.manufacturer"),
            ("Modèle", "ro.product.model"),
            ("Produit", "ro.product.name"),
            ("Appareil", "ro.product.device"),
            ("Android", "ro.build.version.release"),
            ("SDK", "ro.build.version.sdk"),
            ("Build", "ro.build.display.id"),
            ("Fingerprint", "ro.build.fingerprint"),
            ("CPU ABI", "ro.product.cpu.abi"),
            ("Hardware", "ro.hardware"),
            ("Board", "ro.product.board"),
            ("Série", "ro.serialno"),
            ("OEM unlock allowed", "sys.oem_unlock_allowed"),
            ("Bootloader", "ro.bootloader"),
            ("Baseband", "gsm.version.baseband")
        };

        var sb = new StringBuilder();
        sb.AppendLine($"Série ADB: {serial}");
        foreach (var (key, prop) in props)
        {
            var value = await GetPropAsync(serial, prop, ct).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(value))
            {
                sb.AppendLine($"{key}: {value}");
            }
        }

        // Storage / battery (best effort)
        var df = (await ShellAsync(serial, "df /data 2>/dev/null | tail -n 1", ct).ConfigureAwait(false)).Trim();
        if (!string.IsNullOrWhiteSpace(df))
        {
            sb.AppendLine($"Data FS: {df}");
        }

        var battRaw = await ShellAsync(serial, "dumpsys battery", ct).ConfigureAwait(false);
        var battLines = battRaw
            .Split('\n')
            .Select(l => l.Trim())
            .Where(l =>
                l.StartsWith("level", StringComparison.OrdinalIgnoreCase) ||
                l.StartsWith("status", StringComparison.OrdinalIgnoreCase) ||
                l.StartsWith("powered", StringComparison.OrdinalIgnoreCase) ||
                l.StartsWith("USB powered", StringComparison.OrdinalIgnoreCase))
            .Take(8);
        var batt = string.Join(Environment.NewLine, battLines);
        if (!string.IsNullOrWhiteSpace(batt))
        {
            sb.AppendLine("Batterie:");
            sb.AppendLine(batt);
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Remise d'usine via ADB (appareil déjà autorisé). Ouvre aussi l'écran réglages en secours.
    /// </summary>
    public async Task<string> FactoryResetAsync(string serial, CancellationToken ct = default)
    {
        _log.Warn($"Factory reset demandé pour {serial}");

        // 1) Tentative API moderne
        var wipe = await ShellAsync(serial, "cmd device_policy wipe-data 2>/dev/null", ct).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(wipe) &&
            !wipe.Contains("Unknown", StringComparison.OrdinalIgnoreCase) &&
            !wipe.Contains("Exception", StringComparison.OrdinalIgnoreCase) &&
            !wipe.Contains("Permission", StringComparison.OrdinalIgnoreCase))
        {
            return $"Wipe demandé via device_policy.\n{wipe.Trim()}";
        }

        // 2) Intent réglages factory reset (utilisateur confirme sur l'écran)
        await ShellAsync(
            serial,
            "am start -a android.settings.FACTORY_RESET 2>/dev/null || am start -a android.settings.SETTINGS",
            ct).ConfigureAwait(false);

        // 3) Prépare wipe recovery (certains builds)
        await ShellAsync(serial, "recovery --wipe_data 2>/dev/null", ct).ConfigureAwait(false);

        return "Écran de réinitialisation ouvert (ou wipe recovery préparé).\n" +
               "Confirmez sur le téléphone. Si rien ne s'affiche : Settings → System → Reset.";
    }

    public async Task<string> OpenSecuritySettingsAsync(string serial, CancellationToken ct = default)
    {
        await ShellAsync(serial, "am start -a android.settings.SECURITY_SETTINGS", ct).ConfigureAwait(false);
        return "Réglages Sécurité / verrouillage ouverts sur l'appareil.";
    }

    public async Task<string> ShellAsync(string serial, string command, CancellationToken ct = default)
    {
        return await RunAsync($"-s {serial} shell {command}", ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<string>> GetPackageListAsync(
        string serial,
        bool thirdPartyOnly,
        CancellationToken ct = default)
    {
        var flag = thirdPartyOnly ? " -3" : "";
        var output = await ShellAsync(serial, $"pm list packages{flag}", ct).ConfigureAwait(false);
        var lines = output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(l => l.Replace("package:", "", StringComparison.OrdinalIgnoreCase).Trim())
            .Where(l => l.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(l => l, StringComparer.OrdinalIgnoreCase)
            .ToList();

        _log.Info($"{lines.Count} package(s) trouvé(s).");
        return lines;
    }

    public async Task<string> ListPackagesAsync(string serial, bool thirdPartyOnly, CancellationToken ct = default)
    {
        var lines = await GetPackageListAsync(serial, thirdPartyOnly, ct).ConfigureAwait(false);
        return string.Join(Environment.NewLine, lines);
    }

    public async Task<string> InstallApkAsync(string serial, string apkPath, CancellationToken ct = default)
    {
        if (!File.Exists(apkPath))
        {
            throw new FileNotFoundException("APK introuvable.", apkPath);
        }

        _log.Info($"Installation : {Path.GetFileName(apkPath)}");
        var output = await RunAsync($"-s {serial} install -r \"{apkPath}\"", ct).ConfigureAwait(false);
        return output.Trim();
    }

    public async Task<string> UninstallPackageAsync(string serial, string packageName, CancellationToken ct = default)
    {
        _log.Info($"Désinstallation : {packageName}");
        return (await RunAsync($"-s {serial} uninstall {packageName}", ct).ConfigureAwait(false)).Trim();
    }

    public async Task<string> ForceStopAsync(string serial, string packageName, CancellationToken ct = default)
    {
        _log.Info($"Force-stop : {packageName}");
        return (await ShellAsync(serial, $"am force-stop {packageName}", ct).ConfigureAwait(false)).Trim();
    }

    public async Task<string> ClearAppDataAsync(string serial, string packageName, CancellationToken ct = default)
    {
        _log.Info($"Clear data : {packageName}");
        return (await ShellAsync(serial, $"pm clear {packageName}", ct).ConfigureAwait(false)).Trim();
    }

    public async Task<string> ClearAppCacheAsync(string serial, string packageName, CancellationToken ct = default)
    {
        _log.Info($"Clear cache : {packageName}");
        var output = await ShellAsync(serial, $"pm clear --cache-only {packageName}", ct).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(output) ||
            output.Contains("Unknown option", StringComparison.OrdinalIgnoreCase) ||
            output.Contains("Error", StringComparison.OrdinalIgnoreCase))
        {
            return $"Option --cache-only indisponible sur cet Android. Utilisez CLEAR DATA si nécessaire.\n{output.Trim()}";
        }

        return output.Trim();
    }

    public async Task<string> SetPackageEnabledAsync(string serial, string packageName, bool enabled, CancellationToken ct = default)
    {
        var cmd = enabled ? $"pm enable {packageName}" : $"pm disable-user {packageName}";
        _log.Info($"{(enabled ? "Enable" : "Disable")} : {packageName}");
        return (await ShellAsync(serial, cmd, ct).ConfigureAwait(false)).Trim();
    }

    public async Task<string> LaunchAppAsync(string serial, string packageName, CancellationToken ct = default)
    {
        _log.Info($"Launch : {packageName}");
        var output = await ShellAsync(
            serial,
            $"monkey -p {packageName} -c android.intent.category.LAUNCHER 1",
            ct).ConfigureAwait(false);
        return output.Trim();
    }

    public async Task<string> GetAppInfoAsync(string serial, string packageName, CancellationToken ct = default)
    {
        _log.Info($"App info : {packageName}");
        var path = (await ShellAsync(serial, $"pm path {packageName}", ct).ConfigureAwait(false)).Trim();
        var dumpsys = await ShellAsync(serial, $"dumpsys package {packageName}", ct).ConfigureAwait(false);
        var lines = dumpsys
            .Split('\n')
            .Select(l => l.TrimEnd())
            .Where(l =>
                l.Contains("versionName", StringComparison.OrdinalIgnoreCase) ||
                l.Contains("versionCode", StringComparison.OrdinalIgnoreCase) ||
                l.Contains("userId=", StringComparison.OrdinalIgnoreCase) ||
                l.Contains("firstInstallTime", StringComparison.OrdinalIgnoreCase) ||
                l.Contains("lastUpdateTime", StringComparison.OrdinalIgnoreCase) ||
                l.Contains("pkgFlags", StringComparison.OrdinalIgnoreCase) ||
                l.StartsWith("Package [", StringComparison.OrdinalIgnoreCase))
            .Take(40);

        return $"{path}\n{string.Join(Environment.NewLine, lines)}".Trim();
    }

    public async Task<string> PullApkAsync(string serial, string packageName, string destinationFolder, CancellationToken ct = default)
    {
        Directory.CreateDirectory(destinationFolder);
        var pathOut = (await ShellAsync(serial, $"pm path {packageName}", ct).ConfigureAwait(false)).Trim();
        var remote = pathOut
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(l => l.Replace("package:", "", StringComparison.OrdinalIgnoreCase).Trim())
            .FirstOrDefault(l => l.Length > 0);

        if (string.IsNullOrWhiteSpace(remote))
        {
            throw new InvalidOperationException($"APK introuvable pour {packageName}.");
        }

        var local = Path.Combine(destinationFolder, $"{packageName}.apk");
        _log.Info($"Pull APK : {remote} -> {local}");
        var result = await RunAsync($"-s {serial} pull \"{remote}\" \"{local}\"", ct).ConfigureAwait(false);
        return $"{result.Trim()}\nSaved: {local}";
    }

    private async Task<DeviceInfo> EnrichAsync(DeviceInfo baseInfo, CancellationToken ct)
    {
        var model = await GetPropAsync(baseInfo.Serial, "ro.product.model", ct).ConfigureAwait(false);
        var manufacturer = await GetPropAsync(baseInfo.Serial, "ro.product.manufacturer", ct).ConfigureAwait(false);
        var version = await GetPropAsync(baseInfo.Serial, "ro.build.version.release", ct).ConfigureAwait(false);
        var build = await GetPropAsync(baseInfo.Serial, "ro.build.display.id", ct).ConfigureAwait(false);
        var product = await GetPropAsync(baseInfo.Serial, "ro.product.name", ct).ConfigureAwait(false);

        return new DeviceInfo
        {
            Serial = baseInfo.Serial,
            State = baseInfo.State,
            IsAuthorized = baseInfo.IsAuthorized,
            Platform = DevicePlatform.Android,
            Model = string.IsNullOrWhiteSpace(model) ? "Inconnu" : model,
            Manufacturer = string.IsNullOrWhiteSpace(manufacturer) ? "Inconnu" : manufacturer,
            AndroidVersion = string.IsNullOrWhiteSpace(version) ? "-" : version,
            BuildId = string.IsNullOrWhiteSpace(build) ? "-" : build,
            Product = string.IsNullOrWhiteSpace(product) ? "-" : product
        };
    }

    private async Task<string> GetPropAsync(string serial, string prop, CancellationToken ct)
    {
        var value = await RunAsync($"-s {serial} shell getprop {prop}", ct).ConfigureAwait(false);
        return value.Trim();
    }

    private async Task<string> RunAsync(string arguments, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(AdbPath))
        {
            throw new InvalidOperationException("ADB n'est pas disponible.");
        }

        var psi = new ProcessStartInfo
        {
            FileName = AdbPath,
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

        if (process.ExitCode != 0 && !string.IsNullOrWhiteSpace(stderr))
        {
            _log.Warn($"adb {arguments} -> {stderr.Trim()}");
        }

        return string.IsNullOrWhiteSpace(stdout) ? stderr : stdout;
    }

    private static string? ResolveAdbPath()
    {
        var candidates = new List<string>();

        var baseDir = AppContext.BaseDirectory;
        candidates.Add(Path.Combine(baseDir, "platform-tools", "adb.exe"));
        candidates.Add(Path.Combine(baseDir, "tools", "platform-tools", "adb.exe"));

        var current = new DirectoryInfo(baseDir);
        for (var i = 0; i < 6 && current != null; i++)
        {
            candidates.Add(Path.Combine(current.FullName, "tools", "platform-tools", "adb.exe"));
            current = current.Parent;
        }

        var envAdb = Environment.GetEnvironmentVariable("ANDROID_HOME")
                     ?? Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");
        if (!string.IsNullOrWhiteSpace(envAdb))
        {
            candidates.Add(Path.Combine(envAdb, "platform-tools", "adb.exe"));
        }

        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var segment in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            candidates.Add(Path.Combine(segment.Trim('"'), "adb.exe"));
        }

        return candidates.FirstOrDefault(File.Exists);
    }
}
