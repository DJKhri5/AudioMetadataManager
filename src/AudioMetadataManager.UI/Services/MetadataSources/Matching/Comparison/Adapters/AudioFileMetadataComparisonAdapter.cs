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
            * Algunas etiquetas almacenan la versión dentro
            * del título, por ejemplo:
            * "Verknipt Nochmal (Original Mix)".
            *
            * Separamos ambos valores únicamente para realizar
            * una comparación justa. AudioFile no se modifica.
            */
            Version =
                NormalizeOptionalValue(
                    parsedVersion),

            Album =
                NormalizeOptionalValue(
                    audioFile.Album),

            Genre =
                NormalizeOptionalValue(
                    audioFile.Genre),

            /*
             * AudioFile todavía no contiene una propiedad
             * específica para el sello discográfico.
             */
            Label =
                null
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