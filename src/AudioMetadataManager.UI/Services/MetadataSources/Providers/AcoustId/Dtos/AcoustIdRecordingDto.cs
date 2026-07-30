using System.Text.Json.Serialization;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Dtos;

/// <summary>
/// Representa una grabación de MusicBrainz asociada a una
/// coincidencia de huella acústica.
///
/// Este DTO refleja únicamente los campos que utiliza
/// actualmente la aplicación.
/// </summary>
public sealed class AcoustIdRecordingDto
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("duration")]
    public int? Duration { get; init; }

    [JsonPropertyName("artists")]
    public IReadOnlyList<AcoustIdArtistDto>? Artists { get; init; }
}
