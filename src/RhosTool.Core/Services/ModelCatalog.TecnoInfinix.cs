using RhosTool.Core.Models;

namespace RhosTool.Core.Services;

public static partial class ModelCatalog
{
    private static void AddTecno(List<PhoneModel> list)
    {
        // Un choix unique par modèle + toutes les variantes de code affichées
        AddWithVariants(list, "tecno", "Tecno Spark",
            ("Tecno Spark 7", new[] { "KF6", "KF6j", "KF6k", "KF6n", "KF6p" }, "Helio G25"),
            ("Tecno Spark 7P", new[] { "KF7", "KF7j" }, "Helio G25"),
            ("Tecno Spark 7T", new[] { "KF6p", "KF6n" }, "Helio G35"),
            ("Tecno Spark 8", new[] { "KG5", "KG5j", "KG5k", "KG5n" }, "Helio G35"),
            ("Tecno Spark 8C", new[] { "KG5h", "KG5m" }, "Unisoc T606"),
            ("Tecno Spark 8P", new[] { "KG7", "KG7n", "KG7h" }, "Helio G85"),
            ("Tecno Spark 8T", new[] { "KG6k", "KG6p" }, "Helio G37"),
            ("Tecno Spark 9", new[] { "KG5p", "KG5q" }, "Helio G37"),
            ("Tecno Spark 9T", new[] { "KG6n", "KG6p" }, "Helio G37"),
            ("Tecno Spark 9 Pro", new[] { "KH7", "KH7n" }, "Helio G85"),
            ("Tecno Spark Go 2023", new[] { "BG5", "BG5h", "BG5n" }, "Unisoc T606"),
            ("Tecno Spark Go 2024", new[] { "BG6", "BG6h", "BG6n" }, "Unisoc T606"),
            ("Tecno Spark 10", new[] { "KI5", "KI5n", "KI5q", "KI5k" }, "Helio G37"),
            ("Tecno Spark 10C", new[] { "KI5j", "KI5m" }, "Unisoc T606"),
            ("Tecno Spark 10 Pro", new[] { "KI7", "KI7n", "KI7s" }, "Helio G88"),
            ("Tecno Spark 10 5G", new[] { "KI8", "KI8n" }, "Dimensity 6020"),
            ("Tecno Spark 20", new[] { "KJ5", "KJ5n", "KJ5s", "KJ5k" }, "Helio G85"),
            ("Tecno Spark 20C", new[] { "BG7", "BG7n", "BG7h" }, "Helio G36"),
            ("Tecno Spark 20 Pro", new[] { "KJ6", "KJ6n", "KJ6s" }, "Helio G99"),
            ("Tecno Spark 20 Pro+", new[] { "KJ8", "KJ8n", "KJ8s" }, "Helio G99 Ultra"),
            ("Tecno Spark 20 5G", new[] { "KJ7", "KJ7n" }, "Dimensity 6020"),
            ("Tecno Spark 30", new[] { "KL5", "KL5n", "KL5k", "KL5s" }, "Helio G85"),
            ("Tecno Spark 30C", new[] { "KL7", "KL7n", "KL7h" }, "Helio G85"),
            ("Tecno Spark 30 Pro", new[] { "KL6", "KL6n", "KL6s" }, "Helio G100"),
            ("Tecno Spark 30 5G", new[] { "KL8", "KL8n" }, "Dimensity 6300"),
            ("Tecno Spark 40", new[] { "KM5", "KM5n", "KM5k" }, "Helio G100"),
            ("Tecno Spark 40 Pro", new[] { "KM6", "KM6n", "KM6s" }, "Helio G100 Ultra"),
            ("Tecno Spark 40 Pro+", new[] { "KM8", "KM8n" }, "Helio G100 Ultra"),
            ("Tecno Spark 40 5G", new[] { "KM7", "KM7n" }, "Dimensity 6300"),
            ("Tecno Spark 6 Go", new[] { "KE5", "KE5j", "KE5k" }, "Helio A20"),
            ("Tecno Spark 6 Air", new[] { "KE5j", "KE5n" }, "Helio A20"),
            ("Tecno Spark 5 Pro", new[] { "KD7", "KD7j", "KD7n" }, "Helio G80"),
            ("Tecno Spark 4 Air", new[] { "KC1", "KC1j" }, "Helio A20"),
            ("Tecno Spark 4 Lite", new[] { "KC2", "KC2j" }, "Helio A20"));

        AddWithVariants(list, "tecno", "Tecno Camon",
            ("Tecno Camon 17", new[] { "CG6", "CG6n", "CG6j" }, "Helio G85"),
            ("Tecno Camon 17P", new[] { "CG7", "CG7n" }, "Helio G85"),
            ("Tecno Camon 17 Pro", new[] { "CG8", "CG8n" }, "Dimensity 800U"),
            ("Tecno Camon 18", new[] { "CH6", "CH6n", "CH6h" }, "Helio G85"),
            ("Tecno Camon 18P", new[] { "CH7", "CH7n" }, "Helio G96"),
            ("Tecno Camon 18 Premier", new[] { "CH9", "CH9n" }, "Dimensity 900"),
            ("Tecno Camon 18i", new[] { "CH6i", "CH6j" }, "Helio G85"),
            ("Tecno Camon 19", new[] { "CI6", "CI6n" }, "Helio G85"),
            ("Tecno Camon 19 Pro", new[] { "CI7", "CI7n", "CI7s" }, "Helio G96"),
            ("Tecno Camon 19 Pro 5G", new[] { "CI8", "CI8n" }, "Dimensity 810"),
            ("Tecno Camon 19 Neo", new[] { "CI6n", "CI6j" }, "Helio G85"),
            ("Tecno Camon 20", new[] { "CK6", "CK6n", "CK6s" }, "Helio G85"),
            ("Tecno Camon 20 Pro", new[] { "CK7", "CK7n", "CK7s" }, "Helio G99"),
            ("Tecno Camon 20 Pro 5G", new[] { "CK8", "CK8n" }, "Dimensity 8050"),
            ("Tecno Camon 20 Premier", new[] { "CK9", "CK9n" }, "Dimensity 8050"),
            ("Tecno Camon 30", new[] { "CL6", "CL6n", "CL6s" }, "Dimensity 7025"),
            ("Tecno Camon 30S", new[] { "CL7", "CL7n" }, "Helio G100"),
            ("Tecno Camon 30 Pro", new[] { "CL8", "CL8n", "CL8s" }, "Dimensity 8200 Ultimate"),
            ("Tecno Camon 30 Premier", new[] { "CL9", "CL9n" }, "Dimensity 8200 Ultimate"),
            ("Tecno Camon 40", new[] { "CM6", "CM6n", "CM6s" }, "Dimensity 7025 Ultra"),
            ("Tecno Camon 40 Pro", new[] { "CM8", "CM8n", "CM8s" }, "Dimensity 7300 Ultra"),
            ("Tecno Camon 40 Premier", new[] { "CM9", "CM9n" }, "Dimensity 8350"),
            ("Tecno Camon 40 5G", new[] { "CM7", "CM7n" }, "Dimensity 7025 Ultra"));

        AddWithVariants(list, "tecno", "Tecno Pova",
            ("Tecno Pova", new[] { "LD7", "LD7n" }, "Helio G80"),
            ("Tecno Pova 2", new[] { "LE7", "LE7n" }, "Helio G85"),
            ("Tecno Pova 3", new[] { "LF7", "LF7n" }, "Helio G88"),
            ("Tecno Pova Neo", new[] { "LE6", "LE6n" }, "Helio G25"),
            ("Tecno Pova Neo 2", new[] { "LG6", "LG6n" }, "Helio G37"),
            ("Tecno Pova Neo 3", new[] { "LH6", "LH6n" }, "Helio G85"),
            ("Tecno Pova 4", new[] { "LG7", "LG7n", "LG7h" }, "Helio G99"),
            ("Tecno Pova 4 Pro", new[] { "LG8", "LG8n" }, "Dimensity 900"),
            ("Tecno Pova 5", new[] { "LH7", "LH7n", "LH7s" }, "Helio G99"),
            ("Tecno Pova 5 Pro", new[] { "LH8", "LH8n", "LH8s" }, "Dimensity 6080"),
            ("Tecno Pova 6", new[] { "LJ7", "LJ7n", "LJ7s" }, "Dimensity 7020"),
            ("Tecno Pova 6 Neo", new[] { "LJ6", "LJ6n" }, "Helio G99"),
            ("Tecno Pova 6 Pro", new[] { "LJ8", "LJ8n", "LJ8s" }, "Dimensity 6080"),
            ("Tecno Pova 6 5G", new[] { "LJ9", "LJ9n" }, "Dimensity 6300"),
            ("Tecno Pova 7", new[] { "LK7", "LK7n", "LK7s" }, "Dimensity 7025 Ultra"),
            ("Tecno Pova 7 Pro", new[] { "LK8", "LK8n", "LK8s" }, "Dimensity 7300"),
            ("Tecno Pova 7 Ultra", new[] { "LK9", "LK9n" }, "Dimensity 8350"),
            ("Tecno Pova 7 5G", new[] { "LK6", "LK6n" }, "Dimensity 7025"));

        AddWithVariants(list, "tecno", "Tecno Phantom",
            ("Tecno Phantom X", new[] { "AD10", "AD10n" }, "Helio G95"),
            ("Tecno Phantom X2", new[] { "AD11", "AD11n" }, "Dimensity 9000"),
            ("Tecno Phantom X2 Pro", new[] { "AD12", "AD12n" }, "Dimensity 9000"),
            ("Tecno Phantom V Fold", new[] { "AD8", "AD8n" }, "Dimensity 9000+"),
            ("Tecno Phantom V Flip", new[] { "AD9", "AD9n" }, "Dimensity 8050"),
            ("Tecno Phantom V Fold2", new[] { "AE8", "AE8n" }, "Dimensity 9000+"),
            ("Tecno Phantom V Flip2", new[] { "AE9", "AE9n" }, "Dimensity 8050"));

        AddWithVariants(list, "tecno", "Tecno Pop",
            ("Tecno Pop 4", new[] { "BC2", "BC2c", "BC2j" }, "Unisoc SC9863A"),
            ("Tecno Pop 4 Pro", new[] { "BC3", "BC3a" }, "Unisoc SC9863A"),
            ("Tecno Pop 5", new[] { "BD2", "BD2d", "BD2j" }, "Unisoc SC9863A"),
            ("Tecno Pop 5 LTE", new[] { "BD3", "BD3a" }, "Unisoc SC9863A"),
            ("Tecno Pop 5 Pro", new[] { "BD4", "BD4a" }, "Unisoc T606"),
            ("Tecno Pop 6", new[] { "BE6", "BE6j" }, "Unisoc SC9863A"),
            ("Tecno Pop 6 Go", new[] { "BE7", "BE7j" }, "Unisoc SC7731E"),
            ("Tecno Pop 6 Pro", new[] { "BE8", "BE8a" }, "Unisoc T606"),
            ("Tecno Pop 7", new[] { "BF6", "BF6j" }, "Unisoc T606"),
            ("Tecno Pop 7 Pro", new[] { "BF7", "BF7a" }, "Unisoc T606"),
            ("Tecno Pop 8", new[] { "BG6a", "BG6j" }, "Unisoc T606"),
            ("Tecno Pop 9", new[] { "BH6", "BH6j" }, "Unisoc T606"),
            ("Tecno Pop 10", new[] { "BI6", "BI6j" }, "Unisoc T606"));

        AddWithVariants(list, "tecno", "Tecno Mega / Other",
            ("Tecno Mega 1", new[] { "MEGA1", "MEGA1n" }, "Helio G85"),
            ("Tecno Mega 2", new[] { "MEGA2", "MEGA2n" }, "Helio G99"),
            ("Tecno Mega Pad 11", new[] { "MEGAP11", "MEGAP11n" }, "Helio G85"),
            ("Tecno Pouvoir 4", new[] { "LC7", "LC7n" }, "Helio A22"),
            ("Tecno Pouvoir 4 Pro", new[] { "LC8", "LC8n" }, "Helio G80"));
    }

    private static void AddInfinix(List<PhoneModel> list)
    {
        AddWithVariants(list, "infinix", "Infinix Hot",
            ("Infinix Hot 10", new[] { "X682B", "X682C", "X682N" }, "Helio G70"),
            ("Infinix Hot 10 Play", new[] { "X688B", "X688C" }, "Helio G25"),
            ("Infinix Hot 10i", new[] { "X659B", "X659C" }, "Helio A20"),
            ("Infinix Hot 11", new[] { "X689", "X689B", "X689C" }, "Helio G70"),
            ("Infinix Hot 11S", new[] { "X6812", "X6812B" }, "Helio G88"),
            ("Infinix Hot 12", new[] { "X6817", "X6817B", "X6817C" }, "Helio G85"),
            ("Infinix Hot 12 Play", new[] { "X6816", "X6816C" }, "Helio G37"),
            ("Infinix Hot 12i", new[] { "X665", "X665B", "X665C" }, "Helio G25"),
            ("Infinix Hot 12 Pro", new[] { "X668", "X668C" }, "Helio G85"),
            ("Infinix Hot 20", new[] { "X6826", "X6826B", "X6826C" }, "Helio G85"),
            ("Infinix Hot 20i", new[] { "X665E", "X665F" }, "Helio G37"),
            ("Infinix Hot 20 5G", new[] { "X6827", "X6827B" }, "Dimensity 810"),
            ("Infinix Hot 20S", new[] { "X6821", "X6821B" }, "Helio G85"),
            ("Infinix Hot 30", new[] { "X6831", "X6831B", "X6831C" }, "Helio G85"),
            ("Infinix Hot 30i", new[] { "X669", "X669C", "X669D" }, "Helio G37"),
            ("Infinix Hot 30 5G", new[] { "X6832", "X6832B" }, "Dimensity 6020"),
            ("Infinix Hot 30 Play", new[] { "X6833", "X6833B" }, "Helio G37"),
            ("Infinix Hot 40", new[] { "X6836", "X6836B", "X6836C" }, "Helio G85"),
            ("Infinix Hot 40i", new[] { "X6528", "X6528B" }, "Unisoc T606"),
            ("Infinix Hot 40 Pro", new[] { "X6837", "X6837B", "X6837C" }, "Helio G99"),
            ("Infinix Hot 50", new[] { "X6880", "X6880B", "X6880C" }, "Helio G85"),
            ("Infinix Hot 50i", new[] { "X6531", "X6531B" }, "Helio G36"),
            ("Infinix Hot 50 Pro", new[] { "X6881", "X6881B", "X6881C" }, "Helio G100"),
            ("Infinix Hot 50 Pro+", new[] { "X6882", "X6882B" }, "Helio G100"),
            ("Infinix Hot 60", new[] { "X6885", "X6885B", "X6885C" }, "Helio G100"),
            ("Infinix Hot 60 Pro", new[] { "X6886", "X6886B" }, "Helio G100 Ultra"),
            ("Infinix Hot 60 Pro+", new[] { "X6887", "X6887B" }, "Helio G100 Ultra"));

        AddWithVariants(list, "infinix", "Infinix Note",
            ("Infinix Note 10", new[] { "X693", "X693B", "X693C" }, "Helio G85"),
            ("Infinix Note 10 Pro", new[] { "X695", "X695C" }, "Helio G95"),
            ("Infinix Note 11", new[] { "X663", "X663B", "X663C" }, "Helio G88"),
            ("Infinix Note 11 Pro", new[] { "X663D", "X663E" }, "Helio G96"),
            ("Infinix Note 12", new[] { "X670", "X670B", "X670C" }, "Helio G88"),
            ("Infinix Note 12 Pro", new[] { "X671B", "X671C" }, "Helio G99"),
            ("Infinix Note 12 Pro 5G", new[] { "X676B", "X676C" }, "Dimensity 810"),
            ("Infinix Note 12 VIP", new[] { "X672", "X672B" }, "Dimensity 920"),
            ("Infinix Note 30", new[] { "X6716", "X6716B", "X6716C" }, "Helio G99"),
            ("Infinix Note 30 Pro", new[] { "X678B", "X678C" }, "Helio G99"),
            ("Infinix Note 30 VIP", new[] { "X6710", "X6710B" }, "Dimensity 8200"),
            ("Infinix Note 30 5G", new[] { "X6711", "X6711B" }, "Dimensity 6080"),
            ("Infinix Note 40", new[] { "X6853", "X6853B", "X6853C" }, "Helio G99 Ultimate"),
            ("Infinix Note 40 Pro", new[] { "X6851", "X6851B", "X6851C" }, "Helio G99 Ultimate"),
            ("Infinix Note 40 Pro 5G", new[] { "X6850", "X6850B" }, "Dimensity 7020"),
            ("Infinix Note 40 Pro+", new[] { "X6852", "X6852B" }, "Dimensity 7020"),
            ("Infinix Note 50", new[] { "X6855", "X6855B", "X6855C" }, "Helio G100 Ultimate"),
            ("Infinix Note 50 Pro", new[] { "X6856", "X6856B" }, "Dimensity 7025 Ultra"),
            ("Infinix Note 50 Pro+", new[] { "X6857", "X6857B" }, "Dimensity 7300"));

        AddWithVariants(list, "infinix", "Infinix Zero",
            ("Infinix Zero 5G", new[] { "X6815", "X6815B", "X6815C" }, "Dimensity 900"),
            ("Infinix Zero 5G 2023", new[] { "X6815D", "X6815N" }, "Dimensity 920"),
            ("Infinix Zero Ultra", new[] { "X6820", "X6820B" }, "Dimensity 920"),
            ("Infinix Zero 20", new[] { "X6821Z", "X6821N" }, "Helio G99"),
            ("Infinix Zero 30", new[] { "X6731", "X6731B", "X6731C" }, "Dimensity 8020"),
            ("Infinix Zero 40", new[] { "X6861", "X6861B", "X6861C" }, "Dimensity 8020 Ultra"),
            ("Infinix Zero 50", new[] { "X6865", "X6865B", "X6865C" }, "Dimensity 8200 Ultimate"));

        AddWithVariants(list, "infinix", "Infinix Smart",
            ("Infinix Smart 6", new[] { "X6511", "X6511B", "X6511E" }, "Unisoc SC9863A"),
            ("Infinix Smart 6 HD", new[] { "X6512", "X6512B" }, "Unisoc SC9863A"),
            ("Infinix Smart 7", new[] { "X6515", "X6515B", "X6515C" }, "Unisoc SC9863A"),
            ("Infinix Smart 7 HD", new[] { "X6516", "X6516B" }, "Unisoc SC9863A"),
            ("Infinix Smart 8", new[] { "X6525", "X6525B", "X6525C" }, "Helio G36"),
            ("Infinix Smart 8 HD", new[] { "X6526", "X6526B" }, "Helio G36"),
            ("Infinix Smart 8 Pro", new[] { "X6525P", "X6525Q" }, "Helio G85"),
            ("Infinix Smart 9", new[] { "X6532", "X6532B", "X6532C" }, "Helio G81"),
            ("Infinix Smart 10", new[] { "X6535", "X6535B", "X6535C" }, "Helio G81 Ultra"));

        AddWithVariants(list, "infinix", "Infinix GT",
            ("Infinix GT 10 Pro", new[] { "X6739", "X6739B" }, "Dimensity 8050"),
            ("Infinix GT 20 Pro", new[] { "X6871", "X6871B" }, "Dimensity 8200 Ultimate"),
            ("Infinix GT 30 Pro", new[] { "X6875", "X6875B" }, "Dimensity 8350"));
    }

    private static void AddWithVariants(
        List<PhoneModel> list,
        string brandId,
        string series,
        params (string Name, string[] Codes, string Chip)[] models)
    {
        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, codes, chip) in models)
        {
            if (!seenNames.Add(name.Trim()))
                continue;
            list.Add(Mv(brandId, series, name, codes, chip));
        }
    }
}
