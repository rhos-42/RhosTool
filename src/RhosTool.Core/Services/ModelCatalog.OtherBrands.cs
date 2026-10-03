using RhosTool.Core.Models;

namespace RhosTool.Core.Services;

public static partial class ModelCatalog
{
    private static void AddGooglePixel(List<PhoneModel> list)
    {
        var pixels = new (string Name, string Code, string Chip)[]
        {
            ("Pixel 6", "oriole", "Tensor"),
            ("Pixel 6 Pro", "raven", "Tensor"),
            ("Pixel 6a", "bluejay", "Tensor"),
            ("Pixel 7", "panther", "Tensor G2"),
            ("Pixel 7 Pro", "cheetah", "Tensor G2"),
            ("Pixel 7a", "lynx", "Tensor G2"),
            ("Pixel 8", "shiba", "Tensor G3"),
            ("Pixel 8 Pro", "husky", "Tensor G3"),
            ("Pixel 8a", "akita", "Tensor G3"),
            ("Pixel 9", "tokay", "Tensor G4"),
            ("Pixel 9 Pro", "caiman", "Tensor G4"),
            ("Pixel 9 Pro XL", "komodo", "Tensor G4"),
            ("Pixel 9 Pro Fold", "comet", "Tensor G4"),
            ("Pixel 9a", "tegu", "Tensor G4"),
            ("Pixel 10", "blazer", "Tensor G5"),
            ("Pixel 10 Pro", "mustang", "Tensor G5"),
            ("Pixel 10 Pro XL", "tigris", "Tensor G5"),
            ("Pixel 10 Pro Fold", "rango", "Tensor G5"),
            ("Pixel 10a", "frankel", "Tensor G5"),
            ("Pixel Fold", "felix", "Tensor G2"),
            ("Pixel Tablet", "tangorpro", "Tensor G2")
        };
        foreach (var (name, code, chip) in pixels)
            list.Add(M("google", "Pixel", $"Google {name}", code, chip, "edl", "fastboot"));
    }

    private static void AddSony(List<PhoneModel> list)
    {
        AddSimple(list, "sony", "Xperia",
            ("Sony Xperia 1 IV", "XQ-CT54", "Snapdragon 8 Gen 1"),
            ("Sony Xperia 1 V", "XQ-DQ54", "Snapdragon 8 Gen 2"),
            ("Sony Xperia 1 VI", "XQ-EC54", "Snapdragon 8 Gen 3"),
            ("Sony Xperia 1 VII", "XQ-FS54", "Snapdragon 8 Elite"),
            ("Sony Xperia 5 IV", "XQ-CQ54", "Snapdragon 8 Gen 1"),
            ("Sony Xperia 5 V", "XQ-DE54", "Snapdragon 8 Gen 2"),
            ("Sony Xperia 10 V", "XQ-DC54", "Snapdragon 695"),
            ("Sony Xperia 10 VI", "XQ-ES54", "Snapdragon 6 Gen 1"),
            ("Sony Xperia 10 VII", "XQ-FT54", "Snapdragon 6s Gen 3"),
            ("Sony Xperia Pro-I", "XQ-BE72", "Snapdragon 888"));
    }

    private static void AddOnePlus(List<PhoneModel> list)
    {
        AddSimple(list, "oneplus", "OnePlus",
            ("OnePlus 10 Pro", "NE2213", "Snapdragon 8 Gen 1"),
            ("OnePlus 10T", "CPH2413", "Snapdragon 8+ Gen 1"),
            ("OnePlus 11", "CPH2449", "Snapdragon 8 Gen 2"),
            ("OnePlus 12", "CPH2573", "Snapdragon 8 Gen 3"),
            ("OnePlus 12R", "CPH2585", "Snapdragon 8 Gen 2"),
            ("OnePlus 13", "CPH2653", "Snapdragon 8 Elite"),
            ("OnePlus 13R", "CPH2655", "Snapdragon 8 Gen 3"),
            ("OnePlus 13T", "CPH2721", "Snapdragon 8 Elite"),
            ("OnePlus 14", "CPH2801", "Snapdragon 8 Elite Gen 2"),
            ("OnePlus 14R", "CPH2803", "Snapdragon 8 Elite"));

        AddSimple(list, "oneplus", "OnePlus Nord",
            ("OnePlus Nord 3", "CPH2493", "Dimensity 9000"),
            ("OnePlus Nord 4", "CPH2661", "Snapdragon 7+ Gen 3"),
            ("OnePlus Nord 5", "CPH2747", "Dimensity 8350"),
            ("OnePlus Nord CE 3", "CPH2465", "Snapdragon 782G"),
            ("OnePlus Nord CE 4", "CPH2613", "Snapdragon 7 Gen 3"),
            ("OnePlus Nord CE4 Lite", "CPH2619", "Snapdragon 695"),
            ("OnePlus Nord CE 5", "CPH2757", "Snapdragon 7s Gen 3"),
            ("OnePlus Nord CE5 Lite", "CPH2759", "Dimensity 7025"));

        AddSimple(list, "oneplus", "OnePlus Open / Ace",
            ("OnePlus Open", "CPH2551", "Snapdragon 8 Gen 2"),
            ("OnePlus Open 2", "CPH2787", "Snapdragon 8 Elite"),
            ("OnePlus Ace 3", "PJE110", "Snapdragon 8 Gen 2"),
            ("OnePlus Ace 3 Pro", "PJX110", "Snapdragon 8 Gen 3"),
            ("OnePlus Ace 5", "PKG110", "Snapdragon 8 Gen 3"),
            ("OnePlus Ace 5 Pro", "PKB110", "Snapdragon 8 Elite"));
    }

    private static void AddNothing(List<PhoneModel> list)
    {
        AddSimple(list, "nothing", "Nothing Phone",
            ("Nothing Phone (1)", "A063", "Snapdragon 778G+"),
            ("Nothing Phone (2)", "A065", "Snapdragon 8+ Gen 1"),
            ("Nothing Phone (2a)", "A104", "Dimensity 7200 Pro"),
            ("Nothing Phone (2a) Plus", "A142", "Dimensity 7350 Pro"),
            ("Nothing Phone (3)", "A015", "Snapdragon 8s Gen 4"),
            ("Nothing Phone (3a)", "A059", "Snapdragon 7s Gen 3"),
            ("Nothing Phone (3a) Pro", "A059P", "Snapdragon 7s Gen 3"),
            ("Nothing Phone (3a) Lite", "A059L", "Dimensity 7300"));

        AddSimple(list, "nothing", "CMF",
            ("CMF Phone 1", "A015C", "Dimensity 7300"),
            ("CMF Phone 2 Pro", "A015P", "Dimensity 7300 Pro"),
            ("CMF Phone 2", "A015B", "Dimensity 7025 Ultra"));
    }

    private static void AddMotorola(List<PhoneModel> list)
    {
        AddSimple(list, "motorola", "Moto G",
            ("Moto G14", "XT2341", "Unisoc T616"),
            ("Moto G24", "XT2421", "Helio G85"),
            ("Moto G34", "XT2345", "Snapdragon 695"),
            ("Moto G35", "XT2423", "Dimensity 7020"),
            ("Moto G54", "XT2343", "Dimensity 7020"),
            ("Moto G55", "XT2435", "Dimensity 7025"),
            ("Moto G64", "XT2437", "Dimensity 7025"),
            ("Moto G84", "XT2347", "Snapdragon 695"),
            ("Moto G85", "XT2427", "Snapdragon 6s Gen 3"),
            ("Moto G86", "XT2521", "Snapdragon 6s Gen 3"),
            ("Moto G Power 2024", "XT2425", "Dimensity 7020"),
            ("Moto G Power 2025", "XT2525", "Dimensity 7025"));

        AddSimple(list, "motorola", "Edge",
            ("Motorola Edge 40", "XT2303", "Dimensity 8020"),
            ("Motorola Edge 40 Pro", "XT2301", "Snapdragon 8 Gen 2"),
            ("Motorola Edge 50", "XT2401", "Snapdragon 7 Gen 1"),
            ("Motorola Edge 50 Fusion", "XT2429E", "Snapdragon 7s Gen 2"),
            ("Motorola Edge 50 Pro", "XT2403", "Snapdragon 7 Gen 3"),
            ("Motorola Edge 50 Ultra", "XT2405", "Snapdragon 8s Gen 3"),
            ("Motorola Edge 60", "XT2501", "Dimensity 7400"),
            ("Motorola Edge 60 Pro", "XT2503", "Dimensity 7400 Ultra"),
            ("Motorola Edge 60 Fusion", "XT2505", "Dimensity 7300"),
            ("Motorola Edge 60 Stylus", "XT2507", "Dimensity 7300"));

        AddSimple(list, "motorola", "Razr",
            ("Motorola Razr 40", "XT2321", "Snapdragon 7 Gen 1"),
            ("Motorola Razr 40 Ultra", "XT2323", "Snapdragon 8+ Gen 1"),
            ("Motorola Razr 50", "XT2429", "Dimensity 7300X"),
            ("Motorola Razr 50 Ultra", "XT2451", "Snapdragon 8s Gen 3"),
            ("Motorola Razr 60", "XT2529", "Dimensity 7400X"),
            ("Motorola Razr 60 Ultra", "XT2551", "Snapdragon 8 Elite"));
    }

    private static void AddNokia(List<PhoneModel> list)
    {
        AddSimple(list, "nokia", "Nokia",
            ("Nokia G22", "TA-1528", "Unisoc T606"),
            ("Nokia G42", "TA-1581", "Snapdragon 480+"),
            ("Nokia G60", "TA-1481", "Snapdragon 695"),
            ("Nokia G410", "TA-1630", "Unisoc T606"),
            ("Nokia X30", "TA-1450", "Snapdragon 695"),
            ("Nokia XR21", "TA-1530", "Snapdragon 695"),
            ("Nokia C32", "TA-1534", "Unisoc SC9863A"),
            ("Nokia C210", "TA-1610", "Unisoc T606"),
            ("Nokia C310", "TA-1640", "Unisoc T606"),
            ("Nokia 3210 4G", "TA-1650", "Unisoc T107"));
    }

    private static void AddAsus(List<PhoneModel> list)
    {
        AddSimple(list, "asus", "Zenfone",
            ("Asus Zenfone 9", "AI2202", "Snapdragon 8+ Gen 1"),
            ("Asus Zenfone 10", "AI2302", "Snapdragon 8 Gen 2"),
            ("Asus Zenfone 11 Ultra", "AI2401", "Snapdragon 8 Gen 3"),
            ("Asus Zenfone 12 Ultra", "AI2502", "Snapdragon 8 Elite"));

        AddSimple(list, "asus", "ROG Phone",
            ("ROG Phone 6", "AI2201", "Snapdragon 8+ Gen 1"),
            ("ROG Phone 7", "AI2301", "Snapdragon 8 Gen 2"),
            ("ROG Phone 8", "AI2401R", "Snapdragon 8 Gen 3"),
            ("ROG Phone 8 Pro", "AI2401P", "Snapdragon 8 Gen 3"),
            ("ROG Phone 9", "AI2501", "Snapdragon 8 Elite"),
            ("ROG Phone 9 Pro", "AI2501P", "Snapdragon 8 Elite"),
            ("ROG Phone 9 FE", "AI2501F", "Snapdragon 8 Gen 3"));
    }

    private static void AddLg(List<PhoneModel> list)
    {
        AddSimple(list, "lg", "LG",
            ("LG K22", "LM-K200", "Snapdragon 215"),
            ("LG K42", "LM-K420", "Helio P22"),
            ("LG K52", "LM-K520", "Helio P35"),
            ("LG K62", "LM-K525", "Helio P35"),
            ("LG Velvet", "LM-G900", "Snapdragon 765G"),
            ("LG Velvet 2 Pro", "LM-V700", "Snapdragon 888"),
            ("LG Wing", "LM-F100", "Snapdragon 765G"),
            ("LG V60 ThinQ", "LM-V600", "Snapdragon 865"),
            ("LG G8 ThinQ", "LM-G820", "Snapdragon 855"));
    }

    private static void AddLenovo(List<PhoneModel> list)
    {
        AddSimple(list, "lenovo", "Lenovo Phone",
            ("Lenovo K14 Plus", "XT2151", "Snapdragon 662"),
            ("Lenovo Legion Y70", "L71061", "Snapdragon 8 Gen 1"),
            ("Lenovo Legion Y90", "L70081", "Snapdragon 8 Gen 1"),
            ("Lenovo Legion Y700", "TB320FC", "Snapdragon 8+ Gen 1"),
            ("Lenovo ThinkPhone", "XT2309", "Snapdragon 8+ Gen 1"),
            ("Lenovo ThinkPhone 25", "XT2509", "Snapdragon 8s Gen 3"));

        AddSimple(list, "lenovo", "Lenovo Tab",
            ("Lenovo Tab M9", "TB310FU", "Helio G80"),
            ("Lenovo Tab M10 Plus", "TB125FU", "Helio G80"),
            ("Lenovo Tab M11", "TB330FU", "Helio G88"),
            ("Lenovo Tab P11", "TB-J606F", "Snapdragon 662"),
            ("Lenovo Tab P12", "TB370FU", "Dimensity 7050"),
            ("Lenovo Tab Plus", "TB351FU", "Helio G99"),
            ("Lenovo Yoga Tab 13", "YT-K606F", "Snapdragon 870"),
            ("Lenovo Yoga Tab Plus", "YT-X705F", "Snapdragon 8s Gen 3"));
    }

    private static void AddZte(List<PhoneModel> list)
    {
        AddSimple(list, "zte", "ZTE",
            ("ZTE Blade A54", "ZTE 8046", "Unisoc T606"),
            ("ZTE Blade V50", "ZTE 8045", "Dimensity 6020"),
            ("ZTE Blade V60", "ZTE 8055", "Dimensity 6300"),
            ("ZTE Axon 40 Ultra", "A2023P", "Snapdragon 8 Gen 1"),
            ("ZTE Axon 50 Ultra", "A2024P", "Snapdragon 8 Gen 2"),
            ("ZTE Axon 60 Ultra", "A2025P", "Snapdragon 8 Gen 3"),
            ("nubia Red Magic 8 Pro", "NX729J", "Snapdragon 8 Gen 2"),
            ("nubia Red Magic 9 Pro", "NX769J", "Snapdragon 8 Gen 3"),
            ("nubia Red Magic 9S Pro", "NX769S", "Snapdragon 8 Gen 3"),
            ("nubia Red Magic 10 Pro", "NX789J", "Snapdragon 8 Elite"),
            ("nubia Red Magic 10S Pro", "NX789S", "Snapdragon 8 Elite"),
            ("nubia Z70 Ultra", "NX733J", "Snapdragon 8 Elite"));
    }

    private static void AddHtc(List<PhoneModel> list)
    {
        AddSimple(list, "htc", "HTC",
            ("HTC U11", "2PZC100", "Snapdragon 835"),
            ("HTC U12+", "2Q55100", "Snapdragon 845"),
            ("HTC Desire 20 Pro", "2Q8N100", "Snapdragon 665"),
            ("HTC Desire 21 Pro 5G", "2Q9N100", "Snapdragon 690"),
            ("HTC Desire 22 Pro", "2Q9R200", "Snapdragon 695"),
            ("HTC U23", "2Q9R150", "Snapdragon 7 Gen 1"),
            ("HTC U23 Pro", "2Q9R100", "Snapdragon 7 Gen 1"),
            ("HTC U24 Pro", "2Q9R300", "Snapdragon 7s Gen 2"));
    }

    private static void AddMeizu(List<PhoneModel> list)
    {
        AddSimple(list, "meizu", "Meizu",
            ("Meizu 18", "M181Q", "Snapdragon 888"),
            ("Meizu 18 Pro", "M182Q", "Snapdragon 888"),
            ("Meizu 20", "M2011", "Snapdragon 8 Gen 2"),
            ("Meizu 20 Pro", "M2012", "Snapdragon 8 Gen 2"),
            ("Meizu 21", "M2111", "Snapdragon 8 Gen 3"),
            ("Meizu 21 Note", "M2161", "Snapdragon 7s Gen 2"),
            ("Meizu 21 Pro", "M2112", "Snapdragon 8 Gen 3"),
            ("Meizu Lucky 08", "M2211", "Snapdragon 7 Gen 1"),
            ("Meizu Note 21", "M2162", "Snapdragon 4 Gen 2"),
            ("Meizu Note 22", "M2262", "Snapdragon 6s Gen 3"));
    }

    private static void AddVsmart(List<PhoneModel> list)
    {
        AddSimple(list, "vsmart", "Vsmart",
            ("Vsmart Joy 3", "VVS-J3", "Snapdragon 632"),
            ("Vsmart Joy 4", "VVS-J4", "Snapdragon 665"),
            ("Vsmart Live", "VVS-L1", "Snapdragon 675"),
            ("Vsmart Live 4", "VVS-L4", "Snapdragon 675"),
            ("Vsmart Active 3", "VVS-A3", "Helio P60"),
            ("Vsmart Star 5", "VVS-S5", "Unisoc SC9863A"),
            ("Vsmart Aris", "VVS-AR", "Snapdragon 730"),
            ("Vsmart Aris Pro", "VVS-ARP", "Snapdragon 730"));
    }

    private static void AddTcl(List<PhoneModel> list)
    {
        AddSimple(list, "tcl", "TCL",
            ("TCL 40 SE", "T610K", "Helio G37"),
            ("TCL 40 NxtPaper", "T612B", "Helio G80"),
            ("TCL 50 SE", "T650H", "Helio G85"),
            ("TCL 50 XL", "T653H", "Dimensity 6080"),
            ("TCL 50 NxtPaper", "T655H", "Dimensity 6080"),
            ("TCL 60 XE", "T660H", "Unisoc T606"),
            ("TCL 60 SE", "T661H", "Helio G85"),
            ("TCL 60 NxtPaper", "T665H", "Dimensity 6300"),
            ("TCL NxtPaper 11", "9183X", "Helio G80"),
            ("TCL NxtPaper 14", "9494X", "Dimensity 7020"));
    }

    private static void AddAlcatel(List<PhoneModel> list)
    {
        AddSimple(list, "alcatel", "Alcatel",
            ("Alcatel 1B 2022", "5031G", "Unisoc SC9863A"),
            ("Alcatel 1L Pro", "5030F", "Unisoc SC9863A"),
            ("Alcatel 3L 2021", "5034D", "Helio A22"),
            ("Alcatel 3X 2020", "5061U", "Helio P22"),
            ("Alcatel V3 Ultra", "5094A", "Dimensity 7020"),
            ("Alcatel 1T 10", "8088X", "Unisoc SC7731E"),
            ("Alcatel 3T 10", "8094X", "Unisoc T606"));
    }

    private static void AddSharp(List<PhoneModel> list)
    {
        AddSimple(list, "sharp", "Aquos",
            ("Sharp Aquos Sense 7", "SH-M24", "Snapdragon 695"),
            ("Sharp Aquos Sense 8", "SH-M25", "Snapdragon 6 Gen 1"),
            ("Sharp Aquos Sense 9", "SH-M26", "Snapdragon 7s Gen 2"),
            ("Sharp Aquos R8", "SH-52D", "Snapdragon 8 Gen 2"),
            ("Sharp Aquos R8 Pro", "SH-51D", "Snapdragon 8 Gen 2"),
            ("Sharp Aquos R9", "SH-51E", "Snapdragon 7 Gen 3"),
            ("Sharp Aquos R9 Pro", "SH-52E", "Snapdragon 8 Gen 3"),
            ("Sharp Aquos Wish 3", "A302SH", "Snapdragon 480+"),
            ("Sharp Aquos Wish 4", "A402SH", "Snapdragon 6 Gen 1"));
    }

    private static void AddQualcomm(List<PhoneModel> list)
    {
        var platforms = new[]
        {
            "Snapdragon 680", "Snapdragon 695", "Snapdragon 7 Gen 3", "Snapdragon 7s Gen 2",
            "Snapdragon 7s Gen 3", "Snapdragon 8 Gen 2", "Snapdragon 8 Gen 3",
            "Snapdragon 8s Gen 3", "Snapdragon 8 Elite", "Snapdragon 8 Elite Gen 2"
        };
        for (var n = 1; n <= 100; n++)
        {
            list.Add(M("qualcomm", "EDL Devices",
                $"Qualcomm EDL Device {n:000}", $"QC-EDL-{n:000}",
                platforms[n % platforms.Length], "edl", "testpoint"));
        }
    }

    private static void AddMediatek(List<PhoneModel> list)
    {
        var platforms = new[]
        {
            "Helio G85", "Helio G99", "Helio G100", "Dimensity 6300", "Dimensity 7025",
            "Dimensity 7300", "Dimensity 8300", "Dimensity 8400", "Dimensity 9300", "Dimensity 9400"
        };
        for (var n = 1; n <= 100; n++)
        {
            list.Add(M("mediatek", "BROM Devices",
                $"MediaTek BROM Device {n:000}", $"MTK-BROM-{n:000}",
                platforms[n % platforms.Length], "preloader", "custom da", "testpoint"));
        }
    }

    private static void AddSpreadtrum(List<PhoneModel> list)
    {
        var platforms = new[] { "Unisoc T606", "Unisoc T612", "Unisoc T616", "Unisoc T760", "SC9863A" };
        for (var n = 1; n <= 80; n++)
        {
            list.Add(M("spreadtrum", "SPD Devices",
                $"Spreadtrum / Unisoc Device {n:000}", $"SPD-{n:000}",
                platforms[n % platforms.Length], "preloader", "testpoint"));
        }
    }

    private static void AddAndroidGeneric(List<PhoneModel> list)
    {
        for (var n = 1; n <= 50; n++)
        {
            list.Add(M("android", "ADB", $"Generic ADB Android {n:000}", $"ADB-{n:000}", "Unknown", "adb"));
            list.Add(M("android", "Fastboot", $"Generic Fastboot Device {n:000}", $"FB-{n:000}", "Unknown", "fastboot"));
            list.Add(M("android", "Recovery", $"Generic Recovery Device {n:000}", $"REC-{n:000}", "Unknown", "recovery"));
        }
    }

    private static void AddApple(List<PhoneModel> list)
    {
        var iphones = new (string Name, string Chip)[]
        {
            ("iPhone 11", "A13"), ("iPhone 11 Pro", "A13"), ("iPhone 11 Pro Max", "A13"),
            ("iPhone 12", "A14"), ("iPhone 12 mini", "A14"), ("iPhone 12 Pro", "A14"), ("iPhone 12 Pro Max", "A14"),
            ("iPhone 13", "A15"), ("iPhone 13 mini", "A15"), ("iPhone 13 Pro", "A15"), ("iPhone 13 Pro Max", "A15"),
            ("iPhone 14", "A15"), ("iPhone 14 Plus", "A15"), ("iPhone 14 Pro", "A16"), ("iPhone 14 Pro Max", "A16"),
            ("iPhone 15", "A16"), ("iPhone 15 Plus", "A16"), ("iPhone 15 Pro", "A17 Pro"), ("iPhone 15 Pro Max", "A17 Pro"),
            ("iPhone 16", "A18"), ("iPhone 16 Plus", "A18"), ("iPhone 16 Pro", "A18 Pro"), ("iPhone 16 Pro Max", "A18 Pro"),
            ("iPhone 16e", "A18"), ("iPhone SE (3rd)", "A15"),
            ("iPhone 17", "A19"), ("iPhone 17 Air", "A19"), ("iPhone 17 Pro", "A19 Pro"), ("iPhone 17 Pro Max", "A19 Pro")
        };
        foreach (var (name, chip) in iphones)
        {
            foreach (var storage in new[] { "128GB", "256GB", "512GB", "1TB" })
            {
                if (storage == "1TB" && !name.Contains("Pro", StringComparison.Ordinal))
                    continue;
                list.Add(M("apple", "iPhone", $"{name} {storage}",
                    name.Replace(" ", "").Replace("(", "").Replace(")", ""), chip, "ios"));
            }
        }

        var ipads = new (string Name, string Chip)[]
        {
            ("iPad (10th gen)", "A14"),
            ("iPad (11th gen)", "A16"),
            ("iPad Air (M2)", "M2"),
            ("iPad Air (M3)", "M3"),
            ("iPad mini (6th gen)", "A15"),
            ("iPad mini (A17 Pro)", "A17 Pro"),
            ("iPad Pro 11 (M4)", "M4"),
            ("iPad Pro 13 (M4)", "M4"),
            ("iPad Pro 11 (M5)", "M5"),
            ("iPad Pro 13 (M5)", "M5")
        };
        foreach (var (name, chip) in ipads)
            list.Add(M("apple", "iPad", name, name.Replace(" ", ""), chip, "ios"));
    }

    private static void AddSimple(
        List<PhoneModel> list,
        string brandId,
        string series,
        params (string Name, string Code, string Chip)[] models)
    {
        foreach (var (name, code, chip) in models)
            list.Add(M(brandId, series, name, code, chip));
    }
}
