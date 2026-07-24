using System.Text;
using AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Models;

using ConsensusResult =
    AudioMetadataManager.UI.Services.MetadataSources
        .Consensus.Models.MetadataConsensusResult;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Diagnostics;

/// <summary>
/// Genera un informe legible del resultado producido por el
/// nuevo motor de consenso de metadatos.
///
/// El diagnóstico conserva la decisión por campo, confianza,
/// fuentes participantes, contribuciones y razones globales.
/// </summary>
public static class MetadataConsensusDiagnostics
{
    /// <summary>
    /// Construye el informe completo del consenso.
    /// </summary>
    public static string BuildReport(
        ConsensusResult result)
    {
        ArgumentNullException.ThrowIfNull(
            result);

        StringBuilder builder =
            new();

        builder.AppendLine(
            "=== Diagnóstico de Metadata Consensus ===");

        builder.AppendLine();

        AppendGeneralInformation(
            builder,
            result);

        AppendFields(
            builder,
            result);

        AppendReasons(
            builder,
            result);

        builder.AppendLine(
            "=== Fin del diagnóstico de consenso ===");

        return builder.ToString();
    }

    private static void AppendGeneralInformation(
        StringBuilder builder,
        ConsensusResult result)
    {
        builder.AppendLine(
            $"Id de evaluación: " +
            $"{result.EvaluationId}");

        builder.AppendLine(
            $"Fecha UTC: " +
            $"{result.CreatedAtUtc:O}");

        builder.AppendLine(
            $"Campos evaluados: " +
            $"{result.EvaluatedFieldCount}");

        builder.AppendLine(
            $"Campos con valor seleccionado: " +
            $"{result.SelectedFieldCount}");

        builder.AppendLine(
            $"Conflictos detectados: " +
            $"{result.ConflictCount}");

        builder.AppendLine(
            $"Confianza global: " +
            $"{result.OverallConfidenceDisplay}");

        builder.AppendLine(
            $"Revisión manual: " +
            $"{ToSpanish(
                result.RequiresManualReview)}");

        builder.AppendLine(
            $"Existe información de consenso: " +
            $"{ToSpanish(
                result.HasConsensusData)}");

        builder.AppendLine(
            $"Resumen: " +
            $"{result.Summary}");

        builder.AppendLine();
    }

    private static void AppendFields(
        StringBuilder builder,
        ConsensusResult result)
    {
        builder.AppendLine(
            "--- Resultados por campo ---");

        builder.AppendLine();

        if (result.Fields.Count == 0)
        {
            builder.AppendLine(
                "No existen campos evaluados.");

            builder.AppendLine();

            return;
        }

        foreach (
            MetadataConsensusFieldResult field
            in result.Fields)
        {
            AppendField(
                builder,
                field);
        }
    }

    private static void AppendField(
        StringBuilder builder,
        MetadataConsensusFieldResult field)
    {
        builder.AppendLine(
            $"[{field.Field}]");

        builder.AppendLine(
            $"Valor seleccionado: " +
            $"{DisplayValue(
                field.SelectedValue)}");

        builder.AppendLine(
            $"Valor normalizado: " +
            $"{DisplayValue(
                field.SelectedNormalizedValue)}");

        builder.AppendLine(
            $"Estado: " +
            $"{GetStatusDisplay(
                field.Status)}");

        builder.AppendLine(
            $"Confianza: " +
            $"{field.ConfidenceDisplay}");

        builder.AppendLine(
            $"Fuentes participantes: " +
            $"{field.ContributingSourceCount}");

        builder.AppendLine(
            $"Contribuciones recibidas: " +
            $"{field.Contributions.Count}");

        builder.AppendLine(
            $"Contribuciones ganadoras: " +
            $"{field.WinningContributions.Count}");

        builder.AppendLine(
            $"Conflicto: " +
            $"{ToSpanish(
                field.HasConflict)}");

        builder.AppendLine(
            $"Revisión manual: " +
            $"{ToSpanish(
                field.RequiresManualReview)}");

        builder.AppendLine(
            $"Explicación: " +
            $"{DisplayValue(
                field.Explanation)}");

        builder.AppendLine();

        AppendContributions(
            builder,
            field);

        builder.AppendLine(
            "----------------------------------------");

        builder.AppendLine();
    }

    private static void AppendContributions(
        StringBuilder builder,
        MetadataConsensusFieldResult field)
    {
        if (field.Contributions.Count == 0)
        {
            builder.AppendLine(
                "Contribuciones: ninguna.");

            builder.AppendLine();

            return;
        }

        builder.AppendLine(
            "Contribuciones:");

        int position =
            0;

        foreach (
            MetadataConsensusContribution contribution
            in field.Contributions
                .OrderByDescending(
                    contribution =>
                        contribution.WeightedSupport)
                .ThenBy(
                    contribution =>
                        NormalizeSourceRank(
                            contribution.SourceRank))
                .ThenBy(
                    contribution =>
                        contribution.SourceName,
                    StringComparer.OrdinalIgnoreCase))
        {
            position++;

            bool supportsWinner =
                field.HasSelectedValue &&
                string.Equals(
                    contribution.NormalizedValue,
                    field.SelectedNormalizedValue,
                    StringComparison.OrdinalIgnoreCase);

            builder.AppendLine(
                $"  {position}. Fuente: " +
                $"{DisplayValue(
                    contribution.SourceDisplay)}");

            builder.AppendLine(
                $"     Valor: " +
                $"{DisplayValue(
                    contribution.Value)}");

            builder.AppendLine(
                $"     Normalizado: " +
                $"{DisplayValue(
                    contribution.NormalizedValue)}");

            builder.AppendLine(
                $"     Confianza del candidato: " +
                $"{Math.Clamp(
                    contribution.CandidateConfidence,
                    0,
                    1) * 100:0.00}%");

            builder.AppendLine(
                $"     Peso de la fuente: " +
                $"{Math.Clamp(
                    contribution.SourceWeight,
                    0,
                    1) * 100:0.00}%");

            builder.AppendLine(
                $"     Soporte ponderado: " +
                $"{Math.Clamp(
                    contribution.WeightedSupport,
                    0,
                    1) * 100:0.00}%");

            builder.AppendLine(
                $"     Rango original: " +
                $"{DisplaySourceRank(
                    contribution.SourceRank)}");

            builder.AppendLine(
                $"     Respalda el valor seleccionado: " +
                $"{ToSpanish(
                    supportsWinner)}");

            builder.AppendLine(
                $"     Aprobación manual requerida: " +
                $"{ToSpanish(
                    contribution.RequiresManualApproval)}");
        }

        builder.AppendLine();
    }

    private static void AppendReasons(
        StringBuilder builder,
        ConsensusResult result)
    {
        builder.AppendLine(
            "--- Razones globales ---");

        builder.AppendLine();

        if (result.Reasons.Count == 0)
        {
            builder.AppendLine(
                "- No se registraron razones adicionales.");

            builder.AppendLine();

            return;
        }

        foreach (string reason in result.Reasons)
        {
            if (string.IsNullOrWhiteSpace(
                    reason))
            {
                continue;
            }

            builder.AppendLine(
                $"- {reason.Trim()}");
        }

        builder.AppendLine();
    }

    private static string GetStatusDisplay(
        MetadataConsensusStatus status)
    {
        return status switch
        {
            MetadataConsensusStatus.ConsensusReached =>
                "Consenso alcanzado",

            MetadataConsensusStatus.SingleSource =>
                "Respaldado por una sola fuente",

            MetadataConsensusStatus.MajorityReached =>
                "Mayoría ponderada alcanzada",

            MetadataConsensusStatus.Conflict =>
                "Conflicto sin resolver",

            MetadataConsensusStatus.NoInformation =>
                "Sin información",

            MetadataConsensusStatus.NotApplicable =>
                "No aplicable",

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

    private static int NormalizeSourceRank(
        int sourceRank)
    {
        return sourceRank > 0
            ? sourceRank
            : int.MaxValue;
    }

    private static string ToSpanish(
        bool value)
    {
        return value
            ? "Sí"
            : "No";
    }
}