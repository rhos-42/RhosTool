namespace RhosTool.Core.Models;

public sealed class FunctionTab
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required IReadOnlyList<DeviceOperation> Operations { get; init; }
}

public sealed class BrandModule
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string ShortName { get; init; }
    public required string Platform { get; init; }
    public required string AccentHex { get; init; }
    public required IReadOnlyList<string> ModeTabs { get; init; }
    public required IReadOnlyList<FunctionTab> FunctionTabs { get; init; }
}
