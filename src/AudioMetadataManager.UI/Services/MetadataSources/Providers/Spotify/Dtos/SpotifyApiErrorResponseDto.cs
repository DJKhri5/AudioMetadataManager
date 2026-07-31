using System.Text.Json.Serialization;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Dtos;

/// <summary>
/// Representa el error informado por la API de búsqueda de
/// Spotify.
/// </summary>
public sealed class SpotifyApiErrorResponseDto
{
    [JsonPropertyName("error")]
    public SpotifyApiErrorDto? Error { get; init; }
}

/// <summary>
/// Detalle del error informado por la API de Spotify.
/// </summary>
public sealed class SpotifyApiErrorDto
{
    [JsonPropertyName("status")]
    public int Status { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }
}
