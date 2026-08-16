using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Validation;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration
    .FieldMapping.Interfaces;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration
    .FieldMapping.Models;
using System.IO;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration
    .FieldMapping;

/// <summary>
/// Implementa la traducción común entre MetadataField y
/// TagLib.Tag.
///
/// Version se almacena mediante TagLib.Tag.Subtitle, que en ID3v2
/// corresponde al frame TIT3. Label se almacena mediante
/// TagLib.Tag.Publisher, que en ID3v2 corresponde al frame TPUB.
/// De esta forma ambos campos participan en la misma escritura y
/// verificación posterior que los campos productivos existentes.
/// </summary>
public sealed class TagLibFieldMapper
    : ITagLibFieldMapper
{
    /// <inheritdoc />
    public bool IsSupported(
        MetadataField field)
    {
        return MetadataProductiveFieldSupport.IsSupported(
            field);
    }

    /// <inheritdoc />
    public TagLibFieldMappingResult PrepareChange(
        TagLib.File file,
        MetadataFieldChange change)
    {
        ArgumentNullException.ThrowIfNull(
            file);

        ArgumentNullException.ThrowIfNull(
            change);

        bool requiresExplicitId3v2Label =
            change.Field == MetadataField.Label &&
            string.Equals(
                Path.GetExtension(
                    file.Name),
                ".mp3",
                StringComparison.OrdinalIgnoreCase);

        if (!requiresExplicitId3v2Label)
        {
            return PrepareChange(
                file.Tag,
                change);
        }

        TagLib.Id3v2.Tag? id3v2Tag =
            file.GetTag(
                TagLib.TagTypes.Id3v2,
                true)
            as TagLib.Id3v2.Tag;

        if (id3v2Tag is null)
        {
            return new TagLibFieldMappingResult
            {
                Field =
                    change.Field,

                OriginalValue =
                    string.Empty,

                RequestedValue =
                    NormalizeValue(
                        change.NewValue),

                PreparedValue =
                    string.Empty,

                IsSupported =
                    true,

                ValuePrepared =
                    false,

                Message =
                    "No fue posible obtener o crear " +
                    "la etiqueta ID3v2 del archivo."
            };
        }

        string requestedValue =
            NormalizeValue(
                change.NewValue);

        TagLib.Id3v2.TextInformationFrame frame =
            TagLib.Id3v2.TextInformationFrame.Get(
                id3v2Tag,
                "TPUB",
                true);

        string originalValue =
            frame.Text is { Length: > 0 }
                ? NormalizeValue(
                    frame.Text[0])
                : string.Empty;

        frame.Text =
            string.IsNullOrWhiteSpace(
                requestedValue)
                ? Array.Empty<string>()
                : new[]
                {
                    requestedValue
                };

        string preparedValue =
            frame.Text is { Length: > 0 }
                ? NormalizeValue(
                    frame.Text[0])
                : string.Empty;

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
                    ? "El sello fue preparado mediante " +
                      "el frame ID3v2 TPUB."
                    : "El frame TPUB no conservó " +
                      "el valor solicitado."
        };
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

            MetadataField.Version =>
                NormalizeValue(
                    tag.Subtitle),

            MetadataField.Album =>
                NormalizeValue(
                    tag.Album),

            MetadataField.Label =>
                NormalizeValue(
                    tag.Publisher),

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

            case MetadataField.Version:
                tag.Subtitle =
                    value;
                break;

            case MetadataField.Album:
                tag.Album =
                    value;
                break;

            case MetadataField.Label:
                tag.Publisher =
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
