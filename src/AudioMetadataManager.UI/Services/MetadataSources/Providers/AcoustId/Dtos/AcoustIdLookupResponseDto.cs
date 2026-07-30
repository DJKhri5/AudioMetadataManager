using System.Text.Json.Serialization;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Dtos;

/// <summary>
/// Representa la respuesta JSON raíz de una consulta de
/// identificación realizada en AcoustID.
/// </summary>
public sealed class AcoustIdLookupResponseDto
{
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("results")]
    public IReadOnlyList<AcoustIdResultDto>? Results { get; init; }

    [JsonPropertyName("error")]
    public AcoustIdErrorDto? Error { get; init; }
}
