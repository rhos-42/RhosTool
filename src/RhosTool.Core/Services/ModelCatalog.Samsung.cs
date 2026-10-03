using RhosTool.Core.Models;

namespace RhosTool.Core.Services;

public static partial class ModelCatalog
{
    private static void AddSamsung(List<PhoneModel> list)
    {
        var aSeries = new (string Num, string Name, string Chip)[]
        {
            ("05", "A05", "Helio G85"), ("05s", "A05s", "Snapdragon 680"),
            ("06", "A06", "Helio G85"), ("07", "A07", "Helio G85"),
            ("14", "A14", "Helio G80"), ("15", "A15", "Helio G99"),
            ("16", "A16", "Helio G99"), ("17", "A17", "Helio G99"),
            ("17", "A17 5G", "Dimensity 6300"),
            ("25", "A25", "Exynos 1280"), ("26", "A26", "Exynos 1380"),
            ("34", "A34", "Dimensity 1080"), ("35", "A35", "Exynos 1380"),
            ("36", "A36", "Exynos 1380"), ("53", "A53", "Exynos 1280"),
            ("54", "A54", "Exynos 1380"), ("55", "A55", "Exynos 1480"),
            ("56", "A56", "Exynos 1580"), ("57", "A57", "Exynos 1580"),
            ("73", "A73", "Snapdragon 778G")
        };
        foreach (var (num, name, chip) in aSeries)
        {
            foreach (var (region, label, bit) in SamsungRegions())
            {
                var codeSuffix = name.Contains("5G", StringComparison.Ordinal) ? "5G" : "";
                list.Add(M("samsung", "Galaxy A",
                    $"Samsung {name} ({label}) (BIT {bit})",
                    $"SM-A{num.ToUpperInvariant()}{region}{codeSuffix}", chip, "odin", "testpoint"));
            }
        }

        var sSeries = new (int Gen, string[] Variants, string ChipEu, string ChipUs)[]
        {
            (21, new[] { "", "+", " Ultra", " FE" }, "Exynos 2100", "Snapdragon 888"),
            (22, new[] { "", "+", " Ultra" }, "Exynos 2200", "Snapdragon 8 Gen 1"),
            (23, new[] { "", "+", " Ultra", " FE" }, "Snapdragon 8 Gen 2", "Snapdragon 8 Gen 2"),
            (24, new[] { "", "+", " Ultra", " FE" }, "Exynos 2400", "Snapdragon 8 Gen 3"),
            (25, new[] { "", "+", " Ultra", " FE", " Edge" }, "Snapdragon 8 Elite", "Snapdragon 8 Elite"),
            (26, new[] { "", "+", " Ultra", " FE", " Edge" }, "Snapdragon 8 Elite Gen 2", "Snapdragon 8 Elite Gen 2")
        };
        foreach (var (gen, variants, chipEu, chipUs) in sSeries)
        {
            foreach (var v in variants)
            {
                foreach (var (region, label, bit) in SamsungRegions().Take(6))
                {
                    var chip = region is "U" or "U1" or "W" ? chipUs : chipEu;
                    list.Add(M("samsung", "Galaxy S",
                        $"Samsung S{gen}{v} ({label}) (BIT {bit})",
                        $"SM-S{gen}{region}", chip, "odin", "edl"));
                }
            }
        }

        foreach (var note in new[] { 10, 20 })
        {
            foreach (var (region, label, bit) in SamsungRegions().Take(4))
            {
                list.Add(M("samsung", "Galaxy Note",
                    $"Samsung Note{note} ({label}) (BIT {bit})",
                    $"SM-N{note}{region}", "Exynos/Snapdragon", "odin"));
            }
        }

        for (var fold = 3; fold <= 7; fold++)
        {
            list.Add(M("samsung", "Galaxy Z", $"Samsung Z Fold{fold} (EU) (BIT {fold})", $"SM-F9{fold}B", "Snapdragon 8", "odin", "edl"));
            list.Add(M("samsung", "Galaxy Z", $"Samsung Z Flip{fold} (EU) (BIT {fold})", $"SM-F7{fold}B", "Snapdragon 8", "odin"));
            list.Add(M("samsung", "Galaxy Z", $"Samsung Z Fold{fold} (US) (BIT {fold})", $"SM-F9{fold}U", "Snapdragon 8", "odin", "edl"));
            list.Add(M("samsung", "Galaxy Z", $"Samsung Z Flip{fold} (US) (BIT {fold})", $"SM-F7{fold}U", "Snapdragon 8", "odin"));
        }

        foreach (var m in new[] { 14, 15, 16, 23, 33, 34, 35, 36, 53, 54, 55, 56 })
        {
            list.Add(M("samsung", "Galaxy M", $"Samsung M{m} (Open) (BIT {(m % 9) + 1})", $"SM-M{m}F", "Exynos/Helio", "odin", "preloader"));
            list.Add(M("samsung", "Galaxy F", $"Samsung F{m} (Open) (BIT {(m % 8) + 1})", $"SM-E{m}F", "Helio G", "odin", "preloader"));
        }

        foreach (var tab in new[]
        {
            ("A9", "SM-X115"), ("A9+", "SM-X210"), ("S9", "SM-X710"), ("S9+", "SM-X810"),
            ("S9 Ultra", "SM-X910"), ("S10", "SM-X820"), ("S10+", "SM-X920"), ("S10 Ultra", "SM-X920U")
        })
        {
            list.Add(M("samsung", "Galaxy Tab", $"Samsung Tab {tab.Item1}", tab.Item2, "Snapdragon/Exynos", "odin"));
        }
    }

    private static IEnumerable<(string Region, string Label, int Bit)> SamsungRegions()
    {
        yield return ("B", "EU", 5);
        yield return ("F", "Open", 4);
        yield return ("N", "KR", 6);
        yield return ("U", "US", 7);
        yield return ("U1", "US Unlocked", 7);
        yield return ("W", "CA", 5);
        yield return ("M", "LATAM", 3);
        yield return ("0", "JP", 4);
        yield return ("E", "EE", 4);
    }
}
