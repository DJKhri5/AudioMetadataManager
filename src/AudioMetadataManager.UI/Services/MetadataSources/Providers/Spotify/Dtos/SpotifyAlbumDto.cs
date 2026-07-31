using System.Text.Json.Serialization;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Dtos;

/// <summary>
/// Representa el álbum al que pertenece una pista de Spotify.
/// </summary>
public sealed class SpotifyAlbumDto
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; init; }

    [JsonPropertyName("images")]
    public IReadOnlyList<SpotifyImageDto>? Images { get; init; }
}
