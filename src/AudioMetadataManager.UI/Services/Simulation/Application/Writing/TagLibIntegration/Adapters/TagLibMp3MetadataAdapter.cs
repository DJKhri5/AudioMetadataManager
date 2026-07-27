using System.IO;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Adapters;

/// <summary>
/// Encapsula el acceso de sólo lectura a archivos MP3 mediante
/// TagLibSharp.
///
/// Ningún método de esta clase guarda modificaciones.
/// </summary>
public sealed class TagLibMp3MetadataAdapter
{
    /// <summary>
    /// Inspecciona un archivo MP3 sin modificarlo.
    /// </summary>
    public TagLibMp3InspectionResult Inspect(
        string? filePath)
    {
        List<string> messages =
            new();

        string normalizedPath =
            NormalizePath(
                filePath);

        if (string.IsNullOrWhiteSpace(
                normalizedPath))
        {
            messages.Add(
                "No se recibió una ruta de archivo válida.");

            return BuildFailure(
                normalizedPath,
                messages);
        }

        if (!File.Exists(
                normalizedPath))
        {
            messages.Add(
                "El archivo indicado no existe.");

            return BuildFailure(
                normalizedPath,
                messages);
        }

        string extension =
            Path.GetExtension(
                normalizedPath);

        if (!string.Equals(
                extension,
                ".mp3",
                StringComparison.OrdinalIgnoreCase))
        {
            messages.Add(
                "El adaptador actual sólo acepta archivos " +
                "con extensión .mp3.");

            return BuildFailure(
                normalizedPath,
                messages);
        }

        try
        {
            using TagLib.File tagFile =
                TagLib.File.Create(
                    normalizedPath);

            TagLib.Tag tag =
                tagFile.Tag;

            TagLib.Properties properties =
                tagFile.Properties;

            string[] performers =
                NormalizeArray(
                    tag.Performers);

            string[] albumArtists =
                NormalizeArray(
                    tag.AlbumArtists);

            string[] genres =
                NormalizeArray(
                    tag.Genres);

            int pictureCount =
                tag.Pictures?.Length ?? 0;

            messages.Add(
                "TagLibSharp abrió correctamente el archivo.");

            messages.Add(
                $"Se detectaron {pictureCount} imagen(es) " +
                "incrustada(s).");

            messages.Add(
                "La inspección terminó sin ejecutar Save().");

            return new TagLibMp3InspectionResult
            {
                FilePath =
                    normalizedPath,

                WasSuccessful =
                    true,

                Title =
                    NormalizeValue(
                        tag.Title),

                Performers =
                    performers,

                AlbumArtists =
                    albumArtists,

                Album =
                    NormalizeValue(
                        tag.Album),

                Genres =
                    genres,

                Year =
                    tag.Year,

                Track =
                    tag.Track,

                TrackCount =
                    tag.TrackCount,

                Disc =
                    tag.Disc,

                DiscCount =
                    tag.DiscCount,

                Comment =
                    NormalizeValue(
                        tag.Comment),

                EmbeddedPictureCount =
                    pictureCount,

                Duration =
                    properties.Duration,

                AudioBitrateKbps =
                    properties.AudioBitrate,

                AudioSampleRateHz =
                    properties.AudioSampleRate,

                AudioChannels =
                    properties.AudioChannels,

                TagTypes =
                    tagFile.TagTypes.ToString(),

                Messages =
                    messages.ToArray()
            };
        }
        catch (TagLib.UnsupportedFormatException exception)
        {
            messages.Add(
                "TagLibSharp no reconoce el formato del " +
                $"archivo: {exception.Message}");

            return BuildFailure(
                normalizedPath,
                messages);
        }
        catch (TagLib.CorruptFileException exception)
        {
            messages.Add(
                "TagLibSharp considera que el archivo está " +
                $"dañado o contiene etiquetas inválidas: " +
                $"{exception.Message}");

            return BuildFailure(
                normalizedPath,
                messages);
        }
        catch (UnauthorizedAccessException exception)
        {
            messages.Add(
                "Windows rechazó el acceso de lectura al " +
                $"archivo: {exception.Message}");

            return BuildFailure(
                normalizedPath,
                messages);
        }
        catch (IOException exception)
        {
            messages.Add(
                "Ocurrió un error de entrada o salida al " +
                $"inspeccionar el archivo: {exception.Message}");

            return BuildFailure(
                normalizedPath,
                messages);
        }
        catch (Exception exception)
        {
            messages.Add(
                "Ocurrió un error inesperado durante la " +
                $"inspección: {exception.Message}");

            return BuildFailure(
                normalizedPath,
                messages);
        }
    }

    private static TagLibMp3InspectionResult BuildFailure(
        string filePath,
        IReadOnlyList<string> messages)
    {
        return new TagLibMp3InspectionResult
        {
            FilePath =
                filePath,

            WasSuccessful =
                false,

            Messages =
                messages.ToArray()
        };
    }

    private static string NormalizePath(
        string? filePath)
    {
        if (string.IsNullOrWhiteSpace(
                filePath))
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
        return string.IsNullOrWhiteSpace(
                value)
            ? string.Empty
            : value.Trim();
    }

    private static string[] NormalizeArray(
        IEnumerable<string>? values)
    {
        if (values is null)
        {
            return Array.Empty<string>();
        }

        return values
            .Where(
                value =>
                    !string.IsNullOrWhiteSpace(
                        value))
            .Select(
                value =>
                    value.Trim())
            .Distinct(
                StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}