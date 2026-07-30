using System.Text.Json.Serialization;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Dtos;

/// <summary>
/// Representa el error informado por AcoustID cuando una
/// consulta no puede completarse.
/// </summary>
public sealed class AcoustIdErrorDto
{
    [JsonPropertyName("code")]
    public int Code { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }
}
