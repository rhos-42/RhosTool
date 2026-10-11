using RhosTool.Core.Models;

namespace RhosTool.Core.Services;

/// <summary>
/// Modes et opérations calqués sur UnlockTool (FLASH / META / MTK UNIVERSAL + GENERAL / SPECIAL).
/// Réel en local : ADB/fastboot, packs, factory reset, guides. PartnerStub = FRP/bypass/NV/flash bas niveau.
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
            Op("backup-nv", "BACKUP NV", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("restore-nv", "RESTORE NV", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("erase-nv", "ERASE NV", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("format-data", "FORMAT DATA", "Remise d'usine / wipe data via ADB (appareil autorisé).", OperationKind.DeviceAction),
            Op("erase-frp", "ERASE FRP", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("fix-orange", "FIX ORANGE MSG", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("read-rpmb", "READ RPMB", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("write-rpmb", "WRITE RPMB", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("unlock-bl", "UNLOCK BL", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("relock-bl", "RELOCK BL", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("flash", "FLASH", "Prépare le pack chargé (LOAD PACK). Flash bas niveau = partenaire.", OperationKind.DeviceAction, false),
            Op("flash-back", "FLASH BACK", "Revalide / rappelle le pack chargé pour flash atelier.", OperationKind.DeviceAction, false),
            Op("auth-bypass", "AUTH BYPASS", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub)),
        SpecialTab(
            Op("enter-dl", "[COM] ENTER DOWNLOAD", "Guide mode Download / Odin / BROM.", OperationKind.DeviceAction, false),
            Op("enable-adb", "[ODIN] ENABLE ADB", "Démarre le serveur ADB et liste les appareils.", OperationKind.DeviceAction, false),
            Op("odin-restore", "[ODIN] RESTORE", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("screen-lock", "SCREEN LOCK", "Ouvre les réglages Sécurité (ne retire pas le verrou).", OperationKind.DeviceAction),
            Op("factory", "FACTORY RESET", "Remise d'usine via ADB (confirmation sur le téléphone).", OperationKind.DeviceAction),
            Op("skip-pbl", "SKIP PBL", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("network", "NETWORK UNLOCK", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("read-info", "READ INFO", "Infos appareil via ADB.", OperationKind.ReadInfo),
            Op("reboot", "REBOOT", "Redémarrage ADB.", OperationKind.Reboot),
            Op("boot-device", "BOOT DEVICE", "Redémarrage ADB de l'appareil.", OperationKind.DeviceAction),
            Op("drivers", "USB DRIVERS", "Gestionnaire de périphériques.", OperationKind.OpenDeviceManager, false)),
        AdbToolsTab(),
        MtpFilesTab(),
        AppControlTab(),
        AllDriverTab(),
        RamdiskA12A13Tab(),
        RamdiskIphoneTab(),
        RamdiskIpadTab(),
        ToolAndroidPcTab(),
        ResourcesTab()
    };

    // --- Xiaomi ---

    private static IReadOnlyList<FunctionTab> XiaomiTabs() => new[]
    {
        GeneralTab(
            Op("backup-nv", "BACKUP NV", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("restore-nv", "RESTORE NV", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("erase-nv", "ERASE NV", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("format-data", "FORMAT DATA", "Remise d'usine / wipe data via ADB (appareil autorisé).", OperationKind.DeviceAction),
            Op("erase-frp", "ERASE FRP", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("fix-orange", "FIX ORANGE MSG", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("read-rpmb", "READ RPMB", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("write-rpmb", "WRITE RPMB", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("unlock-bl", "UNLOCK BL", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("relock-bl", "RELOCK BL", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("flash", "FLASH", "Prépare le pack chargé (LOAD PACK). Flash bas niveau = partenaire.", OperationKind.DeviceAction, false),
            Op("flash-back", "FLASH BACK", "Revalide / rappelle le pack chargé pour flash atelier.", OperationKind.DeviceAction, false),
            Op("auth-bypass", "AUTH BYPASS", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub)),
        SpecialTab(
            Op("mi-account", "MI ACCOUNT", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("micloud", "MI CLOUD", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("edl", "ENTER EDL", "Guide connexion EDL / test-point (pas de flash forcé).", OperationKind.DeviceAction, false),
            Op("auth-flash", "AUTH FLASH", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("factory", "FACTORY RESET", "Remise d'usine via ADB (confirmation sur le téléphone).", OperationKind.DeviceAction),
            Op("screen-lock", "SCREEN LOCK", "Ouvre les réglages Sécurité (ne retire pas le verrou).", OperationKind.DeviceAction),
            Op("network", "NETWORK UNLOCK", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("read-info", "READ INFO", "Infos ADB.", OperationKind.ReadInfo),
            Op("reboot", "REBOOT", "Redémarrage ADB.", OperationKind.Reboot),
            Op("reboot-fb", "REBOOT BOOTLOADER", "Redémarrage en mode bootloader / fastboot.", OperationKind.DeviceAction),
            Op("boot-device", "BOOT DEVICE", "Redémarrage ADB de l'appareil.", OperationKind.DeviceAction),
            Op("drivers", "USB DRIVERS", "Gestionnaire de périphériques.", OperationKind.OpenDeviceManager, false)),
        AdbToolsTab(),
        MtpFilesTab(),
        AppControlTab(),
        AllDriverTab(),
        RamdiskA12A13Tab(),
        RamdiskIphoneTab(),
        RamdiskIpadTab(),
        ToolAndroidPcTab(),
        ResourcesTab()
    };

    // --- MTK-heavy brands (Tecno / Infinix / Itel) ---

    private static IReadOnlyList<FunctionTab> MtkBrandTabs(string brand) => new[]
    {
        GeneralTab(
            Op("backup-nv", "BACKUP NV", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("restore-nv", "RESTORE NV", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("erase-nv", "ERASE NV", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("format-data", "FORMAT DATA", "Remise d'usine / wipe data via ADB (appareil autorisé).", OperationKind.DeviceAction),
            Op("erase-frp", "ERASE FRP", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("fix-orange", "FIX ORANGE MSG", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("read-rpmb", "READ RPMB", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("write-rpmb", "WRITE RPMB", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("unlock-bl", "UNLOCK BL", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("relock-bl", "RELOCK BL", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("flash", "FLASH", "Prépare le pack chargé (LOAD PACK). Flash bas niveau = partenaire.", OperationKind.DeviceAction, false),
            Op("flash-back", "FLASH BACK", "Revalide / rappelle le pack chargé pour flash atelier.", OperationKind.DeviceAction, false),
            Op("auth-bypass", "AUTH BYPASS", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub)),
        SpecialTab(
            Op("mtk-meta", "MTK META", $"Guide mode META {brand}.", OperationKind.DeviceAction, false),
            Op("mtk-preloader", "MTK PRELOADER", "Guide mode preloader MTK.", OperationKind.DeviceAction, false),
            Op("custom-da", "CUSTOM DA", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("force-brom", "FORCE BROM", "Guide entrée BROM / preloader.", OperationKind.DeviceAction, false),
            Op("wipe-efs", "WIPE EFS", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("factory", "FACTORY RESET", "Remise d'usine via ADB (confirmation sur le téléphone).", OperationKind.DeviceAction),
            Op("screen-lock", "SCREEN LOCK", "Ouvre les réglages Sécurité (ne retire pas le verrou).", OperationKind.DeviceAction),
            Op("read-info", "READ INFO", $"Infos {brand} via ADB.", OperationKind.ReadInfo),
            Op("reboot", "REBOOT", "Redémarrage ADB.", OperationKind.Reboot),
            Op("boot-device", "BOOT DEVICE", "Redémarrage ADB de l'appareil.", OperationKind.DeviceAction),
            Op("drivers", "USB DRIVERS", "Gestionnaire de périphériques.", OperationKind.OpenDeviceManager, false)),
        AdbToolsTab(),
        MtpFilesTab(),
        AppControlTab(),
        AllDriverTab(),
        RamdiskA12A13Tab(),
        RamdiskIphoneTab(),
        RamdiskIpadTab(),
        ToolAndroidPcTab(),
        ResourcesTab()
    };

    private static IReadOnlyList<FunctionTab> OppoFamilyTabs(string brand) => new[]
    {
        GeneralTab(
            Op("backup-nv", "BACKUP NV", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("restore-nv", "RESTORE NV", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("erase-nv", "ERASE NV", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("format-data", "FORMAT DATA", "Remise d'usine / wipe data via ADB (appareil autorisé).", OperationKind.DeviceAction),
            Op("erase-frp", "ERASE FRP", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("fix-orange", "FIX ORANGE MSG", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("read-rpmb", "READ RPMB", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("write-rpmb", "WRITE RPMB", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("unlock-bl", "UNLOCK BL", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("relock-bl", "RELOCK BL", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("flash", "FLASH", "Prépare le pack chargé (LOAD PACK). Flash bas niveau = partenaire.", OperationKind.DeviceAction, false),
            Op("flash-back", "FLASH BACK", "Revalide / rappelle le pack chargé pour flash atelier.", OperationKind.DeviceAction, false),
            Op("auth-bypass", "AUTH BYPASS", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub)),
        SpecialTab(
            Op("oppo-id", $"{brand.ToUpperInvariant()} ID", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("demo", "DEMO REMOVE", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("factory", "FACTORY RESET", "Remise d'usine via ADB (confirmation sur le téléphone).", OperationKind.DeviceAction),
            Op("screen-lock", "SCREEN LOCK", "Ouvre les réglages Sécurité (ne retire pas le verrou).", OperationKind.DeviceAction),
            Op("network", "NETWORK UNLOCK", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("read-info", "READ INFO", $"Infos {brand} via ADB.", OperationKind.ReadInfo),
            Op("reboot", "REBOOT", "Redémarrage ADB.", OperationKind.Reboot),
            Op("boot-device", "BOOT DEVICE", "Redémarrage ADB de l'appareil.", OperationKind.DeviceAction),
            Op("drivers", "USB DRIVERS", "Gestionnaire de périphériques.", OperationKind.OpenDeviceManager, false)),
        AdbToolsTab(),
        MtpFilesTab(),
        AppControlTab(),
        AllDriverTab(),
        RamdiskA12A13Tab(),
        RamdiskIphoneTab(),
        RamdiskIpadTab(),
        ToolAndroidPcTab(),
        ResourcesTab()
    };

    private static IReadOnlyList<FunctionTab> GenericAndroidTabs(string brand) => new[]
    {
        GeneralTab(
            Op("backup-nv", "BACKUP NV", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("restore-nv", "RESTORE NV", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("erase-nv", "ERASE NV", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("format-data", "FORMAT DATA", "Remise d'usine / wipe data via ADB (appareil autorisé).", OperationKind.DeviceAction),
            Op("erase-frp", "ERASE FRP", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("fix-orange", "FIX ORANGE MSG", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("read-rpmb", "READ RPMB", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("write-rpmb", "WRITE RPMB", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("unlock-bl", "UNLOCK BL", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("relock-bl", "RELOCK BL", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("flash", "FLASH", "Prépare le pack chargé (LOAD PACK). Flash bas niveau = partenaire.", OperationKind.DeviceAction, false),
            Op("flash-back", "FLASH BACK", "Revalide / rappelle le pack chargé pour flash atelier.", OperationKind.DeviceAction, false),
            Op("auth-bypass", "AUTH BYPASS", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub)),
        SpecialTab(
            Op("factory", "FACTORY RESET", "Remise d'usine via ADB (confirmation sur le téléphone).", OperationKind.DeviceAction),
            Op("screen-lock", "SCREEN LOCK", "Ouvre les réglages Sécurité (ne retire pas le verrou).", OperationKind.DeviceAction),
            Op("network", "NETWORK UNLOCK", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("edl-brom", "EDL / BROM", "Guide EDL / BROM / preloader.", OperationKind.DeviceAction, false),
            Op("read-info", "READ INFO", $"Infos {brand} via ADB.", OperationKind.ReadInfo),
            Op("reboot", "REBOOT", "Redémarrage ADB.", OperationKind.Reboot),
            Op("reboot-rec", "REBOOT RECOVERY", "Recovery ADB.", OperationKind.RebootRecovery),
            Op("boot-device", "BOOT DEVICE", "Redémarrage ADB de l'appareil.", OperationKind.DeviceAction),
            Op("drivers", "USB DRIVERS", "Gestionnaire de périphériques.", OperationKind.OpenDeviceManager, false)),
        AdbToolsTab(),
        MtpFilesTab(),
        AppControlTab(),
        AllDriverTab(),
        RamdiskA12A13Tab(),
        RamdiskIphoneTab(),
        RamdiskIpadTab(),
        ToolAndroidPcTab(),
        ResourcesTab()
    };

    private static IReadOnlyList<FunctionTab> QualcommTabs() => new[]
    {
        GeneralTab(
            Op("edl", "QUALCOMM EDL", "Guide connexion EDL Qualcomm.", OperationKind.DeviceAction, false),
            Op("firehose", "FIREHOSE", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("format-data", "FORMAT DATA", "Remise d'usine / wipe data via ADB (appareil autorisé).", OperationKind.DeviceAction),
            Op("erase-frp", "ERASE FRP", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("flash", "FLASH", "Prépare le pack chargé (LOAD PACK). Flash bas niveau = partenaire.", OperationKind.DeviceAction, false),
            Op("auth-bypass", "AUTH BYPASS", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub)),
        SpecialTab(
            Op("read-info", "READ INFO", "Infos ADB.", OperationKind.ReadInfo),
            Op("reboot", "REBOOT", "Redémarrage ADB.", OperationKind.Reboot),
            Op("drivers", "USB DRIVERS", "Gestionnaire de périphériques.", OperationKind.OpenDeviceManager, false)),
        AdbToolsTab(),
        MtpFilesTab(),
        AppControlTab(),
        AllDriverTab(),
        RamdiskA12A13Tab(),
        RamdiskIphoneTab(),
        RamdiskIpadTab(),
        ToolAndroidPcTab(),
        ResourcesTab()
    };

    private static IReadOnlyList<FunctionTab> MtkPlatformTabs() => new[]
    {
        GeneralTab(
            Op("backup-nv", "BACKUP NV", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("restore-nv", "RESTORE NV", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("erase-nv", "ERASE NV", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("format-data", "FORMAT DATA", "Remise d'usine / wipe data via ADB (appareil autorisé).", OperationKind.DeviceAction),
            Op("erase-frp", "ERASE FRP", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("fix-orange", "FIX ORANGE MSG", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("read-rpmb", "READ RPMB", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("write-rpmb", "WRITE RPMB", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("unlock-bl", "UNLOCK BL", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("relock-bl", "RELOCK BL", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("flash", "FLASH", "Prépare le pack chargé (LOAD PACK). Flash bas niveau = partenaire.", OperationKind.DeviceAction, false),
            Op("flash-back", "FLASH BACK", "Revalide / rappelle le pack chargé pour flash atelier.", OperationKind.DeviceAction, false),
            Op("auth-bypass", "AUTH BYPASS", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub)),
        SpecialTab(
            Op("brom", "MTK BROM", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("meta", "MTK META", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("custom-da", "CUSTOM DA", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("force-brom", "FORCE BROM", "Guide entrée BROM / preloader.", OperationKind.DeviceAction, false),
            Op("read-info", "READ INFO", "Infos ADB.", OperationKind.ReadInfo),
            Op("reboot", "REBOOT", "Redémarrage ADB.", OperationKind.Reboot),
            Op("boot-device", "BOOT DEVICE", "Redémarrage ADB de l'appareil.", OperationKind.DeviceAction),
            Op("drivers", "USB DRIVERS", "Gestionnaire de périphériques.", OperationKind.OpenDeviceManager, false)),
        AdbToolsTab(),
        MtpFilesTab(),
        AppControlTab(),
        AllDriverTab(),
        RamdiskA12A13Tab(),
        RamdiskIphoneTab(),
        RamdiskIpadTab(),
        ToolAndroidPcTab(),
        ResourcesTab()
    };

    private static IReadOnlyList<FunctionTab> AppleTabs() => new[]
    {
        GeneralTab(
            Op("backup-pass", "BACKUP PASSCODE", "Service partenaire licencié (ramdisk).", OperationKind.PartnerStub),
            Op("restore-pass", "RESTORE PASSCODE", "Service partenaire licencié (ramdisk).", OperationKind.PartnerStub),
            Op("read-account", "READ ACCOUNT", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("read-hw", "READ HW INFO", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("read-ios", "READ iOS", "Infos iOS si ideviceinfo.", OperationKind.ReadInfo),
            Op("factory", "FACTORY RESET", "Remise d'usine via ADB (confirmation sur le téléphone).", OperationKind.DeviceAction),
            Op("hidden-account", "HIDDEN ACCOUNT", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub)),
        Tab("special", "SPECIAL",
            Op("icloud", "ICLOUD / ACTIVATION", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("mdm", "MDM", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("network", "NETWORK UNLOCK", "Module partenaire (FRP/bypass/NV/flash bas niveau) — non exécuté en local.", OperationKind.PartnerStub),
            Op("drivers", "USB DRIVERS", "Gestionnaire de périphériques.", OperationKind.OpenDeviceManager, false)),
        AllDriverTab(),
        RamdiskA12A13Tab(),
        RamdiskIphoneTab(),
        RamdiskIpadTab(),
        ToolAndroidPcTab(),
        ResourcesTab()
    };

    /// <summary>Ressources téléchargeables (URLs à renseigner dans resource-links.json).</summary>
    private static FunctionTab ResourcesTab() => Tab("resources", "RESOURCES",
        Op("res-diag-iphone-ipad", "DIAG IPHONE - IPAD", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("res-all-ramdisk-iphone", "ALL RAMDISK IPHONE", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("res-all-ramdisk-ipad", "ALL RAMDISK IPAD", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false));

    private static FunctionTab ToolAndroidPcTab() => Tab("tool-android-pc", "TOOL ANDROID-PC",
        Op("tool-pc", "TOOL PC", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("tool-winra1n", "WINRA1N 2.1", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false));

    private static FunctionTab RamdiskIpadTab() => Tab("ramdisk-ipad", "RAMDISK IPAD",
        Op("rd-ipad-5", "IPAD 5", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-ipad-6", "IPAD 6", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-ipad-7", "IPAD 7", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-ipad-air-2", "IPAD AIR 2", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-ipad-mini-4", "IPAD MINI 4", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-ipad-pro-105", "IPAD PRO 10.5", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-ipad-pro-129-g1", "IPAD PRO 12.9 G1", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-ipad-pro-129-g2", "IPAD PRO 12.9 G2", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-ipad-pro-97", "IPAD PRO 9.7", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false));

    private static FunctionTab RamdiskIphoneTab() => Tab("ramdisk-iphone", "RAMDISK IPHONE",
        Op("rd-iphone-5se", "IPHONE 5SE", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-iphone-6g", "IPHONE 6G", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-iphone-6-plus", "IPHONE 6 PLUS", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-iphone-6s", "IPHONE 6S", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-iphone-6s-plus", "IPHONE 6S PLUS", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-iphone-7g", "IPHONE 7G", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-iphone-7-plus", "IPHONE 7 PLUS", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-iphone-8g", "IPHONE 8G", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-iphone-8-plus", "IPHONE 8 PLUS", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-iphone-x", "IPHONE X", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false));

    private static FunctionTab RamdiskA12A13Tab() => Tab("ramdisk-a12", "RAMDISK A12-A13",
        Op("rd-a12-iphone-xr", "IPHONE XR", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-a12-iphone-xs", "IPHONE XS", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-a12-iphone-xs-max", "IPHONE XS MAX", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-a12-iphone-11", "IPHONE 11", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-a12-iphone-11-pro", "IPHONE 11 PRO", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-a12-iphone-11-pro-max", "IPHONE 11 PRO MAX", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-a12-iphone-se-2020", "IPHONE SE 2020", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-a12-ipad-8", "IPAD 8", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-a12-ipad-9", "IPAD 9", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-a12-ipad-air-3", "IPAD AIR 3", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("rd-a12-ipad-mini-5", "IPAD MINI 5", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false));

    private static FunctionTab AllDriverTab() => Tab("alldriver", "ALL DRIVER",
        Op("drv-all-mobile", "ALL DRIVER MOBILE", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("drv-cert-fix", "CERT-FIX.zip", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("drv-apple-usb", "DRIVER APPLE USB", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("drv-huawei-kirin", "DRIVER HUAWEI KIRIN", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("drv-huawei-usb10", "DRIVER HUAWEI USB1.0", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("drv-libusb-filter", "DRIVER LIBUSB FILTER", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("drv-nokia-mtk", "DRIVER NOKIA MTK", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("drv-nokia-full", "DRIVER NOKIA FULL", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("drv-qc-mtk", "DRIVER QC + MTK", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("drv-qc-mtk-mf", "DRIVER QC + MTK (MF)", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("drv-spd-77xx", "DRIVER SPD 77XX", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("drv-spd-77xx-mf", "DRIVER SPD 77XX (MF)", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("drv-sprd-npi", "DRIVER SPRD NPI", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("drv-installer-pack", "DRIVER INSTALL PACK", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("drv-installer-alt", "DRIVER INSTALLER PACK", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("drv-fix-mtk-com", "FIX DRIVER MTK COM", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("drv-fix-mtk-com-mf", "FIX DRIVER MTK COM (MF)", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("drv-fix-qc", "FIX DRIVER QUALCOMM", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("drv-winra1n", "WINRA1N 2.1", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false),
        Op("drv-vcredist-2015", "VCREDIST 2015", "Ouvre le lieu MediaFire (dossier catégorie).", OperationKind.OpenResourceLink, false));

    private static DeviceOperation[] PackOps() => new[]
    {
        Op("load-pack", "LOAD PACK", "Importer un paquet firmware (ZIP/TAR/dossier) et générer le manifeste.", OperationKind.FirmwarePack, false),
        Op("validate-pack", "VALIDATE PACK", "Valider le dernier paquet chargé (ou un chemin).", OperationKind.FirmwarePack, false),
        Op("show-manifest", "SHOW MANIFEST", "Afficher le manifeste du paquet firmware chargé.", OperationKind.FirmwarePack, false)
    };

    private static FunctionTab GeneralTab(params DeviceOperation[] ops) =>
        Tab("general", "GENERAL", PackOps().Concat(ops).ToArray());

    private static FunctionTab SpecialTab(params DeviceOperation[] ops) =>
        Tab("special", "SPECIAL", ops);

    private static FunctionTab AdbToolsTab() => Tab("adbtools", "ADB TOOLS",
        Op("adb-status", "ADB DEVICES", "Démarre le serveur et liste les appareils.", OperationKind.AdbTools, false),
        Op("adb-wait", "WAIT DEVICE", "Attend qu'un appareil ADB soit prêt.", OperationKind.AdbTools, false),
        Op("adb-state", "GET STATE", "État / série ADB de l'appareil.", OperationKind.AdbTools),
        Op("adb-tcpip", "TCPIP 5555", "Active ADB en Wi‑Fi (port 5555).", OperationKind.AdbTools),
        Op("adb-connect", "CONNECT WIFI", "Connecte ADB à IP:port.", OperationKind.AdbTools, false),
        Op("adb-disconnect", "DISCONNECT", "Déconnecte une session ADB réseau.", OperationKind.AdbTools, false),
        Op("adb-ls", "LIST /SDCARD", "Liste un dossier distant (défaut /sdcard).", OperationKind.AdbTools),
        Op("adb-push", "PUSH FILE", "Envoie un fichier/dossier PC → téléphone.", OperationKind.AdbTools),
        Op("adb-pull", "PULL FILE", "Récupère un fichier téléphone → PC.", OperationKind.AdbTools),
        Op("adb-shell", "SHELL", "Exécute une commande shell ADB.", OperationKind.AdbTools),
        Op("adb-logcat", "LOGCAT DUMP", "Dump logcat (option sauver fichier).", OperationKind.AdbTools),
        Op("adb-screencap", "SCREENSHOT", "Capture d'écran PNG via screencap.", OperationKind.AdbTools),
        Op("adb-bugreport", "BUGREPORT", "Génère un bugreport (peut être long).", OperationKind.AdbTools),
        Op("adb-reboot", "REBOOT", "Redémarrage ADB.", OperationKind.AdbTools),
        Op("adb-reboot-recovery", "REBOOT RECOVERY", "Redémarrage recovery.", OperationKind.AdbTools),
        Op("adb-reboot-bootloader", "REBOOT BOOTLOADER", "Redémarrage bootloader/fastboot.", OperationKind.AdbTools),
        Op("adb-sideload", "REBOOT SIDELOAD", "Redémarrage mode sideload.", OperationKind.AdbTools),
        Op("adb-root", "ADB ROOT", "Redémarre adbd en root (si build le permet).", OperationKind.AdbTools),
        Op("adb-unroot", "ADB UNROOT", "Repasse adbd en mode normal.", OperationKind.AdbTools),
        Op("adb-remount", "REMOUNT", "Remount système RW (souvent root).", OperationKind.AdbTools),
        Op("adb-sync", "SYNC", "adb sync (selon support appareil).", OperationKind.AdbTools),
        Op("adb-forward-list", "FORWARD LIST", "Liste les redirections de ports ADB.", OperationKind.AdbTools));

    private static FunctionTab MtpFilesTab() => Tab("mtp", "MTP FILES",
        Op("mtp-devices", "MTP DEVICES", "Liste les appareils MTP / stockage portable Windows.", OperationKind.MtpFiles, false),
        Op("mtp-list", "MTP LIST", "Liste les fichiers d'un dossier MTP (ex: Internal storage\\DCIM).", OperationKind.MtpFiles, false),
        Op("mtp-download", "MTP DOWNLOAD", "Télécharge un fichier MTP → PC.", OperationKind.MtpFiles, false),
        Op("mtp-upload", "MTP UPLOAD", "Envoie un fichier PC → dossier MTP.", OperationKind.MtpFiles, false),
        Op("mtp-mkdir", "MTP MKDIR", "Crée un dossier sur le stockage MTP.", OperationKind.MtpFiles, false),
        Op("mtp-delete", "MTP DELETE", "Supprime un fichier/dossier MTP.", OperationKind.MtpFiles, false));

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
        bool requiresDevice = true)
    {
        var mode = ResolveRequiredMode(id, kind, requiresDevice);
        return new()
        {
            Id = id,
            Title = title,
            Description = description,
            Kind = kind,
            RequiresDevice = requiresDevice && mode is not DeviceRequiredMode.None
                                            and not DeviceRequiredMode.Unavailable,
            RequiredMode = mode
        };
    }

    private static DeviceRequiredMode ResolveRequiredMode(
        string id,
        OperationKind kind,
        bool requiresDevice)
    {
        if (kind == OperationKind.PartnerStub)
            return DeviceRequiredMode.Unavailable;

        if (kind is OperationKind.OpenDeviceManager or OperationKind.FirmwarePack
            or OperationKind.OpenResourceLink)
            return DeviceRequiredMode.None;

        if (kind == OperationKind.MtpFiles)
        {
            return id == "mtp-devices"
                ? DeviceRequiredMode.None
                : DeviceRequiredMode.Mtp;
        }

        if (id is "fastboot-devices" or "fb-unlock-status")
            return DeviceRequiredMode.Fastboot;

        if (id is "enter-dl" or "edl" or "edl-brom" or "force-brom"
            or "mtk-preloader" or "mtk-meta" or "drivers"
            or "enable-adb" or "adb-status" or "adb-devices"
            or "adb-wait" or "adb-connect" or "adb-disconnect"
            or "flash" or "flash-back")
            return DeviceRequiredMode.None;

        if (!requiresDevice)
            return DeviceRequiredMode.None;

        if (kind is OperationKind.Reboot or OperationKind.RebootRecovery)
            return DeviceRequiredMode.AdbTransport;

        if (id is "boot-device" or "reboot" or "reboot-rec" or "reboot-fb"
            or "adb-reboot" or "adb-reboot-recovery" or "adb-reboot-bootloader"
            or "adb-sideload" or "adb-state")
            return DeviceRequiredMode.AdbTransport;

        return DeviceRequiredMode.AdbDevice;
    }
}
