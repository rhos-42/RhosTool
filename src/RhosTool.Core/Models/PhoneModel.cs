namespace RhosTool.Core.Models;

public sealed class PhoneModel
{
    public required string BrandId { get; init; }
    public required string DisplayName { get; init; }
    public required string ModelCode { get; init; }
    public required string Chipset { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}
