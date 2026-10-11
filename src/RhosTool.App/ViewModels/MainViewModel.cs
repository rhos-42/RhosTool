using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RhosTool.App.Services;
using RhosTool.Core.Models;
using RhosTool.Core.Services;

namespace RhosTool.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly LogService _log;
    private readonly AdbService _adb;
    private readonly IosDeviceService _ios;
    private readonly FirmwarePackService _packs;
    private readonly FastbootService _fastboot;
    private readonly MtpService _mtp;
    private readonly OperationRunner _runner;
    private readonly AuthService _auth;
    private readonly DispatcherTimer _pollTimer;
    private readonly DispatcherTimer _clockTimer;
    private CancellationTokenSource? _pollCts;
    private CancellationTokenSource? _opCts;
    private bool _polling;
    private List<PhoneModel> _brandModels = new();

    public MainViewModel(AuthService auth)
    {
        _auth = auth;
        _log = new LogService();
        _adb = new AdbService(_log);
        _ios = new IosDeviceService(_log);
        _packs = new FirmwarePackService();
        _fastboot = new FastbootService(_log);
        _mtp = new MtpService(_log);
        _runner = new OperationRunner(_adb, _ios, _log, _packs, _fastboot);

        _log.LineAdded += line =>
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                LogLines.Add(line);
                while (LogLines.Count > 500)
                {
                    LogLines.RemoveAt(0);
                }
            });
        };

        Brands = new ObservableCollection<BrandModule>(BrandCatalog.All);
        SelectedBrand = Brands.FirstOrDefault(b => b.Id == "samsung") ?? Brands.First();
        PcName = Environment.MachineName;
        ApplyAuthUi();
        RefreshComPorts();
        ApplyBrandSelection(silent: true);

        _pollTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _pollTimer.Tick += async (_, _) => await PollDevicesAsync();

        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (_, _) => ClockText = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        ClockText = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
    }

    private void ApplyAuthUi()
    {
        var session = _auth.CurrentSession;
        if (session is null)
        {
            UserDisplay = Environment.UserName;
            CreditsDisplay = "Credits: —";
            ServerStatus = "Server : offline";
            return;
        }

        UserDisplay = string.IsNullOrWhiteSpace(session.Username)
            ? session.Email
            : session.Username;
        CreditsDisplay = $"Credits: {session.Credits:0.##}";
        ServerStatus = "Online : Supabase";
    }

    public ObservableCollection<BrandModule> Brands { get; }
    public ObservableCollection<string> LogLines { get; } = new();
    public ObservableCollection<PhoneModel> FilteredModels { get; } = new();
    public ObservableCollection<string> ModeTabs { get; } = new();
    public ObservableCollection<FunctionTab> FunctionTabs { get; } = new();
    public ObservableCollection<DeviceOperation> Operations { get; } = new();
    public ObservableCollection<string> ComPorts { get; } = new();

    [ObservableProperty] private BrandModule? selectedBrand;
    [ObservableProperty] private string? selectedModeTab;
    [ObservableProperty] private FunctionTab? selectedFunctionTab;
    [ObservableProperty] private PhoneModel? selectedModel;
    [ObservableProperty] private string modelFilter = string.Empty;
    [ObservableProperty] private DeviceInfo? connectedDevice;
    [ObservableProperty] private string usbStatus = "- Waiting for devices -";
    [ObservableProperty] private string connectionModeText = "Mode: —";
    [ObservableProperty] private string selectedComPort = "AUTO";
    [ObservableProperty] private bool fastConnect = true;
    [ObservableProperty] private string selectedChip = "AUTO";
    [ObservableProperty] private string creditsDisplay = "Credits: —";
    [ObservableProperty] private string userDisplay = "";
    [ObservableProperty] private string pcName = "";
    [ObservableProperty] private string serverStatus = "Server : offline";
    [ObservableProperty] private string modelCountText = "";
    [ObservableProperty] private string catalogStats = "";
    [ObservableProperty] private string clockText = "";
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private double progressValue = 100;
    [ObservableProperty] private string progressText = "100%";
    [ObservableProperty] private Brush connectionBrush = MakeBrush(0xFF, 0x98, 0x00);

    private string? _fastbootSerial;
    private int _mtpCount;

    [ObservableProperty] private string lastPackageName = "com.android.settings";
    [ObservableProperty] private string packStatusText = "Pack: —";
    [ObservableProperty] private string packPathText = "";

    public ObservableCollection<string> ChipOptions { get; } = new()
    {
        "AUTO", "Exynos", "Snapdragon", "Dimensity", "Helio", "Kirin", "Unisoc"
    };

    partial void OnSelectedBrandChanged(BrandModule? value)
    {
        if (value is null) return;
        ApplyBrandSelection();
    }

    partial void OnSelectedModeTabChanged(string? value) => _log.Info($"Mode: {value}");

    partial void OnSelectedFunctionTabChanged(FunctionTab? value)
    {
        Operations.Clear();
        if (value is null) return;
        foreach (var op in value.Operations)
        {
            Operations.Add(op);
        }
    }

    partial void OnModelFilterChanged(string value) => RefreshFilteredModels();

    public async Task InitializeAsync()
    {
        _log.Info("========================================");
        _log.Info(" RhosTool");
        _log.Info("========================================");
        CatalogStats = $"Models loaded: {ModelCatalog.Count:N0}";
        _log.Success(CatalogStats);

        ApplyAuthUi();
        if (_auth.IsAuthenticated)
        {
            _log.Success($"Connecté : {UserDisplay} | {CreditsDisplay}");
            try
            {
                await _auth.RefreshProfileAsync();
                if (_auth.CurrentSession is not null)
                {
                    LocalAuthStore.SaveSession(_auth.CurrentSession);
                }

                ApplyAuthUi();
            }
            catch (Exception ex)
            {
                _log.Warn($"Profil : {ex.Message}");
                ServerStatus = "Online : Supabase (profil indisponible)";
            }
        }
        else
        {
            _log.Warn("Non authentifié.");
        }

        if (_adb.IsAvailable)
        {
            await _adb.EnsureServerAsync();
            _log.Success("ADB server ready.");
        }
        else
        {
            _log.Warn("ADB missing — run installer/setup-platform-tools.ps1");
        }

        _clockTimer.Start();
        _pollTimer.Start();
        await PollDevicesAsync();
    }

    private void ApplyBrandSelection(bool silent = false)
    {
        if (SelectedBrand is null) return;

        ModeTabs.Clear();
        foreach (var m in SelectedBrand.ModeTabs)
        {
            ModeTabs.Add(m);
        }

        // Style UnlockTool : dernier mode (MTK UNIVERSAL / SECURITY) actif par défaut
        SelectedModeTab = ModeTabs.LastOrDefault() ?? ModeTabs.FirstOrDefault();

        FunctionTabs.Clear();
        foreach (var t in SelectedBrand.FunctionTabs)
        {
            FunctionTabs.Add(t);
        }

        // GENERAL en premier (comme le panneau 1-Click UnlockTool)
        SelectedFunctionTab = FunctionTabs.FirstOrDefault();

        _brandModels = ModelCatalog.ForBrand(SelectedBrand.Id).ToList();
        ModelCountText = $"{_brandModels.Count:N0} models";
        RefreshFilteredModels();

        ServerStatus = _auth.IsAuthenticated
            ? $"Online : Supabase | {_brandModels.Count} models"
            : $"Init : {_brandModels.Count} models";

        if (!silent)
        {
            _log.Info($"Brand selected: {SelectedBrand.Name} ({_brandModels.Count:N0} models)");
        }
    }

    private void RefreshFilteredModels()
    {
        FilteredModels.Clear();
        var filter = ModelFilter?.Trim() ?? "";
        IEnumerable<PhoneModel> q = _brandModels;
        if (!string.IsNullOrEmpty(filter))
        {
            q = q.Where(m =>
                m.DisplayName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                m.Series.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                m.MatchesCode(filter) ||
                m.Chipset.Contains(filter, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var m in q.Take(2500))
        {
            FilteredModels.Add(m);
        }

        if (SelectedModel is null || !FilteredModels.Contains(SelectedModel))
        {
            SelectedModel = FilteredModels.FirstOrDefault();
        }
    }

    private void RefreshComPorts()
    {
        ComPorts.Clear();
        ComPorts.Add("AUTO");
        try
        {
            foreach (var p in SerialPort.GetPortNames().OrderBy(x => x))
            {
                ComPorts.Add(p);
            }
        }
        catch
        {
            // ignore
        }

        SelectedComPort = "AUTO";
    }

    private async Task PollDevicesAsync()
    {
        if (_polling) return;
        _polling = true;
        _pollCts?.Cancel();
        _pollCts = new CancellationTokenSource();
        var ct = _pollCts.Token;

        try
        {
            var android = _adb.IsAvailable ? await _adb.GetDevicesAsync(ct) : Array.Empty<DeviceInfo>();
            var ios = await _ios.GetDevicesAsync(ct);
            var fbSerials = _fastboot.IsAvailable
                ? await _fastboot.GetSerialsAsync(ct)
                : Array.Empty<string>();
            _fastbootSerial = fbSerials.FirstOrDefault();
            _mtpCount = _mtp.CountDevices();

            var all = android.Concat(ios).ToList();

            if (_fastbootSerial is not null && all.Count == 0)
            {
                ConnectedDevice = null;
                UsbStatus = $"Fastboot [{_fastbootSerial}]";
                ConnectionModeText = "Mode: FASTBOOT";
                ConnectionBrush = MakeBrush(0x1E, 0x88, 0xE5);
            }
            else if (all.Count == 0)
            {
                ConnectedDevice = null;
                if (_mtpCount > 0)
                {
                    UsbStatus = $"MTP ({_mtpCount}) — pas d'ADB";
                    ConnectionModeText = "Mode: MTP";
                    ConnectionBrush = MakeBrush(0xFB, 0x8C, 0x00);
                }
                else
                {
                    UsbStatus = "- Waiting for devices -";
                    ConnectionModeText = "Mode: —";
                    ConnectionBrush = MakeBrush(0xFF, 0x98, 0x00);
                }
            }
            else
            {
                var preferred = SelectedBrand?.Id == "apple"
                    ? all.FirstOrDefault(d => d.Platform == DevicePlatform.Ios) ?? all[0]
                    : all.FirstOrDefault(d => d.Platform == DevicePlatform.Android) ?? all[0];
                ConnectedDevice = preferred;
                var state = preferred.State;
                UsbStatus = $"{preferred.DisplayName} [{preferred.Serial}] ({state})";
                ConnectionModeText = preferred.Platform == DevicePlatform.Ios
                    ? "Mode: iOS"
                    : $"Mode: ADB/{state}";
                ConnectionBrush = preferred.IsAuthorized ||
                                  state is "recovery" or "sideload"
                    ? MakeBrush(0x43, 0xA0, 0x47)
                    : MakeBrush(0xE5, 0x39, 0x35);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _log.Error($"USB: {ex.Message}");
        }
        finally
        {
            _polling = false;
        }
    }

    private string? ValidateOperationGate(DeviceOperation operation)
    {
        switch (operation.RequiredMode)
        {
            case DeviceRequiredMode.Unavailable:
                return $"ÉCHEC — {operation.Title} : module partenaire non disponible en local.";

            case DeviceRequiredMode.None:
                return null;

            case DeviceRequiredMode.AdbDevice:
                if (ConnectedDevice is null)
                    return "ÉCHEC — aucun appareil ADB. Branchez le téléphone (débogage USB ON).";
                if (ConnectedDevice.Platform != DevicePlatform.Android)
                    return "ÉCHEC — opération Android ADB uniquement.";
                if (!ConnectedDevice.IsAuthorized ||
                    !ConnectedDevice.State.Equals("device", StringComparison.OrdinalIgnoreCase))
                {
                    return $"ÉCHEC — appareil en mode « {ConnectedDevice.State} ». " +
                           "Autorisez le débogage USB (state=device) pour cette opération.";
                }

                return null;

            case DeviceRequiredMode.AdbTransport:
                if (ConnectedDevice is null)
                    return "ÉCHEC — aucun appareil ADB (device/recovery/sideload).";
                if (ConnectedDevice.Platform != DevicePlatform.Android)
                    return "ÉCHEC — opération Android ADB uniquement.";
                if (ConnectedDevice.State is not ("device" or "recovery" or "sideload"))
                {
                    return $"ÉCHEC — mode « {ConnectedDevice.State} » incompatible. " +
                           "Attendu : device, recovery ou sideload.";
                }

                return null;

            case DeviceRequiredMode.Fastboot:
                if (string.IsNullOrWhiteSpace(_fastbootSerial))
                    return "ÉCHEC — aucun appareil fastboot. REBOOT BOOTLOADER puis reconnectez.";
                return null;

            case DeviceRequiredMode.Mtp:
                if (_mtpCount <= 0)
                    return "ÉCHEC — aucun appareil MTP. Mode « Transfert de fichiers » requis.";
                return null;

            default:
                return null;
        }
    }

    [RelayCommand]
    private async Task RunOperationAsync(DeviceOperation? operation)
    {
        if (operation is null || IsBusy) return;

        if (operation.Kind is OperationKind.Reboot or OperationKind.RebootRecovery ||
            operation.Id is "factory" or "format-data" or "boot-device" or "reboot-fb" or "reboot" or "reboot-rec")
        {
            var ok = MessageBox.Show(
                operation.Id is "factory" or "format-data"
                    ? $"{operation.Title} va réinitialiser l'appareil (données perdues).\nContinuer ?"
                    : $"Confirmer {operation.Title} ?",
                "RhosTool",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (ok != MessageBoxResult.Yes) return;
        }

        string? userInput = null;
        if (operation.Kind == OperationKind.AdbAppControl)
        {
            userInput = await ResolveAppControlInputAsync(operation);
            if (userInput is null && NeedsUserInput(operation.Id))
            {
                return;
            }
        }
        else if (operation.Kind == OperationKind.FirmwarePack)
        {
            userInput = ResolveFirmwarePackInput(operation);
            if (userInput is null && operation.Id == "load-pack")
            {
                return;
            }
        }
        else if (operation.Kind == OperationKind.AdbTools)
        {
            userInput = ResolveAdbToolsInput(operation);
            if (userInput is null && AdbToolsNeedsInput(operation.Id))
            {
                return;
            }
        }
        else if (operation.Kind == OperationKind.MtpFiles)
        {
            userInput = ResolveMtpInput(operation);
            if (userInput is null && MtpNeedsInput(operation.Id))
            {
                return;
            }
        }

        IsBusy = true;
        _opCts = new CancellationTokenSource();
        ProgressValue = 0;
        ProgressText = "…";

        try
        {
            await PollDevicesAsync();

            var gate = ValidateOperationGate(operation);
            if (gate is not null)
            {
                _log.Error(gate);
                ProgressValue = 0;
                ProgressText = "FAIL";
                return;
            }

            var model = SelectedModel?.DisplayName ?? "Generic";
            var mode = ConnectionModeText;
            _log.Info($">>> {operation.Title} | {model} | {mode} | Chip={SelectedChip}");

            ProgressValue = 30;
            ProgressText = "RUN";

            OperationResult outcome;
            if (operation.Kind == OperationKind.OpenResourceLink)
            {
                outcome = OpenResourceLinkInteractive(operation);
            }
            else if (operation.Kind == OperationKind.MtpFiles)
            {
                outcome = await _mtp.RunAsync(operation.Id, userInput, _opCts.Token);
            }
            else
            {
                outcome = await _runner.RunAsync(operation, ConnectedDevice, userInput, _opCts.Token);
            }

            if (operation.Kind == OperationKind.FirmwarePack)
            {
                ApplyPackUi(_runner.LastPackManifest);
            }

            var result = outcome.Message ?? "";
            var isGuide = result.StartsWith("GUIDE", StringComparison.OrdinalIgnoreCase);

            if (!outcome.Ok)
            {
                ProgressValue = 0;
                ProgressText = "FAIL";
                foreach (var line in result.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Take(40))
                {
                    _log.Error(line);
                }

                return;
            }

            if (isGuide)
            {
                ProgressValue = 100;
                ProgressText = "INFO";
                foreach (var line in result.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Take(40))
                {
                    _log.Info(line);
                }

                return;
            }

            ProgressValue = 100;
            ProgressText = "OK";

            if (operation.Id is "list-pkgs" or "list-3rd")
            {
                var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                _log.Success($"OK — {lines.Length} package(s)");
                foreach (var line in lines.Take(80))
                {
                    _log.Info(line);
                }

                if (lines.Length > 80)
                {
                    _log.Warn($"… +{lines.Length - 80} autres");
                }
            }
            else
            {
                _log.Success($"OK — {operation.Title}");
                foreach (var line in result.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Take(40))
                {
                    _log.Info(line);
                }
            }

            await PollDevicesAsync();
        }
        catch (OperationCanceledException)
        {
            _log.Warn("Operation cancelled.");
            ProgressText = "STOP";
        }
        catch (Exception ex)
        {
            _log.Error(ex.Message);
            ProgressText = "FAIL";
            ProgressValue = 0;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private OperationResult OpenResourceLinkInteractive(DeviceOperation operation)
    {
        ResourceLinkStore.EnsureFile();
        var err = ResourceLinkStore.TryOpen(operation.Id);

        if (err == "EMPTY")
        {
            var owner = Application.Current?.MainWindow;
            var pasted = InputDialog.Prompt(
                owner!,
                "Lien MediaFire",
                $"Collez le lien MediaFire (direct ou partage) pour :\n{operation.Title}",
                ResourceLinkStore.GetUrl(operation.Id));

            if (string.IsNullOrWhiteSpace(pasted))
                return OperationResult.Fail("Annulé — aucun lien collé.");

            ResourceLinkStore.SetUrl(operation.Id, pasted);
            err = ResourceLinkStore.TryOpen(operation.Id);
        }

        if (err is null)
        {
            var url = ResourceLinkStore.GetUrl(operation.Id);
            return OperationResult.Success($"Ouverture MediaFire / lien : {operation.Title}\n{url}");
        }

        return OperationResult.Fail(err);
    }

    private static bool NeedsUserInput(string operationId) =>
        operationId is not ("list-pkgs" or "list-3rd");

    private static bool NeedsPackagePick(string operationId) =>
        operationId is not ("list-pkgs" or "list-3rd" or "install");

    private void ApplyPackUi(FirmwarePackManifest? manifest)
    {
        if (manifest is null)
        {
            PackStatusText = "Pack: —";
            PackPathText = "";
            return;
        }

        PackStatusText = $"Pack: {manifest.ShortStatus}";
        PackPathText = string.IsNullOrWhiteSpace(manifest.PackDir)
            ? manifest.SourcePath
            : manifest.PackDir;
    }

    private static bool AdbToolsNeedsInput(string id) =>
        id is "adb-connect" or "adb-push" or "adb-pull" or "adb-shell" or "adb-screencap" or "adb-bugreport";

    private static bool MtpNeedsInput(string id) =>
        id is "mtp-list" or "mtp-download" or "mtp-upload" or "mtp-mkdir" or "mtp-delete";

    private string? ResolveAdbToolsInput(DeviceOperation operation)
    {
        var owner = Application.Current?.MainWindow;
        switch (operation.Id)
        {
            case "adb-connect":
            {
                var host = Microsoft.VisualBasic.Interaction.InputBox(
                    "Adresse ADB Wi‑Fi (IP:port)",
                    "CONNECT WIFI",
                    "192.168.1.20:5555");
                return string.IsNullOrWhiteSpace(host) ? null : host.Trim();
            }

            case "adb-disconnect":
            {
                var host = Microsoft.VisualBasic.Interaction.InputBox(
                    "IP:port à déconnecter (vide = tous)",
                    "DISCONNECT",
                    "");
                return host?.Trim() ?? "";
            }

            case "adb-push":
            {
                var dlg = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Fichier à envoyer (PUSH)",
                    CheckFileExists = true
                };
                if (dlg.ShowDialog(owner) != true) return null;
                var remote = Microsoft.VisualBasic.Interaction.InputBox(
                    "Chemin distant sur le téléphone",
                    "ADB PUSH",
                    "/sdcard/Download/" + Path.GetFileName(dlg.FileName));
                if (string.IsNullOrWhiteSpace(remote)) return null;
                return $"{dlg.FileName}|{remote.Trim()}";
            }

            case "adb-pull":
            {
                var remote = Microsoft.VisualBasic.Interaction.InputBox(
                    "Chemin distant à récupérer",
                    "ADB PULL",
                    "/sdcard/Download/");
                if (string.IsNullOrWhiteSpace(remote)) return null;
                var save = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Enregistrer sous",
                    FileName = Path.GetFileName(remote.TrimEnd('/', '\\'))
                };
                if (save.ShowDialog(owner) != true) return null;
                return $"{remote.Trim()}|{save.FileName}";
            }

            case "adb-ls":
            {
                var path = Microsoft.VisualBasic.Interaction.InputBox(
                    "Dossier distant",
                    "LIST DIR",
                    "/sdcard");
                return string.IsNullOrWhiteSpace(path) ? "/sdcard" : path.Trim();
            }

            case "adb-shell":
            {
                var cmd = Microsoft.VisualBasic.Interaction.InputBox(
                    "Commande shell",
                    "ADB SHELL",
                    "getprop ro.product.model");
                return string.IsNullOrWhiteSpace(cmd) ? null : cmd.Trim();
            }

            case "adb-logcat":
            {
                var save = MessageBox.Show(
                    owner,
                    "Oui = sauver dans un fichier\nNon = afficher les dernières lignes",
                    "LOGCAT",
                    MessageBoxButton.YesNoCancel);
                if (save == MessageBoxResult.Cancel) return null;
                if (save == MessageBoxResult.No) return null;
                var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Sauver logcat",
                    Filter = "Log (*.txt)|*.txt",
                    FileName = $"logcat-{DateTime.Now:yyyyMMdd-HHmmss}.txt"
                };
                return dlg.ShowDialog(owner) == true ? dlg.FileName : null;
            }

            case "adb-screencap":
            {
                var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Capture d'écran",
                    Filter = "PNG (*.png)|*.png",
                    FileName = $"screen-{DateTime.Now:yyyyMMdd-HHmmss}.png"
                };
                return dlg.ShowDialog(owner) == true ? dlg.FileName : null;
            }

            case "adb-bugreport":
            {
                var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Bugreport",
                    Filter = "Zip (*.zip)|*.zip|Tous (*.*)|*.*",
                    FileName = $"bugreport-{DateTime.Now:yyyyMMdd-HHmmss}.zip"
                };
                return dlg.ShowDialog(owner) == true ? dlg.FileName : null;
            }

            default:
                return null;
        }
    }

    private string? ResolveMtpInput(DeviceOperation operation)
    {
        var owner = Application.Current?.MainWindow;
        switch (operation.Id)
        {
            case "mtp-devices":
                return null;

            case "mtp-list":
            {
                var path = Microsoft.VisualBasic.Interaction.InputBox(
                    "Dossier MTP (vide = racine du 1er stockage)\nEx: Internal storage\\DCIM",
                    "MTP LIST",
                    "Internal storage\\DCIM");
                return path?.Trim() ?? "";
            }

            case "mtp-download":
            {
                var remote = Microsoft.VisualBasic.Interaction.InputBox(
                    "Fichier MTP distant\nEx: Internal storage\\DCIM\\Camera\\IMG.jpg",
                    "MTP DOWNLOAD",
                    "Internal storage\\DCIM\\");
                if (string.IsNullOrWhiteSpace(remote)) return null;
                var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Enregistrer le fichier MTP",
                    FileName = Path.GetFileName(remote.TrimEnd('\\', '/'))
                };
                if (dlg.ShowDialog(owner) != true) return null;
                return $"{remote.Trim()}|{dlg.FileName}";
            }

            case "mtp-upload":
            {
                var dlg = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Fichier à envoyer via MTP",
                    CheckFileExists = true
                };
                if (dlg.ShowDialog(owner) != true) return null;
                var remote = Microsoft.VisualBasic.Interaction.InputBox(
                    "Dossier MTP de destination",
                    "MTP UPLOAD",
                    "Internal storage\\Download");
                if (string.IsNullOrWhiteSpace(remote)) return null;
                return $"{dlg.FileName}|{remote.Trim()}";
            }

            case "mtp-mkdir":
            {
                var path = Microsoft.VisualBasic.Interaction.InputBox(
                    "Dossier MTP à créer",
                    "MTP MKDIR",
                    "Internal storage\\RhosTool");
                return string.IsNullOrWhiteSpace(path) ? null : path.Trim();
            }

            case "mtp-delete":
            {
                var path = Microsoft.VisualBasic.Interaction.InputBox(
                    "Chemin MTP à supprimer",
                    "MTP DELETE",
                    "");
                if (string.IsNullOrWhiteSpace(path)) return null;
                var ok = MessageBox.Show(
                    owner,
                    $"Supprimer définitivement ?\n{path}",
                    "MTP DELETE",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                return ok == MessageBoxResult.Yes ? path.Trim() : null;
            }

            default:
                return null;
        }
    }

    private string? ResolveFirmwarePackInput(DeviceOperation operation)
    {
        var owner = Application.Current?.MainWindow;
        var brand = SelectedBrand?.Id ?? "unknown";
        var model = SelectedModel?.ModelCode ?? SelectedModel?.DisplayName ?? "generic";

        switch (operation.Id)
        {
            case "load-pack":
            {
                var choice = MessageBox.Show(
                    owner,
                    "Oui = fichier (ZIP/TAR)\nNon = dossier firmware\nAnnuler = abandon",
                    "LOAD PACK",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (choice == MessageBoxResult.Cancel) return null;

                if (choice == MessageBoxResult.Yes)
                {
                    var dlg = new Microsoft.Win32.OpenFileDialog
                    {
                        Title = "Sélectionner un paquet firmware",
                        Filter =
                            "Firmware (*.zip;*.tar;*.tar.md5)|*.zip;*.tar;*.tar.md5|Tous les fichiers (*.*)|*.*",
                        CheckFileExists = true
                    };
                    if (dlg.ShowDialog(owner) != true) return null;
                    var extract = dlg.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ? "1" : "0";
                    return $"{dlg.FileName}|{brand}|{model}|{extract}";
                }

                var folderDlg = new Microsoft.Win32.OpenFolderDialog
                {
                    Title = "Sélectionner un dossier firmware"
                };
                if (folderDlg.ShowDialog(owner) != true) return null;
                return $"{folderDlg.FolderName}|{brand}|{model}|0";
            }

            case "validate-pack":
            {
                if (_runner.LastPackManifest is not null)
                    return null; // runner utilisera le dernier pack

                var dlg = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Valider un paquet (fichier)",
                    Filter =
                        "Firmware (*.zip;*.tar;*.tar.md5)|*.zip;*.tar;*.tar.md5|Tous les fichiers (*.*)|*.*",
                    CheckFileExists = true
                };
                return dlg.ShowDialog(owner) == true ? dlg.FileName : null;
            }

            case "show-manifest":
                return null;

            default:
                return null;
        }
    }

    private async Task<string?> ResolveAppControlInputAsync(DeviceOperation operation)
    {
        var owner = Application.Current?.MainWindow!;

        switch (operation.Id)
        {
            case "list-pkgs":
            case "list-3rd":
                return null;

            case "install":
            {
                var dlg = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Sélectionner un APK",
                    Filter = "Android Package (*.apk)|*.apk|Tous les fichiers (*.*)|*.*",
                    CheckFileExists = true
                };
                return dlg.ShowDialog(owner) == true ? dlg.FileName : null;
            }

            case "pull-apk":
            {
                var package = await PickPackageAsync(owner, operation.Title);
                if (string.IsNullOrWhiteSpace(package)) return null;
                LastPackageName = package;

                var folderDlg = new Microsoft.Win32.OpenFolderDialog
                {
                    Title = "Dossier de destination pour l'APK"
                };
                if (folderDlg.ShowDialog(owner) != true)
                {
                    return null;
                }

                return $"{package}|{folderDlg.FolderName}";
            }

            case "uninstall":
            case "clear-data":
            case "disable-app":
            {
                var package = await PickPackageAsync(owner, operation.Title);
                if (string.IsNullOrWhiteSpace(package)) return null;

                var confirm = MessageBox.Show(
                    $"{operation.Title} pour\n{package} ?",
                    "RhosTool",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (confirm != MessageBoxResult.Yes) return null;

                LastPackageName = package;
                return package;
            }

            default:
            {
                if (!NeedsPackagePick(operation.Id))
                {
                    return null;
                }

                var package = await PickPackageAsync(owner, operation.Title);
                if (string.IsNullOrWhiteSpace(package)) return null;
                LastPackageName = package;
                return package;
            }
        }
    }

    private async Task<string?> PickPackageAsync(Window owner, string title)
    {
        var serial = ConnectedDevice?.Serial;
        if (string.IsNullOrWhiteSpace(serial) || ConnectedDevice?.Platform != DevicePlatform.Android)
        {
            return InputDialog.Prompt(
                owner,
                "ADB App Control",
                $"{title}\nNom du package (ex: com.android.settings) :",
                LastPackageName);
        }

        try
        {
            _log.Info("Chargement des packages…");
            var third = await _adb.GetPackageListAsync(serial, thirdPartyOnly: true);
            var all = await _adb.GetPackageListAsync(serial, thirdPartyOnly: false);

            var picker = new PackagePickerWindow(
                owner,
                title,
                all,
                third,
                LastPackageName,
                preferThirdParty: true);

            if (picker.ShowDialog() != true)
            {
                return null;
            }

            if (picker.UseManualEntry)
            {
                return InputDialog.Prompt(
                    owner,
                    "ADB App Control",
                    $"{title}\nNom du package :",
                    LastPackageName);
            }

            return picker.SelectedPackage;
        }
        catch (Exception ex)
        {
            _log.Warn($"Liste packages indisponible : {ex.Message}");
            return InputDialog.Prompt(
                owner,
                "ADB App Control",
                $"{title}\nNom du package :",
                LastPackageName);
        }
    }

    [RelayCommand]
    private void Stop()
    {
        _opCts?.Cancel();
        ProgressText = "STOP";
        _log.Warn("STOP pressed.");
    }

    [RelayCommand]
    private async Task RefreshNowAsync()
    {
        // Bouton FASTBOOT de la barre utilitaire
        RefreshComPorts();
        var serials = await _fastboot.GetSerialsAsync();
        var fb = await _fastboot.DevicesAsync();
        foreach (var line in fb.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            _log.Info(line);
        }

        await PollDevicesAsync();
        if (serials.Count == 0)
            _log.Warn("Fastboot refresh — aucun appareil.");
        else
            _log.Success($"Fastboot OK — {serials.Count} appareil(s).");
    }

    [RelayCommand]
    private void OpenDeviceManager()
    {
        Process.Start(new ProcessStartInfo { FileName = "devmgmt.msc", UseShellExecute = true });
        _log.Info("Device Manager opened.");
    }

    [RelayCommand]
    private async Task CheckAdbAsync()
    {
        var status = await _adb.StatusAsync();
        foreach (var line in status.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            _log.Info(line);
        }

        await PollDevicesAsync();
        if (!_adb.IsAvailable)
        {
            _log.Error("ADB introuvable.");
        }
        else if (ConnectedDevice is null)
        {
            _log.Warn("ADB OK — aucun appareil branché.");
        }
        else
        {
            _log.Success($"ADB OK — {ConnectedDevice.Serial} ({ConnectedDevice.State}).");
        }
    }

    private const string WebsiteUrl = "https://github.com/rhos-42/RhosTool/releases/tag/v1.0.0";

    [RelayCommand]
    private void OpenConfig()
    {
        var session = _auth.CurrentSession;
        var configPath = LocalAuthStore.ConfigPath;
        var info =
            $"Compte : {session?.Email ?? "—"}\n" +
            $"User : {UserDisplay}\n" +
            $"{CreditsDisplay}\n" +
            $"Serveur : {ServerStatus}\n\n" +
            $"Config : {configPath}\n" +
            $"Site web : {WebsiteUrl}\n\n" +
            "Oui = Déconnexion\nNon = Ouvrir le dossier config\nAnnuler = Ouvrir le site web";

        var result = MessageBox.Show(
            info,
            "RhosTool Config",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Information);

        if (result == MessageBoxResult.Yes)
        {
            _auth.Logout();
            LocalAuthStore.ClearSession();
            _log.Warn("Déconnexion — redémarrage login.");
            RestartToLogin();
            return;
        }

        if (result == MessageBoxResult.No)
        {
            LocalAuthStore.LoadOrCreateConfig();
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = LocalAuthStore.AppDataDir,
                UseShellExecute = true
            });
            _log.Info($"Config folder: {LocalAuthStore.AppDataDir}");
            return;
        }

        if (result == MessageBoxResult.Cancel)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = WebsiteUrl,
                    UseShellExecute = true
                });
                _log.Info($"Site web: {WebsiteUrl}");
            }
            catch (Exception ex)
            {
                _log.Warn($"Impossible d'ouvrir le site : {ex.Message}");
            }
        }
    }

    private static void RestartToLogin()
    {
        var exe = Environment.ProcessPath
                    ?? Path.Combine(AppContext.BaseDirectory, "RhosTool.exe");
        var dll = Path.Combine(AppContext.BaseDirectory, "RhosTool.dll");

        try
        {
            if (File.Exists(dll) && (exe.EndsWith("dotnet.exe", StringComparison.OrdinalIgnoreCase) ||
                                     !File.Exists(exe)))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"\"{dll}\"",
                    WorkingDirectory = AppContext.BaseDirectory,
                    UseShellExecute = true
                });
            }
            else
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = exe,
                    WorkingDirectory = AppContext.BaseDirectory,
                    UseShellExecute = true
                });
            }
        }
        catch
        {
            // ignore
        }

        Application.Current.Shutdown();
    }

    private static SolidColorBrush MakeBrush(byte r, byte g, byte b)
    {
        var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }
}
