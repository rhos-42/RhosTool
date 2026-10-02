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
        _runner = new OperationRunner(_adb, _ios, _log);

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

    [ObservableProperty] private string lastPackageName = "com.android.settings";

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

        SelectedModeTab = ModeTabs.LastOrDefault() ?? ModeTabs.FirstOrDefault();

        FunctionTabs.Clear();
        foreach (var t in SelectedBrand.FunctionTabs)
        {
            FunctionTabs.Add(t);
        }

        SelectedFunctionTab = FunctionTabs.LastOrDefault() ?? FunctionTabs.FirstOrDefault();

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
                m.ModelCode.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
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
            var all = android.Concat(ios).ToList();

            if (all.Count == 0)
            {
                ConnectedDevice = null;
                UsbStatus = "- Waiting for devices -";
                ConnectionBrush = MakeBrush(0xFF, 0x98, 0x00);
            }
            else
            {
                var preferred = SelectedBrand?.Id == "apple"
                    ? all.FirstOrDefault(d => d.Platform == DevicePlatform.Ios) ?? all[0]
                    : all.FirstOrDefault(d => d.Platform == DevicePlatform.Android) ?? all[0];
                ConnectedDevice = preferred;
                UsbStatus = $"{preferred.DisplayName} [{preferred.Serial}]";
                ConnectionBrush = preferred.IsAuthorized
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

    [RelayCommand]
    private async Task RunOperationAsync(DeviceOperation? operation)
    {
        if (operation is null || IsBusy) return;

        if (operation.Kind is OperationKind.Reboot or OperationKind.RebootRecovery)
        {
            var ok = MessageBox.Show(
                $"Confirm {operation.Title} ?",
                "RhosTool",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (ok != MessageBoxResult.Yes) return;
        }

        string? userInput = null;
        if (operation.Kind == OperationKind.AdbAppControl)
        {
            userInput = ResolveAppControlInput(operation);
            if (userInput is null && NeedsUserInput(operation.Id))
            {
                return;
            }
        }

        IsBusy = true;
        _opCts = new CancellationTokenSource();
        ProgressValue = 0;
        ProgressText = "0%";

        try
        {
            var model = SelectedModel?.DisplayName ?? "Generic";
            _log.Info($">>> {operation.Title} | {model} | Chip={SelectedChip} | COM={SelectedComPort}");

            for (var i = 10; i <= 90; i += 20)
            {
                if (_opCts.IsCancellationRequested) break;
                ProgressValue = i;
                ProgressText = $"{i}%";
                await Task.Delay(80, _opCts.Token);
            }

            if (_opCts.IsCancellationRequested)
            {
                _log.Warn("Operation cancelled.");
                ProgressText = "STOP";
                return;
            }

            var result = await _runner.RunAsync(operation, ConnectedDevice, userInput, _opCts.Token);
            ProgressValue = 100;
            ProgressText = "100%";

            // Listes packages : journaliser ligne par ligne (tronqué)
            if (operation.Id is "list-pkgs" or "list-3rd")
            {
                var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                _log.Success($"{lines.Length} package(s)");
                foreach (var line in lines.Take(80))
                {
                    _log.Info(line);
                }

                if (lines.Length > 80)
                {
                    _log.Warn($"… +{lines.Length - 80} autres (voir sortie complète dans le journal tronqué)");
                }
            }
            else
            {
                foreach (var line in result.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Take(30))
                {
                    _log.Success(line);
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
            ProgressText = "ERR";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static bool NeedsUserInput(string operationId) =>
        operationId is not ("list-pkgs" or "list-3rd");

    private string? ResolveAppControlInput(DeviceOperation operation)
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
                var package = InputDialog.Prompt(owner, "ADB App Control", "Nom du package :", LastPackageName);
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
                var package = InputDialog.Prompt(
                    owner,
                    "ADB App Control",
                    $"{operation.Title}\nNom du package :",
                    LastPackageName);
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
                var package = InputDialog.Prompt(
                    owner,
                    "ADB App Control",
                    $"{operation.Title}\nNom du package (ex: com.android.settings) :",
                    LastPackageName);
                if (string.IsNullOrWhiteSpace(package)) return null;
                LastPackageName = package;
                return package;
            }
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
        RefreshComPorts();
        await PollDevicesAsync();
        _log.Info("Refresh done.");
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
        if (!_adb.IsAvailable)
        {
            _log.Warn("ADB not found.");
            return;
        }

        await _adb.EnsureServerAsync();
        await PollDevicesAsync();
        _log.Success("ADB check OK.");
    }

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
            $"Config : {configPath}\n\n" +
            "Oui = Déconnexion\nNon = Ouvrir le dossier config\nAnnuler = Fermer";

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
