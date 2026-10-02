namespace RhosTool.Core.Models;

public enum OperationKind
{
    ReadInfo,
    Reboot,
    RebootRecovery,
    OpenDeviceManager,
    PartnerStub,
    AdbAppControl
}

public sealed class DeviceOperation
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required OperationKind Kind { get; init; }
    public bool RequiresDevice { get; init; } = true;
}
