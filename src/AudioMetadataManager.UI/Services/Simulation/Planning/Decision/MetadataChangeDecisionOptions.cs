namespace AudioMetadataManager.UI.Services.Simulation
    .Planning.Decision;

/// <summary>
/// Define los umbrales utilizados por el motor para clasificar
/// propuestas de modificación de metadatos.
/// </summary>
public sealed class MetadataChangeDecisionOptions
{
    /// <summary>
    /// Confianza mínima para considerar una propuesta
    /// automáticamente aplicable.
    /// </summary>
    public double AutomaticApplyConfidenceThreshold
    { get; init; } =
            0.95;

    /// <summary>
    /// Cantidad mínima de fuentes distintas requerida para una
    /// propuesta automática.
    /// </summary>
    public int MinimumSourcesForAutomaticApply
    { get; init; } =
            2;

    /// <summary>
    /// Confianza mínima para conservar una propuesta en la cola
    /// de revisión manual.
    /// </summary>
    public double ManualReviewConfidenceThreshold
    { get; init; } =
            0.60;

    /// <summary>
    /// Indica si una propuesta respaldada por una sola fuente
    /// siempre debe requerir revisión manual.
    /// </summary>
    public bool RequireManualReviewForSingleSource
    { get; init; } =
            true;

    /// <summary>
    /// Indica si los campos críticos deben requerir consenso
    /// entre varias fuentes antes de ser automáticos.
    /// </summary>
    public bool RequireMultipleSourcesForCriticalFields
    { get; init; } =
            true;

    /// <summary>
    /// Indica si la configuración contiene valores válidos.
    /// </summary>
    public bool IsValid =>
        AutomaticApplyConfidenceThreshold is > 0 and <= 1 &&
        ManualReviewConfidenceThreshold is >= 0 and <= 1 &&
        ManualReviewConfidenceThreshold <=
            AutomaticApplyConfidenceThreshold &&
        MinimumSourcesForAutomaticApply >= 2;
}