namespace RhosTool.Core.Models;

public enum DevicePlatform
{
    Unknown,
    Android,
    Ios
}

public sealed class DeviceInfo
{
    public string Serial { get; init; } = string.Empty;
    public string Model { get; init; } = "Inconnu";
    public string Manufacturer { get; init; } = "Inconnu";
    public string AndroidVersion { get; init; } = "-";
    public string BuildId { get; init; } = "-";
    public string Product { get; init; } = "-";
    public DevicePlatform Platform { get; init; } = DevicePlatform.Unknown;
    public bool IsAuthorized { get; init; }
    public string State { get; init; } = "offline";

    public string DisplayName =>
        string.IsNullOrWhiteSpace(Model) || Model == "Inconnu"
            ? Serial
            : $"{Manufacturer} {Model}";
}
