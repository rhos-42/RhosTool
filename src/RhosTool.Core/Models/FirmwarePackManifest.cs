namespace RhosTool.Core.Models;

public enum FirmwarePackFormat
{
    Unknown,
    MtkScatter,
    OdinTar,
    Qualcomm,
    OtaZip
}

public sealed class FirmwarePartitionEntry
{
    public required string Name { get; init; }
    public required string File { get; init; }
    public long Size { get; init; }
    public string? Sha256 { get; init; }
}

public sealed class FirmwarePackManifest
{
    public required string PackId { get; init; }
    public required string Brand { get; init; }
    public required string ModelHint { get; init; }
    public string ChipsetHint { get; init; } = "";
    public FirmwarePackFormat Format { get; init; }
    public required string SourcePath { get; init; }
    public string? PackDir { get; init; }
    public IReadOnlyList<FirmwarePartitionEntry> Partitions { get; init; } = Array.Empty<FirmwarePartitionEntry>();
    public bool RequiredOk { get; init; }
    public IReadOnlyList<string> Missing { get; init; } = Array.Empty<string>();
    public DateTimeOffset CreatedUtc { get; init; }

    public string FormatKey => Format switch
    {
        FirmwarePackFormat.MtkScatter => "mtk_scatter",
        FirmwarePackFormat.OdinTar => "odin_tar",
        FirmwarePackFormat.Qualcomm => "qualcomm",
        FirmwarePackFormat.OtaZip => "ota_zip",
        _ => "unknown"
    };

    public string ShortStatus =>
        RequiredOk
            ? $"OK · {FormatKey} · {Partitions.Count} fichier(s)"
            : $"INCOMPLET · {FormatKey} · manquant: {string.Join(", ", Missing)}";
}
