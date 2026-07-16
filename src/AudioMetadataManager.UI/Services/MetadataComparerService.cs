using AudioMetadataManager.UI.Models;

namespace AudioMetadataManager.UI.Services;

public class MetadataComparerService
{
    private readonly TextNormalizationService _normalizer = new();
    public ComparisonResult Compare(AudioFile audioFile)
    {
        ComparisonResult result = new();

        if (audioFile.ParsedName == null)
        {
            result.Summary = "No existe información analizada del nombre.";

            return result;
        }

        result.ArtistMatches =
            EqualsNormalized(
                audioFile.Artist,
                audioFile.ParsedName.Artist);

        result.TitleMatches =
            EqualsNormalized(
                audioFile.Title,
                audioFile.ParsedName.Title);

        result.AlbumMatches = true;
        result.GenreMatches = true;
        result.YearMatches = true;

        return result;
    }

    private bool EqualsNormalized(string? first, string? second)
    {
        string normalizedFirst =
            _normalizer.Normalize(first);

        string normalizedSecond =
            _normalizer.Normalize(second);

        return !string.IsNullOrWhiteSpace(normalizedFirst) &&
               string.Equals(
                   normalizedFirst,
                   normalizedSecond,
                   StringComparison.Ordinal);
    }
}