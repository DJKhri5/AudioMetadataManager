namespace AudioMetadataManager.UI.Services.Simulation
    .Planning.Models;

/// <summary>
/// Representa el plan completo de modificaciones propuesto
/// para un archivo.
///
/// Este objeto pertenece al modo simulación y no modifica
/// archivos por sí mismo.
/// </summary>
public sealed class MetadataChangePlan
{
    /// <summary>
    /// Identificador único del plan.
    /// </summary>
    public Guid PlanId { get; init; } =
        Guid.NewGuid();

    /// <summary>
    /// Momento UTC en que se creó el plan.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; init; } =
        DateTimeOffset.UtcNow;

    /// <summary>
    /// Ruta completa del archivo asociado.
    /// </summary>
    public string FilePath { get; init; } =
        string.Empty;

    /// <summary>
    /// Nombre del archivo asociado.
    /// </summary>
    public string FileName { get; init; } =
        string.Empty;

    /// <summary>
    /// Decisiones individuales por campo.
    /// </summary>
    public IReadOnlyList<MetadataChangeProposal>
        Proposals
    { get; init; } =
            Array.Empty<MetadataChangeProposal>();

    /// <summary>
    /// Estado global del plan.
    /// </summary>
    public MetadataChangePlanStatus Status { get; init; } =
        MetadataChangePlanStatus.Pending;

    /// <summary>
    /// Explicaciones y advertencias generales.
    /// </summary>
    public IReadOnlyList<string> Reasons { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Propuestas que representarían un cambio real.
    /// </summary>
    public IReadOnlyList<MetadataChangeProposal>
        ActualChanges =>
            Proposals
                .Where(
                    proposal =>
                        proposal.HasActualChange)
                .ToArray();

    /// <summary>
    /// Propuestas elegibles para aplicación automática.
    /// </summary>
    public IReadOnlyList<MetadataChangeProposal>
        AutomaticProposals =>
            ActualChanges
                .Where(
                    proposal =>
                        proposal.IsAutomaticApplyEligible)
                .ToArray();

    /// <summary>
    /// Propuestas que deben revisarse manualmente.
    /// </summary>
    public IReadOnlyList<MetadataChangeProposal>
        ManualReviewProposals =>
            ActualChanges
                .Where(
                    proposal =>
                        proposal.RequiresManualReview)
                .ToArray();

    /// <summary>
    /// Propuestas bloqueadas por conflictos.
    /// </summary>
    public IReadOnlyList<MetadataChangeProposal>
        ConflictedProposals =>
            ActualChanges
                .Where(
                    proposal =>
                        proposal.Decision ==
                        MetadataChangeDecision.Conflict)
                .ToArray();

    /// <summary>
    /// Cantidad total de propuestas evaluadas.
    /// </summary>
    public int ProposalCount =>
        Proposals.Count;

    /// <summary>
    /// Cantidad de modificaciones reales.
    /// </summary>
    public int ActualChangeCount =>
        ActualChanges.Count;

    /// <summary>
    /// Cantidad de cambios potencialmente automáticos.
    /// </summary>
    public int AutomaticChangeCount =>
        AutomaticProposals.Count;

    /// <summary>
    /// Cantidad de cambios que requieren revisión.
    /// </summary>
    public int ManualReviewCount =>
        ManualReviewProposals.Count;

    /// <summary>
    /// Cantidad de conflictos.
    /// </summary>
    public int ConflictCount =>
        ConflictedProposals.Count;

    /// <summary>
    /// Indica si el plan contiene al menos una modificación.
    /// </summary>
    public bool HasChanges =>
        ActualChangeCount > 0;

    /// <summary>
    /// Indica si el plan puede continuar hacia la simulación.
    ///
    /// Esto no significa que pueda escribirse directamente:
    /// todavía faltarán aprobación y copia de seguridad.
    /// </summary>
    public bool CanProceedToSimulation =>
        Status is
            MetadataChangePlanStatus.ReadyForSimulation or
            MetadataChangePlanStatus.PartiallyReady;

    /// <summary>
    /// Indica si existe algún motivo para impedir la aplicación
    /// automática completa.
    /// </summary>
    public bool RequiresManualReview =>
        ManualReviewCount > 0 ||
        ConflictCount > 0;

    /// <summary>
    /// Resumen compacto del plan.
    /// </summary>
    public string Summary
    {
        get
        {
            if (!HasChanges)
            {
                return
                    $"{FileName}: no se requieren " +
                    "modificaciones.";
            }

            return
                $"{FileName}: {ActualChangeCount} cambio(s) " +
                $"propuesto(s). Automáticos: " +
                $"{AutomaticChangeCount}. Revisión manual: " +
                $"{ManualReviewCount}. Conflictos: " +
                $"{ConflictCount}. Estado: {Status}.";
        }
    }
}