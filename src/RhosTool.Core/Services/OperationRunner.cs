using System.Diagnostics;
using RhosTool.Core.Models;

namespace RhosTool.Core.Services;

public sealed class OperationRunner
{
    private readonly AdbService _adb;
    private readonly IosDeviceService _ios;
    private readonly LogService _log;

    public OperationRunner(AdbService adb, IosDeviceService ios, LogService log)
    {
        _adb = adb;
        _ios = ios;
        _log = log;
    }

    public async Task<string> RunAsync(
        DeviceOperation operation,
        DeviceInfo? device,
        string? userInput = null,
        CancellationToken ct = default)
    {
        if (operation.RequiresDevice && device is null)
        {
            _log.Warn("Aucun appareil connecté pour cette opération.");
            return "Aucun appareil connecté.";
        }

        switch (operation.Kind)
        {
            case OperationKind.ReadInfo:
                return await ReadInfoAsync(device!, ct).ConfigureAwait(false);

            case OperationKind.Reboot:
                await _adb.RebootAsync(device!.Serial, null, ct).ConfigureAwait(false);
                return "Redémarrage demandé.";

            case OperationKind.RebootRecovery:
                await _adb.RebootAsync(device!.Serial, "recovery", ct).ConfigureAwait(false);
                return "Redémarrage recovery demandé.";

            case OperationKind.OpenDeviceManager:
                _log.Info("Ouverture du Gestionnaire de périphériques…");
                Process.Start(new ProcessStartInfo
                {
                    FileName = "devmgmt.msc",
                    UseShellExecute = true
                });
                return "Gestionnaire de périphériques ouvert.";

            case OperationKind.PartnerStub:
                _log.Warn($"{operation.Title} : nécessite un partenaire licencié. Non exécuté localement.");
                return "Opération partenaire non disponible dans cette version locale.";

            case OperationKind.AdbAppControl:
                return await RunAppControlAsync(operation.Id, device!, userInput, ct).ConfigureAwait(false);

            default:
                return "Opération inconnue.";
        }
    }

    private async Task<string> RunAppControlAsync(
        string operationId,
        DeviceInfo device,
        string? userInput,
        CancellationToken ct)
    {
        if (device.Platform == DevicePlatform.Ios)
        {
            return "ADB App Control est réservé aux appareils Android.";
        }

        switch (operationId)
        {
            case "list-pkgs":
                return await _adb.ListPackagesAsync(device.Serial, thirdPartyOnly: false, ct).ConfigureAwait(false);

            case "list-3rd":
                return await _adb.ListPackagesAsync(device.Serial, thirdPartyOnly: true, ct).ConfigureAwait(false);

            case "install":
                if (string.IsNullOrWhiteSpace(userInput))
                {
                    return "Aucun fichier APK sélectionné.";
                }

                return await _adb.InstallApkAsync(device.Serial, userInput, ct).ConfigureAwait(false);

            case "uninstall":
                return await _adb.UninstallPackageAsync(device.Serial, RequirePackage(userInput), ct).ConfigureAwait(false);

            case "force-stop":
                return await _adb.ForceStopAsync(device.Serial, RequirePackage(userInput), ct).ConfigureAwait(false);

            case "clear-data":
                return await _adb.ClearAppDataAsync(device.Serial, RequirePackage(userInput), ct).ConfigureAwait(false);

            case "clear-cache":
                return await _adb.ClearAppCacheAsync(device.Serial, RequirePackage(userInput), ct).ConfigureAwait(false);

            case "enable-app":
                return await _adb.SetPackageEnabledAsync(device.Serial, RequirePackage(userInput), enabled: true, ct)
                    .ConfigureAwait(false);

            case "disable-app":
                return await _adb.SetPackageEnabledAsync(device.Serial, RequirePackage(userInput), enabled: false, ct)
                    .ConfigureAwait(false);

            case "launch-app":
                return await _adb.LaunchAppAsync(device.Serial, RequirePackage(userInput), ct).ConfigureAwait(false);

            case "app-info":
                return await _adb.GetAppInfoAsync(device.Serial, RequirePackage(userInput), ct).ConfigureAwait(false);

            case "pull-apk":
                if (string.IsNullOrWhiteSpace(userInput) || !userInput.Contains('|'))
                {
                    return "Package et dossier de destination requis.";
                }

                {
                    var parts = userInput.Split('|', 2);
                    return await _adb.PullApkAsync(device.Serial, RequirePackage(parts[0]), parts[1], ct)
                        .ConfigureAwait(false);
                }

            default:
                return $"App Control inconnu : {operationId}";
        }
    }

    private static string RequirePackage(string? packageName)
    {
        if (string.IsNullOrWhiteSpace(packageName))
        {
            throw new InvalidOperationException("Nom de package requis (ex: com.android.settings).");
        }

        return packageName.Trim();
    }

    private async Task<string> ReadInfoAsync(DeviceInfo device, CancellationToken ct)
    {
        if (device.Platform == DevicePlatform.Ios)
        {
            var list = await _ios.GetDevicesAsync(ct).ConfigureAwait(false);
            var fresh = list.FirstOrDefault() ?? device;
            var text =
                $"Plateforme : iOS\n" +
                $"Modèle : {fresh.Model}\n" +
                $"Fabricant : {fresh.Manufacturer}\n" +
                $"Version : {fresh.AndroidVersion}\n" +
                $"Build : {fresh.BuildId}\n" +
                $"UDID : {fresh.Serial}";
            _log.Success("Infos iOS lues.");
            return text;
        }

        var android = await _adb.RefreshDeviceAsync(device.Serial, ct).ConfigureAwait(false) ?? device;
        var summary =
            $"Plateforme : Android\n" +
            $"Fabricant : {android.Manufacturer}\n" +
            $"Modèle : {android.Model}\n" +
            $"Produit : {android.Product}\n" +
            $"Android : {android.AndroidVersion}\n" +
            $"Build : {android.BuildId}\n" +
            $"Série : {android.Serial}\n" +
            $"État : {android.State}";
        _log.Success($"Infos lues : {android.DisplayName}");
        return summary;
    }
}
