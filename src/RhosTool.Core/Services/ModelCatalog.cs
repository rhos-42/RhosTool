using RhosTool.Core.Models;

namespace RhosTool.Core.Services;

/// <summary>
/// Catalogue modèles restructuré par marque / série (style atelier UnlockTool).
/// </summary>
public static partial class ModelCatalog
{
    private static IReadOnlyList<PhoneModel>? _all;
    private static IReadOnlyDictionary<string, IReadOnlyList<PhoneModel>>? _byBrand;

    public static IReadOnlyList<PhoneModel> All => _all ??= BuildAll();

    public static int Count => All.Count;

    public static IEnumerable<PhoneModel> ForBrand(string brandId)
    {
        var map = _byBrand ??= BuildIndex(All);
        return map.TryGetValue(brandId, out var models)
            ? models
            : Array.Empty<PhoneModel>();
    }

    private static IReadOnlyList<PhoneModel> BuildAll()
    {
        var list = new List<PhoneModel>(4500);

        // Import principal : liste publique UnlockTool (~3000 modèles)
        AddUnlockToolImport(list);

        // Marques absentes / partielles sur la liste UT publique
        AddGooglePixel(list);
        AddSony(list);
        AddNothing(list);
        AddQualcomm(list);
        AddMediatek(list);
        AddSpreadtrum(list);
        AddAndroidGeneric(list);

        // Déduplique brand+nom+code principal (UT prioritaire)
        list = list
            .GroupBy(m => $"{m.BrandId}|{m.DisplayName}|{m.ModelCode}", StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        // Tri stable : série puis nom
        return list
            .OrderBy(m => m.BrandId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(m => m.Series, StringComparer.OrdinalIgnoreCase)
            .ThenBy(m => m.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<PhoneModel>> BuildIndex(
        IReadOnlyList<PhoneModel> all)
    {
        return all
            .GroupBy(m => m.BrandId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<PhoneModel>)g.ToList(),
                StringComparer.OrdinalIgnoreCase);
    }

    internal static PhoneModel M(
        string brandId,
        string series,
        string name,
        string code,
        string chip,
        params string[] tags) =>
        Mv(brandId, series, name, new[] { code }, chip, tags);

    /// <summary>Un modèle unique avec toutes ses variantes de code (BG6 / BG6h / BG6n).</summary>
    internal static PhoneModel Mv(
        string brandId,
        string series,
        string name,
        string[] codes,
        string chip,
        params string[] tags)
    {
        var variants = codes
            .Select(c => c.Trim())
            .Where(c => c.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (variants.Length == 0)
            variants = new[] { "?" };

        return new PhoneModel
        {
            BrandId = brandId,
            Series = series,
            DisplayName = name.Trim(),
            ModelCode = variants[0],
            CodeVariants = variants,
            Chipset = chip,
            Tags = tags.Length > 0 ? tags : TagsForChip(chip)
        };
    }

    internal static IReadOnlyList<string> TagsForChip(string chip)
    {
        if (ContainsAny(chip, "MT", "Dimensity", "Helio", "Kompanio"))
            return new[] { "preloader", "testpoint", "custom da" };
        if (ContainsAny(chip, "Snapdragon", "SDM", "SM8", "SM7", "SM4"))
            return new[] { "edl", "testpoint" };
        if (ContainsAny(chip, "Exynos"))
            return new[] { "odin", "testpoint" };
        if (ContainsAny(chip, "Kirin", "Harmony"))
            return new[] { "testpoint", "edl" };
        if (ContainsAny(chip, "Tensor"))
            return new[] { "edl", "fastboot" };
        if (ContainsAny(chip, "Unisoc", "Spreadtrum", "Tiger", "T606", "T612", "T616"))
            return new[] { "preloader", "testpoint" };
        if (ContainsAny(chip, "Apple", "A1", "A2", "M1", "M2", "M3", "M4"))
            return new[] { "ios" };
        return new[] { "edl" };
    }

    private static bool ContainsAny(string haystack, params string[] needles) =>
        needles.Any(n => haystack.Contains(n, StringComparison.OrdinalIgnoreCase));
}
