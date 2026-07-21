using System.Text.Json.Serialization;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Dtos;

/// <summary>
/// Representa la respuesta JSON raíz de una búsqueda
/// realizada en Discogs.
/// </summary>
public sealed class DiscogsSearchResponseDto
{
    [JsonPropertyName("pagination")]
    public DiscogsPaginationDto? Pagination { get; init; }

    [JsonPropertyName("results")]
    public IReadOnlyList<DiscogsSearchItemDto>? Results { get; init; }
}