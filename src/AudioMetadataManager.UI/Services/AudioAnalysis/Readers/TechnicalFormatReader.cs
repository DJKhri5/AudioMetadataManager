using AudioMetadataManager.UI.Services.AudioAnalysis.Models;
using System.IO;
using TagLib;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Readers;

/// <summary>
/// Lee propiedades técnicas declaradas por el archivo
/// mediante TagLibSharp.
///
/// Este lector no decodifica PCM, no ejecuta FFT y no
/// modifica el archivo.
/// </summary>
public class TechnicalFormatReader
{
    /// <summary>
    /// Lee las propiedades técnicas disponibles del archivo.
    /// </summary>
    public AudioTechnicalFormatInfo Read(
        string filePath)
    {
        ValidateFilePath(
            filePath);

        using TagLib.File file =
            TagLib.File.Create(
                filePath);

        TagLib.Properties properties =
            file.Properties;

        string extension =
            Path.GetExtension(
                    filePath)
                .Trim()
                .ToLowerInvariant();

        int declaredBitrateBitsPerSecond =
            properties.AudioBitrate > 0
                ? checked(
                    properties.AudioBitrate *
                    1000)
                : 0;

        int sampleRate =
            properties.AudioSampleRate > 0
                ? properties.AudioSampleRate
                : 0;

        int channels =
            properties.AudioChannels > 0
                ? properties.AudioChannels
                : 0;

        int bitsPerSample =
            properties.BitsPerSample > 0
                ? properties.BitsPerSample
                : 0;

        bool isLossless =
            extension is
                ".flac" or
                ".wav" or
                ".aif" or
                ".aiff";

        return new AudioTechnicalFormatInfo
        {
            FilePath =
                filePath,

            FileExtension =
                extension,

            ContainerName =
                GetContainerName(
                    extension),

            CodecName =
                GetCodecName(
                    properties,
                    extension),

            DeclaredBitrateBitsPerSecond =
                declaredBitrateBitsPerSecond,

            EstimatedAverageBitrateBitsPerSecond =
                0,

            DeclaredSampleRate =
                sampleRate,

            DeclaredChannels =
                channels,

            BitsPerSample =
                bitsPerSample,

            IsLossless =
                isLossless,

            IsLossy =
                !isLossless
        };
    }

    /// <summary>
    /// Obtiene un nombre legible del contenedor.
    /// </summary>
    private static string GetContainerName(
        string extension)
    {
        return extension switch
        {
            ".mp3" =>
                "MPEG Audio",

            ".flac" =>
                "FLAC",

            ".wav" =>
                "WAV",

            ".aif" or ".aiff" =>
                "AIFF",

            ".m4a" =>
                "MPEG-4 Audio",

            ".aac" =>
                "AAC",

            ".ogg" =>
                "Ogg",

            ".opus" =>
                "Ogg Opus",

            ".wma" =>
                "Windows Media",

            _ =>
                string.IsNullOrWhiteSpace(
                    extension)
                    ? "Contenedor sin identificar"
                    : extension
                        .TrimStart('.')
                        .ToUpperInvariant()
        };
    }

    /// <summary>
    /// Obtiene una descripción del códec desde las
    /// propiedades disponibles.
    /// </summary>
    private static string GetCodecName(
        TagLib.Properties properties,
        string extension)
    {
        string description =
            properties.Description?.Trim() ??
            string.Empty;

        if (!string.IsNullOrWhiteSpace(
                description))
        {
            return description;
        }

        return extension switch
        {
            ".mp3" =>
                "MPEG Layer III",

            ".flac" =>
                "FLAC",

            ".wav" =>
                "PCM / WAV",

            ".aif" or ".aiff" =>
                "AIFF",

            ".m4a" =>
                "AAC / ALAC",

            ".aac" =>
                "AAC",

            ".ogg" =>
                "Ogg Vorbis",

            ".opus" =>
                "Opus",

            ".wma" =>
                "Windows Media Audio",

            _ =>
                "Códec sin identificar"
        };
    }

    /// <summary>
    /// Comprueba que la ruta sea válida.
    /// </summary>
    private static void ValidateFilePath(
        string filePath)
    {
        if (string.IsNullOrWhiteSpace(
                filePath))
        {
            throw new ArgumentException(
                "La ruta del archivo está vacía.",
                nameof(filePath));
        }

        if (!Path.IsPathFullyQualified(
                filePath))
        {
            throw new ArgumentException(
                "La ruta del archivo debe ser completa.",
                nameof(filePath));
        }

        if (!System.IO.File.Exists(
                filePath))
        {
            throw new FileNotFoundException(
                "No se encontró el archivo de audio.",
                filePath);
        }
    }
}