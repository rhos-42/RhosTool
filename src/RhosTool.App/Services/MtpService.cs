using System.IO;
using System.Text;
using MediaDevices;
using RhosTool.Core.Models;
using RhosTool.Core.Services;

namespace RhosTool.App.Services;

/// <summary>
/// Accès MTP / WPD via Windows Portable Devices (fonctionne sans ADB).
/// Chemins distants : "FriendlyName\Storage\Folder\file.ext" ou relatif au 1er stockage.
/// </summary>
public sealed class MtpService
{
    private readonly LogService _log;

    public MtpService(LogService log)
    {
        _log = log;
    }

    public int CountDevices()
    {
        try
        {
            return MediaDevice.GetDevices().Count();
        }
        catch
        {
            return 0;
        }
    }

    public Task<OperationResult> RunAsync(string operationId, string? userInput, CancellationToken ct = default) =>
        Task.Run(() => RunCore(operationId, userInput), ct);

    private OperationResult RunCore(string operationId, string? userInput)
    {
        try
        {
            if (operationId is not "mtp-devices" && CountDevices() == 0)
            {
                return OperationResult.Fail(
                    "ÉCHEC — aucun appareil MTP.\n" +
                    "Branchez le téléphone et choisissez « Transfert de fichiers / MTP ».");
            }

            var text = operationId switch
            {
                "mtp-devices" => ListDevices(),
                "mtp-list" => ListFolder(userInput),
                "mtp-download" => Download(userInput),
                "mtp-upload" => Upload(userInput),
                "mtp-mkdir" => Mkdir(userInput),
                "mtp-delete" => Delete(userInput),
                _ => $"MTP inconnu : {operationId}"
            };

            if (text.StartsWith("Aucun appareil MTP", StringComparison.OrdinalIgnoreCase) ||
                text.StartsWith("Erreur MTP", StringComparison.OrdinalIgnoreCase) ||
                text.StartsWith("MTP inconnu", StringComparison.OrdinalIgnoreCase))
            {
                return OperationResult.Fail(text);
            }

            return OperationResult.Success(text);
        }
        catch (Exception ex)
        {
            _log.Error($"MTP: {ex.Message}");
            return OperationResult.Fail(
                $"Erreur MTP : {ex.Message}\nVérifiez que le téléphone est en mode Transfert de fichiers (MTP), pas Charge seule.");
        }
    }

    private string ListDevices()
    {
        var devices = MediaDevice.GetDevices().ToList();
        if (devices.Count == 0)
        {
            return "Aucun appareil MTP détecté.\n" +
                   "Branche le téléphone, choisis « Transfert de fichiers / MTP » dans la notif USB.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"{devices.Count} appareil(s) MTP :");
        foreach (var d in devices)
        {
            try
            {
                d.Connect();
                sb.AppendLine($"- {d.FriendlyName} | {d.Manufacturer} {d.Description}");
                foreach (var drive in d.GetDrives())
                {
                    sb.AppendLine($"    stockage: {drive.Name}  root={drive.RootDirectory?.FullName}");
                }

                d.Disconnect();
            }
            catch (Exception ex)
            {
                sb.AppendLine($"- {d.FriendlyName} (erreur: {ex.Message})");
            }
        }

        _log.Success($"{devices.Count} MTP device(s)");
        return sb.ToString().TrimEnd();
    }

    private string ListFolder(string? pathHint)
    {
        using var ctx = OpenContext(pathHint, requireFile: false);
        var folder = string.IsNullOrWhiteSpace(ctx.RelativePath)
            ? ctx.Root
            : CombineMtp(ctx.Root, ctx.RelativePath);

        if (!ctx.Device.DirectoryExists(folder))
            return $"Dossier introuvable : {folder}";

        var sb = new StringBuilder();
        sb.AppendLine($"MTP [{ctx.Device.FriendlyName}] {folder}");
        foreach (var dir in ctx.Device.GetDirectories(folder).OrderBy(x => x))
        {
            sb.AppendLine($"  <DIR>  {Path.GetFileName(dir.TrimEnd('\\'))}");
        }

        foreach (var file in ctx.Device.GetFiles(folder).OrderBy(x => x))
        {
            try
            {
                var info = ctx.Device.GetFileInfo(file);
                sb.AppendLine($"  {info.Length,12:N0}  {Path.GetFileName(file)}");
            }
            catch
            {
                sb.AppendLine($"           ?  {Path.GetFileName(file)}");
            }
        }

        return sb.ToString().TrimEnd();
    }

    private string Download(string? userInput)
    {
        // remote|localFile
        var parts = (userInput ?? "").Split('|', 2);
        if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            return "Usage : cheminMTP|fichierLocal";

        using var ctx = OpenContext(parts[0], requireFile: true);
        var remote = CombineMtp(ctx.Root, ctx.RelativePath);
        var local = parts[1].Trim();
        Directory.CreateDirectory(Path.GetDirectoryName(local)!);

        if (!ctx.Device.FileExists(remote))
            return $"Fichier MTP introuvable : {remote}";

        _log.Info($"MTP download {remote} -> {local}");
        ctx.Device.DownloadFile(remote, local);
        return File.Exists(local) ? $"Téléchargé : {local}" : "Échec téléchargement MTP.";
    }

    private string Upload(string? userInput)
    {
        // localFile|remoteFolder
        var parts = (userInput ?? "").Split('|', 2);
        if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            return "Usage : fichierLocal|dossierMTP";

        var local = parts[0].Trim();
        if (!File.Exists(local))
            return $"Fichier local introuvable : {local}";

        using var ctx = OpenContext(parts[1], requireFile: false);
        var remoteDir = string.IsNullOrWhiteSpace(ctx.RelativePath)
            ? ctx.Root
            : CombineMtp(ctx.Root, ctx.RelativePath);

        if (!ctx.Device.DirectoryExists(remoteDir))
            return $"Dossier MTP introuvable : {remoteDir}";

        var remoteFile = CombineMtp(remoteDir, Path.GetFileName(local));
        _log.Info($"MTP upload {local} -> {remoteFile}");
        ctx.Device.UploadFile(local, remoteFile);
        return ctx.Device.FileExists(remoteFile)
            ? $"Envoyé : {remoteFile}"
            : "Upload demandé (vérifiez sur le téléphone).";
    }

    private string Mkdir(string? pathHint)
    {
        using var ctx = OpenContext(pathHint, requireFile: false);
        if (string.IsNullOrWhiteSpace(ctx.RelativePath))
            return "Indiquez un chemin de dossier à créer (ex: Internal storage\\RhosTool).";

        var full = CombineMtp(ctx.Root, ctx.RelativePath);
        if (ctx.Device.DirectoryExists(full))
            return $"Déjà existant : {full}";

        ctx.Device.CreateDirectory(full);
        return ctx.Device.DirectoryExists(full) ? $"Dossier créé : {full}" : $"CreateDirectory : {full}";
    }

    private string Delete(string? pathHint)
    {
        using var ctx = OpenContext(pathHint, requireFile: false);
        if (string.IsNullOrWhiteSpace(ctx.RelativePath))
            return "Indiquez le chemin MTP à supprimer.";

        var full = CombineMtp(ctx.Root, ctx.RelativePath);
        if (ctx.Device.FileExists(full))
        {
            ctx.Device.DeleteFile(full);
            return $"Fichier supprimé : {full}";
        }

        if (ctx.Device.DirectoryExists(full))
        {
            ctx.Device.DeleteDirectory(full, recursive: true);
            return $"Dossier supprimé : {full}";
        }

        return $"Introuvable : {full}";
    }

    private MtpContext OpenContext(string? pathHint, bool requireFile)
    {
        var devices = MediaDevice.GetDevices().ToList();
        if (devices.Count == 0)
            throw new InvalidOperationException("Aucun appareil MTP.");

        var hint = (pathHint ?? "").Trim();
        MediaDevice? chosen = null;
        string relative = hint;

        // Si le chemin commence par le friendly name
        foreach (var d in devices)
        {
            if (!string.IsNullOrEmpty(hint) &&
                hint.StartsWith(d.FriendlyName, StringComparison.OrdinalIgnoreCase))
            {
                chosen = d;
                relative = hint[d.FriendlyName.Length..].TrimStart('\\', '/');
                break;
            }
        }

        chosen ??= devices[0];
        chosen.Connect();

        var drives = chosen.GetDrives().ToList();
        if (drives.Count == 0)
            throw new InvalidOperationException("Aucun stockage MTP (déverrouillez le téléphone).");

        var root = drives[0].RootDirectory?.FullName?.TrimEnd('\\') ?? @"\";
        // Si relative commence par un nom de drive
        foreach (var drive in drives)
        {
            var name = drive.Name?.Trim() ?? "";
            var driveRoot = drive.RootDirectory?.FullName?.TrimEnd('\\') ?? "";
            if (!string.IsNullOrEmpty(name) &&
                relative.StartsWith(name, StringComparison.OrdinalIgnoreCase))
            {
                root = driveRoot;
                relative = relative[name.Length..].TrimStart('\\', '/');
                break;
            }

            if (!string.IsNullOrEmpty(driveRoot) &&
                relative.StartsWith(driveRoot, StringComparison.OrdinalIgnoreCase))
            {
                root = driveRoot;
                relative = relative[driveRoot.Length..].TrimStart('\\', '/');
                break;
            }
        }

        if (requireFile && string.IsNullOrWhiteSpace(relative))
            throw new InvalidOperationException("Chemin fichier MTP requis.");

        return new MtpContext(chosen, root, relative.Replace('/', '\\'));
    }

    private static string CombineMtp(string root, string relative)
    {
        if (string.IsNullOrWhiteSpace(relative))
            return root;
        if (relative.StartsWith('\\') || relative.StartsWith('/'))
            return relative.Replace('/', '\\');
        return $"{root.TrimEnd('\\')}\\{relative.TrimStart('\\')}";
    }

    private sealed class MtpContext : IDisposable
    {
        public MediaDevice Device { get; }
        public string Root { get; }
        public string RelativePath { get; }

        public MtpContext(MediaDevice device, string root, string relativePath)
        {
            Device = device;
            Root = root;
            RelativePath = relativePath;
        }

        public void Dispose()
        {
            try { Device.Disconnect(); } catch { /* ignore */ }
            Device.Dispose();
        }
    }
}
