using System.Text.RegularExpressions;

namespace AudioMetadataManager.Services;

public sealed record ParsedName(string Artist, string Title, string Version, string CleanStem, IReadOnlyList<string> Warnings);

public static partial class FileNameParser
{
    private static readonly string[] NoiseTokens = ["4clubbers.pl", "www.4clubbers.pl", "by P77"];

    public static ParsedName Parse(string fileName)
    {
        var stem = Path.GetFileNameWithoutExtension(fileName).Trim();
        var warnings = new List<string>();
        var original = stem;
        stem = TrackPrefix().Replace(stem, "").Trim();
        stem = stem.Replace("Ti#U00ebsto", "Tiësto", StringComparison.OrdinalIgnoreCase);
        foreach (var token in NoiseTokens)
            stem = Regex.Replace(stem, $@"\[?{Regex.Escape(token)}\]?", "", RegexOptions.IgnoreCase).Trim();
        stem = Regex.Replace(stem, @"\s+MASTER\s*$", "", RegexOptions.IgnoreCase).Trim();
        stem = Regex.Replace(stem, @"\s{2,}", " ").Trim(' ', '-', '_');
        if (!string.Equals(original, stem, StringComparison.Ordinal)) warnings.Add("Nombre limpiado");

        var separator = stem.IndexOf(" - ", StringComparison.Ordinal);
        if (separator <= 0 || separator >= stem.Length - 3)
            return new ParsedName("", stem, "", stem, warnings.Append("No se pudo separar artista y título").ToArray());

        var artist = stem[..separator].Trim();
        var titleFull = stem[(separator + 3)..].Trim();
        var version = "";
        var title = titleFull;
        var match = EndingVersion().Match(titleFull);
        if (match.Success)
        {
            title = match.Groups["title"].Value.Trim();
            version = match.Groups["version"].Value.Trim();
        }
        var cleanStem = string.IsNullOrWhiteSpace(version) ? $"{artist} - {title}" : $"{artist} - {title} ({version})";
        return new ParsedName(artist, title, version, cleanStem, warnings);
    }

    [GeneratedRegex(@"^\s*\d{1,3}\s*[.\-_)]\s*")]
    private static partial Regex TrackPrefix();
    [GeneratedRegex(@"^(?<title>.+?)\s*\((?<version>[^()]*(?:Mix|Remix|Edit|Rework|Bootleg|Mashup|Dub|Version)[^()]*)\)\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex EndingVersion();
}
