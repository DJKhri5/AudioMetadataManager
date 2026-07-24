using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Grouping;

/// <summary>
/// Representa un conjunto de contribuciones equivalentes
/// para un mismo campo de metadatos.
///
/// Las propuestas se agrupan mediante su valor normalizado,
/// pero conservan sus textos originales y su procedencia.
/// </summary>
public sealed class MetadataConsensusContributionGroup
{
    /// <summary>
    /// Campo al que pertenece el grupo.
    /// </summary>
    public MetadataField Field { get; init; } =
        MetadataField.Unknown;

    /// <summary>
    /// Valor normalizado utilizado como clave de agrupación.
    /// </summary>
    public string NormalizedValue { get; init; } =
        string.Empty;

    /// <summary>
    /// Contribuciones que respaldan este grupo.
    /// </summary>
    public IReadOnlyList<MetadataConsensusContribution>
        Contributions
    { get; init; } =
            Array.Empty<MetadataConsensusContribution>();

    /// <summary>
    /// Indica si el grupo contiene información utilizable.
    /// </summary>
    public bool IsUsable =>
        Field != MetadataField.Unknown &&
        !string.IsNullOrWhiteSpace(
            NormalizedValue) &&
        Contributions.Count > 0;

    /// <summary>
    /// Cantidad de contribuciones individuales.
    /// </summary>
    public int ContributionCount =>
        Contributions.Count;

    /// <summary>
    /// Cantidad de fuentes distintas que respaldan el valor.
    /// </summary>
    public int DistinctSourceCount =>
        Contributions
            .Where(
                contribution =>
                    contribution.IsUsable)
            .Select(
                contribution =>
                    contribution.SourceName)
            .Where(
                sourceName =>
                    !string.IsNullOrWhiteSpace(
                        sourceName))
            .Distinct(
                StringComparer.OrdinalIgnoreCase)
            .Count();

    /// <summary>
    /// Suma del soporte ponderado de todas las contribuciones.
    ///
    /// Esta métrica permitirá comparar posteriormente
    /// diferentes grupos candidatos.
    /// </summary>
    public double TotalWeightedSupport =>
        Contributions
            .Where(
                contribution =>
                    contribution.IsUsable)
            .Sum(
                contribution =>
                    contribution.WeightedSupport);

    /// <summary>
    /// Soporte ponderado promedio del grupo.
    /// </summary>
    public double AverageWeightedSupport =>
        ContributionCount == 0
            ? 0
            : TotalWeightedSupport /
              ContributionCount;

    /// <summary>
    /// Mayor confianza individual encontrada en el grupo.
    /// </summary>
    public double MaximumCandidateConfidence =>
        Contributions.Count == 0
            ? 0
            : Contributions.Max(
                contribution =>
                    Math.Clamp(
                        contribution.CandidateConfidence,
                        0,
                        1));

    /// <summary>
    /// Indica si alguna contribución exige aprobación manual.
    /// </summary>
    public bool RequiresManualApproval =>
        Contributions.Any(
            contribution =>
                contribution.RequiresManualApproval);

    /// <summary>
    /// Valor original recomendado para representar el grupo.
    ///
    /// Se prioriza la contribución con mayor soporte ponderado,
    /// seguida de la mejor posición original.
    /// </summary>
    public string RepresentativeValue
    {
        get
        {
            MetadataConsensusContribution? representative =
                Contributions
                    .Where(
                        contribution =>
                            contribution.IsUsable)
                    .OrderByDescending(
                        contribution =>
                            contribution.WeightedSupport)
                    .ThenByDescending(
                        contribution =>
                            contribution.CandidateConfidence)
                    .ThenBy(
                        contribution =>
                            NormalizeSourceRank(
                                contribution.SourceRank))
                    .ThenBy(
                        contribution =>
                            contribution.SourceName,
                        StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault();

            return representative?.Value ??
                   string.Empty;
        }
    }

    /// <summary>
    /// Fuentes que respaldan el grupo.
    /// </summary>
    public IReadOnlyList<string> SourceNames =>
        Contributions
            .Where(
                contribution =>
                    !string.IsNullOrWhiteSpace(
                        contribution.SourceName))
            .Select(
                contribution =>
                    contribution.SourceName)
            .Distinct(
                StringComparer.OrdinalIgnoreCase)
            .OrderBy(
                sourceName =>
                    sourceName,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();

    /// <summary>
    /// Resumen compacto del grupo.
    /// </summary>
    public string Summary =>
        $"{Field}: {RepresentativeValue}. " +
        $"Fuentes: {DistinctSourceCount}. " +
        $"Contribuciones: {ContributionCount}. " +
        $"Soporte total: {TotalWeightedSupport * 100:0.00}%. " +
        $"Aprobación manual: " +
        $"{(RequiresManualApproval ? "Sí" : "No")}.";

    private static int NormalizeSourceRank(
        int sourceRank)
    {
        return sourceRank > 0
            ? sourceRank
            : int.MaxValue;
    }
}