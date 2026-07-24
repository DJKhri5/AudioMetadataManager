using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Planning.Models;

/// <summary>
/// Representa una propuesta individual de modificación para un
/// campo de metadatos.
///
/// Conserva el valor actual, el valor sugerido, la evidencia y
/// la decisión tomada por el motor.
/// </summary>
public sealed class MetadataChangeProposal
{
    /// <summary>
    /// Campo que podría modificarse.
    /// </summary>
    public MetadataField Field { get; init; } =
        MetadataField.Unknown;

    /// <summary>
    /// Valor almacenado actualmente en el archivo.
    /// </summary>
    public string CurrentValue { get; init; } =
        string.Empty;

    /// <summary>
    /// Valor propuesto por el motor de consenso.
    /// </summary>
    public string ProposedValue { get; init; } =
        string.Empty;

    /// <summary>
    /// Valor normalizado de la propuesta.
    /// Se utiliza para comparación, no para escribir etiquetas.
    /// </summary>
    public string ProposedNormalizedValue { get; init; } =
        string.Empty;

    /// <summary>
    /// Confianza del consenso para este campo, entre 0 y 1.
    /// </summary>
    public double ConsensusConfidence { get; init; }

    /// <summary>
    /// Cantidad de fuentes distintas que respaldan la
    /// propuesta.
    /// </summary>
    public int SupportingSourceCount { get; init; }

    /// <summary>
    /// Fuentes externas que respaldan el valor.
    /// </summary>
    public IReadOnlyList<string> SupportingSources
    { get; init; } =
            Array.Empty<string>();

    /// <summary>
    /// Decisión asignada por el motor.
    /// </summary>
    public MetadataChangeDecision Decision { get; init; } =
        MetadataChangeDecision.Pending;

    /// <summary>
    /// Explicación legible de la decisión.
    /// </summary>
    public string Explanation { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si una plataforma exige aprobación manual.
    /// SoundCloud deberá mantener esta señal.
    /// </summary>
    public bool SourceRequiresManualApproval { get; init; }

    /// <summary>
    /// Indica si existe un valor propuesto utilizable.
    /// </summary>
    public bool HasProposedValue =>
        !string.IsNullOrWhiteSpace(
            ProposedValue);

    /// <summary>
    /// Indica si el archivo ya contiene algún valor.
    /// </summary>
    public bool HasCurrentValue =>
        !string.IsNullOrWhiteSpace(
            CurrentValue);

    /// <summary>
    /// Indica si la propuesta representa una modificación real.
    ///
    /// Esta comparación preliminar ignora mayúsculas,
    /// minúsculas y espacios exteriores.
    /// </summary>
    public bool HasActualChange =>
        HasProposedValue &&
        !string.Equals(
            NormalizeForComparison(
                CurrentValue),
            NormalizeForComparison(
                ProposedValue),
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Indica si la propuesta podría aplicarse automáticamente
    /// después de todas las validaciones y del respaldo.
    /// </summary>
    public bool IsAutomaticApplyEligible =>
        Decision ==
        MetadataChangeDecision.EligibleForAutomaticApply;

    /// <summary>
    /// Indica si una modificación real debe mostrarse en la cola
    /// de revisión manual.
    ///
    /// Una propuesta que no cambia el archivo queda cerrada aunque
    /// su evidencia provenga de una sola fuente.
    /// </summary>
    public bool RequiresManualReview =>
        HasActualChange &&
        (
            Decision ==
                MetadataChangeDecision.ManualReviewRequired ||
            Decision ==
                MetadataChangeDecision.Conflict ||
            SourceRequiresManualApproval
        );

    /// <summary>
    /// Confianza preparada para mostrarse.
    /// </summary>
    public string ConfidenceDisplay =>
        $"{Math.Clamp(
            ConsensusConfidence,
            0,
            1) * 100:0.00}%";

    /// <summary>
    /// Resumen compacto de la propuesta.
    /// </summary>
    public string Summary =>
        $"{Field}: " +
        $"{DisplayValue(CurrentValue)} → " +
        $"{DisplayValue(ProposedValue)}. " +
        $"Decisión: {Decision}. " +
        $"Confianza: {ConfidenceDisplay}.";

    private static string NormalizeForComparison(
        string? value)
    {
        return string.IsNullOrWhiteSpace(
                value)
            ? string.Empty
            : value.Trim();
    }

    private static string DisplayValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(
                value)
            ? "(sin información)"
            : value.Trim();
    }
}