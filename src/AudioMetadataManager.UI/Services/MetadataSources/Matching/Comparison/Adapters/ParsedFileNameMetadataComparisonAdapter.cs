using AudioMetadataManager.UI.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison.Adapters;

/// <summary>
/// Convierte un resultado del parser de nombre de archivo
/// en una entrada neutral para el motor de comparación.
///
/// Este adaptador no analiza nombres, no modifica archivos
/// y no ejecuta comparaciones.
/// </summary>
public sealed class ParsedFileNameMetadataComparisonAdapter
{
    /// <summary>
    /// Crea una entrada de comparación a partir de un
    /// resultado ya producido por FileNameParserService.
    /// </summary>
    public MetadataComparisonInput CreateInput(
        ParsedFileName parsedFileName,
        string sourceName = "Nombre del archivo")
    {
        ArgumentNullException.ThrowIfNull(
            parsedFileName);

        return new MetadataComparisonInput
        {
            SourceName =
                NormalizeSourceName(
                    sourceName),

            Artist =
                NormalizeOptionalValue(
                    parsedFileName.Artist),

            Title =
                NormalizeOptionalValue(
                    parsedFileName.Title),

            Version =
                NormalizeOptionalValue(
                    parsedFileName.Version),

            /*
             * ParsedFileName actualmente no contiene
             * información de álbum.
             */
            Album =
                null,

            /*
             * ParsedFileName actualmente no contiene
             * información de género.
             */
            Genre =
                null,

            /*
             * ParsedFileName actualmente no contiene
             * información de sello discográfico.
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
            ? "Nombre del archivo"
            : sourceName.Trim();
    }

    /// <summary>
    /// Convierte textos vacíos en null para representar
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