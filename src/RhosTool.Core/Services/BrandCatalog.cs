using RhosTool.Core.Models;

namespace RhosTool.Core.Services;

/// <summary>
/// Modes et opérations calqués sur UnlockTool (FLASH / META / MTK UNIVERSAL + GENERAL / SPECIAL).
/// Les opérations licenciées restent PartnerStub ; ADB App Control est opérationnel.
/// </summary>
public static class BrandCatalog
{
    public static IReadOnlyList<BrandModule> All { get; } = Build();

    private static IReadOnlyList<BrandModule> Build()
    {
        return new[]
        {
            B("xiaomi", "Mi", "Mi", "#FF6A00", AndroidModes(), XiaomiTabs()),
            B("samsung", "Samsung", "Samsung", "#0D47A1", SamsungModes(), SamsungTabs()),
            B("huawei", "Huawei", "Huawei", "#C62828", AndroidModes(), GenericAndroidTabs("Huawei")),
            B("honor", "Honor", "Honor", "#000000", AndroidModes(), GenericAndroidTabs("Honor")),
            B("oppo", "Oppo", "Oppo", "#00A651", AndroidModes(), OppoFamilyTabs("Oppo")),
            B("realme", "Realme", "Realme", "#FFC107", AndroidModes(), OppoFamilyTabs("Realme")),
            B("vivo", "Vivo", "Vivo", "#2962FF", AndroidModes(), GenericAndroidTabs("Vivo")),
            B("google", "Google", "Pixel", "#1E88E5", AndroidModes(), GenericAndroidTabs("Google Pixel")),
            B("sony", "Sony", "Sony", "#000000", AndroidModes(), GenericAndroidTabs("Sony")),
            B("oneplus", "OnePlus", "OnePlus", "#E53935", AndroidModes(), GenericAndroidTabs("OnePlus")),
            B("nothing", "Nothing", "Nothing", "#000000", AndroidModes(), GenericAndroidTabs("Nothing")),
            B("motorola", "Motorola", "Motorola", "#616161", AndroidModes(), GenericAndroidTabs("Motorola")),
            B("nokia", "Nokia", "Nokia", "#0D47A1", AndroidModes(), GenericAndroidTabs("Nokia")),
            B("asus", "Asus", "Asus", "#0A192F", AndroidModes(), GenericAndroidTabs("Asus")),
            B("lg", "LG", "LG", "#880E4F", AndroidModes(), GenericAndroidTabs("LG")),
            B("lenovo", "Lenovo", "Lenovo", "#D32F2F", AndroidModes(), GenericAndroidTabs("Lenovo")),
            B("zte", "ZTE", "ZTE", "#1565C0", AndroidModes(), GenericAndroidTabs("ZTE")),
            B("htc", "HTC", "HTC", "#757575", AndroidModes(), GenericAndroidTabs("HTC")),
            B("tecno", "Tecno", "Tecno", "#1565C0", AndroidModes(), MtkBrandTabs("Tecno")),
            B("infinix", "Infinix", "Infinix", "#2E7D32", AndroidModes(), MtkBrandTabs("Infinix")),
            B("itel", "Itel", "Itel", "#00897B", AndroidModes(), MtkBrandTabs("Itel")),
            B("meizu", "Meizu", "Meizu", "#8BC34A", AndroidModes(), GenericAndroidTabs("Meizu")),
            B("vsmart", "Vsmart", "Vsmart", "#AD1457", AndroidModes(), GenericAndroidTabs("Vsmart")),
            B("tcl", "TCL", "TCL", "#D50000", AndroidModes(), GenericAndroidTabs("TCL")),
            B("alcatel", "Alcatel", "Alcatel", "#EF6C00", AndroidModes(), GenericAndroidTabs("Alcatel")),
            B("sharp", "Sharp", "Sharp", "#D50000", AndroidModes(), GenericAndroidTabs("Sharp")),
            B("qualcomm", "Qualcomm", "Qualcomm", "#3949AB", new[] { "EDL TOOLS", "FLASH", "SECURITY" }, QualcommTabs()),
            B("mediatek", "MediaTek", "MediaTek", "#FFB300", new[] { "FLASH", "META", "MTK UNIVERSAL" }, MtkPlatformTabs()),
            B("spreadtrum", "Spreadtrum", "Spreadtrum", "#546E7A", new[] { "SPD TOOLS", "FLASH", "SECURITY" }, GenericAndroidTabs("Spreadtrum")),
            B("android", "Android", "Android", "#43A047", new[] { "ADB TOOLS", "FLASH", "SECURITY" }, GenericAndroidTabs("Android")),
            B("apple", "Apple", "Apple", "#000000", new[] { "RAMDISK", "SECURITY" }, AppleTabs())
        };
    }

    /// <summary>Onglets mode UnlockTool (capture MTK UNIVERSAL).</summary>
    private static string[] AndroidModes() => new[] { "FLASH", "META", "MTK UNIVERSAL" };

    private static string[] SamsungModes() => new[] { "ODIN FLASH", "META", "MTK UNIVERSAL" };

    private static BrandModule B(
        string id,
        string name,
        string shortName,
        string accent,
        IReadOnlyList<string> modes,
        IReadOnlyList<FunctionTab> tabs)
    {
        return new BrandModule
        {
            Id = id,
            Name = name,
            ShortName = shortName,
            Platform = id == "apple" ? "iOS" : "Android",
            AccentHex = accent,
            ModeTabs = modes,
            FunctionTabs = tabs
        };
    }

    // --- Samsung (Odin + général UT) ---

    private static IReadOnlyList<FunctionTab> SamsungTabs() => new[]
    {
        GeneralTab(
            Op("backup-nv", "BACKUP NV", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("restore-nv", "RESTORE NV", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("erase-nv", "ERASE NV", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("format-data", "FORMAT DATA", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("erase-frp", "ERASE FRP", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("fix-orange", "FIX ORANGE MSG", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("read-rpmb", "READ RPMB", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("write-rpmb", "WRITE RPMB", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("unlock-bl", "UNLOCK BL", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("relock-bl", "RELOCK BL", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("flash", "FLASH", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("flash-back", "FLASH BACK", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("auth-bypass", "AUTH BYPASS", "Service partenaire licencié.", OperationKind.PartnerStub)),
        SpecialTab(
            Op("enter-dl", "[COM] ENTER DOWNLOAD", "Guide mode Download / Odin.", OperationKind.PartnerStub, false),
            Op("enable-adb", "[ODIN] ENABLE ADB", "Vérifie / démarre ADB.", OperationKind.ReadInfo),
            Op("odin-restore", "[ODIN] RESTORE", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("screen-lock", "SCREEN LOCK", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("factory", "FACTORY RESET", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("skip-pbl", "SKIP PBL", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("network", "NETWORK UNLOCK", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("read-info", "READ INFO", "Infos appareil via ADB.", OperationKind.ReadInfo),
            Op("reboot", "REBOOT", "Redémarrage ADB.", OperationKind.Reboot),
            Op("boot-device", "BOOT DEVICE", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("drivers", "USB DRIVERS", "Gestionnaire de périphériques.", OperationKind.OpenDeviceManager, false)),
        AppControlTab()
    };

    // --- Xiaomi ---

    private static IReadOnlyList<FunctionTab> XiaomiTabs() => new[]
    {
        GeneralTab(
            Op("backup-nv", "BACKUP NV", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("restore-nv", "RESTORE NV", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("erase-nv", "ERASE NV", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("format-data", "FORMAT DATA", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("erase-frp", "ERASE FRP", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("fix-orange", "FIX ORANGE MSG", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("read-rpmb", "READ RPMB", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("write-rpmb", "WRITE RPMB", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("unlock-bl", "UNLOCK BL", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("relock-bl", "RELOCK BL", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("flash", "FLASH", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("flash-back", "FLASH BACK", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("auth-bypass", "AUTH BYPASS", "Service partenaire licencié.", OperationKind.PartnerStub)),
        SpecialTab(
            Op("mi-account", "MI ACCOUNT", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("micloud", "MI CLOUD", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("edl", "ENTER EDL", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("auth-flash", "AUTH FLASH", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("factory", "FACTORY RESET", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("screen-lock", "SCREEN LOCK", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("network", "NETWORK UNLOCK", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("read-info", "READ INFO", "Infos ADB.", OperationKind.ReadInfo),
            Op("reboot", "REBOOT", "Redémarrage ADB.", OperationKind.Reboot),
            Op("reboot-fb", "REBOOT BOOTLOADER", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("boot-device", "BOOT DEVICE", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("drivers", "USB DRIVERS", "Gestionnaire de périphériques.", OperationKind.OpenDeviceManager, false)),
        AppControlTab()
    };

    // --- MTK-heavy brands (Tecno / Infinix / Itel) ---

    private static IReadOnlyList<FunctionTab> MtkBrandTabs(string brand) => new[]
    {
        GeneralTab(
            Op("backup-nv", "BACKUP NV", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("restore-nv", "RESTORE NV", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("erase-nv", "ERASE NV", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("format-data", "FORMAT DATA", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("erase-frp", "ERASE FRP", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("fix-orange", "FIX ORANGE MSG", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("read-rpmb", "READ RPMB", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("write-rpmb", "WRITE RPMB", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("unlock-bl", "UNLOCK BL", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("relock-bl", "RELOCK BL", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("flash", "FLASH", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("flash-back", "FLASH BACK", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("auth-bypass", "AUTH BYPASS", "Service partenaire licencié.", OperationKind.PartnerStub)),
        SpecialTab(
            Op("mtk-meta", "MTK META", $"Mode META {brand} — service partenaire.", OperationKind.PartnerStub),
            Op("mtk-preloader", "MTK PRELOADER", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("custom-da", "CUSTOM DA", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("force-brom", "FORCE BROM", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("wipe-efs", "WIPE EFS", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("factory", "FACTORY RESET", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("screen-lock", "SCREEN LOCK", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("read-info", "READ INFO", $"Infos {brand} via ADB.", OperationKind.ReadInfo),
            Op("reboot", "REBOOT", "Redémarrage ADB.", OperationKind.Reboot),
            Op("boot-device", "BOOT DEVICE", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("drivers", "USB DRIVERS", "Gestionnaire de périphériques.", OperationKind.OpenDeviceManager, false)),
        AppControlTab()
    };

    private static IReadOnlyList<FunctionTab> OppoFamilyTabs(string brand) => new[]
    {
        GeneralTab(
            Op("backup-nv", "BACKUP NV", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("restore-nv", "RESTORE NV", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("erase-nv", "ERASE NV", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("format-data", "FORMAT DATA", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("erase-frp", "ERASE FRP", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("fix-orange", "FIX ORANGE MSG", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("read-rpmb", "READ RPMB", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("write-rpmb", "WRITE RPMB", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("unlock-bl", "UNLOCK BL", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("relock-bl", "RELOCK BL", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("flash", "FLASH", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("flash-back", "FLASH BACK", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("auth-bypass", "AUTH BYPASS", "Service partenaire licencié.", OperationKind.PartnerStub)),
        SpecialTab(
            Op("oppo-id", $"{brand.ToUpperInvariant()} ID", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("demo", "DEMO REMOVE", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("factory", "FACTORY RESET", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("screen-lock", "SCREEN LOCK", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("network", "NETWORK UNLOCK", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("read-info", "READ INFO", $"Infos {brand} via ADB.", OperationKind.ReadInfo),
            Op("reboot", "REBOOT", "Redémarrage ADB.", OperationKind.Reboot),
            Op("boot-device", "BOOT DEVICE", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("drivers", "USB DRIVERS", "Gestionnaire de périphériques.", OperationKind.OpenDeviceManager, false)),
        AppControlTab()
    };

    private static IReadOnlyList<FunctionTab> GenericAndroidTabs(string brand) => new[]
    {
        GeneralTab(
            Op("backup-nv", "BACKUP NV", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("restore-nv", "RESTORE NV", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("erase-nv", "ERASE NV", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("format-data", "FORMAT DATA", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("erase-frp", "ERASE FRP", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("fix-orange", "FIX ORANGE MSG", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("read-rpmb", "READ RPMB", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("write-rpmb", "WRITE RPMB", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("unlock-bl", "UNLOCK BL", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("relock-bl", "RELOCK BL", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("flash", "FLASH", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("flash-back", "FLASH BACK", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("auth-bypass", "AUTH BYPASS", "Service partenaire licencié.", OperationKind.PartnerStub)),
        SpecialTab(
            Op("factory", "FACTORY RESET", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("screen-lock", "SCREEN LOCK", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("network", "NETWORK UNLOCK", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("edl-brom", "EDL / BROM", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("read-info", "READ INFO", $"Infos {brand} via ADB.", OperationKind.ReadInfo),
            Op("reboot", "REBOOT", "Redémarrage ADB.", OperationKind.Reboot),
            Op("reboot-rec", "REBOOT RECOVERY", "Recovery ADB.", OperationKind.RebootRecovery),
            Op("boot-device", "BOOT DEVICE", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("drivers", "USB DRIVERS", "Gestionnaire de périphériques.", OperationKind.OpenDeviceManager, false)),
        AppControlTab()
    };

    private static IReadOnlyList<FunctionTab> QualcommTabs() => new[]
    {
        GeneralTab(
            Op("edl", "QUALCOMM EDL", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("firehose", "FIREHOSE", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("format-data", "FORMAT DATA", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("erase-frp", "ERASE FRP", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("flash", "FLASH", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("auth-bypass", "AUTH BYPASS", "Service partenaire licencié.", OperationKind.PartnerStub)),
        SpecialTab(
            Op("read-info", "READ INFO", "Infos ADB.", OperationKind.ReadInfo),
            Op("reboot", "REBOOT", "Redémarrage ADB.", OperationKind.Reboot),
            Op("drivers", "USB DRIVERS", "Gestionnaire de périphériques.", OperationKind.OpenDeviceManager, false)),
        AppControlTab()
    };

    private static IReadOnlyList<FunctionTab> MtkPlatformTabs() => new[]
    {
        GeneralTab(
            Op("backup-nv", "BACKUP NV", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("restore-nv", "RESTORE NV", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("erase-nv", "ERASE NV", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("format-data", "FORMAT DATA", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("erase-frp", "ERASE FRP", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("fix-orange", "FIX ORANGE MSG", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("read-rpmb", "READ RPMB", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("write-rpmb", "WRITE RPMB", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("unlock-bl", "UNLOCK BL", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("relock-bl", "RELOCK BL", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("flash", "FLASH", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("flash-back", "FLASH BACK", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("auth-bypass", "AUTH BYPASS", "Service partenaire licencié.", OperationKind.PartnerStub)),
        SpecialTab(
            Op("brom", "MTK BROM", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("meta", "MTK META", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("custom-da", "CUSTOM DA", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("force-brom", "FORCE BROM", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("read-info", "READ INFO", "Infos ADB.", OperationKind.ReadInfo),
            Op("reboot", "REBOOT", "Redémarrage ADB.", OperationKind.Reboot),
            Op("boot-device", "BOOT DEVICE", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("drivers", "USB DRIVERS", "Gestionnaire de périphériques.", OperationKind.OpenDeviceManager, false)),
        AppControlTab()
    };

    private static IReadOnlyList<FunctionTab> AppleTabs() => new[]
    {
        Tab("general", "GENERAL",
            Op("backup-pass", "BACKUP PASSCODE", "Service partenaire licencié (ramdisk).", OperationKind.PartnerStub),
            Op("restore-pass", "RESTORE PASSCODE", "Service partenaire licencié (ramdisk).", OperationKind.PartnerStub),
            Op("read-account", "READ ACCOUNT", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("read-hw", "READ HW INFO", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("read-ios", "READ iOS", "Infos iOS si ideviceinfo.", OperationKind.ReadInfo),
            Op("factory", "FACTORY RESET", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("hidden-account", "HIDDEN ACCOUNT", "Service partenaire licencié.", OperationKind.PartnerStub)),
        Tab("special", "SPECIAL",
            Op("icloud", "ICLOUD / ACTIVATION", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("mdm", "MDM", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("network", "NETWORK UNLOCK", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("drivers", "USB DRIVERS", "Gestionnaire de périphériques.", OperationKind.OpenDeviceManager, false))
    };

    private static FunctionTab GeneralTab(params DeviceOperation[] ops) =>
        Tab("general", "GENERAL", ops);

    private static FunctionTab SpecialTab(params DeviceOperation[] ops) =>
        Tab("special", "SPECIAL", ops);

    private static FunctionTab AppControlTab() => Tab("appctl", "ADB APP CONTROL",
        Op("list-pkgs", "LIST PACKAGES", "Liste tous les packages installés.", OperationKind.AdbAppControl),
        Op("list-3rd", "LIST 3RD PARTY", "Liste les apps utilisateur (hors système).", OperationKind.AdbAppControl),
        Op("install", "INSTALL APK", "Installe un fichier APK via ADB.", OperationKind.AdbAppControl),
        Op("uninstall", "UNINSTALL APP", "Désinstalle un package (ex: com.example.app).", OperationKind.AdbAppControl),
        Op("force-stop", "FORCE STOP", "Force l'arrêt d'une application.", OperationKind.AdbAppControl),
        Op("clear-data", "CLEAR DATA", "Efface les données d'une application.", OperationKind.AdbAppControl),
        Op("clear-cache", "CLEAR CACHE", "Tente d'effacer le cache d'une application.", OperationKind.AdbAppControl),
        Op("enable-app", "ENABLE APP", "Réactive un package désactivé.", OperationKind.AdbAppControl),
        Op("disable-app", "DISABLE APP", "Désactive un package (user).", OperationKind.AdbAppControl),
        Op("launch-app", "LAUNCH APP", "Lance l'activité launcher du package.", OperationKind.AdbAppControl),
        Op("app-info", "APP INFO", "Affiche version / chemin APK du package.", OperationKind.AdbAppControl),
        Op("pull-apk", "PULL APK", "Extrait l'APK du package vers le PC.", OperationKind.AdbAppControl));

    private static FunctionTab Tab(string id, string title, params DeviceOperation[] ops) =>
        new() { Id = id, Title = title, Operations = ops };

    private static DeviceOperation Op(
        string id,
        string title,
        string description,
        OperationKind kind,
        bool requiresDevice = true) =>
        new()
        {
            Id = id,
            Title = title,
            Description = description,
            Kind = kind,
            RequiresDevice = requiresDevice
        };
}
