using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Grouping;
using AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Evaluation;

/// <summary>
/// Evalúa los grupos de propuestas de un campo y determina
/// si existe consenso, mayoría, una sola fuente o conflicto.
///
/// Para evitar que una plataforma domine por devolver muchos
/// candidatos, sólo se considera la mejor contribución de cada
/// fuente dentro de cada grupo.
/// </summary>
public sealed class MetadataConsensusFieldEvaluator
{
    private static readonly MetadataField[] SupportedFields =
    {
        MetadataField.Artist,
        MetadataField.Title,
        MetadataField.Version,
        MetadataField.Album,
        MetadataField.Genre,
        MetadataField.Label
    };

    private readonly MetadataConsensusEvaluationOptions
        _options;

    /// <summary>
    /// Crea el evaluador con la política predeterminada.
    /// </summary>
    public MetadataConsensusFieldEvaluator()
        : this(
            new MetadataConsensusEvaluationOptions())
    {
    }

    /// <summary>
    /// Crea el evaluador con una política personalizada.
    /// </summary>
    public MetadataConsensusFieldEvaluator(
        MetadataConsensusEvaluationOptions options)
    {
        _options =
            options ??
            throw new ArgumentNullException(
                nameof(options));

        if (!_options.IsValid)
        {
            throw new ArgumentException(
                "La configuración del evaluador de consenso " +
                "contiene valores no válidos.",
                nameof(options));
        }
    }

    /// <summary>
    /// Evalúa todos los campos actualmente soportados.
    /// </summary>
    public IReadOnlyList<MetadataConsensusFieldResult>
        EvaluateAll(
            IEnumerable<MetadataConsensusContributionGroup>
                groups)
    {
        ArgumentNullException.ThrowIfNull(
            groups);

        IReadOnlyList<MetadataConsensusContributionGroup>
            availableGroups =
                groups
                    .Where(
                        group =>
                            group is not null &&
                            group.IsUsable)
                    .ToArray();

        return SupportedFields
            .Select(
                field =>
                    Evaluate(
                        field,
                        availableGroups))
            .ToArray();
    }

    /// <summary>
    /// Evalúa los grupos correspondientes a un campo concreto.
    /// </summary>
    public MetadataConsensusFieldResult Evaluate(
        MetadataField field,
        IEnumerable<MetadataConsensusContributionGroup>
            groups)
    {
        ArgumentNullException.ThrowIfNull(
            groups);

        if (field == MetadataField.Unknown)
        {
            return CreateNotApplicableResult(
                field);
        }

        List<MetadataConsensusContributionGroup> fieldGroups =
            groups
                .Where(
                    group =>
                        group is not null &&
                        group.IsUsable &&
                        group.Field == field)
                .OrderByDescending(
                    CalculateEffectiveSupport)
                .ThenByDescending(
                    GetEffectiveSourceCount)
                .ThenByDescending(
                    group =>
                        group.AverageWeightedSupport)
                .ThenBy(
                    group =>
                        group.RepresentativeValue,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        if (fieldGroups.Count == 0)
        {
            return CreateNoInformationResult(
                field);
        }

        IReadOnlyList<MetadataConsensusContribution>
            allContributions =
                fieldGroups
                    .SelectMany(
                        group =>
                            group.Contributions)
                    .Where(
                        contribution =>
                            contribution.IsUsable)
                    .OrderByDescending(
                        contribution =>
                            contribution.WeightedSupport)
                    .ThenBy(
                        contribution =>
                            NormalizeSourceRank(
                                contribution.SourceRank))
                    .ToArray();

        MetadataConsensusContributionGroup winner =
            fieldGroups[0];

        double winnerSupport =
            CalculateEffectiveSupport(
                winner);

        double totalSupport =
            fieldGroups.Sum(
                CalculateEffectiveSupport);

        double runnerUpSupport =
            fieldGroups.Count > 1
                ? CalculateEffectiveSupport(
                    fieldGroups[1])
                : 0;

        double winnerShare =
            totalSupport <= 0
                ? 0
                : winnerSupport /
                  totalSupport;

        double normalizedLead =
            totalSupport <= 0
                ? 0
                : Math.Max(
                    0,
                    winnerSupport -
                    runnerUpSupport) /
                  totalSupport;

        int winnerSourceCount =
            GetEffectiveSourceCount(
                winner);

        double confidence =
            CalculateConfidence(
                winner,
                winnerShare);

        if (fieldGroups.Count == 1)
        {
            return CreateSingleGroupResult(
                field,
                winner,
                allContributions,
                winnerSourceCount,
                confidence);
        }

        bool hasClearMajority =
            winnerSourceCount >=
                _options.MinimumSourcesForConsensus &&
            winnerShare >=
                _options.MajoritySupportShareThreshold &&
            normalizedLead >=
                _options.MinimumSupportLead;

        if (hasClearMajority)
        {
            return new MetadataConsensusFieldResult
            {
                Field =
                    field,

                SelectedValue =
                    winner.RepresentativeValue,

                SelectedNormalizedValue =
                    winner.NormalizedValue,

                Status =
                    MetadataConsensusStatus.MajorityReached,

                Confidence =
                    confidence,

                Contributions =
                    allContributions,

                Explanation =
                    $"El valor seleccionado obtuvo una mayoría " +
                    $"ponderada del {winnerShare * 100:0.00}% " +
                    $"y una ventaja del " +
                    $"{normalizedLead * 100:0.00}% sobre la " +
                    $"segunda propuesta. Fue respaldado por " +
                    $"{winnerSourceCount} fuente(s)."
            };
        }

        return new MetadataConsensusFieldResult
        {
            Field =
                field,

            SelectedValue =
                string.Empty,

            SelectedNormalizedValue =
                string.Empty,

            Status =
                MetadataConsensusStatus.Conflict,

            Confidence =
                Math.Clamp(
                    winnerShare,
                    0,
                    1),

            Contributions =
                allContributions,

            Explanation =
                $"Se encontraron {fieldGroups.Count} valores " +
                $"incompatibles y ninguna propuesta alcanzó " +
                $"una mayoría suficientemente clara. El grupo " +
                $"mejor posicionado obtuvo el " +
                $"{winnerShare * 100:0.00}% del soporte."
        };
    }

    private MetadataConsensusFieldResult
        CreateSingleGroupResult(
            MetadataField field,
            MetadataConsensusContributionGroup winner,
            IReadOnlyList<MetadataConsensusContribution>
                allContributions,
            int winnerSourceCount,
            double confidence)
    {
        bool hasMultipleSources =
            winnerSourceCount >=
            _options.MinimumSourcesForConsensus;

        MetadataConsensusStatus status =
            hasMultipleSources
                ? MetadataConsensusStatus.ConsensusReached
                : MetadataConsensusStatus.SingleSource;

        double finalConfidence =
            hasMultipleSources
                ? confidence
                : Math.Min(
                    confidence,
                    _options.SingleSourceConfidenceCap);

        string explanation =
            hasMultipleSources
                ? $"Todas las propuestas utilizables se " +
                  $"agruparon en un único valor, respaldado " +
                  $"por {winnerSourceCount} fuentes distintas."
                : "Sólo una fuente aportó un valor utilizable. " +
                  "La propuesta se conserva, pero todavía no " +
                  "constituye consenso entre plataformas.";

        return new MetadataConsensusFieldResult
        {
            Field =
                field,

            SelectedValue =
                winner.RepresentativeValue,

            SelectedNormalizedValue =
                winner.NormalizedValue,

            Status =
                status,

            Confidence =
                finalConfidence,

            Contributions =
                allContributions,

            Explanation =
                explanation
        };
    }

    private static MetadataConsensusFieldResult
        CreateNoInformationResult(
            MetadataField field)
    {
        return new MetadataConsensusFieldResult
        {
            Field =
                field,

            Status =
                MetadataConsensusStatus.NoInformation,

            Confidence =
                0,

            Explanation =
                "Ninguna fuente proporcionó un valor utilizable " +
                "para este campo."
        };
    }

    private static MetadataConsensusFieldResult
        CreateNotApplicableResult(
            MetadataField field)
    {
        return new MetadataConsensusFieldResult
        {
            Field =
                field,

            Status =
                MetadataConsensusStatus.NotApplicable,

            Confidence =
                0,

            Explanation =
                "El campo no corresponde a la evaluación " +
                "de consenso actual."
        };
    }

    /// <summary>
    /// Calcula el soporte sin permitir que una misma fuente
    /// participe más de una vez dentro de un grupo.
    /// </summary>
    private static double CalculateEffectiveSupport(
        MetadataConsensusContributionGroup group)
    {
        return GetEffectiveContributions(
                group)
            .Sum(
                contribution =>
                    contribution.WeightedSupport);
    }

    /// <summary>
    /// Obtiene la cantidad real de fuentes que respaldan
    /// el grupo.
    /// </summary>
    private static int GetEffectiveSourceCount(
        MetadataConsensusContributionGroup group)
    {
        return GetEffectiveContributions(
                group)
            .Count;
    }

    /// <summary>
    /// Conserva únicamente la mejor contribución de cada
    /// plataforma dentro de un grupo.
    /// </summary>
    private static IReadOnlyList<
        MetadataConsensusContribution>
        GetEffectiveContributions(
            MetadataConsensusContributionGroup group)
    {
        return group.Contributions
            .Where(
                contribution =>
                    contribution.IsUsable)
            .GroupBy(
                contribution =>
                    NormalizeSourceName(
                        contribution.SourceName),
                StringComparer.OrdinalIgnoreCase)
            .Select(
                sourceGroup =>
                    sourceGroup
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
                        .First())
            .ToArray();
    }

    /// <summary>
    /// Combina la ventaja relativa del grupo ganador con la
    /// calidad promedio de las fuentes que lo respaldan.
    /// </summary>
    private static double CalculateConfidence(
        MetadataConsensusContributionGroup winner,
        double winnerShare)
    {
        IReadOnlyList<MetadataConsensusContribution>
            effectiveContributions =
                GetEffectiveContributions(
                    winner);

        if (effectiveContributions.Count == 0)
        {
            return 0;
        }

        double averageSupport =
            effectiveContributions.Average(
                contribution =>
                    contribution.WeightedSupport);

        double confidence =
            (Math.Clamp(
                winnerShare,
                0,
                1) *
             0.60) +
            (Math.Clamp(
                averageSupport,
                0,
                1) *
             0.40);

        return Math.Clamp(
            confidence,
            0,
            1);
    }

    private static string NormalizeSourceName(
        string? sourceName)
    {
        return string.IsNullOrWhiteSpace(
            sourceName)
                ? "(fuente sin identificar)"
                : sourceName.Trim();
    }

    private static int NormalizeSourceRank(
        int sourceRank)
    {
        return sourceRank > 0
            ? sourceRank
            : int.MaxValue;
    }
}