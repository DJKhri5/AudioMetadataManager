using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Dtos;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Mapping;

/// <summary>
/// Convierte los DTO recibidos desde Spotify en candidatos
/// normalizados utilizados por la aplicación.
/// </summary>
public sealed class SpotifySearchCandidateMapper
{
    /// <summary>
    /// Convierte todos los resultados utilizables.
    /// </summary>
    public IReadOnlyList<SpotifySearchCandidate> Map(
        IEnumerable<SpotifyTrackDto>? items)
    {
        if (items is null)
        {
            return Array.Empty<SpotifySearchCandidate>();
        }

        return items
            .Select(MapItem)
            .Where(candidate =>
                candidate.HasUsableMetadata)
            .ToArray();
    }

    private static SpotifySearchCandidate MapItem(
        SpotifyTrackDto item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new SpotifySearchCandidate
        {
            Id =
                NormalizeText(item.Id) ??
                string.Empty,

            Artist =
                JoinArtists(item.Artists),

            Title =
                NormalizeText(item.Name),

            Album =
                NormalizeText(item.Album?.Name),

            ReleaseDate =
                NormalizeText(item.Album?.ReleaseDate),

            Duration =
                item.DurationMs > 0
                    ? TimeSpan.FromMilliseconds(item.DurationMs)
                    : TimeSpan.Zero,

            Popularity =
                Math.Clamp(item.Popularity, 0, 100),

            ArtworkUrl =
                SelectBestImage(item.Album?.Images),

            SpotifyUri =
                BuildAbsoluteUri(
                    item.ExternalUrls?.Spotify)
        };
    }

    private static string? JoinArtists(
        IEnumerable<SpotifyArtistDto>? artists)
    {
        if (artists is null)
        {
            return null;
        }

        string[] names =
            artists
                .Select(artist =>
                    NormalizeText(artist.Name))
                .Where(name =>
                    name is not null)
                .Cast<string>()
                .ToArray();

        return names.Length == 0
            ? null
            : string.Join(
                ", ",
                names);
    }

    private static string? SelectBestImage(
        IEnumerable<SpotifyImageDto>? images)
    {
        if (images is null)
        {
            return null;
        }

        return images
            .OrderByDescending(image =>
                image.Width ?? 0)
            .Select(image =>
                NormalizeText(image.Url))
            .FirstOrDefault(url =>
                url is not null);
    }

    private static Uri? BuildAbsoluteUri(
        string? value)
    {
        string? normalized =
            NormalizeText(value);

        return Uri.TryCreate(
            normalized,
            UriKind.Absolute,
            out Uri? uri)
                ? uri
                : null;
    }

    private static string? NormalizeText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
