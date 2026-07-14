using AudioMetadataManager.Models;
using TagLib;

namespace AudioMetadataManager.Services;

public static class MetadataReader
{
    public static AudioItem Read(string root, string path)
    {
        var info = new FileInfo(path);
        var parsed = FileNameParser.Parse(info.Name);
        var item = new AudioItem
        {
            FullPath = path,
            RelativePath = Path.GetRelativePath(root, path),
            FileName = info.Name,
            Extension = info.Extension.ToLowerInvariant(),
            SizeBytes = info.Length,
            Artist = parsed.Artist,
            Title = parsed.Title,
            Version = parsed.Version,
            ProposedFileName = parsed.CleanStem + info.Extension.ToLowerInvariant(),
            Warnings = string.Join("; ", parsed.Warnings)
        };

        try
        {
            using var media = TagLib.File.Create(path);
            item.Duration = media.Properties.Duration;
            item.AudioBitrateKbps = media.Properties.AudioBitrate;
            item.SampleRateHz = media.Properties.AudioSampleRate;
            item.BitsPerSample = media.Properties.BitsPerSample;
            item.Channels = media.Properties.AudioChannels;
            item.Codec = string.Join(", ", media.Properties.Codecs.Select(c => c.Description).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct());
            item.HasArtwork = media.Tag.Pictures?.Length > 0;
            item.Album = media.Tag.Album ?? "";
            item.Genre = media.Tag.FirstGenre ?? "";
            item.Year = media.Tag.Year;

            if (string.IsNullOrWhiteSpace(item.Artist) || string.IsNullOrWhiteSpace(item.Title))
            {
                item.Artist = media.Tag.FirstPerformer ?? "";
                item.Title = media.Tag.Title ?? item.Title;
                item.SourceUsed = "Etiquetas";
                item.ProposedFileName = BuildFileName(item.Artist, item.Title, info.Extension);
            }

            var warnings = new List<string>(item.Warnings.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
            if (string.IsNullOrWhiteSpace(media.Tag.FirstPerformer) || string.IsNullOrWhiteSpace(media.Tag.Title)) warnings.Add("Etiquetas incompletas");
            if (!string.IsNullOrWhiteSpace(parsed.Artist) && !string.IsNullOrWhiteSpace(media.Tag.FirstPerformer) && !Equivalent(parsed.Artist, media.Tag.FirstPerformer)) warnings.Add("Conflicto artista nombre/etiqueta");
            if (item.Extension == ".mp3" && item.AudioBitrateKbps >= 320 && item.SizeBytes > 0) warnings.Add("Calidad real pendiente de análisis espectral");
            item.Warnings = string.Join("; ", warnings.Distinct());
            item.Status = item.Warnings.Length == 0 ? "Listo para revisión" : "Revisión manual";
        }
        catch (Exception ex)
        {
            item.Status = "Error de lectura";
            item.Warnings = string.IsNullOrWhiteSpace(item.Warnings) ? ex.Message : item.Warnings + "; " + ex.Message;
        }
        return item;
    }

    private static string BuildFileName(string artist, string title, string ext) =>
        string.IsNullOrWhiteSpace(artist) ? Path.GetFileNameWithoutExtension(title) + ext.ToLowerInvariant() : $"{artist} - {title}{ext.ToLowerInvariant()}";

    private static bool Equivalent(string a, string b) => Normalize(a) == Normalize(b);
    private static string Normalize(string value) => new(value.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
}
