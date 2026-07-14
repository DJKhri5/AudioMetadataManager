using AudioMetadataManager.Models;
namespace AudioMetadataManager.Services;
public static class DuplicateDetector
{
    public static void AssignGroups(IReadOnlyCollection<AudioItem> items)
    {
        var groups = items.Where(x => !string.IsNullOrWhiteSpace(x.Artist) && !string.IsNullOrWhiteSpace(x.Title))
            .GroupBy(x => Key(x.Artist, x.Title));
        var n = 1;
        foreach (var group in groups.Where(g => g.Count() > 1))
        {
            var id = $"DUP-{n++:000}";
            foreach (var item in group)
            {
                item.DuplicateGroup = id;
                item.Status = "Duplicado probable";
                item.Warnings = string.Join("; ", new[] { item.Warnings, "Revisión manual obligatoria" }.Where(s => !string.IsNullOrWhiteSpace(s)));
            }
        }
    }
    private static string Key(string artist, string title) => Normalize(artist) + "|" + Normalize(title);
    private static string Normalize(string value) => new(value.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
}
