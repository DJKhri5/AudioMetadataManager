namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

/// <summary>
/// Consolida la segunda fase de una aplicación productiva
/// por lote después de una preparación verificada.
/// </summary>
public sealed class MetadataProductiveBatchCompletionResult
{
    /// <summary>
    /// Identificador del lote preparado.
    /// </summary>
    public Guid BatchId { get; init; }

    /// <summary>
    /// Decisión global solicitada para el lote.
    /// </summary>
    public MetadataPromotionDecision PromotionDecision
    { get; init; } =
        MetadataPromotionDecision.NotRequested;

    /// <summary>
    /// Momento UTC de inicio de la finalización.
    /// </summary>
    public DateTime StartedAtUtc { get; init; }

    /// <summary>
    /// Momento UTC de término de la finalización.
    /// </summary>
    public DateTime FinishedAtUtc { get; init; }

    /// <summary>
    /// Cantidad total de preparaciones que debían recibir
    /// la decisión global.
    /// </summary>
    public int RequestedCount { get; init; }

    /// <summary>
    /// Resultados que llegaron a procesarse utilizando la
    /// decisión global solicitada.
    /// </summary>
    public IReadOnlyList<MetadataProductiveApplicationResult>
        DecisionResults
    { get; init; } =
        Array.Empty<MetadataProductiveApplicationResult>();

    /// <summary>
    /// Resultados generados al descartar con Declined las
    /// preparaciones que quedaron pendientes después de un
    /// fallo o una interrupción.
    /// </summary>
    public IReadOnlyList<MetadataProductiveApplicationResult>
        CleanupResults
    { get; init; } =
        Array.Empty<MetadataProductiveApplicationResult>();

    /// <summary>
    /// Mensajes auditables producidos durante la finalización.
    /// </summary>
    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    public int DecisionResultCount =>
        DecisionResults.Count;

    public int CleanupResultCount =>
        CleanupResults.Count;

    /// <summary>
    /// Cantidad de resultados que completaron correctamente
    /// la decisión global solicitada.
    /// </summary>
    public int SuccessfulDecisionCount =>
        PromotionDecision switch
        {
            MetadataPromotionDecision.Approved =>
                DecisionResults.Count(
                    result =>
                        result.WasSuccessfullyPromoted),

            MetadataPromotionDecision.Declined =>
                DecisionResults.Count(
                    result =>
                        result.WasSafelyDeclined),

            _ =>
                0
        };

    public int FailedDecisionCount =>
        DecisionResultCount -
        SuccessfulDecisionCount;

    /// <summary>
    /// Indica si todas las preparaciones descartadas durante
    /// la recuperación terminaron correctamente con Declined.
    /// </summary>
    public bool CleanupWasSuccessful =>
        CleanupResults.Count == 0 ||
        CleanupResults.All(
            result =>
                result.WasSafelyDeclined);

    /// <summary>
    /// Indica si el lote terminó completamente según la decisión
    /// global solicitada y sin requerir recuperación adicional.
    /// </summary>
    public bool WasSuccessful =>
        RequestedCount > 0 &&
        DecisionResultCount == RequestedCount &&
        SuccessfulDecisionCount == RequestedCount &&
        FailedDecisionCount == 0 &&
        CleanupResultCount == 0;

    /// <summary>
    /// Indica si todos los resultados producidos dejaron sus
    /// archivos originales en un estado seguro.
    /// </summary>
    public bool OriginalsEndedInSafeState =>
        DecisionResults.All(
            result =>
                result.OriginalEndedInSafeState) &&
        CleanupResults.All(
            result =>
                result.OriginalEndedInSafeState);

    public TimeSpan Duration =>
        FinishedAtUtc >= StartedAtUtc
            ? FinishedAtUtc - StartedAtUtc
            : TimeSpan.Zero;

    public string Summary
    {
        get
        {
            if (WasSuccessful)
            {
                return PromotionDecision ==
                    MetadataPromotionDecision.Approved
                    ? $"{RequestedCount} archivo(s) fueron " +
                      "promovidos correctamente."
                    : $"{RequestedCount} preparación(es) fueron " +
                      "rechazadas y limpiadas correctamente.";
            }

            return
                $"La finalización procesó " +
                $"{DecisionResultCount} de {RequestedCount} " +
                $"preparación(es). Correctas según la decisión: " +
                $"{SuccessfulDecisionCount}. Fallidas: " +
                $"{FailedDecisionCount}. Limpiezas de recuperación: " +
                $"{CleanupResultCount}.";
        }
    }
}