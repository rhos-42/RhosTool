namespace RhosTool.Core.Models;

public sealed class PhoneModel
{
    public required string BrandId { get; init; }
    public required string DisplayName { get; init; }
    /// <summary>Code usine principal.</summary>
    public required string ModelCode { get; init; }
    /// <summary>Toutes les variantes (BG6, BG6h, BG6n…) — un seul choix modèle.</summary>
    public IReadOnlyList<string> CodeVariants { get; init; } = Array.Empty<string>();
    public required string Chipset { get; init; }
    public string Series { get; init; } = "";
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>Affichage : BG6 / BG6h / BG6n</summary>
    public string CodesDisplay =>
        CodeVariants.Count > 0
            ? string.Join(" / ", CodeVariants)
            : ModelCode;

    public bool MatchesCode(string query) =>
        ModelCode.Contains(query, StringComparison.OrdinalIgnoreCase) ||
        CodesDisplay.Contains(query, StringComparison.OrdinalIgnoreCase) ||
        CodeVariants.Any(c => c.Contains(query, StringComparison.OrdinalIgnoreCase));
}
