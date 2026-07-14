using AudioMetadataManager.Models;

namespace AudioMetadataManager.Services;
public static class LibraryScanner
{
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase) { ".mp3", ".wav", ".flac", ".aif", ".aiff" };
    public static Task<List<AudioItem>> ScanAsync(string root, IProgress<(int done, string file)>? progress, CancellationToken token) => Task.Run(() =>
    {
        var paths = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(p => Extensions.Contains(Path.GetExtension(p)))
            .Where(p => !p.Split(Path.DirectorySeparatorChar).Any(s => s.Equals("_Respaldo Audio", StringComparison.OrdinalIgnoreCase)))
            .ToList();
        var items = new List<AudioItem>(paths.Count);
        for (var i = 0; i < paths.Count; i++)
        {
            token.ThrowIfCancellationRequested();
            items.Add(MetadataReader.Read(root, paths[i]));
            progress?.Report((i + 1, paths[i]));
        }
        DuplicateDetector.AssignGroups(items);
        return items;
    }, token);
}
