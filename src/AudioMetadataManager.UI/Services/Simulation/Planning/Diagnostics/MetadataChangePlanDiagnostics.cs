using System.Text;
using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Planning.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Planning.Diagnostics;

/// <summary>
/// Genera un informe legible y auditable de un plan de
/// modificaciones de metadatos.
///
/// Este diagnóstico pertenece al modo simulación y no modifica
/// archivos.
/// </summary>
public static class MetadataChangePlanDiagnostics
{
    /// <summary>
    /// Construye el informe completo del plan.
    /// </summary>
    public static string BuildReport(
        MetadataChangePlan plan)
    {
        ArgumentNullException.ThrowIfNull(
            plan);

        StringBuilder builder =
            new();

        builder.AppendLine(
            "=== Diagnóstico del plan de modificaciones ===");

        builder.AppendLine();

        AppendGeneralInformation(
            builder,
            plan);

        AppendProposals(
            builder,
            plan);

        AppendReasons(
            builder,
            plan);

        builder.AppendLine(
            "=== Fin del diagnóstico del plan ===");

        return builder.ToString();
    }

    private static void AppendGeneralInformation(
        StringBuilder builder,
        MetadataChangePlan plan)
    {
        builder.AppendLine(
            $"Id del plan: {plan.PlanId}");

        builder.AppendLine(
            $"Fecha UTC: {plan.CreatedAtUtc:O}");

        builder.AppendLine(
            $"Archivo: {DisplayValue(plan.FileName)}");

        builder.AppendLine(
            $"Ruta: {DisplayValue(plan.FilePath)}");

        builder.AppendLine(
            $"Estado global: " +
            $"{GetPlanStatusDisplay(plan.Status)}");

        builder.AppendLine(
            $"Propuestas evaluadas: " +
            $"{plan.ProposalCount}");

        builder.AppendLine(
            $"Cambios reales: " +
            $"{plan.ActualChangeCount}");

        builder.AppendLine(
            $"Cambios automáticos posibles: " +
            $"{plan.AutomaticChangeCount}");

        builder.AppendLine(
            $"Cambios con revisión manual: " +
            $"{plan.ManualReviewCount}");

        builder.AppendLine(
            $"Conflictos: " +
            $"{plan.ConflictCount}");

        builder.AppendLine(
            $"Puede continuar a simulación: " +
            $"{ToSpanish(plan.CanProceedToSimulation)}");

        builder.AppendLine(
            $"Revisión manual requerida: " +
            $"{ToSpanish(plan.RequiresManualReview)}");

        builder.AppendLine(
            $"Resumen: {plan.Summary}");

        builder.AppendLine();
    }

    private static void AppendProposals(
        StringBuilder builder,
        MetadataChangePlan plan)
    {
        builder.AppendLine(
            "--- Propuestas por campo ---");

        builder.AppendLine();

        if (plan.Proposals.Count == 0)
        {
            builder.AppendLine(
                "No existen propuestas evaluadas.");

            builder.AppendLine();

            return;
        }

        foreach (
            MetadataChangeProposal proposal
            in plan.Proposals)
        {
            AppendProposal(
                builder,
                proposal);
        }
    }

    private static void AppendProposal(
        StringBuilder builder,
        MetadataChangeProposal proposal)
    {
        builder.AppendLine(
            $"[{GetFieldDisplay(proposal.Field)}]");

        builder.AppendLine(
            $"Valor actual: " +
            $"{DisplayValue(proposal.CurrentValue)}");

        builder.AppendLine(
            $"Valor propuesto: " +
            $"{DisplayValue(proposal.ProposedValue)}");

        builder.AppendLine(
            $"Valor normalizado: " +
            $"{DisplayValue(
                proposal.ProposedNormalizedValue)}");

        builder.AppendLine(
            $"Decisión: " +
            $"{GetDecisionDisplay(proposal.Decision)}");

        builder.AppendLine(
            $"Confianza del consenso: " +
            $"{proposal.ConfidenceDisplay}");

        builder.AppendLine(
            $"Fuentes de respaldo: " +
            $"{proposal.SupportingSourceCount}");

        builder.AppendLine(
            $"Fuentes: " +
            $"{DisplaySources(
                proposal.SupportingSources)}");

        builder.AppendLine(
            $"Representa un cambio real: " +
            $"{ToSpanish(proposal.HasActualChange)}");

        builder.AppendLine(
            $"Elegible para aplicación automática: " +
            $"{ToSpanish(
                proposal.IsAutomaticApplyEligible)}");

        builder.AppendLine(
            $"Revisión manual: " +
            $"{ToSpanish(
                proposal.RequiresManualReview)}");

        builder.AppendLine(
            $"La fuente exige aprobación manual: " +
            $"{ToSpanish(
                proposal.SourceRequiresManualApproval)}");

        builder.AppendLine(
            $"Explicación: " +
            $"{DisplayValue(proposal.Explanation)}");

        builder.AppendLine();

        builder.AppendLine(
            "----------------------------------------");

        builder.AppendLine();
    }

    private static void AppendReasons(
        StringBuilder builder,
        MetadataChangePlan plan)
    {
        builder.AppendLine(
            "--- Razones globales ---");

        builder.AppendLine();

        if (plan.Reasons.Count == 0)
        {
            builder.AppendLine(
                "- No se registraron razones adicionales.");

            builder.AppendLine();

            return;
        }

        foreach (string reason in plan.Reasons)
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

    private static string GetFieldDisplay(
        MetadataField field)
    {
        return field switch
        {
            MetadataField.Artist =>
                "Artista",

            MetadataField.Title =>
                "Título",

            MetadataField.Version =>
                "Versión",

            MetadataField.Album =>
                "Álbum / lanzamiento",

            MetadataField.Genre =>
                "Género",

            MetadataField.Label =>
                "Sello",

            _ =>
                field.ToString()
        };
    }

    private static string GetDecisionDisplay(
        MetadataChangeDecision decision)
    {
        return decision switch
        {
            MetadataChangeDecision.NoChangeRequired =>
                "No requiere cambios",

            MetadataChangeDecision.EligibleForAutomaticApply =>
                "Elegible para aplicación automática",

            MetadataChangeDecision.ManualReviewRequired =>
                "Revisión manual requerida",

            MetadataChangeDecision.InsufficientEvidence =>
                "Evidencia insuficiente",

            MetadataChangeDecision.Conflict =>
                "Conflicto sin resolver",

            MetadataChangeDecision.Rejected =>
                "Propuesta rechazada",

            _ =>
                "Pendiente"
        };
    }

    private static string GetPlanStatusDisplay(
        MetadataChangePlanStatus status)
    {
        return status switch
        {
            MetadataChangePlanStatus.NoChangesRequired =>
                "No se requieren cambios",

            MetadataChangePlanStatus.ManualReviewRequired =>
                "Revisión manual requerida",

            MetadataChangePlanStatus.ReadyForSimulation =>
                "Preparado para simulación",

            MetadataChangePlanStatus.PartiallyReady =>
                "Parcialmente preparado",

            MetadataChangePlanStatus.BlockedByConflicts =>
                "Bloqueado por conflictos",

            MetadataChangePlanStatus.InsufficientEvidence =>
                "Evidencia insuficiente",

            _ =>
                "Pendiente"
        };
    }

    private static string DisplaySources(
        IReadOnlyList<string> sources)
    {
        string[] usableSources =
            sources
                .Where(
                    source =>
                        !string.IsNullOrWhiteSpace(source))
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        return usableSources.Length == 0
            ? "(sin fuentes)"
            : string.Join(
                ", ",
                usableSources);
    }

    private static string DisplayValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "(sin información)"
            : value.Trim();
    }

    private static string ToSpanish(
        bool value)
    {
        return value
            ? "Sí"
            : "No";
    }
}