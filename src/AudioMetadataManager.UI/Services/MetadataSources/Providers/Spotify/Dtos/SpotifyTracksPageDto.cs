using System.Text.Json.Serialization;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Dtos;

/// <summary>
/// Representa la página de resultados de pistas dentro de una
/// respuesta de búsqueda de Spotify.
/// </summary>
public sealed class SpotifyTracksPageDto
{
    [JsonPropertyName("items")]
    public IReadOnlyList<SpotifyTrackDto>? Items { get; init; }

    [JsonPropertyName("total")]
    public int Total { get; init; }
}
