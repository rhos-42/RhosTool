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
    MtpFiles,
    /// <summary>Ouvre un lien ressource (URL à configurer plus tard).</summary>
    OpenResourceLink
}

/// <summary>Mode USB / transport exigé pour qu'une opération soit exécutable.</summary>
public enum DeviceRequiredMode
{
    /// <summary>Pas d'appareil requis (drivers, packs, guides, diagnostics liste).</summary>
    None,
    /// <summary>Module partenaire — toujours refusé en local.</summary>
    Unavailable,
    /// <summary>Android ADB autorisé (state = device).</summary>
    AdbDevice,
    /// <summary>Android ADB présent (device / recovery / sideload).</summary>
    AdbTransport,
    /// <summary>Appareil en mode fastboot.</summary>
    Fastboot,
    /// <summary>Appareil MTP Windows détecté.</summary>
    Mtp
}

public sealed class DeviceOperation
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required OperationKind Kind { get; init; }
    public bool RequiresDevice { get; init; } = true;
    public DeviceRequiredMode RequiredMode { get; init; } = DeviceRequiredMode.AdbDevice;
}

/// <summary>Résultat d'une opération — Ok=false ne doit jamais être journalisé en succès.</summary>
public sealed class OperationResult
{
    public bool Ok { get; init; }
    public string Message { get; init; } = "";

    public static OperationResult Success(string message) => new() { Ok = true, Message = message };
    public static OperationResult Fail(string message) => new() { Ok = false, Message = message };
}
