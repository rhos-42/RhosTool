using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace RhosTool.App.Services;

/// <summary>
/// Liens ressources — lieux MediaFire (dossiers) ou fichiers.
/// Fichier : %AppData%\RhosTool\resource-links.json
/// </summary>
public static class ResourceLinkStore
{
    private const string MfAllDriver = "https://www.mediafire.com/folder/wlbrfxg0zdztx/All_Driver";
    private const string MfRamdiskA12 = "https://www.mediafire.com/folder/x5r397fbnv65r/Ramdisk_A12-A13";
    private const string MfRamdiskIphone = "https://www.mediafire.com/folder/167i63pk2kg37/Ramdisk_iPhone";
    private const string MfRamdiskIpad = "https://www.mediafire.com/folder/i50xlceciezlh/Ramdisk_iPad";
    private const string MfTools = "https://www.mediafire.com/folder/9z3c0klwqfw71/Tool_Android-PC";
    private const string MfPacks = "https://www.mediafire.com/folder/k7o5g32nyytdr/Packs_Diag";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private static readonly Regex MediaFireFileRegex = new(
        @"mediafire\.com/(?:file|download|view|file_premium)/([a-zA-Z0-9]+)(?:/([^/?#]+))?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex MediaFireFolderRegex = new(
        @"mediafire\.com/folder/([a-zA-Z0-9]+)(?:/([^/?#]+))?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string FilePath => Path.Combine(LocalAuthStore.AppDataDir, "resource-links.json");

    public static IReadOnlyList<ResourceLinkItem> Defaults { get; } = BuildDefaults();

    private static IReadOnlyList<ResourceLinkItem> BuildDefaults()
    {
        var list = new List<ResourceLinkItem>
        {
            D("drv-all-mobile", "All Driver", "All Driver Mobile (link direct)", MfAllDriver),
            D("drv-cert-fix", "All Driver", "CERT-FIX.zip - Fix Certificate Win 7/XP", MfAllDriver),
            D("drv-apple-usb", "All Driver", "Driver Apple usb.zip", MfAllDriver),
            D("drv-huawei-kirin", "All Driver", "Driver Huawei USB 1.0 Kirin Flash Device 2.01.02.00.zip", MfAllDriver),
            D("drv-huawei-usb10", "All Driver", "Driver Huawei USB1.0.zip", MfAllDriver),
            D("drv-libusb-filter", "All Driver", "Driver libusb win32 devel-filter-1.2.6.0.zip", MfAllDriver),
            D("drv-nokia-mtk", "All Driver", "Driver Nokia 20 MTK_105_107_109.zip", MfAllDriver),
            D("drv-nokia-full", "All Driver", "Driver Nokia Full.zip", MfAllDriver),
            D("drv-qc-mtk", "All Driver", "Driver Qualcomm (QC) + MTK 2.0.1.zip", MfAllDriver),
            D("drv-qc-mtk-mf", "All Driver", "Driver Qualcomm (QC) + MTK 2.0.1.zip (mediafire)", MfAllDriver),
            D("drv-spd-77xx", "All Driver", "Driver SPD Spreadtrum 77xx.zip", MfAllDriver),
            D("drv-spd-77xx-mf", "All Driver", "Driver SPD Spreadtrum 77xx.zip (mediafire)", MfAllDriver),
            D("drv-sprd-npi", "All Driver", "Driver SPRD NPI USB_1.4.zip", MfAllDriver),
            D("drv-installer-pack", "All Driver", "Driver Install Pack.zip (mediafire)", MfAllDriver),
            D("drv-installer-alt", "All Driver", "Driver Installer Pack.zip (mediafire)", MfAllDriver),
            D("drv-fix-mtk-com", "All Driver", "Fix Driver MTK (Error COM) For Win 10/11", MfAllDriver),
            D("drv-fix-mtk-com-mf", "All Driver", "Fix Driver MTK (Error COM) For Win 10/11 (mediafire)", MfAllDriver),
            D("drv-fix-qc", "All Driver", "Fix Driver Qualcomm (QC) Setup Installer-10037.3.zip", MfAllDriver),
            D("drv-winra1n", "All Driver", "Tool WinRa1n2.1 Jailbreak iPhone - iPad (mediafire)", MfAllDriver),
            D("drv-vcredist-2015", "All Driver", "Vcredist 2015.zip", MfAllDriver),

            D("rd-a12-iphone-xr", "Ramdisk A12 - A13", "iPhone XR", MfRamdiskA12),
            D("rd-a12-iphone-xs", "Ramdisk A12 - A13", "iPhone Xs", MfRamdiskA12),
            D("rd-a12-iphone-xs-max", "Ramdisk A12 - A13", "iPhone Xs Max", MfRamdiskA12),
            D("rd-a12-iphone-11", "Ramdisk A12 - A13", "iPhone 11", MfRamdiskA12),
            D("rd-a12-iphone-11-pro", "Ramdisk A12 - A13", "iPhone 11 Pro", MfRamdiskA12),
            D("rd-a12-iphone-11-pro-max", "Ramdisk A12 - A13", "iPhone 11 Pro Max", MfRamdiskA12),
            D("rd-a12-iphone-se-2020", "Ramdisk A12 - A13", "iPhone SE 2020", MfRamdiskA12),
            D("rd-a12-ipad-8", "Ramdisk A12 - A13", "iPad 8 (8th generation)", MfRamdiskA12),
            D("rd-a12-ipad-9", "Ramdisk A12 - A13", "iPad 9 (9th generation)", MfRamdiskA12),
            D("rd-a12-ipad-air-3", "Ramdisk A12 - A13", "iPad Air 3 (3rd generation)", MfRamdiskA12),
            D("rd-a12-ipad-mini-5", "Ramdisk A12 - A13", "iPad Mini 5 (5th generation)", MfRamdiskA12),

            D("rd-iphone-5se", "Ramdisk iPhone", "Ramdisk iPhone 5SE", MfRamdiskIphone),
            D("rd-iphone-6g", "Ramdisk iPhone", "Ramdisk iPhone 6G", MfRamdiskIphone),
            D("rd-iphone-6-plus", "Ramdisk iPhone", "Ramdisk iPhone 6 Plus", MfRamdiskIphone),
            D("rd-iphone-6s", "Ramdisk iPhone", "Ramdisk iPhone 6s", MfRamdiskIphone),
            D("rd-iphone-6s-plus", "Ramdisk iPhone", "Ramdisk iPhone 6s Plus", MfRamdiskIphone),
            D("rd-iphone-7g", "Ramdisk iPhone", "Ramdisk iPhone 7G", MfRamdiskIphone),
            D("rd-iphone-7-plus", "Ramdisk iPhone", "Ramdisk iPhone 7 Plus", MfRamdiskIphone),
            D("rd-iphone-8g", "Ramdisk iPhone", "Ramdisk iPhone 8G", MfRamdiskIphone),
            D("rd-iphone-8-plus", "Ramdisk iPhone", "Ramdisk iPhone 8 Plus", MfRamdiskIphone),
            D("rd-iphone-x", "Ramdisk iPhone", "Ramdisk iPhone X", MfRamdiskIphone),

            D("rd-ipad-5", "Ramdisk iPad", "iPad 5 (iPad6,11 & iPad6,12)", MfRamdiskIpad),
            D("rd-ipad-6", "Ramdisk iPad", "iPad 6 (iPad7,5 & iPad7,6)", MfRamdiskIpad),
            D("rd-ipad-7", "Ramdisk iPad", "iPad 7 (iPad7,11 & iPad7,12)", MfRamdiskIpad),
            D("rd-ipad-air-2", "Ramdisk iPad", "iPad Air 2 (iPad5,3 & iPad5,4)", MfRamdiskIpad),
            D("rd-ipad-mini-4", "Ramdisk iPad", "iPad mini 4 (iPad5,1 & iPad5,2)", MfRamdiskIpad),
            D("rd-ipad-pro-105", "Ramdisk iPad", "iPad Pro 10.5 (iPad7,3 & iPad7,4)", MfRamdiskIpad),
            D("rd-ipad-pro-129-g1", "Ramdisk iPad", "iPad Pro 12.9 Gen 1 (iPad6,7 & iPad6,8)", MfRamdiskIpad),
            D("rd-ipad-pro-129-g2", "Ramdisk iPad", "iPad Pro 12.9 Gen 2 (iPad7,1 & iPad7,2)", MfRamdiskIpad),
            D("rd-ipad-pro-97", "Ramdisk iPad", "iPad Pro 9.7-inch (iPad6,3 & iPad6,4)", MfRamdiskIpad),

            D("tool-pc", "Tool Android - PC", "Tool PC", MfTools),
            D("tool-winra1n", "Tool Android - PC", "Tool WinRa1n2.1 Jailbreak iPhone - iPad", MfTools),

            D("res-diag-iphone-ipad", "Diag", "Diag iPhone - iPad", MfPacks),
            D("res-all-ramdisk-iphone", "Ramdisk", "All Ramdisk iPhone", MfPacks),
            D("res-all-ramdisk-ipad", "Ramdisk", "All Ramdisk iPad", MfPacks)
        };
        return list;
    }

    private static ResourceLinkItem D(string id, string group, string title, string url = "") =>
        new(id, group, title, url);

    public static string GetTitle(string operationId) =>
        Defaults.FirstOrDefault(d => d.Id == operationId)?.Title ?? operationId;

    public static string GetUrl(string operationId)
    {
        EnsureFile();
        var urls = LoadUrlsMutable();
        return urls.TryGetValue(operationId, out var url) ? (url ?? "").Trim() : "";
    }

    public static void SetUrl(string operationId, string url)
    {
        EnsureFile();
        var map = LoadUrlsMutable();
        map[operationId] = NormalizeUrl(url.Trim());
        Save(map);
    }

    /// <summary>
    /// Normalise MediaFire : dossier partagé, page fichier, ou CDN direct.
    /// </summary>
    public static string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return "";

        url = url.Trim().Trim('"');

        // Déjà un CDN direct MediaFire
        if (url.Contains("download", StringComparison.OrdinalIgnoreCase) &&
            url.Contains("mediafire.com", StringComparison.OrdinalIgnoreCase) &&
            !url.Contains("/file/", StringComparison.OrdinalIgnoreCase) &&
            !url.Contains("/folder/", StringComparison.OrdinalIgnoreCase))
        {
            return url;
        }

        var folder = MediaFireFolderRegex.Match(url);
        if (folder.Success)
        {
            var key = folder.Groups[1].Value;
            var name = folder.Groups[2].Success ? folder.Groups[2].Value.TrimEnd('/') : "folder";
            return $"https://www.mediafire.com/folder/{key}/{name}";
        }

        var m = MediaFireFileRegex.Match(url);
        if (m.Success)
        {
            var key = m.Groups[1].Value;
            var name = m.Groups[2].Success ? m.Groups[2].Value.TrimEnd('/') : "download";
            return $"https://www.mediafire.com/file/{key}/{name}/file";
        }

        return url;
    }

    public static IReadOnlyDictionary<string, string> LoadUrls() => LoadUrlsMutable();

    private static Dictionary<string, string> LoadUrlsMutable()
    {
        EnsureFile();
        try
        {
            var json = File.ReadAllText(FilePath, Encoding.UTF8);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions)
                   ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static void Save(Dictionary<string, string> map) =>
        File.WriteAllText(FilePath, JsonSerializer.Serialize(map, JsonOptions), Encoding.UTF8);

    public static void EnsureFile()
    {
        Dictionary<string, string> map;
        if (File.Exists(FilePath))
        {
            try
            {
                var json = File.ReadAllText(FilePath, Encoding.UTF8);
                map = JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions)
                      ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }
        else
        {
            map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        var changed = false;
        foreach (var d in Defaults)
        {
            if (!map.ContainsKey(d.Id))
            {
                map[d.Id] = d.Url;
                changed = true;
            }
            else if (string.IsNullOrWhiteSpace(map[d.Id]) && !string.IsNullOrWhiteSpace(d.Url))
            {
                // Remplit les URLs vides avec le lieu MediaFire par défaut
                map[d.Id] = d.Url;
                changed = true;
            }
        }

        foreach (var obsolete in new[] { "res-all-driver", "res-ramdisk-a12-a13", "res-ramdisk-iphone", "res-ramdisk-ipad" })
        {
            if (map.TryGetValue(obsolete, out var old) && string.IsNullOrWhiteSpace(old))
            {
                map.Remove(obsolete);
                changed = true;
            }
        }

        if (changed || !File.Exists(FilePath))
            Save(map);
    }

    /// <summary>Ouvre le lien. Retourne null si OK, sinon message d'erreur.</summary>
    public static string? TryOpen(string operationId)
    {
        EnsureFile();
        var url = GetUrl(operationId);
        var title = GetTitle(operationId);

        if (string.IsNullOrWhiteSpace(url))
            return "EMPTY"; // signal pour demander le collage MediaFire

        url = NormalizeUrl(url);

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != "file"))
        {
            return $"URL invalide pour « {title} » : {url}";
        }

        // Persiste la forme normalisée (utile pour MediaFire)
        SetUrl(operationId, url);

        Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        return null;
    }

    public sealed record ResourceLinkItem(string Id, string Group, string Title, string Url);
}
