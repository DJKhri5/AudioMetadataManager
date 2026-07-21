using System.Text.Json.Serialization;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Dtos;

/// <summary>
/// Representa la paginación incluida en una respuesta
/// de búsqueda de Discogs.
/// </summary>
public sealed class DiscogsPaginationDto
{
    [JsonPropertyName("page")]
    public int Page { get; init; }

    [JsonPropertyName("pages")]
    public int Pages { get; init; }

    [JsonPropertyName("per_page")]
    public int ResultsPerPage { get; init; }

    [JsonPropertyName("items")]
    public int TotalItems { get; init; }
}