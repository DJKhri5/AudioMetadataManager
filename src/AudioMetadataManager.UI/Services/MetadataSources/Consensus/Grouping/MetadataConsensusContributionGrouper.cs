using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Grouping;

/// <summary>
/// Agrupa contribuciones equivalentes por campo y valor
/// normalizado.
///
/// Este componente no selecciona ganadores ni calcula todavía
/// el estado final del consenso.
/// </summary>
public sealed class MetadataConsensusContributionGrouper
{
    /// <summary>
    /// Agrupa todas las contribuciones utilizables.
    /// </summary>
    public IReadOnlyList<MetadataConsensusContributionGroup>
        Group(
            IEnumerable<MetadataConsensusContribution>
                contributions)
    {
        ArgumentNullException.ThrowIfNull(
            contributions);

        return contributions
            .Where(
                contribution =>
                    contribution is not null &&
                    contribution.IsUsable &&
                    !string.IsNullOrWhiteSpace(
                        contribution.NormalizedValue))
            .GroupBy(
                contribution =>
                    new ContributionGroupKey(
                        contribution.Field,
                        contribution.NormalizedValue),
                ContributionGroupKeyComparer.Instance)
            .Select(
                group =>
                    CreateGroup(
                        group.Key,
                        group))
            .OrderBy(
                group =>
                    group.Field)
            .ThenByDescending(
                group =>
                    group.DistinctSourceCount)
            .ThenByDescending(
                group =>
                    group.TotalWeightedSupport)
            .ThenBy(
                group =>
                    group.RepresentativeValue,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    /// Agrupa únicamente las contribuciones de un campo.
    /// </summary>
    public IReadOnlyList<MetadataConsensusContributionGroup>
        GroupField(
            MetadataField field,
            IEnumerable<MetadataConsensusContribution>
                contributions)
    {
        ArgumentNullException.ThrowIfNull(
            contributions);

        if (field == MetadataField.Unknown)
        {
            return
                Array.Empty<
                    MetadataConsensusContributionGroup>();
        }

        return Group(
                contributions.Where(
                    contribution =>
                        contribution.Field == field))
            .ToArray();
    }

    private static MetadataConsensusContributionGroup
        CreateGroup(
            ContributionGroupKey key,
            IEnumerable<MetadataConsensusContribution>
                contributions)
    {
        return new MetadataConsensusContributionGroup
        {
            Field =
                key.Field,

            NormalizedValue =
                key.NormalizedValue,

            Contributions =
                contributions
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
                        StringComparer.OrdinalIgnoreCase)
                    .ToArray()
        };
    }

    private static int NormalizeSourceRank(
        int sourceRank)
    {
        return sourceRank > 0
            ? sourceRank
            : int.MaxValue;
    }

    /// <summary>
    /// Clave interna de agrupación.
    /// </summary>
    private readonly record struct ContributionGroupKey(
        MetadataField Field,
        string NormalizedValue);

    /// <summary>
    /// Comparador estable para la clave de agrupación.
    /// </summary>
    private sealed class ContributionGroupKeyComparer
        : IEqualityComparer<ContributionGroupKey>
    {
        public static ContributionGroupKeyComparer Instance
        { get; } =
                new();

        public bool Equals(
            ContributionGroupKey x,
            ContributionGroupKey y)
        {
            return
                x.Field == y.Field &&
                string.Equals(
                    x.NormalizedValue,
                    y.NormalizedValue,
                    StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(
            ContributionGroupKey obj)
        {
            return HashCode.Combine(
                obj.Field,
                StringComparer.OrdinalIgnoreCase.GetHashCode(
                    obj.NormalizedValue ??
                    string.Empty));
        }
    }
}