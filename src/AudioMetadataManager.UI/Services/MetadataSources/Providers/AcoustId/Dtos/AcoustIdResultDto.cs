using System.Text.Json.Serialization;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Dtos;

/// <summary>
/// Representa una coincidencia individual de huella acústica
/// recibida desde AcoustID.
/// </summary>
public sealed class AcoustIdResultDto
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("score")]
    public double Score { get; init; }

    [JsonPropertyName("recordings")]
    public IReadOnlyList<AcoustIdRecordingDto>? Recordings { get; init; }
}
