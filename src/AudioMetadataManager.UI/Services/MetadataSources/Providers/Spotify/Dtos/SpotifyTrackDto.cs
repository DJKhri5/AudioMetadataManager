using System.Text.Json.Serialization;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Dtos;

/// <summary>
/// Representa una pista individual recibida desde el endpoint
/// de búsqueda de Spotify.
///
/// Este DTO refleja únicamente los campos que utiliza
/// actualmente la aplicación.
/// </summary>
public sealed class SpotifyTrackDto
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("duration_ms")]
    public long DurationMs { get; init; }

    [JsonPropertyName("popularity")]
    public int Popularity { get; init; }

    [JsonPropertyName("artists")]
    public IReadOnlyList<SpotifyArtistDto>? Artists { get; init; }

    [JsonPropertyName("album")]
    public SpotifyAlbumDto? Album { get; init; }

    [JsonPropertyName("external_urls")]
    public SpotifyExternalUrlsDto? ExternalUrls { get; init; }
}
