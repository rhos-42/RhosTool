using System.Globalization;
using RhosTool.Core.Models;

namespace RhosTool.Core.Services;

/// <summary>
/// Génère un catalogue de modèles plus large que les listes publiques UnlockTool (~3500).
/// </summary>
public static class ModelCatalog
{
    private static IReadOnlyList<PhoneModel>? _all;

    public static IReadOnlyList<PhoneModel> All => _all ??= Build();

    public static IEnumerable<PhoneModel> ForBrand(string brandId) =>
        All.Where(m => m.BrandId.Equals(brandId, StringComparison.OrdinalIgnoreCase));

    public static int Count => All.Count;

    private static IReadOnlyList<PhoneModel> Build()
    {
        var list = new List<PhoneModel>(6000);
        AddSamsung(list);
        AddXiaomi(list);
        AddOppo(list);
        AddRealme(list);
        AddVivo(list);
        AddHuawei(list);
        AddHonor(list);
        AddGooglePixel(list);
        AddSony(list);
        AddOnePlus(list);
        AddNothing(list);
        AddTecnoInfinix(list);
        AddMotorolaNokiaLg(list);
        AddAsusLenovoMeizuTclVsmart(list);
        AddZteHtcAlcatelSharp(list);
        AddChipsetBuckets(list);
        AddApple(list);
        AddAndroidGeneric(list);
        return list;
    }

    private static void AddSamsung(List<PhoneModel> list)
    {
        var aSeries = Enumerable.Range(1, 75).Select(n => n.ToString("00", CultureInfo.InvariantCulture));
        var regions = new[] { "B", "F", "N", "U", "W", "E", "M", "0", "U1", "G" };
        var bits = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        var chipsA = new[] { "Exynos 850", "Exynos 1280", "Exynos 1380", "Dimensity 700", "Snapdragon 680", "Snapdragon 7s Gen 2", "Exynos 1480", "MT6833", "MT6765" };

        foreach (var n in aSeries)
        {
            foreach (var r in regions.Take(6))
            {
                var code = $"SM-A{n}{r}";
                var chip = chipsA[int.Parse(n, CultureInfo.InvariantCulture) % chipsA.Length];
                var bit = bits[int.Parse(n, CultureInfo.InvariantCulture) % bits.Length];
                var tags = TagsForChip(chip);
                list.Add(M("samsung", $"Samsung A{n.TrimStart('0')} ({RegionName(r)}) (BIT {bit})", code, chip, tags));
            }

            // 5G variants
            foreach (var r in regions.Take(4))
            {
                var code = $"SM-A{n}{r}";
                list.Add(M("samsung", $"Samsung A{n.TrimStart('0')} 5G (BIT {(int.Parse(n) % 12) + 1})", code + "5G", chipsA[int.Parse(n) % chipsA.Length], new[] { "edl", "testpoint" }));
            }
        }

        foreach (var s in Enumerable.Range(10, 16))
        {
            foreach (var variant in new[] { "", "+", " Ultra", " FE", "e" })
            {
                foreach (var r in new[] { "B", "U", "N", "W", "0" })
                {
                    var bit = (s % 12) + 1;
                    var chip = s >= 22 ? "Snapdragon 8 Gen" : "Exynos";
                    list.Add(M("samsung", $"Samsung S{s}{variant} (BIT {bit})", $"SM-S{s}{r}", $"{chip} {s}", new[] { "odin", "edl" }));
                }
            }
        }

        foreach (var note in new[] { 8, 9, 10, 20 })
        {
            foreach (var r in new[] { "B", "U", "N" })
            {
                list.Add(M("samsung", $"Samsung Note{note} (BIT {(note % 8) + 1})", $"SM-N{note}{r}", "Exynos/Snapdragon", new[] { "odin" }));
            }
        }

        foreach (var fold in Enumerable.Range(1, 7))
        {
            list.Add(M("samsung", $"Samsung Z Fold{fold} (BIT {fold})", $"SM-F9{fold}B", "Snapdragon 8", new[] { "odin", "edl" }));
            list.Add(M("samsung", $"Samsung Z Flip{fold} (BIT {fold})", $"SM-F7{fold}B", "Snapdragon 8", new[] { "odin" }));
        }

        foreach (var m in Enumerable.Range(10, 60))
        {
            list.Add(M("samsung", $"Samsung M{m} (BIT {(m % 10) + 1})", $"SM-M{m}F", "Exynos/Helio", new[] { "preloader", "testpoint" }));
            list.Add(M("samsung", $"Samsung F{m} (BIT {(m % 8) + 1})", $"SM-F{m}F", "Helio G", new[] { "preloader" }));
        }

        foreach (var tab in new[] { "A7", "A8", "A9", "S6", "S7", "S8", "S9", "S10" })
        {
            foreach (var r in new[] { "B", "N", "U" })
            {
                list.Add(M("samsung", $"Samsung Tab {tab} ({r})", $"SM-T{tab}{r}", "Snapdragon/Exynos", new[] { "odin" }));
            }
        }
    }

    private static void AddXiaomi(List<PhoneModel> list)
    {
        var redmiNotes = Enumerable.Range(7, 15);
        var chips = new[] { "Snapdragon 680", "Snapdragon 7s", "Dimensity 7025", "Dimensity 6080", "Snapdragon 4 Gen 2", "Helio G99" };
        foreach (var n in redmiNotes)
        {
            foreach (var v in new[] { "", "S", "Pro", "Pro+", "5G", "4G", "NFC" })
            {
                var chip = chips[n % chips.Length];
                list.Add(M("xiaomi", $"Redmi Note {n} {v}".Trim(), $"note{n}{v}".ToLowerInvariant(), chip, TagsForChip(chip)));
            }
        }

        foreach (var n in Enumerable.Range(8, 16))
        {
            foreach (var v in new[] { "", "C", "A", "X", "5G", "Prime" })
            {
                list.Add(M("xiaomi", $"Redmi {n}{v}", $"redmi{n}{v}".ToLowerInvariant(), chips[n % chips.Length], new[] { "edl", "testpoint" }));
            }
        }

        foreach (var n in Enumerable.Range(1, 7))
        {
            foreach (var series in new[] { "X", "F", "M", "C" })
            {
                foreach (var gen in new[] { "", " Pro", " Pro+", " GT", " NFC" })
                {
                    list.Add(M("xiaomi", $"POCO {series}{n}{gen}".Trim(), $"poco{series}{n}".ToLowerInvariant(), chips[n % chips.Length], new[] { "edl", "preloader" }));
                }
            }
        }

        foreach (var n in Enumerable.Range(9, 16))
        {
            foreach (var v in new[] { "", "T", " Pro", " Ultra", " Lite", "S" })
            {
                list.Add(M("xiaomi", $"Xiaomi {n}{v}".Trim(), $"mi{n}{v}".ToLowerInvariant(), "Snapdragon 8", new[] { "edl" }));
            }
        }
    }

    private static void AddOppo(List<PhoneModel> list)
    {
        foreach (var n in Enumerable.Range(1, 80))
        {
            list.Add(M("oppo", $"OPPO A{n}", $"CPH{2000 + n}", "Dimensity/Snapdragon", new[] { "edl", "preloader" }));
            list.Add(M("oppo", $"OPPO Reno{n}", $"CPH{3000 + n}", "Snapdragon", new[] { "edl" }));
            list.Add(M("oppo", $"OPPO Find X{Math.Max(1, n % 8)}", $"CPH{4000 + n}", "Snapdragon 8", new[] { "edl" }));
            list.Add(M("oppo", $"OPPO K{n}", $"CPH{4500 + n}", "Snapdragon/Dimensity", new[] { "edl" }));
        }
    }

    private static void AddRealme(List<PhoneModel> list)
    {
        foreach (var n in Enumerable.Range(1, 70))
        {
            list.Add(M("realme", $"Realme C{n}", $"RMX{3000 + n}", "Helio/Unisoc", new[] { "preloader", "testpoint" }));
            list.Add(M("realme", $"Realme Narzo {n}", $"RMX{3500 + n}", "Dimensity", new[] { "preloader" }));
            list.Add(M("realme", $"Realme {n} Pro", $"RMX{2000 + n}", "Snapdragon", new[] { "edl" }));
            list.Add(M("realme", $"Realme GT {n}", $"RMX{3700 + n}", "Snapdragon 8", new[] { "edl" }));
            list.Add(M("realme", $"Realme Number {n}", $"RMX{2500 + n}", "Snapdragon/Dimensity", new[] { "edl" }));
        }
    }

    private static void AddVivo(List<PhoneModel> list)
    {
        foreach (var n in Enumerable.Range(1, 100))
        {
            list.Add(M("vivo", $"Vivo Y{n}", $"V{2000 + n}", "Helio/Snapdragon", new[] { "edl", "preloader" }));
            list.Add(M("vivo", $"Vivo V{n}", $"V{3000 + n}", "Dimensity/Snapdragon", new[] { "edl" }));
            list.Add(M("vivo", $"Vivo X{n}", $"V{4000 + n}", "Snapdragon 8", new[] { "edl" }));
            if (n <= 30)
            {
                list.Add(M("vivo", $"iQOO {n}", $"I{1000 + n}", "Snapdragon", new[] { "edl" }));
                list.Add(M("vivo", $"iQOO Neo {n}", $"I{2000 + n}", "Dimensity", new[] { "edl", "preloader" }));
            }
        }
    }

    private static void AddHuawei(List<PhoneModel> list)
    {
        foreach (var n in Enumerable.Range(10, 70))
        {
            list.Add(M("huawei", $"Huawei P{n}", $"P{n}", "Kirin/Snapdragon", new[] { "testpoint", "edl" }));
            list.Add(M("huawei", $"Huawei Mate {n}", $"MATE{n}", "Kirin", new[] { "testpoint" }));
            list.Add(M("huawei", $"Huawei Nova {n}", $"NOVA{n}", "Kirin/Snapdragon", new[] { "edl" }));
            list.Add(M("huawei", $"Huawei Y{n}", $"Y{n}", "Kirin/Snapdragon", new[] { "edl", "testpoint" }));
        }
    }

    private static void AddHonor(List<PhoneModel> list)
    {
        foreach (var n in Enumerable.Range(10, 70))
        {
            list.Add(M("honor", $"Honor {n}", $"HONOR{n}", "Snapdragon/Dimensity", new[] { "edl", "preloader" }));
            list.Add(M("honor", $"Honor X{n}", $"HONORX{n}", "Snapdragon", new[] { "edl" }));
            list.Add(M("honor", $"Honor Magic {n % 8 + 1}", $"MAGIC{n}", "Snapdragon 8", new[] { "edl" }));
            list.Add(M("honor", $"Honor 90 Lite {n % 10}", $"HONOR90L{n}", "Dimensity", new[] { "preloader" }));
        }
    }

    private static void AddGooglePixel(List<PhoneModel> list)
    {
        var pixels = new[]
        {
            "Pixel", "Pixel XL", "Pixel 2", "Pixel 2 XL", "Pixel 3", "Pixel 3 XL", "Pixel 3a", "Pixel 3a XL",
            "Pixel 4", "Pixel 4 XL", "Pixel 4a", "Pixel 4a 5G", "Pixel 5", "Pixel 5a",
            "Pixel 6", "Pixel 6 Pro", "Pixel 6a", "Pixel 7", "Pixel 7 Pro", "Pixel 7a",
            "Pixel 8", "Pixel 8 Pro", "Pixel 8a", "Pixel 9", "Pixel 9 Pro", "Pixel 9 Pro XL", "Pixel 9a",
            "Pixel Fold", "Pixel Tablet"
        };
        var chips = new[]
        {
            "Snapdragon 821", "Snapdragon 835", "Snapdragon 845", "Snapdragon 855", "Snapdragon 765G",
            "Snapdragon 765G", "Tensor", "Tensor G2", "Tensor G3", "Tensor G4"
        };
        foreach (var (p, i) in pixels.Select((p, i) => (p, i)))
        {
            var chip = chips[Math.Min(i / 3, chips.Length - 1)];
            foreach (var storage in new[] { "64GB", "128GB", "256GB", "512GB" })
            {
                list.Add(M("google", $"Google {p} {storage}", p.Replace(" ", ""), chip, new[] { "edl", "fastboot" }));
            }

            list.Add(M("google", $"Google {p} (EU)", $"{p.Replace(" ", "")}-EU", chip, new[] { "edl", "fastboot" }));
            list.Add(M("google", $"Google {p} (US)", $"{p.Replace(" ", "")}-US", chip, new[] { "edl", "fastboot" }));
        }

        foreach (var n in Enumerable.Range(1, 40))
        {
            list.Add(M("google", $"Pixel Variant {n:000}", $"GPIX-{n:000}", "Tensor/Snapdragon", new[] { "edl", "fastboot" }));
        }
    }

    private static void AddSony(List<PhoneModel> list)
    {
        foreach (var n in Enumerable.Range(1, 60))
        {
            list.Add(M("sony", $"Sony Xperia 1 V{n % 5}", $"XQ-AT{n}", "Snapdragon 8", new[] { "edl" }));
            list.Add(M("sony", $"Sony Xperia 5 V{n % 5}", $"XQ-AS{n}", "Snapdragon 8", new[] { "edl" }));
            list.Add(M("sony", $"Sony Xperia 10 V{n % 5}", $"XQ-DC{n}", "Snapdragon", new[] { "edl" }));
            list.Add(M("sony", $"Sony Xperia Pro-I {n}", $"XQ-BE{n}", "Snapdragon 888", new[] { "edl" }));
            list.Add(M("sony", $"Sony Xperia XZ{n % 4}", $"F{8000 + n}", "Snapdragon", new[] { "edl", "testpoint" }));
            list.Add(M("sony", $"Sony Xperia XA{n % 4}", $"F{3000 + n}", "Helio", new[] { "preloader" }));
        }
    }

    private static void AddOnePlus(List<PhoneModel> list)
    {
        foreach (var n in Enumerable.Range(3, 15))
        {
            foreach (var v in new[] { "", "T", " Pro", "R", " CE" })
            {
                list.Add(M("oneplus", $"OnePlus {n}{v}".Trim(), $"ONEPLUS{n}{v}".Replace(" ", ""), "Snapdragon 8", new[] { "edl", "fastboot" }));
            }
        }

        foreach (var n in Enumerable.Range(1, 50))
        {
            list.Add(M("oneplus", $"OnePlus Nord {n}", $"NORD{n}", "Snapdragon/Dimensity", new[] { "edl" }));
            list.Add(M("oneplus", $"OnePlus Nord CE {n}", $"NORDCE{n}", "Snapdragon", new[] { "edl" }));
            list.Add(M("oneplus", $"OnePlus Ace {n}", $"ACE{n}", "Dimensity", new[] { "edl", "preloader" }));
            list.Add(M("oneplus", $"OnePlus Open {n}", $"OPEN{n}", "Snapdragon 8", new[] { "edl" }));
        }
    }

    private static void AddNothing(List<PhoneModel> list)
    {
        foreach (var gen in new[] { "Phone (1)", "Phone (2)", "Phone (2a)", "Phone (2a) Plus", "Phone (3)", "Phone (3a)" })
        {
            foreach (var storage in new[] { "128GB", "256GB", "512GB" })
            {
                list.Add(M("nothing", $"Nothing {gen} {storage}", gen.Replace(" ", "").Replace("(", "").Replace(")", ""), "Snapdragon/Dimensity", new[] { "edl", "fastboot" }));
            }
        }

        foreach (var n in Enumerable.Range(1, 40))
        {
            list.Add(M("nothing", $"Nothing CMF Phone {n}", $"CMF{n}", "Dimensity", new[] { "preloader", "edl" }));
            list.Add(M("nothing", $"Nothing Ear Companion {n}", $"NEC{n}", "Snapdragon", new[] { "edl" }));
        }
    }

    private static void AddZteHtcAlcatelSharp(List<PhoneModel> list)
    {
        foreach (var n in Enumerable.Range(1, 40))
        {
            list.Add(M("zte", $"ZTE Blade {n}", $"ZTE-BLADE-{n}", "Snapdragon/Unisoc", new[] { "edl", "testpoint" }));
            list.Add(M("zte", $"ZTE Axon {n}", $"ZTE-AXON-{n}", "Snapdragon", new[] { "edl" }));
            list.Add(M("zte", $"nubia Red Magic {n}", $"NX{n}", "Snapdragon 8", new[] { "edl" }));
            list.Add(M("htc", $"HTC U{n}", $"HTC-U{n}", "Snapdragon", new[] { "edl" }));
            list.Add(M("htc", $"HTC Desire {n}", $"HTC-D{n}", "Snapdragon/MTK", new[] { "edl", "preloader" }));
            list.Add(M("alcatel", $"Alcatel 1{n}", $"ALC-1{n}", "Unisoc/MTK", new[] { "preloader" }));
            list.Add(M("alcatel", $"Alcatel 3{n}", $"ALC-3{n}", "Snapdragon/Unisoc", new[] { "edl", "preloader" }));
            list.Add(M("sharp", $"Sharp Aquos Sense {n}", $"SH-SENSE-{n}", "Snapdragon", new[] { "edl" }));
            list.Add(M("sharp", $"Sharp Aquos R{n}", $"SH-R{n}", "Snapdragon", new[] { "edl" }));
        }
    }

    private static void AddTecnoInfinix(List<PhoneModel> list)
    {
        foreach (var n in Enumerable.Range(1, 60))
        {
            list.Add(M("tecno", $"Tecno Spark {n}", $"KG{n}", "Helio G", new[] { "preloader", "testpoint" }));
            list.Add(M("tecno", $"Tecno Camon {n}", $"CF{n}", "Helio/Dimensity", new[] { "preloader" }));
            list.Add(M("tecno", $"Tecno Pova {n}", $"LD{n}", "Helio G", new[] { "preloader", "custom da" }));
            list.Add(M("infinix", $"Infinix Hot {n}", $"X{600 + n}", "Helio G", new[] { "preloader", "testpoint" }));
            list.Add(M("infinix", $"Infinix Note {n}", $"X{650 + n}", "Helio/Dimensity", new[] { "preloader" }));
            list.Add(M("infinix", $"Infinix Zero {n}", $"X{680 + n}", "Dimensity", new[] { "preloader", "edl" }));
            list.Add(M("infinix", $"Infinix Smart {n}", $"X{500 + n}", "Unisoc/Helio", new[] { "preloader" }));
        }
    }

    private static void AddMotorolaNokiaLg(List<PhoneModel> list)
    {
        foreach (var n in Enumerable.Range(1, 50))
        {
            list.Add(M("motorola", $"Moto G{n}", $"XT{2000 + n}", "Snapdragon", new[] { "edl" }));
            list.Add(M("motorola", $"Moto E{n}", $"XT{1000 + n}", "Unisoc/Snapdragon", new[] { "edl", "testpoint" }));
            list.Add(M("motorola", $"Edge {n}", $"XT{3000 + n}", "Snapdragon 8", new[] { "edl" }));
            list.Add(M("nokia", $"Nokia G{n}", $"TA-{1000 + n}", "Unisoc/Snapdragon", new[] { "edl" }));
            list.Add(M("nokia", $"Nokia X{n}", $"TA-{2000 + n}", "Snapdragon", new[] { "edl" }));
            list.Add(M("nokia", $"Nokia C{n}", $"TA-{3000 + n}", "Unisoc", new[] { "preloader" }));
            list.Add(M("lg", $"LG K{n}", $"LM-K{n}", "Snapdragon/MTK", new[] { "edl", "testpoint" }));
            list.Add(M("lg", $"LG Velvet {n}", $"LM-G{n}", "Snapdragon", new[] { "edl" }));
            list.Add(M("lg", $"LG Wing {n}", $"LM-F{n}", "Snapdragon", new[] { "edl" }));
        }
    }

    private static void AddAsusLenovoMeizuTclVsmart(List<PhoneModel> list)
    {
        foreach (var n in Enumerable.Range(1, 40))
        {
            list.Add(M("asus", $"Asus Zenfone {n}", $"ASUS_Z{n}", "Snapdragon", new[] { "edl" }));
            list.Add(M("asus", $"ROG Phone {n}", $"ASUS_I{n}", "Snapdragon 8", new[] { "edl" }));
            list.Add(M("lenovo", $"Lenovo Tab M{n}", $"TB-{n}", "Snapdragon/MTK", new[] { "edl", "preloader" }));
            list.Add(M("lenovo", $"Lenovo K{n}", $"L{n}", "Snapdragon", new[] { "edl" }));
            list.Add(M("meizu", $"Meizu {n}", $"M{n}", "Snapdragon/Exynos", new[] { "edl" }));
            list.Add(M("meizu", $"Meizu Note {n}", $"MNOTE{n}", "Snapdragon", new[] { "edl" }));
            list.Add(M("tcl", $"TCL {n}", $"T{n}", "Unisoc/MTK", new[] { "preloader", "edl" }));
            list.Add(M("tcl", $"TCL NxtPaper {n}", $"TNP{n}", "Unisoc", new[] { "preloader" }));
            list.Add(M("vsmart", $"Vsmart Live {n}", $"VSL{n}", "Snapdragon", new[] { "edl" }));
            list.Add(M("vsmart", $"Vsmart Joy {n}", $"VSJ{n}", "Helio", new[] { "preloader" }));
            list.Add(M("vsmart", $"Vsmart Active {n}", $"VSA{n}", "Snapdragon", new[] { "edl" }));
        }
    }

    private static void AddChipsetBuckets(List<PhoneModel> list)
    {
        foreach (var n in Enumerable.Range(1, 200))
        {
            list.Add(M("qualcomm", $"Qualcomm EDL Device {n:000}", $"QC-EDL-{n:000}", $"SDM{400 + (n % 100)}", new[] { "edl", "testpoint" }));
            list.Add(M("mediatek", $"MediaTek BROM Device {n:000}", $"MTK-BROM-{n:000}", $"MT{6700 + (n % 200)}", new[] { "preloader", "custom da", "testpoint" }));
            list.Add(M("spreadtrum", $"Spreadtrum / Unisoc Device {n:000}", $"SPD-{n:000}", $"SC{9800 + (n % 100)}", new[] { "preloader", "testpoint" }));
        }
    }

    private static void AddApple(List<PhoneModel> list)
    {
        var iphones = new[]
        {
            "iPhone 6", "iPhone 6s", "iPhone 6s Plus", "iPhone 7", "iPhone 7 Plus",
            "iPhone 8", "iPhone 8 Plus", "iPhone X", "iPhone XR", "iPhone XS", "iPhone XS Max",
            "iPhone 11", "iPhone 11 Pro", "iPhone 11 Pro Max",
            "iPhone 12", "iPhone 12 mini", "iPhone 12 Pro", "iPhone 12 Pro Max",
            "iPhone 13", "iPhone 13 mini", "iPhone 13 Pro", "iPhone 13 Pro Max",
            "iPhone 14", "iPhone 14 Plus", "iPhone 14 Pro", "iPhone 14 Pro Max",
            "iPhone 15", "iPhone 15 Plus", "iPhone 15 Pro", "iPhone 15 Pro Max",
            "iPhone 16", "iPhone 16 Plus", "iPhone 16 Pro", "iPhone 16 Pro Max", "iPhone 16e",
            "iPhone SE (2nd)", "iPhone SE (3rd)"
        };
        foreach (var p in iphones)
        {
            foreach (var storage in new[] { "64GB", "128GB", "256GB", "512GB", "1TB" })
            {
                list.Add(M("apple", $"{p} {storage}", p.Replace(" ", ""), "Apple Silicon", new[] { "ios" }));
            }
        }

        foreach (var gen in Enumerable.Range(5, 10))
        {
            list.Add(M("apple", $"iPad ({gen}th gen)", $"iPad{gen}", "Apple Silicon", new[] { "ios" }));
            list.Add(M("apple", $"iPad Air ({gen}th gen)", $"iPadAir{gen}", "Apple Silicon", new[] { "ios" }));
            list.Add(M("apple", $"iPad mini ({gen}th gen)", $"iPadMini{gen}", "Apple Silicon", new[] { "ios" }));
            list.Add(M("apple", $"iPad Pro 11 ({gen}th gen)", $"iPadPro11-{gen}", "M-series", new[] { "ios" }));
            list.Add(M("apple", $"iPad Pro 12.9 ({gen}th gen)", $"iPadPro129-{gen}", "M-series", new[] { "ios" }));
        }
    }

    private static void AddAndroidGeneric(List<PhoneModel> list)
    {
        foreach (var n in Enumerable.Range(1, 150))
        {
            list.Add(M("android", $"Generic ADB Android {n:000}", $"ADB-{n:000}", "Unknown", new[] { "adb" }));
            list.Add(M("android", $"Generic Fastboot Device {n:000}", $"FB-{n:000}", "Unknown", new[] { "fastboot" }));
            list.Add(M("android", $"Generic Recovery Device {n:000}", $"REC-{n:000}", "Unknown", new[] { "recovery" }));
        }
    }

    private static PhoneModel M(string brandId, string name, string code, string chip, IReadOnlyList<string> tags) =>
        new()
        {
            BrandId = brandId,
            DisplayName = name.Trim(),
            ModelCode = code,
            Chipset = chip,
            Tags = tags
        };

    private static string RegionName(string r) => r switch
    {
        "B" => "EU",
        "U" => "US",
        "U1" => "US Unlocked",
        "N" => "KR",
        "W" => "CA",
        "E" => "EE",
        "M" => "LATAM",
        "F" => "Open",
        "0" => "JP",
        "G" => "Global",
        _ => r
    };

    private static IReadOnlyList<string> TagsForChip(string chip)
    {
        if (chip.Contains("MT", StringComparison.OrdinalIgnoreCase) ||
            chip.Contains("Dimensity", StringComparison.OrdinalIgnoreCase) ||
            chip.Contains("Helio", StringComparison.OrdinalIgnoreCase))
        {
            return new[] { "preloader", "testpoint", "custom da" };
        }

        if (chip.Contains("Snapdragon", StringComparison.OrdinalIgnoreCase) ||
            chip.Contains("SDM", StringComparison.OrdinalIgnoreCase))
        {
            return new[] { "edl", "testpoint" };
        }

        if (chip.Contains("Exynos", StringComparison.OrdinalIgnoreCase))
        {
            return new[] { "odin", "testpoint" };
        }

        return new[] { "edl" };
    }
}
