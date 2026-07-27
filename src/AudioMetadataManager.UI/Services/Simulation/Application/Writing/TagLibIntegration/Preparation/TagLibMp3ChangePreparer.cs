using System.IO;
using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Preparation;

/// <summary>
/// Prepara cambios de metadatos MP3 sobre un objeto TagLib.Tag
/// exclusivamente en memoria.
///
/// Esta clase nunca ejecuta TagLib.File.Save().
/// </summary>
public sealed class TagLibMp3ChangePreparer
{
    /// <summary>
    /// Abre el MP3, asigna los cambios en memoria, comprueba los
    /// valores preparados y vuelve a abrir el archivo para
    /// verificar que nada fue persistido.
    /// </summary>
    public TagLibMp3PreparationResult Prepare(
        string? filePath,
        IEnumerable<MetadataFieldChange>? changes)
    {
        List<string> messages =
            new();

        string normalizedPath =
            NormalizePath(
                filePath);

        MetadataFieldChange[] validChanges =
            changes?
                .Where(change => change is not null)
                .Where(change => change.IsValidChange)
                .ToArray() ??
            Array.Empty<MetadataFieldChange>();

        if (string.IsNullOrWhiteSpace(normalizedPath))
        {
            messages.Add(
                "No se recibió una ruta válida.");

            return BuildFailure(
                normalizedPath,
                messages);
        }

        if (!File.Exists(normalizedPath))
        {
            messages.Add(
                "El archivo MP3 indicado no existe.");

            return BuildFailure(
                normalizedPath,
                messages);
        }

        if (!string.Equals(
                Path.GetExtension(normalizedPath),
                ".mp3",
                StringComparison.OrdinalIgnoreCase))
        {
            messages.Add(
                "El preparador actual sólo admite archivos MP3.");

            return BuildFailure(
                normalizedPath,
                messages);
        }

        if (validChanges.Length == 0)
        {
            messages.Add(
                "No se recibieron cambios válidos para preparar.");

            return BuildFailure(
                normalizedPath,
                messages);
        }

        try
        {
            Dictionary<MetadataField, string>
                persistedValuesBefore =
                    new();

            List<TagLibMp3FieldPreparationResult>
                fieldResults =
                    new();

            int pictureCountBefore;
            int pictureCountAfter;

            using (TagLib.File tagFile =
                TagLib.File.Create(normalizedPath))
            {
                TagLib.Tag tag =
                    tagFile.Tag;

                pictureCountBefore =
                    tag.Pictures?.Length ?? 0;

                foreach (MetadataFieldChange change
                    in validChanges)
                {
                    string originalValue =
                        ReadValue(
                            tag,
                            change.Field);

                    persistedValuesBefore[change.Field] =
                        originalValue;

                    TagLibMp3FieldPreparationResult result =
                        PrepareField(
                            tag,
                            change,
                            originalValue);

                    fieldResults.Add(
                        result);
                }

                pictureCountAfter =
                    tag.Pictures?.Length ?? 0;

                /*
                 * Deliberadamente no se ejecuta:
                 *
                 * tagFile.Save();
                 */
            }

            bool physicalFileRemainedUnchanged =
                VerifyPersistedValuesUnchanged(
                    normalizedPath,
                    persistedValuesBefore);

            messages.Add(
                "TagLibSharp abrió correctamente el MP3.");

            messages.Add(
                "Los cambios fueron asignados únicamente al " +
                "objeto de etiquetas en memoria.");

            messages.Add(
                "No se ejecutó TagLib.File.Save().");

            messages.Add(
                physicalFileRemainedUnchanged
                    ? "La reapertura confirmó que el archivo " +
                      "físico permaneció sin cambios."
                    : "La reapertura detectó una diferencia " +
                      "inesperada en los valores persistidos.");

            messages.Add(
                pictureCountBefore == pictureCountAfter
                    ? "Las imágenes incrustadas permanecieron " +
                      "intactas durante la preparación."
                    : "La cantidad de imágenes cambió durante " +
                      "la preparación en memoria.");

            return new TagLibMp3PreparationResult
            {
                FilePath =
                    normalizedPath,

                FileOpened =
                    true,

                FieldResults =
                    fieldResults.ToArray(),

                PictureCountBefore =
                    pictureCountBefore,

                PictureCountAfter =
                    pictureCountAfter,

                SaveWasExecuted =
                    false,

                PhysicalFileRemainedUnchanged =
                    physicalFileRemainedUnchanged,

                Messages =
                    messages.ToArray()
            };
        }
        catch (TagLib.UnsupportedFormatException exception)
        {
            messages.Add(
                "TagLibSharp no reconoce el formato: " +
                exception.Message);
        }
        catch (TagLib.CorruptFileException exception)
        {
            messages.Add(
                "El archivo o sus etiquetas parecen estar " +
                $"dañados: {exception.Message}");
        }
        catch (UnauthorizedAccessException exception)
        {
            messages.Add(
                "Windows rechazó el acceso al archivo: " +
                exception.Message);
        }
        catch (IOException exception)
        {
            messages.Add(
                "Ocurrió un error de entrada o salida: " +
                exception.Message);
        }
        catch (Exception exception)
        {
            messages.Add(
                "Ocurrió un error inesperado: " +
                exception.Message);
        }

        return BuildFailure(
            normalizedPath,
            messages);
    }

    private static TagLibMp3FieldPreparationResult PrepareField(
        TagLib.Tag tag,
        MetadataFieldChange change,
        string originalValue)
    {
        string requestedValue =
            NormalizeValue(
                change.NewValue);

        bool isSupported =
            IsSupportedField(
                change.Field);

        if (!isSupported)
        {
            return new TagLibMp3FieldPreparationResult
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

                WasPrepared =
                    false,

                MatchesRequestedValue =
                    false,

                Message =
                    "El campo todavía no dispone de un mapeo " +
                    "seguro en TagLibSharp."
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

        bool matches =
            ValuesEqual(
                preparedValue,
                requestedValue);

        return new TagLibMp3FieldPreparationResult
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

            WasPrepared =
                true,

            MatchesRequestedValue =
                matches,

            Message =
                matches
                    ? "El valor fue asignado correctamente en " +
                      "memoria. No se ejecutó Save()."
                    : "El valor leído después de la asignación " +
                      "no coincide con el solicitado."
        };
    }

    private static bool IsSupportedField(
        MetadataField field)
    {
        return field is
            MetadataField.Artist or
            MetadataField.Title or
            MetadataField.Album or
            MetadataField.Genre;
    }

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

    private static string ReadValue(
        TagLib.Tag tag,
        MetadataField field)
    {
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

    private static bool VerifyPersistedValuesUnchanged(
        string filePath,
        IReadOnlyDictionary<MetadataField, string>
            expectedValues)
    {
        using TagLib.File reopenedFile =
            TagLib.File.Create(
                filePath);

        TagLib.Tag reopenedTag =
            reopenedFile.Tag;

        return expectedValues.All(
            pair =>
                ValuesEqual(
                    ReadValue(
                        reopenedTag,
                        pair.Key),
                    pair.Value));
    }

    private static TagLibMp3PreparationResult BuildFailure(
        string filePath,
        IReadOnlyList<string> messages)
    {
        return new TagLibMp3PreparationResult
        {
            FilePath =
                filePath,

            FileOpened =
                false,

            SaveWasExecuted =
                false,

            PhysicalFileRemainedUnchanged =
                true,

            Messages =
                messages.ToArray()
        };
    }

    private static string NormalizePath(
        string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return string.Empty;
        }

        try
        {
            return Path.GetFullPath(
                filePath.Trim());
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string NormalizeValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim();
    }

    private static string JoinValues(
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

    private static bool ValuesEqual(
        string? firstValue,
        string? secondValue)
    {
        return string.Equals(
            NormalizeValue(firstValue),
            NormalizeValue(secondValue),
            StringComparison.OrdinalIgnoreCase);
    }
}