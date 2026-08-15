using System.IO;
using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Verification.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Verification.Engine;

/// <summary>
/// Reabre un archivo mediante TagLibSharp y verifica los valores
/// que debían quedar persistidos después de una escritura.
/// </summary>
public sealed class MetadataWriterVerificationEngine :
    IMetadataWriterVerificationEngine
{
    public MetadataVerificationResult Verify(
        string? filePath,
        IEnumerable<MetadataFieldChange>? changes,
        int pictureCountBefore)
    {
        List<string> messages =
            new();

        string normalizedPath =
            NormalizePath(filePath);

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
                pictureCountBefore,
                messages);
        }

        if (!File.Exists(normalizedPath))
        {
            messages.Add(
                "El archivo que debía verificarse no existe.");

            return BuildFailure(
                normalizedPath,
                pictureCountBefore,
                messages);
        }

        if (validChanges.Length == 0)
        {
            messages.Add(
                "No se recibieron cambios válidos para verificar.");

            return BuildFailure(
                normalizedPath,
                pictureCountBefore,
                messages);
        }

        try
        {
            using TagLib.File tagFile =
                TagLib.File.Create(
                    normalizedPath);

            List<MetadataFieldVerificationResult>
                fieldResults =
                    new();

            foreach (MetadataFieldChange change
                in validChanges)
            {
                bool isSupported =
                    IsSupportedField(
                        change.Field);

                string expectedValue =
                    NormalizeValue(
                        change.NewValue);

                string persistedValue =
                    isSupported
                        ? ReadValue(
                            tagFile.Tag,
                            change.Field)
                        : string.Empty;

                bool matches =
                    isSupported &&
                    ValuesEqual(
                        persistedValue,
                        expectedValue);

                fieldResults.Add(
                    new MetadataFieldVerificationResult
                    {
                        Field =
                            change.Field,

                        ExpectedValue =
                            expectedValue,

                        PersistedValue =
                            persistedValue,

                        IsSupported =
                            isSupported,

                        MatchesExpectedValue =
                            matches,

                        Message =
                            BuildFieldMessage(
                                isSupported,
                                matches)
                    });
            }

            int pictureCountAfter =
                tagFile.Tag.Pictures?.Length ?? 0;

            messages.Add(
                "TagLibSharp reabrió correctamente el archivo.");

            messages.Add(
                pictureCountBefore == pictureCountAfter
                    ? "Las imágenes incrustadas fueron " +
                      "preservadas."
                    : "La cantidad de imágenes incrustadas " +
                      "cambió después del guardado.");

            return new MetadataVerificationResult
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
            pictureCountBefore,
            messages);
    }

    private static MetadataVerificationResult BuildFailure(
        string filePath,
        int pictureCountBefore,
        IReadOnlyList<string> messages)
    {
        return new MetadataVerificationResult
        {
            FilePath =
                filePath,

            FileOpened =
                false,

            PictureCountBefore =
                pictureCountBefore,

            PictureCountAfter =
                pictureCountBefore,

            Messages =
                messages.ToArray()
        };
    }

    private static bool IsSupportedField(
        MetadataField field)
    {
        return field is
            MetadataField.Artist or
            MetadataField.Title or
            MetadataField.Version or
            MetadataField.Album or
            MetadataField.Label or
            MetadataField.Genre;
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

    private static string BuildFieldMessage(
        bool isSupported,
        bool matches)
    {
        if (!isSupported)
        {
            return
                "El campo todavía no está soportado por el " +
                "verificador.";
        }

        return matches
            ? "El valor persistido coincide con el solicitado."
            : "El valor persistido no coincide con el solicitado.";
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