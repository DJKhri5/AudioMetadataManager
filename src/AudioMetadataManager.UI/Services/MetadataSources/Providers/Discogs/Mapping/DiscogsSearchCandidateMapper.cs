using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Dtos;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Models;
using System.Globalization;
using System.Text.Json;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Mapping;

/// <summary>
/// Convierte los DTO recibidos desde Discogs en candidatos
/// normalizados utilizados por la aplicación.
/// </summary>
public sealed class DiscogsSearchCandidateMapper
{
    private const string DiscogsWebsiteBaseAddress =
        "https://www.discogs.com";

    /// <summary>
    /// Convierte todos los resultados utilizables.
    /// </summary>
    public IReadOnlyList<DiscogsSearchCandidate> Map(
        IEnumerable<DiscogsSearchItemDto>? items)
    {
        if (items is null)
        {
            return Array.Empty<DiscogsSearchCandidate>();
        }

        return items
            .Select(MapItem)
            .Where(candidate =>
                candidate.HasUsableMetadata)
            .ToArray();
    }

    private static DiscogsSearchCandidate MapItem(
        DiscogsSearchItemDto item)
    {
        ArgumentNullException.ThrowIfNull(item);

        ParseRawTitle(
            item.RawTitle,
            out string? artist,
            out string? title);

        return new DiscogsSearchCandidate
        {
            Id =
                item.Id,

            ResourceType =
                NormalizeText(item.ResourceType) ??
                string.Empty,

            Artist =
                artist,

            Title =
                title,

            Version =
                null,

            Album =
                title,

            Label =
                FirstAvailable(item.Labels),

            Genre =
                FirstAvailable(item.Genres),

            Style =
                FirstAvailable(item.Styles),

            Year =
                ParseYear(item.Year),

            Country =
                NormalizeText(item.Country),

            Format =
                JoinValues(item.Formats),

            DiscogsUri =
                BuildDiscogsUri(item.RelativeUri),

            CoverImageUri =
                BuildAbsoluteUri(item.CoverImage),

            RawTitle =
                NormalizeText(item.RawTitle) ??
                string.Empty
        };
    }

    private static void ParseRawTitle(
        string? rawTitle,
        out string? artist,
        out string? title)
    {
        artist = null;
        title = null;

        string? normalized =
            NormalizeText(rawTitle);

        if (normalized is null)
        {
            return;
        }

        const string separator = " - ";

        int separatorIndex =
            normalized.IndexOf(
                separator,
                StringComparison.Ordinal);

        if (separatorIndex <= 0)
        {
            title = normalized;
            return;
        }

        artist =
            NormalizeText(
                normalized[..separatorIndex]);

        title =
            NormalizeText(
                normalized[
                    (separatorIndex + separator.Length)..]);
    }

    private static int? ParseYear(
    JsonElement value)
    {
        if (value.ValueKind ==
            JsonValueKind.Number)
        {
            return value.TryGetInt32(
                out int numericYear)
                    && numericYear > 0
                        ? numericYear
                        : null;
        }

        if (value.ValueKind ==
            JsonValueKind.String)
        {
            string? text =
                value.GetString();

            return int.TryParse(
                text,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int textYear)
                    && textYear > 0
                        ? textYear
                        : null;
        }

        return null;
    }

    private static string? FirstAvailable(
        IEnumerable<string>? values)
    {
        return values?
            .Select(NormalizeText)
            .FirstOrDefault(value =>
                value is not null);
    }

    private static string? JoinValues(
        IEnumerable<string>? values)
    {
        if (values is null)
        {
            return null;
        }

        string[] normalizedValues =
            values
                .Select(NormalizeText)
                .Where(value =>
                    value is not null)
                .Cast<string>()
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        return normalizedValues.Length == 0
            ? null
            : string.Join(
                ", ",
                normalizedValues);
    }

    private static Uri? BuildDiscogsUri(
        string? relativeUri)
    {
        string? normalized =
            NormalizeText(relativeUri);

        if (normalized is null)
        {
            return null;
        }

        if (Uri.TryCreate(
                normalized,
                UriKind.Absolute,
                out Uri? absoluteUri))
        {
            return absoluteUri;
        }

        return Uri.TryCreate(
            DiscogsWebsiteBaseAddress + normalized,
            UriKind.Absolute,
            out Uri? combinedUri)
                ? combinedUri
                : null;
    }

    private static Uri? BuildAbsoluteUri(
        string? value)
    {
        string? normalized =
            NormalizeText(value);

        return Uri.TryCreate(
            normalized,
            UriKind.Absolute,
            out Uri? uri)
                ? uri
                : null;
    }

    private static string? NormalizeText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}