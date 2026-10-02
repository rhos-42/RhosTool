using RhosTool.Core.Models;

namespace RhosTool.Core.Services;

public static class BrandCatalog
{
    public static IReadOnlyList<BrandModule> All { get; } = Build();

    private static IReadOnlyList<BrandModule> Build()
    {
        return new[]
        {
            B("xiaomi", "Mi", "Mi", "#FF6A00", AndroidModes(), XiaomiTabs()),
            B("samsung", "Samsung", "Samsung", "#1428A0", SamsungModes(), SamsungTabs()),
            B("huawei", "Huawei", "Huawei", "#CF0A2C", AndroidModes(), GenericAndroidTabs("Huawei")),
            B("honor", "Honor", "Honor", "#000000", AndroidModes(), GenericAndroidTabs("Honor")),
            B("oppo", "Oppo", "Oppo", "#1AAF5D", AndroidModes(), GenericAndroidTabs("Oppo")),
            B("realme", "Realme", "Realme", "#FFC915", AndroidModes(), GenericAndroidTabs("Realme")),
            B("vivo", "Vivo", "Vivo", "#415FFF", AndroidModes(), GenericAndroidTabs("Vivo")),
            B("google", "Google", "Pixel", "#4285F4", AndroidModes(), GenericAndroidTabs("Google Pixel")),
            B("sony", "Sony", "Sony", "#000000", AndroidModes(), GenericAndroidTabs("Sony")),
            B("oneplus", "OnePlus", "OnePlus", "#F5010C", AndroidModes(), GenericAndroidTabs("OnePlus")),
            B("nothing", "Nothing", "Nothing", "#1A1A1A", AndroidModes(), GenericAndroidTabs("Nothing")),
            B("motorola", "Motorola", "Motorola", "#5A5A5A", AndroidModes(), GenericAndroidTabs("Motorola")),
            B("nokia", "Nokia", "Nokia", "#124191", AndroidModes(), GenericAndroidTabs("Nokia")),
            B("asus", "Asus", "Asus", "#0A1F44", AndroidModes(), GenericAndroidTabs("Asus")),
            B("lg", "LG", "LG", "#A50034", AndroidModes(), GenericAndroidTabs("LG")),
            B("lenovo", "Lenovo", "Lenovo", "#E2231A", AndroidModes(), GenericAndroidTabs("Lenovo")),
            B("zte", "ZTE", "ZTE", "#0A56A8", AndroidModes(), GenericAndroidTabs("ZTE")),
            B("htc", "HTC", "HTC", "#6D6E71", AndroidModes(), GenericAndroidTabs("HTC")),
            B("tecno", "Tecno", "Tecno", "#0033A0", AndroidModes(), GenericAndroidTabs("Tecno")),
            B("infinix", "Infinix", "Infinix", "#00A651", AndroidModes(), GenericAndroidTabs("Infinix")),
            B("meizu", "Meizu", "Meizu", "#7CB518", AndroidModes(), GenericAndroidTabs("Meizu")),
            B("vsmart", "Vsmart", "Vsmart", "#8B1E3F", AndroidModes(), GenericAndroidTabs("Vsmart")),
            B("tcl", "TCL", "TCL", "#E2001A", AndroidModes(), GenericAndroidTabs("TCL")),
            B("alcatel", "Alcatel", "Alcatel", "#FF6600", AndroidModes(), GenericAndroidTabs("Alcatel")),
            B("sharp", "Sharp", "Sharp", "#E60012", AndroidModes(), GenericAndroidTabs("Sharp")),
            B("qualcomm", "Qualcomm", "Qualcomm", "#3253DC", new[] { "EDL TOOLS", "SECURITY" }, QualcommTabs()),
            B("mediatek", "MediaTek", "MediaTek", "#F2A900", new[] { "BROM TOOLS", "SECURITY" }, MtkTabs()),
            B("spreadtrum", "Spreadtrum", "Spreadtrum", "#4A5568", new[] { "SPD TOOLS", "SECURITY" }, GenericAndroidTabs("Spreadtrum")),
            B("android", "Android", "Android", "#3DDC84", new[] { "ADB TOOLS", "SECURITY" }, GenericAndroidTabs("Android")),
            B("apple", "Apple", "Apple", "#3A3A3A", new[] { "CHECKRA1N", "SECURITY" }, AppleTabs())
        };
    }

    private static string[] AndroidModes() => new[] { "FLASH", "SECURITY" };
    private static string[] SamsungModes() => new[] { "ODIN FLASH", "SECURITY" };

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

    private static IReadOnlyList<FunctionTab> SamsungTabs() => new[]
    {
        Tab("odin", "ODIN MODE",
            Op("enter-dl", "[COM] ENTER DOWNLOAD", "Guide mode Download / Odin.", OperationKind.PartnerStub, false),
            Op("enable-adb", "[ODIN] ENABLE ADB", "Vérifie / démarre ADB.", OperationKind.ReadInfo),
            Op("restore", "[ODIN] RESTORE", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("remove-frp", "[ADB] REMOVE FRP", "Service partenaire licencié.", OperationKind.PartnerStub)),
        Tab("functions", "FUNCTIONS",
            Op("read-info", "READ INFO", "Infos appareil via ADB.", OperationKind.ReadInfo),
            Op("reboot", "REBOOT", "Redémarrage ADB.", OperationKind.Reboot),
            Op("reboot-rec", "REBOOT RECOVERY", "Recovery ADB.", OperationKind.RebootRecovery),
            Op("drivers", "USB DRIVERS", "Gestionnaire de périphériques.", OperationKind.OpenDeviceManager, false),
            Op("screen", "SCREEN LOCK", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("factory", "FACTORY RESET", "Service partenaire licencié.", OperationKind.PartnerStub)),
        Tab("brom", "BROM | EDL",
            Op("edl", "ENTER EDL", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("brom", "BROM CONNECT", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("da", "CUSTOM DA", "Service partenaire licencié.", OperationKind.PartnerStub)),
        Tab("network", "NETWORK",
            Op("net", "NETWORK UNLOCK", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("imei", "IMEI INFO", "Lecture infos si ADB autorisé.", OperationKind.ReadInfo)),
        Tab("exynos", "EXYNOS",
            Op("exy-dl", "[COM] ENTER DOWNLOAD", "Mode Download Exynos.", OperationKind.PartnerStub, false),
            Op("exy-adb", "[ODIN] ENABLE ADB", "Vérifie ADB.", OperationKind.ReadInfo),
            Op("exy-rest", "[ODIN] RESTORE", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("exy-frp", "[ADB] REMOVE FRP", "Service partenaire licencié.", OperationKind.PartnerStub)),
        AppControlTab()
    };

    private static IReadOnlyList<FunctionTab> XiaomiTabs() => new[]
    {
        Tab("flash", "FASTBOOT",
            Op("fb", "FASTBOOT DEVICES", "Liste appareils fastboot si dispo.", OperationKind.ReadInfo),
            Op("reboot-fb", "REBOOT BOOTLOADER", "Redémarrage bootloader.", OperationKind.PartnerStub),
            Op("read-info", "READ INFO", "Infos ADB.", OperationKind.ReadInfo)),
        Tab("functions", "FUNCTIONS",
            Op("reboot", "REBOOT", "Redémarrage ADB.", OperationKind.Reboot),
            Op("reboot-rec", "REBOOT RECOVERY", "Recovery ADB.", OperationKind.RebootRecovery),
            Op("drivers", "USB DRIVERS", "Gestionnaire de périphériques.", OperationKind.OpenDeviceManager, false),
            Op("frp", "ERASE FRP", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("account", "MI ACCOUNT", "Service partenaire licencié.", OperationKind.PartnerStub)),
        Tab("edl", "EDL",
            Op("edl", "ENTER EDL", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("auth", "AUTH FLASH", "Service partenaire licencié.", OperationKind.PartnerStub)),
        Tab("network", "NETWORK",
            Op("net", "NETWORK UNLOCK", "Service partenaire licencié.", OperationKind.PartnerStub)),
        AppControlTab()
    };

    private static IReadOnlyList<FunctionTab> GenericAndroidTabs(string brand) => new[]
    {
        Tab("functions", "FUNCTIONS",
            Op("read-info", "READ INFO", $"Infos {brand} via ADB.", OperationKind.ReadInfo),
            Op("reboot", "REBOOT", "Redémarrage ADB.", OperationKind.Reboot),
            Op("reboot-rec", "REBOOT RECOVERY", "Recovery ADB.", OperationKind.RebootRecovery),
            Op("drivers", "USB DRIVERS", "Gestionnaire de périphériques.", OperationKind.OpenDeviceManager, false),
            Op("frp", "ERASE FRP", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("factory", "FACTORY RESET", "Service partenaire licencié.", OperationKind.PartnerStub)),
        Tab("edl", "BROM | EDL",
            Op("edl", "ENTER EDL / BROM", "Service partenaire licencié.", OperationKind.PartnerStub)),
        Tab("network", "NETWORK",
            Op("net", "NETWORK UNLOCK", "Service partenaire licencié.", OperationKind.PartnerStub)),
        AppControlTab()
    };

    private static IReadOnlyList<FunctionTab> QualcommTabs() => new[]
    {
        Tab("edl", "EDL",
            Op("edl", "QUALCOMM EDL", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("firehose", "FIREHOSE", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("drivers", "USB DRIVERS", "Gestionnaire de périphériques.", OperationKind.OpenDeviceManager, false)),
        Tab("functions", "FUNCTIONS",
            Op("read-info", "READ INFO", "Infos ADB.", OperationKind.ReadInfo),
            Op("frp", "ERASE FRP", "Service partenaire licencié.", OperationKind.PartnerStub)),
        AppControlTab()
    };

    private static IReadOnlyList<FunctionTab> MtkTabs() => new[]
    {
        Tab("brom", "BROM",
            Op("brom", "MTK BROM", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("da", "CUSTOM DA", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("drivers", "USB DRIVERS", "Gestionnaire de périphériques.", OperationKind.OpenDeviceManager, false)),
        Tab("functions", "FUNCTIONS",
            Op("read-info", "READ INFO", "Infos ADB.", OperationKind.ReadInfo),
            Op("frp", "ERASE FRP", "Service partenaire licencié.", OperationKind.PartnerStub)),
        AppControlTab()
    };

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


    private static IReadOnlyList<FunctionTab> AppleTabs() => new[]
    {
        Tab("info", "INFO",
            Op("read-info", "READ INFO", "Infos iOS si ideviceinfo.", OperationKind.ReadInfo),
            Op("drivers", "USB DRIVERS", "Gestionnaire de périphériques.", OperationKind.OpenDeviceManager, false)),
        Tab("security", "SECURITY",
            Op("icloud", "ICLOUD / ACTIVATION", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("mdm", "MDM", "Service partenaire licencié.", OperationKind.PartnerStub),
            Op("net", "NETWORK UNLOCK", "Service partenaire licencié.", OperationKind.PartnerStub))
    };

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
