using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Models;

/// <summary>
/// Contiene la conclusión del consenso para un campo
/// individual de metadatos.
/// </summary>
public sealed class MetadataConsensusFieldResult
{
    /// <summary>
    /// Campo evaluado.
    /// </summary>
    public MetadataField Field { get; init; } =
        MetadataField.Unknown;

    /// <summary>
    /// Valor seleccionado por el motor.
    ///
    /// Puede quedar vacío cuando no se alcanzó una decisión
    /// suficientemente segura.
    /// </summary>
    public string SelectedValue { get; init; } =
        string.Empty;

    /// <summary>
    /// Estado alcanzado durante la evaluación.
    /// </summary>
    public MetadataConsensusStatus Status { get; init; } =
        MetadataConsensusStatus.Unknown;

    /// <summary>
    /// Confianza de la conclusión del campo, entre 0 y 1.
    /// </summary>
    public double Confidence { get; init; }

    /// <summary>
    /// Todas las propuestas utilizables que participaron en la
    /// evaluación del campo.
    /// </summary>
    public IReadOnlyList<MetadataConsensusContribution>
        Contributions
    { get; init; } =
            Array.Empty<MetadataConsensusContribution>();

    /// <summary>
    /// Propuestas que respaldan directamente el valor
    /// seleccionado.
    /// </summary>
    public IReadOnlyList<MetadataConsensusContribution>
        WinningContributions =>
            string.IsNullOrWhiteSpace(SelectedValue)
                ? Array.Empty<MetadataConsensusContribution>()
                : Contributions
                    .Where(
                        contribution =>
                            string.Equals(
                                contribution.NormalizedValue,
                                SelectedNormalizedValue,
                                StringComparison.OrdinalIgnoreCase))
                    .ToArray();

    /// <summary>
    /// Valor normalizado asociado a la decisión ganadora.
    /// </summary>
    public string SelectedNormalizedValue { get; init; } =
        string.Empty;

    /// <summary>
    /// Explicación legible de la decisión tomada.
    /// </summary>
    public string Explanation { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si se seleccionó un valor utilizable.
    /// </summary>
    public bool HasSelectedValue =>
        !string.IsNullOrWhiteSpace(
            SelectedValue);

    /// <summary>
    /// Indica si existen propuestas incompatibles.
    /// </summary>
    public bool HasConflict =>
        Status ==
        MetadataConsensusStatus.Conflict;

    /// <summary>
    /// Indica si el campo debe revisarse manualmente.
    /// </summary>
    public bool RequiresManualReview =>
        HasConflict ||
        Status ==
            MetadataConsensusStatus.SingleSource ||
        Contributions.Any(
            contribution =>
                contribution.RequiresManualApproval);

    /// <summary>
    /// Cantidad de fuentes distintas que aportaron información.
    /// </summary>
    public int ContributingSourceCount =>
        Contributions
            .Where(
                contribution =>
                    contribution.IsUsable)
            .Select(
                contribution =>
                    contribution.SourceName)
            .Distinct(
                StringComparer.OrdinalIgnoreCase)
            .Count();

    /// <summary>
    /// Confianza preparada para mostrarse.
    /// </summary>
    public string ConfidenceDisplay =>
        $"{Math.Clamp(Confidence, 0, 1) * 100:0.00}%";

    /// <summary>
    /// Resumen compacto del campo.
    /// </summary>
    public string Summary
    {
        get
        {
            string valueDisplay =
                HasSelectedValue
                    ? SelectedValue
                    : "(sin valor seleccionado)";

            return
                $"{Field}: {valueDisplay}. " +
                $"Estado: {Status}. " +
                $"Confianza: {ConfidenceDisplay}. " +
                $"Fuentes: {ContributingSourceCount}.";
        }
    }
}