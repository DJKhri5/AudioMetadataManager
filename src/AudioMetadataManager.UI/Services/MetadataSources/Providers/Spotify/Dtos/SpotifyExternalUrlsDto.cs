using System.Text.Json.Serialization;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Dtos;

/// <summary>
/// Representa las direcciones públicas asociadas a un recurso
/// de Spotify.
/// </summary>
public sealed class SpotifyExternalUrlsDto
{
    [JsonPropertyName("spotify")]
    public string? Spotify { get; init; }
}
