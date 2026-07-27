using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration
    .FieldMapping.Interfaces;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration
    .FieldMapping.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration
    .FieldMapping;

/// <summary>
/// Implementa la traducción común entre MetadataField y
/// TagLib.Tag.
///
/// Esta primera versión administra artista, título, álbum y
/// género. Los campos Version y Label continuarán pendientes
/// hasta que definamos una estrategia estable para cada formato.
/// </summary>
public sealed class TagLibFieldMapper
    : ITagLibFieldMapper
{
    /// <inheritdoc />
    public bool IsSupported(
        MetadataField field)
    {
        return field is
            MetadataField.Artist or
            MetadataField.Title or
            MetadataField.Album or
            MetadataField.Genre;
    }

    /// <inheritdoc />
    public string ReadValue(
        TagLib.Tag tag,
        MetadataField field)
    {
        ArgumentNullException.ThrowIfNull(
            tag);

        return field switch
        {
            MetadataField.Artist =>
                JoinValues(
                    tag.Performers),

            MetadataField.Title =>
                NormalizeValue(
                    tag.Title),

            MetadataField.Album =>
                NormalizeValue(
                    tag.Album),

            MetadataField.Genre =>
                JoinValues(
                    tag.Genres),

            _ =>
                string.Empty
        };
    }

    /// <inheritdoc />
    public TagLibFieldMappingResult PrepareChange(
        TagLib.Tag tag,
        MetadataFieldChange change)
    {
        ArgumentNullException.ThrowIfNull(
            tag);

        ArgumentNullException.ThrowIfNull(
            change);

        string originalValue =
            ReadValue(
                tag,
                change.Field);

        string requestedValue =
            NormalizeValue(
                change.NewValue);

        if (!IsSupported(
                change.Field))
        {
            return new TagLibFieldMappingResult
            {
                Field =
                    change.Field,

                OriginalValue =
                    originalValue,

                RequestedValue =
                    requestedValue,

                PreparedValue =
                    originalValue,

                IsSupported =
                    false,

                ValuePrepared =
                    false,

                Message =
                    "El mapper todavía no admite este campo."
            };
        }

        WriteValue(
            tag,
            change.Field,
            requestedValue);

        string preparedValue =
            ReadValue(
                tag,
                change.Field);

        bool valuePrepared =
            ValuesEqual(
                preparedValue,
                requestedValue);

        return new TagLibFieldMappingResult
        {
            Field =
                change.Field,

            OriginalValue =
                originalValue,

            RequestedValue =
                requestedValue,

            PreparedValue =
                preparedValue,

            IsSupported =
                true,

            ValuePrepared =
                valuePrepared,

            Message =
                valuePrepared
                    ? "El valor fue preparado correctamente " +
                      "mediante el mapper común TagLibSharp."
                    : "El valor leído después de la asignación " +
                      "no coincide con el solicitado."
        };
    }

    /// <inheritdoc />
    public string NormalizeValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim();
    }

    /// <inheritdoc />
    public bool ValuesEqual(
        string? firstValue,
        string? secondValue)
    {
        return string.Equals(
            NormalizeValue(firstValue),
            NormalizeValue(secondValue),
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Asigna el valor al campo correspondiente.
    /// </summary>
    private static void WriteValue(
        TagLib.Tag tag,
        MetadataField field,
        string value)
    {
        switch (field)
        {
            case MetadataField.Artist:
                tag.Performers =
                    string.IsNullOrWhiteSpace(value)
                        ? Array.Empty<string>()
                        : new[]
                        {
                            value
                        };
                break;

            case MetadataField.Title:
                tag.Title =
                    value;
                break;

            case MetadataField.Album:
                tag.Album =
                    value;
                break;

            case MetadataField.Genre:
                tag.Genres =
                    string.IsNullOrWhiteSpace(value)
                        ? Array.Empty<string>()
                        : new[]
                        {
                            value
                        };
                break;
        }
    }

    /// <summary>
    /// Convierte un conjunto de valores TagLibSharp en una
    /// representación única y estable.
    /// </summary>
    private string JoinValues(
        IEnumerable<string>? values)
    {
        if (values is null)
        {
            return string.Empty;
        }

        return string.Join(
            ", ",
            values
                .Where(value =>
                    !string.IsNullOrWhiteSpace(value))
                .Select(value =>
                    value.Trim()));
    }
}