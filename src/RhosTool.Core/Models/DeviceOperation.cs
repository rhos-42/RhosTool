namespace RhosTool.Core.Models;

public enum OperationKind
{
    ReadInfo,
    Reboot,
    RebootRecovery,
    OpenDeviceManager,
    PartnerStub,
    AdbAppControl,
    FirmwarePack,
    /// <summary>Action locale réelle dispatchée par Id (ADB / fastboot / guide / pack).</summary>
    DeviceAction,
    /// <summary>Outils ADB transport (push/pull/shell/logcat/wifi…).</summary>
    AdbTools,
    /// <summary>Transfert fichiers MTP (Windows Portable Device).</summary>
    MtpFiles
}

public sealed class DeviceOperation
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required OperationKind Kind { get; init; }
    public bool RequiresDevice { get; init; } = true;
}
