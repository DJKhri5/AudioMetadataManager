using AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Builders;
using AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Evaluation;
using AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Grouping;
using AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Models;
using ConsensusResult =
    AudioMetadataManager.UI.Services.MetadataSources
        .Consensus.Models.MetadataConsensusResult;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Candidates;
using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Engine;

/// <summary>
/// Coordina el flujo completo del nuevo motor de consenso.
///
/// Convierte evaluaciones de candidatos en contribuciones,
/// agrupa propuestas equivalentes, evalúa cada campo y genera
/// un resultado global trazable.
/// </summary>
public sealed class MetadataConsensusOrchestrator
{
    private readonly MetadataConsensusContributionBuilder
        _contributionBuilder;

    private readonly MetadataConsensusContributionGrouper
        _contributionGrouper;

    private readonly MetadataConsensusFieldEvaluator
        _fieldEvaluator;

    /// <summary>
    /// Crea el coordinador con todos sus componentes
    /// predeterminados.
    /// </summary>
    public MetadataConsensusOrchestrator()
        : this(
            new MetadataConsensusContributionBuilder(),
            new MetadataConsensusContributionGrouper(),
            new MetadataConsensusFieldEvaluator())
    {
    }

    /// <summary>
    /// Crea el coordinador con componentes personalizados.
    /// </summary>
    public MetadataConsensusOrchestrator(
        MetadataConsensusContributionBuilder contributionBuilder,
        MetadataConsensusContributionGrouper contributionGrouper,
        MetadataConsensusFieldEvaluator fieldEvaluator)
    {
        _contributionBuilder =
            contributionBuilder ??
            throw new ArgumentNullException(
                nameof(contributionBuilder));

        _contributionGrouper =
            contributionGrouper ??
            throw new ArgumentNullException(
                nameof(contributionGrouper));

        _fieldEvaluator =
            fieldEvaluator ??
            throw new ArgumentNullException(
                nameof(fieldEvaluator));
    }

    /// <summary>
    /// Ejecuta el consenso global sobre una colección de
    /// candidatos previamente evaluados.
    /// </summary>
    public ConsensusResult Evaluate(
        IEnumerable<MetadataCandidateEvaluationResult>
            evaluations)
    {
        ArgumentNullException.ThrowIfNull(
            evaluations);

        IReadOnlyList<MetadataCandidateEvaluationResult>
            evaluationList =
                evaluations
                    .Where(
                        evaluation =>
                            evaluation is not null &&
                            evaluation.IsUsable)
                    .ToArray();

        IReadOnlyList<MetadataConsensusContribution>
            contributions =
                _contributionBuilder.Build(
                    evaluationList);

        IReadOnlyList<MetadataConsensusContributionGroup>
            groups =
                _contributionGrouper.Group(
                    contributions);

        IReadOnlyList<MetadataConsensusFieldResult>
            fieldResults =
                _fieldEvaluator.EvaluateAll(
                    groups);

        double overallConfidence =
            CalculateOverallConfidence(
                fieldResults);

        IReadOnlyList<string> reasons =
            BuildReasons(
                evaluationList,
                contributions,
                groups,
                fieldResults);

        MetadataCandidateEvaluationResult? bestArtworkCandidate =
            SelectBestArtworkCandidate(
                evaluationList);

        return new ConsensusResult
        {
            Fields =
                fieldResults,

            OverallConfidence =
                overallConfidence,

            Reasons =
                reasons,

            ArtworkUrl =
                bestArtworkCandidate?.Candidate.ArtworkUrl ??
                    string.Empty,

            ArtworkSourceName =
                bestArtworkCandidate?.Candidate.SourceName ??
                    string.Empty
        };
    }

    /// <summary>
    /// Elige la carátula propuesta por el candidato mejor
    /// posicionado que ofrezca una, entre los candidatos
    /// utilizables.
    /// </summary>
    private static MetadataCandidateEvaluationResult?
        SelectBestArtworkCandidate(
            IReadOnlyList<MetadataCandidateEvaluationResult>
                evaluationList)
    {
        return evaluationList
            .Where(
                evaluation =>
                    evaluation.Candidate.HasArtwork)
            .OrderByDescending(
                evaluation =>
                    evaluation.DecisionPriority)
            .ThenByDescending(
                evaluation =>
                    evaluation.RankingScore)
            .FirstOrDefault();
    }

    /// <summary>
    /// Ejecuta el consenso directamente desde el resultado de
    /// un lote de evaluación de candidatos.
    /// </summary>
    public ConsensusResult Evaluate(
        MetadataCandidateEvaluationBatchResult batchResult)
    {
        ArgumentNullException.ThrowIfNull(
            batchResult);

        return Evaluate(
            batchResult.Evaluations);
    }

    /// <summary>
    /// Calcula la confianza global usando únicamente campos que
    /// contienen información o una decisión explícita.
    ///
    /// Los campos sin información no reducen artificialmente
    /// el promedio.
    /// </summary>
    private static double CalculateOverallConfidence(
        IReadOnlyList<MetadataConsensusFieldResult>
            fieldResults)
    {
        MetadataConsensusFieldResult[] relevantFields =
            fieldResults
                .Where(
                    field =>
                        field.Status !=
                            MetadataConsensusStatus.NoInformation &&
                        field.Status !=
                            MetadataConsensusStatus.NotApplicable)
                .ToArray();

        if (relevantFields.Length == 0)
        {
            return 0;
        }

        double weightedTotal =
            0;

        double totalWeight =
            0;

        foreach (
            MetadataConsensusFieldResult field
            in relevantFields)
        {
            double fieldWeight =
                GetFieldWeight(
                    field.Field);

            weightedTotal +=
                Math.Clamp(
                    field.Confidence,
                    0,
                    1) *
                fieldWeight;

            totalWeight +=
                fieldWeight;
        }

        return totalWeight <= 0
            ? 0
            : Math.Clamp(
                weightedTotal /
                totalWeight,
                0,
                1);
    }

    /// <summary>
    /// Pesos provisionales para el resultado global.
    ///
    /// En un bloque posterior se extraerán a un proveedor
    /// configurable compartido con otras capas.
    /// </summary>
    private static double GetFieldWeight(
        MetadataField field)
    {
        return field switch
        {
            MetadataField.Artist =>
                0.30,

            MetadataField.Title =>
                0.30,

            MetadataField.Version =>
                0.20,

            MetadataField.Album =>
                0.10,

            MetadataField.Genre =>
                0.05,

            MetadataField.Label =>
                0.05,

            _ =>
                0
        };
    }

    private static IReadOnlyList<string> BuildReasons(
        IReadOnlyList<MetadataCandidateEvaluationResult>
            evaluations,
        IReadOnlyList<MetadataConsensusContribution>
            contributions,
        IReadOnlyList<MetadataConsensusContributionGroup>
            groups,
        IReadOnlyList<MetadataConsensusFieldResult>
            fields)
    {
        List<string> reasons =
            new();

        reasons.Add(
            $"Se procesaron {evaluations.Count} " +
            "candidato(s) evaluado(s).");

        reasons.Add(
            $"Se generaron {contributions.Count} " +
            "contribución(es) utilizables.");

        reasons.Add(
            $"Las contribuciones se organizaron en " +
            $"{groups.Count} grupo(s) equivalentes.");

        int selectedFields =
            fields.Count(
                field =>
                    field.HasSelectedValue);

        int conflicts =
            fields.Count(
                field =>
                    field.HasConflict);

        int singleSourceFields =
            fields.Count(
                field =>
                    field.Status ==
                    MetadataConsensusStatus.SingleSource);

        int consensusFields =
            fields.Count(
                field =>
                    field.Status ==
                        MetadataConsensusStatus.ConsensusReached ||
                    field.Status ==
                        MetadataConsensusStatus.MajorityReached);

        reasons.Add(
            $"Campos con valor seleccionado: " +
            $"{selectedFields}.");

        reasons.Add(
            $"Campos con consenso entre fuentes: " +
            $"{consensusFields}.");

        reasons.Add(
            $"Campos respaldados por una sola fuente: " +
            $"{singleSourceFields}.");

        reasons.Add(
            $"Conflictos sin resolver: " +
            $"{conflicts}.");

        if (evaluations.Count == 1)
        {
            reasons.Add(
                "Sólo existe un candidato externo evaluado; " +
                "los valores seleccionados no constituyen " +
                "todavía consenso entre plataformas.");
        }

        return reasons;
    }
}