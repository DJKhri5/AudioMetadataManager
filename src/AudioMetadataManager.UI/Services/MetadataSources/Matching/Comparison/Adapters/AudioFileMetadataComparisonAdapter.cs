using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.Parsing;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison.Adapters;

/// <summary>
/// Convierte un modelo AudioFile ya analizado en una entrada
/// neutral para el motor de comparación de metadatos.
///
/// Este adaptador no lee archivos, no modifica etiquetas
/// y no ejecuta comparaciones.
/// </summary>
public sealed class AudioFileMetadataComparisonAdapter
{

    private readonly VersionParser
    _versionParser =
        new();

    /// <summary>
    /// Crea una entrada de comparación a partir de los
    /// metadatos actualmente almacenados en AudioFile.
    /// </summary>
    public MetadataComparisonInput CreateInput(
        AudioFile audioFile,
        string sourceName = "Etiquetas internas")
    {
        ArgumentNullException.ThrowIfNull(
            audioFile);

        string originalTitle =
            audioFile.Title?.Trim() ??
            string.Empty;

        (string parsedTitle, string parsedVersion) =
            _versionParser.Parse(
                originalTitle);

        return new MetadataComparisonInput
        {
            SourceName =
                NormalizeSourceName(
                    sourceName),

            Artist =
                NormalizeOptionalValue(
                    audioFile.Artist),

            Title =
                NormalizeOptionalValue(
                    parsedTitle),

            /*
            * Se prioriza la versión explícita almacenada en
            * la etiqueta. El valor interpretado desde el
            * título se conserva como compatibilidad para
            * archivos que todavía no utilizan TIT3.
            */
            Version =
                NormalizeOptionalValue(
                    string.IsNullOrWhiteSpace(
                        audioFile.Version)
                        ? parsedVersion
                        : audioFile.Version),

            Album =
                NormalizeOptionalValue(
                    audioFile.Album),

            Genre =
                NormalizeOptionalValue(
                    audioFile.Genre),

            Label =
                NormalizeOptionalValue(
                    audioFile.Label)
        };
    }

    /// <summary>
    /// Normaliza el nombre descriptivo de la fuente.
    /// </summary>
    private static string NormalizeSourceName(
        string? sourceName)
    {
        return string.IsNullOrWhiteSpace(sourceName)
            ? "Etiquetas internas"
            : sourceName.Trim();
    }

    /// <summary>
    /// Convierte textos vacíos en null para distinguir
    /// correctamente los valores ausentes.
    /// </summary>
    private static string? NormalizeOptionalValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
