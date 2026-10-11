using System.Diagnostics;
using System.IO;
using System.Text;
using RhosTool.Core.Models;

namespace RhosTool.Core.Services;

public sealed class OperationRunner
{
    private readonly AdbService _adb;
    private readonly IosDeviceService _ios;
    private readonly LogService _log;
    private readonly FirmwarePackService _packs;
    private readonly FastbootService _fastboot;

    public FirmwarePackManifest? LastPackManifest { get; private set; }

    public OperationRunner(
        AdbService adb,
        IosDeviceService ios,
        LogService log,
        FirmwarePackService? packs = null,
        FastbootService? fastboot = null)
    {
        _adb = adb;
        _ios = ios;
        _log = log;
        _packs = packs ?? new FirmwarePackService();
        _fastboot = fastboot ?? new FastbootService(log);
    }

    public async Task<OperationResult> RunAsync(
        DeviceOperation operation,
        DeviceInfo? device,
        string? userInput = null,
        CancellationToken ct = default)
    {
        if (operation.RequiredMode == DeviceRequiredMode.Unavailable ||
            operation.Kind == OperationKind.PartnerStub)
        {
            return OperationResult.Fail(PartnerMessage(operation.Title));
        }

        if (operation.RequiresDevice && device is null)
        {
            _log.Warn("Aucun appareil connecté pour cette opération.");
            return OperationResult.Fail("ÉCHEC — aucun appareil connecté.");
        }

        try
        {
            switch (operation.Kind)
            {
                case OperationKind.ReadInfo:
                    return OperationResult.Success(await ReadInfoAsync(device!, ct).ConfigureAwait(false));

                case OperationKind.Reboot:
                    await _adb.RebootAsync(device!.Serial, null, ct).ConfigureAwait(false);
                    return OperationResult.Success("Redémarrage demandé.");

                case OperationKind.RebootRecovery:
                    await _adb.RebootAsync(device!.Serial, "recovery", ct).ConfigureAwait(false);
                    return OperationResult.Success("Redémarrage recovery demandé.");

                case OperationKind.OpenDeviceManager:
                    return OperationResult.Success(OpenDeviceManager());

                case OperationKind.AdbAppControl:
                    return Wrap(await RunAppControlAsync(operation.Id, device!, userInput, ct).ConfigureAwait(false));

                case OperationKind.FirmwarePack:
                    return Wrap(await RunFirmwarePackAsync(operation.Id, userInput, ct).ConfigureAwait(false));

                case OperationKind.DeviceAction:
                    return await RunDeviceActionAsync(operation.Id, device, userInput, ct).ConfigureAwait(false);

                case OperationKind.AdbTools:
                    return await RunAdbToolsAsync(operation.Id, device, userInput, ct).ConfigureAwait(false);

                case OperationKind.MtpFiles:
                    return OperationResult.Fail("MTP est géré par la couche Windows (MainViewModel).");

                case OperationKind.OpenResourceLink:
                    return OperationResult.Fail("Les liens ressources sont ouverts par l'interface (MainViewModel).");

                default:
                    return OperationResult.Fail("Opération inconnue.");
            }
        }
        catch (InvalidOperationException ex)
        {
            return OperationResult.Fail($"ÉCHEC — {ex.Message}");
        }
    }

    private static OperationResult Wrap(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return OperationResult.Fail("ÉCHEC — aucune réponse.");

        if (message.StartsWith("Aucun", StringComparison.OrdinalIgnoreCase) ||
            message.StartsWith("ÉCHEC", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("introuvable", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("non exécuté", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("réservé aux", StringComparison.OrdinalIgnoreCase))
        {
            return OperationResult.Fail(message);
        }

        return OperationResult.Success(message);
    }

    private async Task<OperationResult> RunAdbToolsAsync(
        string operationId,
        DeviceInfo? device,
        string? userInput,
        CancellationToken ct)
    {
        switch (operationId)
        {
            case "adb-status":
            case "adb-devices":
            {
                var status = await _adb.StatusAsync(ct).ConfigureAwait(false);
                if (status.Contains("ADB introuvable", StringComparison.OrdinalIgnoreCase))
                    return OperationResult.Fail(status);
                if (status.Contains("Appareils: 0", StringComparison.Ordinal))
                    return OperationResult.Fail(status + "\nÉCHEC — branchez un appareil (ADB).");
                return OperationResult.Success(status);
            }

            case "adb-wait":
                return Wrap(await _adb.WaitForDeviceAsync(ct).ConfigureAwait(false));

            case "adb-connect":
                return Wrap(await _adb.ConnectAsync(userInput ?? "", ct).ConfigureAwait(false));

            case "adb-disconnect":
                return Wrap(await _adb.DisconnectAsync(userInput, ct).ConfigureAwait(false));

            case "adb-tcpip":
                return OperationResult.Success(
                    await _adb.EnableTcpIpAsync(RequireAndroid(device).Serial, 5555, ct).ConfigureAwait(false));

            case "adb-state":
                return OperationResult.Success(
                    await _adb.GetStateAsync(RequireAndroid(device).Serial, ct).ConfigureAwait(false));

            case "adb-push":
            {
                var parts = (userInput ?? "").Split('|', 2);
                if (parts.Length < 2) return OperationResult.Fail("Usage push : local|remote");
                return OperationResult.Success(
                    await _adb.PushAsync(RequireAndroid(device).Serial, parts[0], parts[1], ct)
                        .ConfigureAwait(false));
            }

            case "adb-pull":
            {
                var parts = (userInput ?? "").Split('|', 2);
                if (parts.Length < 2) return OperationResult.Fail("Usage pull : remote|local");
                return OperationResult.Success(
                    await _adb.PullAsync(RequireAndroid(device).Serial, parts[0], parts[1], ct)
                        .ConfigureAwait(false));
            }

            case "adb-ls":
                return OperationResult.Success(
                    await _adb.ListRemoteDirAsync(RequireAndroid(device).Serial, userInput ?? "/sdcard", ct)
                        .ConfigureAwait(false));

            case "adb-shell":
                return OperationResult.Success(
                    await _adb.ShellCommandAsync(RequireAndroid(device).Serial, userInput ?? "", ct)
                        .ConfigureAwait(false));

            case "adb-logcat":
                return OperationResult.Success(
                    await _adb.LogcatDumpAsync(RequireAndroid(device).Serial, userInput, ct)
                        .ConfigureAwait(false));

            case "adb-screencap":
                if (string.IsNullOrWhiteSpace(userInput))
                    return OperationResult.Fail("Chemin PNG local requis.");
                return OperationResult.Success(
                    await _adb.ScreenshotAsync(RequireAndroid(device).Serial, userInput, ct)
                        .ConfigureAwait(false));

            case "adb-bugreport":
                if (string.IsNullOrWhiteSpace(userInput))
                    return OperationResult.Fail("Chemin ZIP/local requis.");
                return OperationResult.Success(
                    await _adb.BugreportAsync(RequireAndroid(device).Serial, userInput, ct)
                        .ConfigureAwait(false));

            case "adb-root":
                return OperationResult.Success(
                    await _adb.RootAsync(RequireAndroid(device).Serial, ct).ConfigureAwait(false));

            case "adb-unroot":
                return OperationResult.Success(
                    await _adb.UnrootAsync(RequireAndroid(device).Serial, ct).ConfigureAwait(false));

            case "adb-remount":
                return OperationResult.Success(
                    await _adb.RemountAsync(RequireAndroid(device).Serial, ct).ConfigureAwait(false));

            case "adb-sideload":
                return OperationResult.Success(
                    await _adb.RebootSideloadAsync(RequireAndroid(device).Serial, ct).ConfigureAwait(false));

            case "adb-sync":
                return OperationResult.Success(
                    await _adb.SyncAsync(RequireAndroid(device).Serial, ct).ConfigureAwait(false));

            case "adb-forward-list":
                return OperationResult.Success(
                    await _adb.ForwardListAsync(RequireAndroid(device).Serial, ct).ConfigureAwait(false));

            case "adb-reboot":
                await _adb.RebootAsync(RequireAndroid(device).Serial, null, ct).ConfigureAwait(false);
                return OperationResult.Success("Redémarrage demandé.");

            case "adb-reboot-recovery":
                await _adb.RebootAsync(RequireAndroid(device).Serial, "recovery", ct).ConfigureAwait(false);
                return OperationResult.Success("Recovery demandé.");

            case "adb-reboot-bootloader":
                await _adb.RebootAsync(RequireAndroid(device).Serial, "bootloader", ct).ConfigureAwait(false);
                return OperationResult.Success("Bootloader demandé.");

            default:
                return OperationResult.Fail($"Outil ADB inconnu : {operationId}");
        }
    }

    private async Task<OperationResult> RunDeviceActionAsync(
        string operationId,
        DeviceInfo? device,
        string? userInput,
        CancellationToken ct)
    {
        switch (operationId)
        {
            case "enable-adb":
            {
                var status = await _adb.StatusAsync(ct).ConfigureAwait(false);
                if (status.Contains("ADB introuvable", StringComparison.OrdinalIgnoreCase) ||
                    status.Contains("Appareils: 0", StringComparison.Ordinal))
                    return OperationResult.Fail(status + "\nÉCHEC — aucun appareil ADB.");
                return OperationResult.Success(status);
            }

            case "boot-device":
            case "reboot":
                await _adb.RebootAsync(RequireAndroid(device).Serial, null, ct).ConfigureAwait(false);
                return OperationResult.Success("Redémarrage demandé (BOOT DEVICE).");

            case "reboot-rec":
                await _adb.RebootAsync(RequireAndroid(device).Serial, "recovery", ct).ConfigureAwait(false);
                return OperationResult.Success("Redémarrage recovery demandé.");

            case "reboot-fb":
                await _adb.RebootAsync(RequireAndroid(device).Serial, "bootloader", ct).ConfigureAwait(false);
                return OperationResult.Success(
                    "Redémarrage bootloader / fastboot demandé.\nUtilisez FASTBOOT pour vérifier l'appareil.");

            case "factory":
            case "format-data":
                return OperationResult.Success(
                    await _adb.FactoryResetAsync(RequireAndroid(device).Serial, ct).ConfigureAwait(false));

            case "read-info":
            case "imei":
                return OperationResult.Success(await ReadInfoAsync(device!, ct).ConfigureAwait(false));

            case "screen-lock":
                return OperationResult.Success(
                    await _adb.OpenSecuritySettingsAsync(RequireAndroid(device).Serial, ct)
                        .ConfigureAwait(false));

            case "drivers":
                return OperationResult.Success(OpenDeviceManager());

            case "enter-dl":
            case "edl":
            case "edl-brom":
            case "force-brom":
            case "mtk-preloader":
            case "mtk-meta":
                // Guide uniquement — journalisé en INFO (pas un succès matériel)
                return OperationResult.Success("GUIDE\n" + DownloadModeGuide());

            case "flash":
            case "flash-back":
                return Wrap(PrepareFlash());

            case "fastboot-devices":
            {
                var fb = await _fastboot.DevicesAsync(ct).ConfigureAwait(false);
                if (fb.Contains("Aucun appareil fastboot", StringComparison.OrdinalIgnoreCase) ||
                    fb.Contains("introuvable", StringComparison.OrdinalIgnoreCase))
                    return OperationResult.Fail(fb);
                return OperationResult.Success(fb);
            }

            case "fb-unlock-status":
            {
                var serials = await _fastboot.GetSerialsAsync(ct).ConfigureAwait(false);
                if (serials.Count == 0)
                    return OperationResult.Fail("ÉCHEC — aucun appareil fastboot.");
                var devices = await _fastboot.DevicesAsync(ct).ConfigureAwait(false);
                var unlocked = await _fastboot.GetVarAsync("unlocked", ct).ConfigureAwait(false);
                var ability = await _fastboot.GetVarAsync("unlock_ability", ct).ConfigureAwait(false);
                return OperationResult.Success(
                    $"{devices}\n\nunlocked:\n{unlocked}\n\nunlock_ability:\n{ability}");
            }

            default:
                return OperationResult.Fail(PartnerMessage(operationId));
        }
    }

    private string PrepareFlash()
    {
        if (LastPackManifest is null)
        {
            return "Aucun paquet chargé.\n1) LOAD PACK\n2) VALIDATE PACK\n3) Relancez FLASH.\n" +
                   "Le flash bas niveau (Odin / SP Flash / Firehose) reste un service partenaire.";
        }

        var m = LastPackManifest;
        var sb = new StringBuilder();
        sb.AppendLine("Paquet prêt pour flash atelier :");
        sb.AppendLine($"  Format : {m.FormatKey}");
        sb.AppendLine($"  Status : {(m.RequiredOk ? "OK" : "INCOMPLET")}");
        sb.AppendLine($"  Fichiers : {m.Partitions.Count}");
        if (!string.IsNullOrWhiteSpace(m.PackDir))
            sb.AppendLine($"  Dossier : {m.PackDir}");
        sb.AppendLine();
        sb.AppendLine("RhosTool prépare et valide le pack localement.");
        sb.AppendLine("L'écriture partitions (Odin / SP Flash / EDL) nécessite le module partenaire.");
        foreach (var p in m.Partitions.Take(15))
        {
            sb.AppendLine($"  - {p.Name}: {p.File} ({p.Size:N0} o)");
        }

        if (m.Partitions.Count > 15)
            sb.AppendLine($"  … +{m.Partitions.Count - 15}");

        return sb.ToString().TrimEnd();
    }

    private static string DownloadModeGuide() =>
        """
        Mode Download / Odin / BROM — guide :
        1) Éteindre le téléphone.
        2) Samsung : Volume Bas + Bixby/Vol Haut + USB (selon modèle).
        3) MTK : Vol+ / Vol- + USB (preloader/BROM) — driver USB requis.
        4) Qualcomm EDL : test point ou cable EDL selon modèle.
        5) Vérifiez le port COM dans RhosTool, puis LOAD PACK.
        Le flash bas niveau reste un module partenaire.
        """;

    private string PartnerMessage(string title)
    {
        _log.Warn($"{title} : module partenaire / non disponible en local.");
        return $"ÉCHEC — {title} non disponible en local.\n" +
               "Module partenaire requis (FRP / auth / NV / RPMB / réseau / flash bas niveau).\n" +
               "Opérations locales : ADB, READ INFO, REBOOT, FACTORY RESET, packs, MTP, fastboot devices.";
    }

    private string OpenDeviceManager()
    {
        _log.Info("Ouverture du Gestionnaire de périphériques…");
        Process.Start(new ProcessStartInfo
        {
            FileName = "devmgmt.msc",
            UseShellExecute = true
        });
        return "Gestionnaire de périphériques ouvert.";
    }

    private static DeviceInfo RequireAndroid(DeviceInfo? device)
    {
        if (device is null)
            throw new InvalidOperationException("Aucun appareil connecté.");
        if (device.Platform == DevicePlatform.Ios)
            throw new InvalidOperationException("Opération réservée à Android (ADB).");
        return device;
    }

    private async Task<string> RunFirmwarePackAsync(string operationId, string? userInput, CancellationToken ct)
    {
        switch (operationId)
        {
            case "load-pack":
            {
                if (string.IsNullOrWhiteSpace(userInput))
                    return "Aucun fichier/dossier sélectionné.";

                var parts = userInput.Split('|');
                var path = parts.ElementAtOrDefault(0)?.Trim() ?? "";
                var brand = parts.ElementAtOrDefault(1);
                var model = parts.ElementAtOrDefault(2);
                var extract = parts.ElementAtOrDefault(3) != "0";

                _log.Info($"Ingestion paquet: {path}");
                var manifest = await _packs.IngestAsync(path, brand, model, extract, ct).ConfigureAwait(false);
                LastPackManifest = manifest;
                return _packs.FormatSummary(manifest);
            }

            case "validate-pack":
            {
                FirmwarePackManifest manifest;
                if (!string.IsNullOrWhiteSpace(userInput) &&
                    (File.Exists(userInput) || Directory.Exists(userInput)))
                {
                    manifest = await _packs.ValidateOnlyAsync(userInput, ct: ct).ConfigureAwait(false);
                }
                else if (LastPackManifest is not null)
                {
                    if (!string.IsNullOrWhiteSpace(LastPackManifest.PackDir) &&
                        Directory.Exists(LastPackManifest.PackDir))
                    {
                        manifest = _packs.LoadManifest(LastPackManifest.PackDir) ?? LastPackManifest;
                    }
                    else
                    {
                        manifest = await _packs.ValidateOnlyAsync(LastPackManifest.SourcePath, ct: ct)
                            .ConfigureAwait(false);
                    }
                }
                else
                {
                    return "Aucun paquet chargé. Utilisez LOAD PACK d'abord.";
                }

                LastPackManifest = manifest;
                return _packs.FormatSummary(manifest);
            }

            case "show-manifest":
            {
                if (LastPackManifest is null)
                    return "Aucun manifeste en mémoire. Utilisez LOAD PACK d'abord.";

                if (!string.IsNullOrWhiteSpace(LastPackManifest.PackDir))
                {
                    var loaded = _packs.LoadManifest(LastPackManifest.PackDir);
                    if (loaded is not null) LastPackManifest = loaded;
                }

                return _packs.FormatSummary(LastPackManifest);
            }

            default:
                return "Opération paquet inconnue.";
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
                    return "Aucun fichier APK sélectionné.";
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
                    return "Package et dossier de destination requis.";
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
            throw new InvalidOperationException("Nom de package requis (ex: com.android.settings).");
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

        var detailed = await _adb.GetDetailedInfoAsync(device.Serial, ct).ConfigureAwait(false);
        _log.Success($"Infos lues : {device.DisplayName}");
        return detailed;
    }
}
