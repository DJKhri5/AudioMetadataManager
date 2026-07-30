using System.Text.Json.Serialization;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Dtos;

/// <summary>
/// Representa un artista acreditado en una grabación de
/// MusicBrainz, según lo informado por AcoustID.
/// </summary>
public sealed class AcoustIdArtistDto
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}
