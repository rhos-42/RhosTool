using RhosTool.Core.Models;

namespace RhosTool.Core.Services;

public static partial class ModelCatalog
{
    private static void AddOppo(List<PhoneModel> list)
    {
        AddSeries(list, "oppo", "OPPO A",
            ("OPPO A18", "CPH2591", "Helio G85"),
            ("OPPO A3x", "CPH2691", "Helio G85"),
            ("OPPO A38", "CPH2577", "Helio G85"),
            ("OPPO A40", "CPH2669", "Helio G85"),
            ("OPPO A5 2025", "CPH2729", "Snapdragon 6s Gen 3"),
            ("OPPO A58", "CPH2577", "Helio G85"),
            ("OPPO A60", "CPH3669", "Snapdragon 6 Gen 1"),
            ("OPPO A78", "CPH2565", "Snapdragon 680"),
            ("OPPO A79", "CPH2557", "Dimensity 6020"),
            ("OPPO A98", "CPH2529", "Snapdragon 695"),
            ("OPPO A3 Pro", "CPH2693", "Dimensity 7050"));

        AddSeries(list, "oppo", "OPPO Reno",
            ("OPPO Reno10", "CPH2531", "Dimensity 7050"),
            ("OPPO Reno10 Pro", "CPH2525", "Snapdragon 778G"),
            ("OPPO Reno11", "CPH2599", "Dimensity 7050"),
            ("OPPO Reno11 F", "CPH2603", "Dimensity 7050"),
            ("OPPO Reno12", "CPH2625", "Dimensity 7300"),
            ("OPPO Reno12 F", "CPH2637", "Dimensity 6300"),
            ("OPPO Reno12 Pro", "CPH2629", "Dimensity 7300"),
            ("OPPO Reno13", "CPH2695", "Dimensity 8350"),
            ("OPPO Reno13 F", "CPH2707", "Dimensity 6300"),
            ("OPPO Reno13 Pro", "CPH2697", "Dimensity 8350"),
            ("OPPO Reno14", "CPH2751", "Dimensity 8350 Ultra"),
            ("OPPO Reno14 Pro", "CPH2753", "Dimensity 8350 Ultra"));

        AddSeries(list, "oppo", "OPPO Find",
            ("OPPO Find X5 Pro", "CPH2305", "Snapdragon 8 Gen 1"),
            ("OPPO Find X6 Pro", "PGEM10", "Snapdragon 8 Gen 2"),
            ("OPPO Find X7 Ultra", "PJD110", "Snapdragon 8 Gen 3"),
            ("OPPO Find X8", "CPH2651", "Dimensity 9400"),
            ("OPPO Find X8 Pro", "CPH2659", "Dimensity 9400"),
            ("OPPO Find X8s", "CPH2741", "Dimensity 9400"),
            ("OPPO Find X9", "CPH2781", "Dimensity 9400+"),
            ("OPPO Find X9 Pro", "CPH2783", "Dimensity 9400+"));

        AddSeries(list, "oppo", "OPPO F",
            ("OPPO F25 Pro", "CPH2647", "Dimensity 7050"),
            ("OPPO F27", "CPH2715", "Dimensity 6300"),
            ("OPPO F27 Pro+", "CPH2717", "Dimensity 7200"),
            ("OPPO F29", "CPH2761", "Dimensity 7300"),
            ("OPPO F35", "CPH2791", "Dimensity 7300 Ultra"));
    }

    private static void AddRealme(List<PhoneModel> list)
    {
        AddSeries(list, "realme", "Realme C",
            ("Realme C51", "RMX3830", "Unisoc T612"),
            ("Realme C53", "RMX3760", "Unisoc T612"),
            ("Realme C55", "RMX3710", "Helio G88"),
            ("Realme C61", "RMX3930", "Unisoc T612"),
            ("Realme C63", "RMX3939", "Unisoc T612"),
            ("Realme C65", "RMX3910", "Helio G85"),
            ("Realme C67", "RMX3890", "Snapdragon 685"),
            ("Realme C75", "RMX3941", "Helio G92 Max"),
            ("Realme C75x", "RMX3945", "Helio G85"),
            ("Realme C85", "RMX3981", "Helio G100"));

        AddSeries(list, "realme", "Realme Number",
            ("Realme 11", "RMX3780", "Dimensity 7050"),
            ("Realme 11 Pro", "RMX3771", "Dimensity 7050"),
            ("Realme 12", "RMX3999", "Snapdragon 6 Gen 1"),
            ("Realme 12+", "RMX3998", "Dimensity 7050"),
            ("Realme 12 Pro", "RMX3993", "Snapdragon 7s Gen 2"),
            ("Realme 12 Pro+", "RMX3995", "Snapdragon 7s Gen 2"),
            ("Realme 13", "RMX3951", "Snapdragon 6s Gen 3"),
            ("Realme 13+", "RMX3953", "Dimensity 7300"),
            ("Realme 13 Pro", "RMX3992", "Snapdragon 7s Gen 2"),
            ("Realme 14", "RMX5060", "Snapdragon 6s Gen 3"),
            ("Realme 14 Pro", "RMX5070", "Snapdragon 7s Gen 3"),
            ("Realme 14 Pro+", "RMX5072", "Snapdragon 7s Gen 3"),
            ("Realme 15", "RMX5080", "Dimensity 7300 Ultra"),
            ("Realme 15 Pro", "RMX5082", "Snapdragon 7s Gen 3"));

        AddSeries(list, "realme", "Realme GT",
            ("Realme GT Neo 5", "RMX3706", "Snapdragon 8+ Gen 1"),
            ("Realme GT Neo 6", "RMX3851", "Snapdragon 8s Gen 3"),
            ("Realme GT Neo 7", "RMX5011", "Dimensity 9400"),
            ("Realme GT 5", "RMX3820", "Snapdragon 8 Gen 2"),
            ("Realme GT 6", "RMX3853", "Snapdragon 8 Gen 3"),
            ("Realme GT 6T", "RMX3854", "Snapdragon 7+ Gen 3"),
            ("Realme GT 7", "RMX5010", "Snapdragon 8 Elite"),
            ("Realme GT 7 Pro", "RMX5012", "Snapdragon 8 Elite"),
            ("Realme GT 7T", "RMX5014", "Snapdragon 8s Gen 4"));

        AddSeries(list, "realme", "Realme Narzo",
            ("Realme Narzo 60", "RMX3750", "Dimensity 6020"),
            ("Realme Narzo 60 Pro", "RMX3771", "Dimensity 7050"),
            ("Realme Narzo 70", "RMX3867", "Dimensity 7025"),
            ("Realme Narzo 70 Pro", "RMX3869", "Dimensity 7050"),
            ("Realme Narzo 70 Turbo", "RMX3871", "Dimensity 7300"),
            ("Realme Narzo 80", "RMX3881", "Dimensity 7025 Ultra"),
            ("Realme Narzo 80 Pro", "RMX3883", "Dimensity 7300"));
    }

    private static void AddVivo(List<PhoneModel> list)
    {
        AddSeries(list, "vivo", "Vivo Y",
            ("Vivo Y17s", "V2322", "Helio G85"),
            ("Vivo Y27", "V2249", "Helio G85"),
            ("Vivo Y28", "V2327", "Helio G85"),
            ("Vivo Y29", "V2430", "Helio G85"),
            ("Vivo Y36", "V2247", "Snapdragon 680"),
            ("Vivo Y39", "V2434", "Snapdragon 4 Gen 2"),
            ("Vivo Y56", "V2331", "Dimensity 6020"),
            ("Vivo Y78", "V2279", "Dimensity 6020"),
            ("Vivo Y200", "V2413", "Snapdragon 4 Gen 2"),
            ("Vivo Y300", "V2439", "Snapdragon 4 Gen 2"));

        AddSeries(list, "vivo", "Vivo V",
            ("Vivo V27", "V2231", "Dimensity 7200"),
            ("Vivo V29", "V2250", "Snapdragon 778G"),
            ("Vivo V30", "V2318", "Snapdragon 7 Gen 3"),
            ("Vivo V30 Pro", "V2319", "Dimensity 8200"),
            ("Vivo V40", "V2339", "Snapdragon 7 Gen 3"),
            ("Vivo V40 Pro", "V2340", "Dimensity 9200+"),
            ("Vivo V40 Lite", "V2341", "Snapdragon 6 Gen 1"),
            ("Vivo V50", "V2425", "Snapdragon 7 Gen 3"),
            ("Vivo V50 Pro", "V2426", "Dimensity 9200+"),
            ("Vivo V60", "V2451", "Snapdragon 7s Gen 3"));

        AddSeries(list, "vivo", "Vivo X",
            ("Vivo X90 Pro", "V2227", "Dimensity 9200"),
            ("Vivo X100", "V2309", "Dimensity 9300"),
            ("Vivo X100 Pro", "V2324", "Dimensity 9300"),
            ("Vivo X100 Ultra", "V2366", "Snapdragon 8 Gen 3"),
            ("Vivo X200", "V2405", "Dimensity 9400"),
            ("Vivo X200 Pro", "V2404", "Dimensity 9400"),
            ("Vivo X200 Pro mini", "V2407", "Dimensity 9400"),
            ("Vivo X200 FE", "V2415", "Dimensity 9300+"),
            ("Vivo X200s", "V2417", "Dimensity 9400"),
            ("Vivo X300", "V2501", "Dimensity 9400+"),
            ("Vivo X300 Pro", "V2502", "Dimensity 9400+"));

        AddSeries(list, "vivo", "iQOO",
            ("iQOO Neo 7", "I2214", "Dimensity 9000+"),
            ("iQOO Neo 8", "I2221", "Snapdragon 8 Gen 2"),
            ("iQOO Neo 9", "I2304", "Snapdragon 8 Gen 2"),
            ("iQOO Neo 9 Pro", "I2305", "Dimensity 9300"),
            ("iQOO Neo 10", "I2407", "Snapdragon 8s Gen 4"),
            ("iQOO 11", "I2212", "Snapdragon 8 Gen 2"),
            ("iQOO 12", "I2303", "Snapdragon 8 Gen 3"),
            ("iQOO 13", "I2403", "Snapdragon 8 Elite"),
            ("iQOO 13 Pro", "I2404", "Snapdragon 8 Elite"),
            ("iQOO Z9", "I2302", "Dimensity 7200"),
            ("iQOO Z9 Turbo", "I2306", "Snapdragon 8s Gen 3"),
            ("iQOO Z10", "I2410", "Dimensity 7300"));
    }

    private static void AddSeries(
        List<PhoneModel> list,
        string brandId,
        string series,
        params (string Name, string Code, string Chip)[] models)
    {
        // Une entrée unique par modèle (pas de doublon Global / même firmware)
        foreach (var (name, code, chip) in models)
            list.Add(M(brandId, series, name, code, chip));
    }
}
