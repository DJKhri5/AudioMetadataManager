using System.Text.Json.Serialization;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Dtos;

/// <summary>
/// Representa una imagen de portada informada por Spotify.
/// </summary>
public sealed class SpotifyImageDto
{
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    [JsonPropertyName("width")]
    public int? Width { get; init; }

    [JsonPropertyName("height")]
    public int? Height { get; init; }
}
