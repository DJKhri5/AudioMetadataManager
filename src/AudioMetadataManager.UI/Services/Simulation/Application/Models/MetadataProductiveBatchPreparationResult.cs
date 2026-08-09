namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

/// <summary>
/// Consolida las preparaciones productivas individuales
/// realizadas para un lote antes de solicitar una decisión
/// global de promoción.
/// </summary>
public sealed class MetadataProductiveBatchPreparationResult
{
    /// <summary>
    /// Identificador del lote original.
    /// </summary>
    public Guid BatchId { get; init; }

    /// <summary>
    /// Momento UTC en que comenzó la preparación.
    /// </summary>
    public DateTime StartedAtUtc { get; init; }

    /// <summary>
    /// Momento UTC en que terminó la preparación.
    /// </summary>
    public DateTime FinishedAtUtc { get; init; }

    /// <summary>
    /// Cantidad total de solicitudes válidas que debían
    /// prepararse.
    /// </summary>
    public int RequestedCount { get; init; }

    /// <summary>
    /// Resultados individuales producidos durante la fase
    /// de preparación.
    /// </summary>
    public IReadOnlyList<MetadataProductiveApplicationResult>
        PreparationResults
    { get; init; } =
        Array.Empty<MetadataProductiveApplicationResult>();

    /// <summary>
    /// Mensajes auditables de la preparación por lote.
    /// </summary>
    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Indica si una preparación parcial tuvo que abortarse
    /// y sus entornos pendientes fueron descartados.
    /// </summary>
    public bool WasAbortedAndCleanedUp { get; init; }

    /// <summary>
    /// Cantidad de resultados individuales obtenidos.
    /// </summary>
    public int ResultCount =>
        PreparationResults.Count;

    /// <summary>
    /// Cantidad de copias verificadas que fueron preparadas.
    /// </summary>
    public int VerifiedPreparationCount =>
        PreparationResults.Count(
            result =>
                result is not null &&
                result.VerifiedCopyWasPrepared);

    /// <summary>
    /// Indica si todas las solicitudes quedaron preparadas,
    /// pendientes y disponibles para una decisión global.
    /// </summary>
    public bool IsReadyForDecision =>
        !WasAbortedAndCleanedUp &&
        RequestedCount > 0 &&
        ResultCount == RequestedCount &&
        PreparationResults.All(
            result =>
                result is not null &&
                result.VerifiedCopyWasPrepared &&
                result.PromotionDecision ==
                    MetadataPromotionDecision.Pending &&
                string.IsNullOrWhiteSpace(
                    result.ErrorMessage));

    /// <summary>
    /// Duración total de la fase de preparación.
    /// </summary>
    public TimeSpan Duration =>
        FinishedAtUtc >= StartedAtUtc
            ? FinishedAtUtc - StartedAtUtc
            : TimeSpan.Zero;

    /// <summary>
    /// Resumen compacto de la fase de preparación.
    /// </summary>
    public string Summary =>
        IsReadyForDecision
            ? $"{VerifiedPreparationCount} de " +
              $"{RequestedCount} archivo(s) quedaron " +
              "preparados y pendientes de una decisión global."
            : WasAbortedAndCleanedUp
                ? "La preparación del lote fue abortada y los " +
                  "entornos pendientes fueron limpiados."
                : $"{VerifiedPreparationCount} de " +
                  $"{RequestedCount} archivo(s) pudieron prepararse.";
}