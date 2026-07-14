using System.Security.Cryptography;
using System.Text.Json;
using AudioMetadataManager.Models;
namespace AudioMetadataManager.Services;
public sealed record BackupEntry(string OriginalPath, string BackupPath, long SizeBytes, string Sha256, DateTime OriginalLastWriteUtc);
public static class BackupService
{
    public static async Task<string> BackupAsync(string root, IEnumerable<AudioItem> selected, IProgress<string>? progress, CancellationToken token)
    {
        var operation = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
        var destination = Path.Combine(root, "_Respaldo Audio", operation, "Originales");
        Directory.CreateDirectory(destination);
        var entries = new List<BackupEntry>();
        foreach (var item in selected)
        {
            token.ThrowIfCancellationRequested();
            var target = Path.Combine(destination, item.RelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            progress?.Report($"Respaldando {item.FileName}");
            File.Copy(item.FullPath, target, overwrite: false);
            var originalHash = await HashAsync(item.FullPath, token);
            var backupHash = await HashAsync(target, token);
            if (!string.Equals(originalHash, backupHash, StringComparison.OrdinalIgnoreCase)) throw new IOException($"Falló la verificación de {item.FileName}");
            entries.Add(new BackupEntry(item.FullPath, target, item.SizeBytes, originalHash, File.GetLastWriteTimeUtc(item.FullPath)));
        }
        var manifest = Path.Combine(root, "_Respaldo Audio", operation, "manifest.json");
        await File.WriteAllTextAsync(manifest, JsonSerializer.Serialize(new { operation, createdAt = DateTime.Now, entries }, new JsonSerializerOptions { WriteIndented = true }), token);
        return manifest;
    }
    private static async Task<string> HashAsync(string path, CancellationToken token)
    {
        await using var stream = File.OpenRead(path);
        var hash = await SHA256.HashDataAsync(stream, token);
        return Convert.ToHexString(hash);
    }
}
