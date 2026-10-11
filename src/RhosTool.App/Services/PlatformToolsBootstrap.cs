using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace RhosTool.App.Services;

/// <summary>
/// Extrait adb/fastboot embarqués vers %AppData%\RhosTool\platform-tools (single-file).
/// </summary>
public static class PlatformToolsBootstrap
{
    public static string ToolsDir =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RhosTool",
            "platform-tools");

    public static string AdbPath => Path.Combine(ToolsDir, "adb.exe");
    public static string FastbootPath => Path.Combine(ToolsDir, "fastboot.exe");

    public static void EnsureAvailable()
    {
        Directory.CreateDirectory(ToolsDir);

        if (File.Exists(AdbPath) && File.Exists(FastbootPath))
        {
            return;
        }

        var asm = Assembly.GetExecutingAssembly();
        var name = asm.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("platform-tools.zip", StringComparison.OrdinalIgnoreCase));

        if (name is null)
        {
            // Fallback : tools à côté de l'exe (dev / publish non single-file)
            TryCopyFromBaseDirectory();
            return;
        }

        using var stream = asm.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException("Ressource platform-tools.zip introuvable.");

        using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
        foreach (var entry in zip.Entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Name)) continue;
            var dest = Path.Combine(ToolsDir, entry.Name);
            entry.ExtractToFile(dest, overwrite: true);
        }
    }

    private static void TryCopyFromBaseDirectory()
    {
        var src = Path.Combine(AppContext.BaseDirectory, "platform-tools");
        if (!Directory.Exists(src)) return;

        foreach (var file in Directory.EnumerateFiles(src))
        {
            var dest = Path.Combine(ToolsDir, Path.GetFileName(file));
            File.Copy(file, dest, overwrite: true);
        }
    }
}
