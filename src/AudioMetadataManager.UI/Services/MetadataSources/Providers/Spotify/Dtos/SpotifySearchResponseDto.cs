using System.Text.Json.Serialization;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Dtos;

/// <summary>
/// Representa la respuesta JSON raíz de una búsqueda de pistas
/// realizada en Spotify.
/// </summary>
public sealed class SpotifySearchResponseDto
{
    [JsonPropertyName("tracks")]
    public SpotifyTracksPageDto? Tracks { get; init; }
}
