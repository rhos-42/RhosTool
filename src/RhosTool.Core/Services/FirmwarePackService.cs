using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using RhosTool.Core.Models;

namespace RhosTool.Core.Services;

public sealed class FirmwarePackService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static string PackagesRoot =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RhosTool",
            "packages");

    public async Task<FirmwarePackManifest> IngestAsync(
        string inputPath,
        string? brand,
        string? modelCode,
        bool extract,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(inputPath) || (!File.Exists(inputPath) && !Directory.Exists(inputPath)))
            throw new FileNotFoundException("Paquet firmware introuvable.", inputPath);

        var safeBrand = Sanitize(brand, "unknown");
        var safeModel = Sanitize(modelCode, "generic");
        var packId = $"{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}"[..24];
        var packDir = Path.Combine(PackagesRoot, safeBrand, safeModel, packId);
        var sourceDir = Path.Combine(packDir, "source");
        var extractedDir = Path.Combine(packDir, "extracted");

        Directory.CreateDirectory(sourceDir);
        Directory.CreateDirectory(extractedDir);

        var isDir = Directory.Exists(inputPath);
        if (isDir)
        {
            var dest = Path.Combine(sourceDir, Path.GetFileName(Path.GetFullPath(inputPath).TrimEnd(Path.DirectorySeparatorChar)));
            CopyDirectory(inputPath, dest);
        }
        else
        {
            File.Copy(inputPath, Path.Combine(sourceDir, Path.GetFileName(inputPath)), overwrite: true);
        }

        var scanRoot = sourceDir;
        var lower = Path.GetFileName(inputPath).ToLowerInvariant();

        if (extract && !isDir && lower.EndsWith(".zip", StringComparison.Ordinal))
        {
            var zipPath = Path.Combine(sourceDir, Path.GetFileName(inputPath));
            await Task.Run(() => ZipFile.ExtractToDirectory(zipPath, extractedDir, overwriteFiles: true), ct)
                .ConfigureAwait(false);
            scanRoot = extractedDir;
        }
        else if (isDir)
        {
            scanRoot = Path.Combine(sourceDir, Path.GetFileName(Path.GetFullPath(inputPath).TrimEnd(Path.DirectorySeparatorChar)));
        }

        var format = DetectFormat(inputPath, scanRoot);
        var partitions = format == FirmwarePackFormat.MtkScatter
            ? ParseScatterOrInventory(scanRoot)
            : await InventoryAsync(scanRoot, ct).ConfigureAwait(false);

        var (ok, missing) = Validate(format, scanRoot, partitions);
        var manifest = new FirmwarePackManifest
        {
            PackId = packId,
            Brand = safeBrand,
            ModelHint = safeModel,
            ChipsetHint = ChipHint(format),
            Format = format,
            SourcePath = Path.GetFullPath(inputPath),
            PackDir = packDir,
            Partitions = partitions,
            RequiredOk = ok,
            Missing = missing,
            CreatedUtc = DateTimeOffset.UtcNow
        };

        await SaveManifestAsync(packDir, manifest, ct).ConfigureAwait(false);
        return manifest;
    }

    public async Task<FirmwarePackManifest> ValidateOnlyAsync(
        string inputPath,
        string? brand = null,
        string? modelCode = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(inputPath) || (!File.Exists(inputPath) && !Directory.Exists(inputPath)))
            throw new FileNotFoundException("Paquet firmware introuvable.", inputPath);

        var scanRoot = Directory.Exists(inputPath) ? inputPath : Path.GetDirectoryName(inputPath)!;
        var format = DetectFormat(inputPath, scanRoot);
        var partitions = format == FirmwarePackFormat.MtkScatter
            ? ParseScatterOrInventory(scanRoot)
            : await InventoryAsync(scanRoot, ct).ConfigureAwait(false);
        var (ok, missing) = Validate(format, scanRoot, partitions);

        return new FirmwarePackManifest
        {
            PackId = "validate-only",
            Brand = Sanitize(brand, "unknown"),
            ModelHint = Sanitize(modelCode, "generic"),
            ChipsetHint = ChipHint(format),
            Format = format,
            SourcePath = Path.GetFullPath(inputPath),
            PackDir = null,
            Partitions = partitions,
            RequiredOk = ok,
            Missing = missing,
            CreatedUtc = DateTimeOffset.UtcNow
        };
    }

    public FirmwarePackManifest? LoadManifest(string packDir)
    {
        var path = Path.Combine(packDir, "manifest.json");
        if (!File.Exists(path)) return null;
        var dto = JsonSerializer.Deserialize<ManifestDto>(File.ReadAllText(path), JsonOptions);
        return dto?.ToManifest();
    }

    public IReadOnlyList<FirmwarePackManifest> ListPacks(string? brand = null)
    {
        if (!Directory.Exists(PackagesRoot))
            return Array.Empty<FirmwarePackManifest>();

        var root = string.IsNullOrWhiteSpace(brand)
            ? PackagesRoot
            : Path.Combine(PackagesRoot, Sanitize(brand, "unknown"));

        if (!Directory.Exists(root))
            return Array.Empty<FirmwarePackManifest>();

        var list = new List<FirmwarePackManifest>();
        foreach (var manifestPath in Directory.EnumerateFiles(root, "manifest.json", SearchOption.AllDirectories))
        {
            try
            {
                var dto = JsonSerializer.Deserialize<ManifestDto>(File.ReadAllText(manifestPath), JsonOptions);
                var m = dto?.ToManifest();
                if (m is not null) list.Add(m);
            }
            catch
            {
                // ignore corrupt manifests
            }
        }

        return list
            .OrderByDescending(m => m.CreatedUtc)
            .ToList();
    }

    public string FormatSummary(FirmwarePackManifest m)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"PackId: {m.PackId}");
        sb.AppendLine($"Brand/Model: {m.Brand} / {m.ModelHint}");
        sb.AppendLine($"Format: {m.FormatKey}");
        sb.AppendLine($"Chipset: {m.ChipsetHint}");
        sb.AppendLine($"Source: {m.SourcePath}");
        if (!string.IsNullOrWhiteSpace(m.PackDir))
            sb.AppendLine($"PackDir: {m.PackDir}");
        sb.AppendLine($"Status: {(m.RequiredOk ? "OK" : "INCOMPLET")}");
        if (m.Missing.Count > 0)
            sb.AppendLine($"Missing: {string.Join(", ", m.Missing)}");
        sb.AppendLine($"Entries: {m.Partitions.Count}");
        foreach (var p in m.Partitions.Take(40))
        {
            var hash = string.IsNullOrEmpty(p.Sha256) ? "-" : p.Sha256[..Math.Min(12, p.Sha256.Length)];
            sb.AppendLine($"  - {p.Name}: {p.File} ({p.Size:N0} o) sha={hash}");
        }

        if (m.Partitions.Count > 40)
            sb.AppendLine($"  … +{m.Partitions.Count - 40} autres");

        return sb.ToString().TrimEnd();
    }

    private static async Task SaveManifestAsync(string packDir, FirmwarePackManifest m, CancellationToken ct)
    {
        var dto = ManifestDto.From(m);
        var json = JsonSerializer.Serialize(dto, JsonOptions);
        await File.WriteAllTextAsync(Path.Combine(packDir, "manifest.json"), json, ct).ConfigureAwait(false);
    }

    private static FirmwarePackFormat DetectFormat(string inputPath, string scanRoot)
    {
        var name = Path.GetFileName(inputPath).ToLowerInvariant();
        if (name.EndsWith(".tar.md5", StringComparison.Ordinal) || name.EndsWith(".tar", StringComparison.Ordinal))
            return FirmwarePackFormat.OdinTar;

        if (EnumerateFilesSafe(scanRoot, "*scatter*.txt").Any())
            return FirmwarePackFormat.MtkScatter;

        if (EnumerateFilesSafe(scanRoot, "prog_firehose*").Any() ||
            EnumerateFilesSafe(scanRoot, "*firehose*").Any() ||
            EnumerateFilesSafe(scanRoot, "rawprogram*.xml").Any())
            return FirmwarePackFormat.Qualcomm;

        if (Directory.Exists(Path.Combine(scanRoot, "META-INF")) ||
            EnumerateFilesSafe(scanRoot, "payload.bin").Any() ||
            name.EndsWith(".zip", StringComparison.Ordinal))
            return FirmwarePackFormat.OtaZip;

        // also detect META-INF nested
        if (EnumerateDirectoriesSafe(scanRoot, "META-INF").Any())
            return FirmwarePackFormat.OtaZip;

        return FirmwarePackFormat.Unknown;
    }

    private static IReadOnlyList<FirmwarePartitionEntry> ParseScatterOrInventory(string scanRoot)
    {
        var scatter = EnumerateFilesSafe(scanRoot, "*scatter*.txt").FirstOrDefault();
        if (scatter is null)
            return InventoryAsync(scanRoot, CancellationToken.None).GetAwaiter().GetResult();

        var baseDir = Path.GetDirectoryName(scatter)!;
        var list = new List<FirmwarePartitionEntry>();
        string? currentName = null;
        string? currentFile = null;

        void Flush()
        {
            if (string.IsNullOrWhiteSpace(currentName) ||
                string.IsNullOrWhiteSpace(currentFile) ||
                currentFile.Equals("NONE", StringComparison.OrdinalIgnoreCase))
                return;

            var full = Path.Combine(baseDir, currentFile);
            long size = 0;
            string? hash = null;
            if (File.Exists(full))
            {
                size = new FileInfo(full).Length;
                if (size <= 64L * 1024 * 1024)
                    hash = HashFile(full);
            }

            list.Add(new FirmwarePartitionEntry
            {
                Name = currentName.Trim(),
                File = currentFile.Trim().Replace('\\', '/'),
                Size = size,
                Sha256 = hash
            });
        }

        foreach (var line in File.ReadLines(scatter))
        {
            var t = line.Trim();
            if (t.StartsWith("partition_name", StringComparison.OrdinalIgnoreCase))
            {
                Flush();
                currentName = t.Split(':', 2).ElementAtOrDefault(1)?.Trim();
                currentFile = null;
            }
            else if (t.StartsWith("file_name", StringComparison.OrdinalIgnoreCase))
            {
                currentFile = t.Split(':', 2).ElementAtOrDefault(1)?.Trim();
            }
        }

        Flush();
        return list.Count > 0 ? list : InventoryAsync(scanRoot, CancellationToken.None).GetAwaiter().GetResult();
    }

    private static async Task<IReadOnlyList<FirmwarePartitionEntry>> InventoryAsync(string root, CancellationToken ct)
    {
        var list = new List<FirmwarePartitionEntry>();
        if (!Directory.Exists(root)) return list;

        foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            ct.ThrowIfCancellationRequested();
            var info = new FileInfo(file);
            string? hash = null;
            if (info.Length <= 64L * 1024 * 1024)
            {
                hash = await Task.Run(() => HashFile(file), ct).ConfigureAwait(false);
            }

            list.Add(new FirmwarePartitionEntry
            {
                Name = Path.GetFileNameWithoutExtension(file),
                File = Path.GetRelativePath(root, file).Replace('\\', '/'),
                Size = info.Length,
                Sha256 = hash
            });
        }

        return list;
    }

    private static (bool Ok, IReadOnlyList<string> Missing) Validate(
        FirmwarePackFormat format,
        string scanRoot,
        IReadOnlyList<FirmwarePartitionEntry> partitions)
    {
        var missing = new List<string>();
        switch (format)
        {
            case FirmwarePackFormat.MtkScatter:
                if (!EnumerateFilesSafe(scanRoot, "*scatter*.txt").Any())
                    missing.Add("scatter.txt");
                if (!partitions.Any(p => p.Size > 0 &&
                                         !p.File.Equals("NONE", StringComparison.OrdinalIgnoreCase)))
                    missing.Add("referenced_image");
                break;

            case FirmwarePackFormat.OdinTar:
                if (!EnumerateFilesSafe(scanRoot, "*.tar").Any() &&
                    !EnumerateFilesSafe(scanRoot, "*.tar.md5").Any() &&
                    !Directory.EnumerateFiles(scanRoot, "*", SearchOption.AllDirectories)
                        .Any(f => f.EndsWith(".tar", StringComparison.OrdinalIgnoreCase) ||
                                  f.EndsWith(".tar.md5", StringComparison.OrdinalIgnoreCase)))
                    missing.Add("tar_archive");
                break;

            case FirmwarePackFormat.Qualcomm:
                if (!EnumerateFilesSafe(scanRoot, "prog_firehose*").Any() &&
                    !EnumerateFilesSafe(scanRoot, "*firehose*").Any())
                    missing.Add("firehose_programmer");
                break;

            case FirmwarePackFormat.OtaZip:
                var hasMeta = EnumerateDirectoriesSafe(scanRoot, "META-INF").Any() ||
                              Directory.Exists(Path.Combine(scanRoot, "META-INF"));
                var hasPayload = EnumerateFilesSafe(scanRoot, "payload.bin").Any();
                var hasImg = EnumerateFilesSafe(scanRoot, "*.img").Any();
                if (!hasMeta && !hasPayload && !hasImg)
                    missing.Add("ota_artifact");
                break;

            default:
                if (partitions.Count == 0)
                    missing.Add("any_file");
                break;
        }

        return (missing.Count == 0, missing);
    }

    private static string ChipHint(FirmwarePackFormat format) => format switch
    {
        FirmwarePackFormat.MtkScatter => "MediaTek",
        FirmwarePackFormat.OdinTar => "Exynos/Snapdragon",
        FirmwarePackFormat.Qualcomm => "Snapdragon",
        _ => ""
    };

    private static string Sanitize(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value)) return fallback;
        var chars = value.Trim().Select(c => char.IsLetterOrDigit(c) || c is '-' or '_' ? c : '_').ToArray();
        var s = new string(chars).Trim('_').ToLowerInvariant();
        return string.IsNullOrWhiteSpace(s) ? fallback : s;
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static void CopyDirectory(string source, string dest)
    {
        Directory.CreateDirectory(dest);
        foreach (var dir in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(dest, Path.GetRelativePath(source, dir)));
        }

        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(dest, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target, overwrite: true);
        }
    }

    private static IEnumerable<string> EnumerateFilesSafe(string root, string pattern)
    {
        if (!Directory.Exists(root)) yield break;
        foreach (var f in Directory.EnumerateFiles(root, pattern, SearchOption.AllDirectories))
            yield return f;
    }

    private static IEnumerable<string> EnumerateDirectoriesSafe(string root, string name)
    {
        if (!Directory.Exists(root)) yield break;
        foreach (var d in Directory.EnumerateDirectories(root, name, SearchOption.AllDirectories))
            yield return d;
    }

    private sealed class ManifestDto
    {
        public string PackId { get; set; } = "";
        public string Brand { get; set; } = "";
        public string ModelHint { get; set; } = "";
        public string ChipsetHint { get; set; } = "";
        public string Format { get; set; } = "unknown";
        public string SourcePath { get; set; } = "";
        public string? PackDir { get; set; }
        public List<PartitionDto> Partitions { get; set; } = new();
        public bool RequiredOk { get; set; }
        public List<string> Missing { get; set; } = new();
        public DateTimeOffset CreatedUtc { get; set; }

        public static ManifestDto From(FirmwarePackManifest m) => new()
        {
            PackId = m.PackId,
            Brand = m.Brand,
            ModelHint = m.ModelHint,
            ChipsetHint = m.ChipsetHint,
            Format = m.FormatKey,
            SourcePath = m.SourcePath,
            PackDir = m.PackDir,
            Partitions = m.Partitions.Select(p => new PartitionDto
            {
                Name = p.Name,
                File = p.File,
                Size = p.Size,
                Sha256 = p.Sha256
            }).ToList(),
            RequiredOk = m.RequiredOk,
            Missing = m.Missing.ToList(),
            CreatedUtc = m.CreatedUtc
        };

        public FirmwarePackManifest ToManifest() => new()
        {
            PackId = PackId,
            Brand = Brand,
            ModelHint = ModelHint,
            ChipsetHint = ChipsetHint,
            Format = ParseFormat(Format),
            SourcePath = SourcePath,
            PackDir = PackDir,
            Partitions = Partitions.Select(p => new FirmwarePartitionEntry
            {
                Name = p.Name,
                File = p.File,
                Size = p.Size,
                Sha256 = p.Sha256
            }).ToList(),
            RequiredOk = RequiredOk,
            Missing = Missing,
            CreatedUtc = CreatedUtc
        };

        private static FirmwarePackFormat ParseFormat(string f) => f switch
        {
            "mtk_scatter" => FirmwarePackFormat.MtkScatter,
            "odin_tar" => FirmwarePackFormat.OdinTar,
            "qualcomm" => FirmwarePackFormat.Qualcomm,
            "ota_zip" => FirmwarePackFormat.OtaZip,
            _ => FirmwarePackFormat.Unknown
        };
    }

    private sealed class PartitionDto
    {
        public string Name { get; set; } = "";
        public string File { get; set; } = "";
        public long Size { get; set; }
        public string? Sha256 { get; set; }
    }
}
