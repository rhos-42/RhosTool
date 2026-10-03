using RhosTool.Core.Models;

namespace RhosTool.Core.Services;

public static partial class ModelCatalog
{
    private static void AddXiaomi(List<PhoneModel> list)
    {
        var notes = new (string Name, string Code, string Chip)[]
        {
            ("Redmi Note 11", "note11", "Snapdragon 680"),
            ("Redmi Note 11 Pro", "note11pro", "Snapdragon 695"),
            ("Redmi Note 11 Pro 5G", "note11pro5g", "Snapdragon 695"),
            ("Redmi Note 12", "note12", "Snapdragon 4 Gen 1"),
            ("Redmi Note 12 Pro", "note12pro", "Dimensity 1080"),
            ("Redmi Note 12 Pro+", "note12pro+", "Dimensity 1080"),
            ("Redmi Note 13", "note13", "Snapdragon 685"),
            ("Redmi Note 13 5G", "note135g", "Dimensity 6080"),
            ("Redmi Note 13 Pro", "note13pro", "Dimensity 7200 Ultra"),
            ("Redmi Note 13 Pro+", "note13pro+", "Dimensity 7200 Ultra"),
            ("Redmi Note 14", "note14", "Helio G99 Ultra"),
            ("Redmi Note 14 5G", "note145g", "Dimensity 7025 Ultra"),
            ("Redmi Note 14 Pro", "note14pro", "Dimensity 7300 Ultra"),
            ("Redmi Note 14 Pro+", "note14pro+", "Snapdragon 7s Gen 3"),
            ("Redmi Note 14S", "note14s", "Helio G99 Ultra"),
            ("Redmi Note 15", "note15", "Dimensity 7025 Ultra"),
            ("Redmi Note 15 Pro", "note15pro", "Dimensity 7400 Ultra"),
            ("Redmi Note 15 Pro+", "note15pro+", "Snapdragon 7s Gen 3")
        };
        foreach (var (name, code, chip) in notes)
            list.Add(M("xiaomi", "Redmi Note", name, code, chip));

        var redmi = new (string Name, string Code, string Chip)[]
        {
            ("Redmi 12", "redmi12", "Helio G88"),
            ("Redmi 12C", "redmi12c", "Helio G85"),
            ("Redmi 13", "redmi13", "Helio G85"),
            ("Redmi 13C", "redmi13c", "Helio G85"),
            ("Redmi 14C", "redmi14c", "Helio G36"),
            ("Redmi 15C", "redmi15c", "Helio G36"),
            ("Redmi A3", "redmia3", "Helio G36"),
            ("Redmi A3x", "redmia3x", "Helio G36"),
            ("Redmi A5", "redmia5", "Helio G36"),
            ("Redmi A5+", "redmia5p", "Helio G85")
        };
        foreach (var (name, code, chip) in redmi)
            list.Add(M("xiaomi", "Redmi", name, code, chip));

        var poco = new (string Name, string Code, string Chip)[]
        {
            ("POCO X5", "pocox5", "Snapdragon 695"),
            ("POCO X5 Pro", "pocox5pro", "Snapdragon 778G"),
            ("POCO X6", "pocox6", "Snapdragon 7s Gen 2"),
            ("POCO X6 Pro", "pocox6pro", "Dimensity 8300 Ultra"),
            ("POCO X7", "pocox7", "Dimensity 7300 Ultra"),
            ("POCO X7 Pro", "pocox7pro", "Dimensity 8400 Ultra"),
            ("POCO F5", "pocof5", "Snapdragon 7+ Gen 2"),
            ("POCO F5 Pro", "pocof5pro", "Snapdragon 8+ Gen 1"),
            ("POCO F6", "pocof6", "Snapdragon 8s Gen 3"),
            ("POCO F6 Pro", "pocof6pro", "Snapdragon 8 Gen 2"),
            ("POCO F7", "pocof7", "Snapdragon 8s Gen 4"),
            ("POCO F7 Pro", "pocof7pro", "Snapdragon 8 Elite"),
            ("POCO M6", "pocom6", "Dimensity 7025"),
            ("POCO M6 Pro", "pocom6pro", "Helio G99 Ultra"),
            ("POCO M7", "pocom7", "Dimensity 7025 Ultra"),
            ("POCO M7 Pro", "pocom7pro", "Dimensity 7025 Ultra"),
            ("POCO C65", "pococ65", "Helio G85"),
            ("POCO C75", "pococ75", "Helio G81 Ultra"),
            ("POCO C85", "pococ85", "Helio G81 Ultra")
        };
        foreach (var (name, code, chip) in poco)
            list.Add(M("xiaomi", "POCO", name, code, chip));

        var mi = new (string Name, string Code, string Chip)[]
        {
            ("Xiaomi 12", "mi12", "Snapdragon 8 Gen 1"),
            ("Xiaomi 12 Pro", "mi12pro", "Snapdragon 8 Gen 1"),
            ("Xiaomi 12T", "mi12t", "Dimensity 8100 Ultra"),
            ("Xiaomi 12T Pro", "mi12tpro", "Snapdragon 8+ Gen 1"),
            ("Xiaomi 13", "mi13", "Snapdragon 8 Gen 2"),
            ("Xiaomi 13 Pro", "mi13pro", "Snapdragon 8 Gen 2"),
            ("Xiaomi 13T", "mi13t", "Dimensity 8200 Ultra"),
            ("Xiaomi 13T Pro", "mi13tpro", "Dimensity 9200+"),
            ("Xiaomi 14", "mi14", "Snapdragon 8 Gen 3"),
            ("Xiaomi 14 Pro", "mi14pro", "Snapdragon 8 Gen 3"),
            ("Xiaomi 14T", "mi14t", "Dimensity 8300 Ultra"),
            ("Xiaomi 14T Pro", "mi14tpro", "Dimensity 9300+"),
            ("Xiaomi 14 Ultra", "mi14ultra", "Snapdragon 8 Gen 3"),
            ("Xiaomi 15", "mi15", "Snapdragon 8 Elite"),
            ("Xiaomi 15 Pro", "mi15pro", "Snapdragon 8 Elite"),
            ("Xiaomi 15 Ultra", "mi15ultra", "Snapdragon 8 Elite"),
            ("Xiaomi 15T", "mi15t", "Dimensity 8400 Ultra"),
            ("Xiaomi 15T Pro", "mi15tpro", "Dimensity 9400+"),
            ("Xiaomi 16", "mi16", "Snapdragon 8 Elite Gen 2"),
            ("Xiaomi 16 Pro", "mi16pro", "Snapdragon 8 Elite Gen 2"),
            ("Xiaomi 16 Ultra", "mi16ultra", "Snapdragon 8 Elite Gen 2")
        };
        foreach (var (name, code, chip) in mi)
            list.Add(M("xiaomi", "Xiaomi Flagship", name, code, chip));
    }
}
