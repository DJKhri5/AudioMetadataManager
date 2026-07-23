using System.Text;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Candidates.Diagnostics;

/// <summary>
/// Genera un informe legible del ranking de candidatos
/// externos evaluados por los motores de comparación
/// y confianza.
/// </summary>
public static class MetadataCandidateEvaluationDiagnostics
{
    /// <summary>
    /// Construye el informe completo del lote evaluado.
    /// </summary>
    public static string BuildReport(
        MetadataCandidateEvaluationBatchResult batchResult)
    {
        ArgumentNullException.ThrowIfNull(
            batchResult);

        StringBuilder builder =
            new();

        builder.AppendLine(
            "=== Ranking de candidatos externos ===");

        builder.AppendLine();

        builder.AppendLine(
            $"Candidatos evaluados: " +
            $"{batchResult.EvaluatedCandidateCount}");

        builder.AppendLine(
            $"Candidatos con revisión manual: " +
            $"{batchResult.ManualReviewCount}");

        builder.AppendLine(
            $"Existe mejor candidato: " +
            $"{ToSpanish(batchResult.BestCandidate is not null)}");

        builder.AppendLine();

        if (!batchResult.HasEvaluations)
        {
            builder.AppendLine(
                "No existen candidatos utilizables para evaluar.");

            builder.AppendLine();
            builder.AppendLine(
                "=== Fin del ranking de candidatos ===");

            return builder.ToString();
        }

        int position =
            0;

        foreach (
            MetadataCandidateEvaluationResult evaluation
            in batchResult.Evaluations)
        {
            position++;

            AppendEvaluation(
                builder,
                evaluation,
                position);
        }

        builder.AppendLine(
            "--- Mejor candidato ---");

        builder.AppendLine();

        MetadataCandidateEvaluationResult bestCandidate =
            batchResult.BestCandidate!;

        builder.AppendLine(
            $"Fuente: " +
            $"{bestCandidate.SourceDisplay}");

        builder.AppendLine(
            $"Nombre: " +
            $"{bestCandidate.DisplayName}");

        builder.AppendLine(
            $"Confianza: " +
            $"{bestCandidate.RankingScoreDisplay}");

        builder.AppendLine(
            $"Decisión: " +
            $"{GetDecisionDisplay(
                bestCandidate.Confidence.Decision)}");

        builder.AppendLine(
            $"Revisión manual: " +
            $"{ToSpanish(
                bestCandidate.RequiresManualReview)}");

        builder.AppendLine(
            $"Conflictos: " +
            $"{bestCandidate.Comparison.Conflicts}");

        builder.AppendLine(
            $"Similitud efectiva: " +
            $"{bestCandidate.Comparison.EffectiveSimilarity * 100:0.00}%");

        builder.AppendLine(
            $"Cobertura de información: " +
            $"{bestCandidate.Comparison.InformationCoverage * 100:0.00}%");

        builder.AppendLine();

        builder.AppendLine(
            $"Resumen del lote: " +
            $"{batchResult.Summary}");

        builder.AppendLine();

        builder.AppendLine(
            "=== Fin del ranking de candidatos ===");

        return builder.ToString();
    }

    private static void AppendEvaluation(
        StringBuilder builder,
        MetadataCandidateEvaluationResult evaluation,
        int position)
    {
        builder.AppendLine(
            $"Candidato #{position}");

        builder.AppendLine(
            $"Fuente: " +
            $"{evaluation.SourceDisplay}");

        builder.AppendLine(
            $"Nombre: " +
            $"{evaluation.DisplayName}");

        builder.AppendLine(
            $"Rango original: " +
            $"{DisplaySourceRank(
                evaluation.OriginalSourceRank)}");

        builder.AppendLine(
            $"Confianza global: " +
            $"{evaluation.RankingScoreDisplay}");

        builder.AppendLine(
            $"Decisión: " +
            $"{GetDecisionDisplay(
                evaluation.Confidence.Decision)}");

        builder.AppendLine(
            $"Evaluación completada: " +
            $"{ToSpanish(
                evaluation.Confidence.EvaluationCompleted)}");

        builder.AppendLine(
            $"Revisión manual: " +
            $"{ToSpanish(
                evaluation.RequiresManualReview)}");

        builder.AppendLine(
            $"Conflictos: " +
            $"{evaluation.Comparison.Conflicts}");

        builder.AppendLine(
            $"Similitud efectiva: " +
            $"{evaluation.Comparison.EffectiveSimilarity * 100:0.00}%");

        builder.AppendLine(
            $"Cobertura de información: " +
            $"{evaluation.Comparison.InformationCoverage * 100:0.00}%");

        builder.AppendLine(
            $"Similitud ponderada: " +
            $"{evaluation.Confidence.WeightedSimilarity * 100:0.00}%");

        builder.AppendLine(
            $"Cobertura ponderada: " +
            $"{evaluation.Confidence.WeightedCoverage * 100:0.00}%");

        builder.AppendLine();

        builder.AppendLine(
            "--- Evaluación por campo ---");

        builder.AppendLine();

        foreach (
            MetadataFieldComparisonResult field
            in evaluation.Comparison.Fields)
        {
            builder.AppendLine(
                $"[{field.FieldName}]");

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
                $"{DisplayValue(field.Explanation)}");

            builder.AppendLine();
        }

        builder.AppendLine(
            "----------------------------------------");

        builder.AppendLine();
    }

    private static string GetDecisionDisplay(
        MetadataComparisonDecision decision)
    {
        return decision switch
        {
            MetadataComparisonDecision.Accepted =>
                "Coincidencia aceptada",

            MetadataComparisonDecision.AcceptedWithReview =>
                "Coincidencia aceptable con revisión",

            MetadataComparisonDecision.ManualReviewRequired =>
                "Revisión manual requerida",

            MetadataComparisonDecision.Rejected =>
                "Coincidencia rechazada",

            MetadataComparisonDecision.InsufficientData =>
                "Información insuficiente",

            _ =>
                "Sin evaluación"
        };
    }

    private static string DisplayValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(
            value)
                ? "(sin información)"
                : value.Trim();
    }

    private static string DisplaySourceRank(
        int sourceRank)
    {
        return sourceRank > 0
            ? sourceRank.ToString()
            : "(sin información)";
    }

    private static string ToSpanish(
        bool value)
    {
        return value
            ? "Sí"
            : "No";
    }
}