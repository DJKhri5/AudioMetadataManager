using System.Text;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Engine;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Diagnostics;

/// <summary>
/// Ejecuta el motor de confianza y genera un informe legible
/// con la puntuación, decisión, razones y trazabilidad
/// detallada por campo.
/// </summary>
public sealed class MetadataConfidenceDiagnostics
{
    private readonly MetadataConfidenceEngine
        _confidenceEngine;

    /// <summary>
    /// Crea el diagnóstico con el motor predeterminado.
    /// </summary>
    public MetadataConfidenceDiagnostics()
        : this(new MetadataConfidenceEngine())
    {
    }

    /// <summary>
    /// Crea el diagnóstico utilizando un motor personalizado.
    /// </summary>
    public MetadataConfidenceDiagnostics(
        MetadataConfidenceEngine confidenceEngine)
    {
        _confidenceEngine =
            confidenceEngine ??
            throw new ArgumentNullException(
                nameof(confidenceEngine));
    }

    /// <summary>
    /// Evalúa el resultado técnico de comparación y devuelve
    /// un informe completo de confianza.
    /// </summary>
    public string Run(
        MetadataComparisonResult comparisonResult)
    {
        ArgumentNullException.ThrowIfNull(
            comparisonResult);

        MetadataConfidenceResult confidenceResult =
            _confidenceEngine.Evaluate(
                comparisonResult);

        return BuildReport(
            confidenceResult);
    }

    /// <summary>
    /// Construye el informe legible del resultado de confianza.
    /// </summary>
    private static string BuildReport(
        MetadataConfidenceResult result)
    {
        ArgumentNullException.ThrowIfNull(
            result);

        StringBuilder builder =
            new();

        builder.AppendLine(
            "=== Diagnóstico de MetadataConfidenceEngine ===");

        builder.AppendLine();

        builder.AppendLine(
            $"Evaluación completada: " +
            $"{ToSpanish(result.EvaluationCompleted)}");

        builder.AppendLine(
            $"Confianza global: " +
            $"{result.ConfidenceDisplay}");

        builder.AppendLine(
            $"Similitud ponderada: " +
            $"{result.WeightedSimilarityDisplay}");

        builder.AppendLine(
            $"Cobertura ponderada: " +
            $"{result.WeightedCoverageDisplay}");

        builder.AppendLine(
            $"Peso configurado: " +
            $"{result.ConfiguredWeight * 100.0:0.00}%");

        builder.AppendLine(
            $"Peso comparable: " +
            $"{result.ComparableWeight * 100.0:0.00}%");

        builder.AppendLine(
            $"Peso con información disponible: " +
            $"{result.AvailableInformationWeight * 100.0:0.00}%");

        builder.AppendLine(
            $"Conflictos críticos: " +
            $"{result.CriticalConflicts}");

        builder.AppendLine(
            $"Campos críticos no comparables: " +
            $"{result.MissingCriticalFields}");

        builder.AppendLine(
            $"Decisión: " +
            $"{result.DecisionDisplay}");

        builder.AppendLine(
            $"Revisión manual: " +
            $"{ToSpanish(result.RequiresManualReview)}");

        builder.AppendLine();

        builder.AppendLine(
            $"Resumen: {result.Summary}");

        builder.AppendLine();

        builder.AppendLine(
            "--- Razones de la evaluación ---");

        if (result.Reasons.Count == 0)
        {
            builder.AppendLine(
                "(sin razones registradas)");
        }
        else
        {
            foreach (string reason in result.Reasons)
            {
                builder.AppendLine(
                    $"- {reason}");
            }
        }

        builder.AppendLine();

        builder.AppendLine(
            "--- Evaluación ponderada por campo ---");

        if (result.FieldEvaluations.Count == 0)
        {
            builder.AppendLine(
                "(sin evaluaciones de campos)");
        }
        else
        {
            foreach (
                MetadataFieldConfidenceEvaluation field
                in result.FieldEvaluations)
            {
                builder.AppendLine();

                builder.AppendLine(
                    $"[{field.FieldDisplay}]");

                builder.AppendLine(
                    $"Local: " +
                    $"{DisplayValue(field.LocalValue)}");

                builder.AppendLine(
                    $"Referencia: " +
                    $"{DisplayValue(field.ReferenceValue)}");

                builder.AppendLine(
                    $"Estado: " +
                    $"{field.ComparisonStatus}");

                builder.AppendLine(
                    $"Peso: " +
                    $"{field.ConfiguredWeightDisplay}");

                builder.AppendLine(
                    $"Similitud: " +
                    $"{field.SimilarityDisplay}");

                builder.AppendLine(
                    $"Aporte ponderado: " +
                    $"{field.WeightedContributionDisplay}");

                builder.AppendLine(
                    $"Campo crítico: " +
                    $"{ToSpanish(field.IsCritical)}");

                builder.AppendLine(
                    $"Comparable: " +
                    $"{ToSpanish(field.IsComparable)}");

                builder.AppendLine(
                    $"Conflicto: " +
                    $"{ToSpanish(field.HasConflict)}");

                builder.AppendLine(
                    $"Explicación: " +
                    $"{field.Explanation}");
            }
        }

        builder.AppendLine();

        builder.AppendLine(
            "=== Fin del diagnóstico de confianza ===");

        return builder.ToString();
    }

    private static string DisplayValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "(sin información)"
            : value;
    }

    private static string ToSpanish(
        bool value)
    {
        return value
            ? "Sí"
            : "No";
    }
}