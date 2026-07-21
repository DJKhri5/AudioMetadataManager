using System.Text;
using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison.Adapters;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Diagnostics;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison.Diagnostics;

/// <summary>
/// Ejecuta pruebas controladas del motor central de comparación.
///
/// No consulta fuentes externas ni modifica archivos.
/// Su finalidad es verificar el comportamiento del motor
/// utilizando conjuntos de metadatos conocidos.
/// </summary>
public sealed class MetadataComparisonDiagnostics
{
    private readonly MetadataComparisonEngine
        _comparisonEngine;

    private readonly AudioFileMetadataComparisonAdapter
        _audioFileAdapter;

    private readonly ParsedFileNameMetadataComparisonAdapter
        _parsedFileNameAdapter;

    private readonly MetadataConfidenceDiagnostics
        _confidenceDiagnostics;

    /// <summary>
    /// Crea el diagnóstico con los componentes predeterminados.
    /// </summary>
    public MetadataComparisonDiagnostics()
        : this(
            new MetadataComparisonEngine(),
            new AudioFileMetadataComparisonAdapter(),
            new ParsedFileNameMetadataComparisonAdapter(),
            new MetadataConfidenceDiagnostics())
    {
    }

    /// <summary>
    /// Crea el diagnóstico con componentes personalizados,
    /// utilizando el diagnóstico de confianza predeterminado.
    /// </summary>
    public MetadataComparisonDiagnostics(
        MetadataComparisonEngine comparisonEngine,
        AudioFileMetadataComparisonAdapter audioFileAdapter,
        ParsedFileNameMetadataComparisonAdapter parsedFileNameAdapter)
        : this(
            comparisonEngine,
            audioFileAdapter,
            parsedFileNameAdapter,
            new MetadataConfidenceDiagnostics())
    {
    }

    /// <summary>
    /// Crea el diagnóstico con todos sus componentes
    /// personalizados.
    /// </summary>
    public MetadataComparisonDiagnostics(
        MetadataComparisonEngine comparisonEngine,
        AudioFileMetadataComparisonAdapter audioFileAdapter,
        ParsedFileNameMetadataComparisonAdapter parsedFileNameAdapter,
        MetadataConfidenceDiagnostics confidenceDiagnostics)
    {
        _comparisonEngine =
            comparisonEngine ??
            throw new ArgumentNullException(
                nameof(comparisonEngine));

        _audioFileAdapter =
            audioFileAdapter ??
            throw new ArgumentNullException(
                nameof(audioFileAdapter));

        _parsedFileNameAdapter =
            parsedFileNameAdapter ??
            throw new ArgumentNullException(
                nameof(parsedFileNameAdapter));

        _confidenceDiagnostics =
            confidenceDiagnostics ??
            throw new ArgumentNullException(
                nameof(confidenceDiagnostics));
    }

    /// <summary>
    /// Ejecuta una comparación controlada y devuelve
    /// un informe legible.
    /// </summary>
    public string RunSample()
    {
        MetadataComparisonInput localMetadata =
            new()
            {
                SourceName =
                    "Archivo local",

                Artist =
                    "Armin van Buuren & W&W",

                Title =
                    "Late Checkout",

                Version =
                    "Original Mix",

                Album =
                    null,

                Genre =
                    "Trance",

                Label =
                    null
            };

        MetadataComparisonInput referenceMetadata =
            new()
            {
                SourceName =
                    "Beatport",

                Artist =
                    "Armin van Buuren & W&W",

                Title =
                    "Late Checkout",

                Version =
                    "Extended Mix",

                Album =
                    null,

                Genre =
                    "Trance",

                Label =
                    "Armind"
            };

        MetadataComparisonResult result =
            _comparisonEngine.CompareMetadata(
                localMetadata,
                referenceMetadata);

        return BuildCombinedReport(
            result);
    }

    /// <summary>
    /// Compara las etiquetas internas de un archivo con los
    /// metadatos obtenidos desde su nombre interpretado.
    /// </summary>
    public string Run(
        AudioFile audioFile,
        ParsedFileName parsedFileName)
    {
        ArgumentNullException.ThrowIfNull(
            audioFile);

        ArgumentNullException.ThrowIfNull(
            parsedFileName);

        MetadataComparisonInput localMetadata =
            _audioFileAdapter.CreateInput(
                audioFile,
                "Etiquetas internas");

        MetadataComparisonInput parsedMetadata =
            _parsedFileNameAdapter.CreateInput(
                parsedFileName,
                "Nombre del archivo");

        MetadataComparisonResult result =
            _comparisonEngine.CompareMetadata(
                localMetadata,
                parsedMetadata);

        return BuildCombinedReport(
            result);
    }

    /// <summary>
    /// Combina el diagnóstico técnico de comparación con la
    /// evaluación global de confianza.
    /// </summary>
    private string BuildCombinedReport(
        MetadataComparisonResult result)
    {
        ArgumentNullException.ThrowIfNull(
            result);

        string comparisonReport =
            BuildReport(
                result);

        string confidenceReport =
            _confidenceDiagnostics.Run(
                result);

        return
            comparisonReport +
            Environment.NewLine +
            confidenceReport;
    }

    /// <summary>
    /// Construye un informe de diagnóstico del resultado.
    /// </summary>
    private static string BuildReport(
        MetadataComparisonResult result)
    {
        ArgumentNullException.ThrowIfNull(
            result);

        StringBuilder builder =
            new();

        builder.AppendLine(
            "=== Diagnóstico de MetadataComparisonEngine ===");

        builder.AppendLine();

        builder.AppendLine(
            $"Fuente local: " +
            $"{result.LocalSourceDisplayName}");

        builder.AppendLine(
            $"Fuente de referencia: " +
            $"{result.ReferenceSourceDisplayName}");

        builder.AppendLine();

        foreach (
            MetadataFieldComparisonResult field
            in result.Fields)
        {
            builder.AppendLine(
                $"[{field.EffectiveFieldName}]");

            builder.AppendLine(
                $"Local: " +
                $"{DisplayValue(field.LocalValue)}");

            builder.AppendLine(
                $"Referencia: " +
                $"{DisplayValue(field.ReferenceValue)}");

            builder.AppendLine(
                $"Estado: " +
                $"{field.Status}");

            builder.AppendLine(
                $"Similitud: " +
                $"{field.Similarity * 100:0.00}%");

            builder.AppendLine(
                $"Explicación: " +
                $"{field.Explanation}");

            builder.AppendLine();
        }

        builder.AppendLine(
            $"Campos comparados: " +
            $"{result.TotalFields}");

        builder.AppendLine(
            $"Campos con información: " +
            $"{result.FieldsWithAnyValue}");

        builder.AppendLine(
            $"Campos realmente comparables: " +
            $"{result.ComparableFields}");

        builder.AppendLine(
            $"Coincidencias exactas: " +
            $"{result.ExactMatches}");

        builder.AppendLine(
            $"Coincidencias normalizadas: " +
            $"{result.NormalizedMatches}");

        builder.AppendLine(
            $"Coincidencias probables: " +
            $"{result.ProbableMatches}");

        builder.AppendLine(
            $"Conflictos: " +
            $"{result.Conflicts}");

        builder.AppendLine(
            $"Valores locales ausentes: " +
            $"{result.MissingLocalValues}");

        builder.AppendLine(
            $"Valores de referencia ausentes: " +
            $"{result.MissingReferenceValues}");

        builder.AppendLine(
            $"Valores ausentes en ambas fuentes: " +
            $"{result.MissingBothValues}");

        builder.AppendLine(
            $"Similitud promedio: " +
            $"{result.AverageSimilarity * 100:0.00}%");

        builder.AppendLine(
            $"Similitud efectiva: " +
            $"{result.EffectiveSimilarity * 100:0.00}%");

        builder.AppendLine(
            $"Cobertura de información: " +
            $"{result.InformationCoverage * 100:0.00}%");

        builder.AppendLine();

        builder.AppendLine(
            "=== Fin del diagnóstico ===");

        return builder.ToString();
    }

    /// <summary>
    /// Prepara un valor opcional para mostrarlo.
    /// </summary>
    private static string DisplayValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "(sin información)"
            : value;
    }
}