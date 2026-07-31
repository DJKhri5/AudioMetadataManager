using System.Text.Json.Serialization;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Dtos;

/// <summary>
/// Representa un artista acreditado en una pista de Spotify.
/// </summary>
public sealed class SpotifyArtistDto
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}
