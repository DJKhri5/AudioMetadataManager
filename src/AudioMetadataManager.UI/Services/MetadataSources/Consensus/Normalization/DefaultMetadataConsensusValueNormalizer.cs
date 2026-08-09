using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation.Application.Testing.Infrastructure;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Normalization;

/// <summary>
/// Aplica reglas de normalización seguras y específicas para
/// cada campo de metadatos.
/// </summary>
public sealed class DefaultMetadataConsensusValueNormalizer
    : IMetadataConsensusValueNormalizer
{
    private static readonly Regex MultipleWhitespaceRegex =
        new(
            @"\s+",
            RegexOptions.Compiled);

    private static readonly Regex ArtistSeparatorRegex =
        new(
            @"[\*\._\-]+",
            RegexOptions.Compiled);

    private static readonly Regex ArtistConnectorRegex =
        new(
            @"\s+(AND|FEATURING|FEAT|FT|VERSUS|VS|WITH)\s+",
            RegexOptions.Compiled |
            RegexOptions.IgnoreCase);

    /// <summary>
    /// Normaliza el valor según el campo recibido.
    /// </summary>
    public string Normalize(
        MetadataField field,
        string? value)
    {
        string normalized =
            NormalizeBase(
                value);

        if (string.IsNullOrWhiteSpace(
                normalized))
        {
            return string.Empty;
        }

        return field switch
        {
            MetadataField.Artist =>
                NormalizeArtist(
                    normalized),

            MetadataField.Title =>
                NormalizeGeneralText(
                    normalized),

            MetadataField.Version =>
                NormalizeVersion(
                    normalized),

            MetadataField.Album =>
                NormalizeGeneralText(
                    normalized),

            MetadataField.Genre =>
                NormalizeGenre(
                    normalized),

            MetadataField.Label =>
                NormalizeLabel(
                    normalized),

            _ =>
                NormalizeGeneralText(
                    normalized)
        };
    }

    private static string NormalizeArtist(
        string value)
    {
        string normalized =
            ArtistSeparatorRegex.Replace(
                value,
                " ");

        normalized =
            ArtistConnectorRegex.Replace(
                normalized,
                " & ");

        normalized =
            NormalizeWhitespace(
                normalized);

        return RemoveWhitespace(
            normalized);
    }

    private static string NormalizeVersion(
        string value)
    {
        string normalized =
            value
                .Replace(
                    "EXTENDED VERSION",
                    "EXTENDED MIX",
                    StringComparison.OrdinalIgnoreCase)
                .Replace(
                    "ORIGINAL VERSION",
                    "ORIGINAL MIX",
                    StringComparison.OrdinalIgnoreCase)
                .Replace(
                    "RADIO VERSION",
                    "RADIO EDIT",
                    StringComparison.OrdinalIgnoreCase);

        return NormalizeGeneralText(
            normalized);
    }

    private static string NormalizeGenre(
        string value)
    {
        string normalized =
            value
                .Replace(
                    "ELECTRONICA",
                    DiagnosticMetadataTestValues.CreateGenre(),
                    StringComparison.OrdinalIgnoreCase)
                .Replace(
                    "ELECTRONIC MUSIC",
                    DiagnosticMetadataTestValues.CreateGenre(),
                    StringComparison.OrdinalIgnoreCase);

        return NormalizeGeneralText(
            normalized);
    }

    private static string NormalizeLabel(
        string value)
    {
        return NormalizeWhitespace(
            value);
    }

    private static string NormalizeGeneralText(
        string value)
    {
        return NormalizeWhitespace(
            value);
    }

    private static string NormalizeBase(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(
                value))
        {
            return string.Empty;
        }

        string decomposed =
            value
                .Trim()
                .ToUpperInvariant()
                .Normalize(
                    NormalizationForm.FormD);

        StringBuilder builder =
            new();

        foreach (char character in decomposed)
        {
            UnicodeCategory category =
                CharUnicodeInfo.GetUnicodeCategory(
                    character);

            if (category !=
                UnicodeCategory.NonSpacingMark)
            {
                builder.Append(
                    character);
            }
        }

        return builder
            .ToString()
            .Normalize(
                NormalizationForm.FormC);
    }

    private static string NormalizeWhitespace(
        string value)
    {
        return MultipleWhitespaceRegex
            .Replace(
                value.Trim(),
                " ");
    }

    private static string RemoveWhitespace(
        string value)
    {
        return value.Replace(
            " ",
            string.Empty,
            StringComparison.Ordinal);
    }
}