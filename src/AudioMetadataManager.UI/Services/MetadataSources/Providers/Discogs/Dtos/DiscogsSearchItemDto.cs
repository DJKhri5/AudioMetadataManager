using System.Text.Json;
using System.Text.Json.Serialization;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Dtos;

/// <summary>
/// Representa un elemento individual recibido desde el
/// endpoint database/search de Discogs.
///
/// Este DTO refleja únicamente los campos que utiliza
/// actualmente la aplicación.
/// </summary>
public sealed class DiscogsSearchItemDto
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("type")]
    public string? ResourceType { get; init; }

    [JsonPropertyName("title")]
    public string? RawTitle { get; init; }

    [JsonPropertyName("year")]
    public JsonElement Year { get; init; }

    [JsonPropertyName("country")]
    public string? Country { get; init; }

    [JsonPropertyName("format")]
    public IReadOnlyList<string>? Formats { get; init; }

    [JsonPropertyName("label")]
    public IReadOnlyList<string>? Labels { get; init; }

    [JsonPropertyName("genre")]
    public IReadOnlyList<string>? Genres { get; init; }

    [JsonPropertyName("style")]
    public IReadOnlyList<string>? Styles { get; init; }

    [JsonPropertyName("uri")]
    public string? RelativeUri { get; init; }

    [JsonPropertyName("cover_image")]
    public string? CoverImage { get; init; }

    [JsonPropertyName("thumb")]
    public string? Thumbnail { get; init; }
}