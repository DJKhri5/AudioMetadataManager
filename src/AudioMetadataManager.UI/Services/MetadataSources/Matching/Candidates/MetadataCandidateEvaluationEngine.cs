using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Engine;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Candidates;

/// <summary>
/// Evalúa candidatos externos frente a una identidad local
/// normalizada y los ordena según la calidad de coincidencia.
///
/// El motor no modifica archivos, no aplica metadatos y no
/// selecciona automáticamente un resultado definitivo.
/// </summary>
public sealed class MetadataCandidateEvaluationEngine
{
    private readonly MetadataComparisonEngine
        _comparisonEngine;

    private readonly MetadataConfidenceEngine
        _confidenceEngine;

    private readonly MetadataCandidateComparisonAdapter
        _candidateAdapter;

    /// <summary>
    /// Crea el motor con todos sus componentes predeterminados.
    /// </summary>
    public MetadataCandidateEvaluationEngine()
        : this(
            new MetadataComparisonEngine(),
            new MetadataConfidenceEngine(),
            new MetadataCandidateComparisonAdapter())
    {
    }

    /// <summary>
    /// Crea el motor utilizando componentes personalizados.
    ///
    /// Este constructor permite pruebas automatizadas y futuras
    /// configuraciones específicas de comparación o confianza.
    /// </summary>
    public MetadataCandidateEvaluationEngine(
        MetadataComparisonEngine comparisonEngine,
        MetadataConfidenceEngine confidenceEngine,
        MetadataCandidateComparisonAdapter candidateAdapter)
    {
        _comparisonEngine =
            comparisonEngine ??
            throw new ArgumentNullException(
                nameof(comparisonEngine));

        _confidenceEngine =
            confidenceEngine ??
            throw new ArgumentNullException(
                nameof(confidenceEngine));

        _candidateAdapter =
            candidateAdapter ??
            throw new ArgumentNullException(
                nameof(candidateAdapter));
    }

    /// <summary>
    /// Evalúa y ordena todos los candidatos utilizables.
    /// </summary>
    /// <param name="localMetadata">
    /// Identidad local contra la cual se compararán los
    /// candidatos externos.
    /// </param>
    /// <param name="candidates">
    /// Resultados normalizados obtenidos desde una o varias
    /// fuentes externas.
    /// </param>
    public IReadOnlyList<MetadataCandidateEvaluationResult>
        EvaluateAndRank(
            MetadataComparisonInput localMetadata,
            IEnumerable<MetadataCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(
            localMetadata);

        ArgumentNullException.ThrowIfNull(
            candidates);

        List<MetadataCandidateEvaluationResult> evaluations =
            candidates
                .Where(
                    candidate =>
                        candidate is not null &&
                        candidate.HasIdentity)
                .Select(
                    candidate =>
                        Evaluate(
                            localMetadata,
                            candidate))
                .OrderByDescending(
                    evaluation =>
                        evaluation.DecisionPriority)
                .ThenByDescending(
                    evaluation =>
                        evaluation.RankingScore)
                .ThenBy(
                    evaluation =>
                        evaluation.RequiresManualReview)
                .ThenBy(
                    evaluation =>
                        evaluation.Comparison.Conflicts)
                .ThenByDescending(
                    evaluation =>
                        evaluation.Comparison.EffectiveSimilarity)
                .ThenByDescending(
                    evaluation =>
                        evaluation.Comparison.InformationCoverage)
                .ThenBy(
                    evaluation =>
                        NormalizeSourceRank(
                            evaluation.OriginalSourceRank))
                .ThenBy(
                    evaluation =>
                        evaluation.SourceDisplay,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        return evaluations;
    }

    /// <summary>
    /// Evalúa un único candidato externo.
    /// </summary>
    public MetadataCandidateEvaluationResult Evaluate(
        MetadataComparisonInput localMetadata,
        MetadataCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(
            localMetadata);

        ArgumentNullException.ThrowIfNull(
            candidate);

        if (!candidate.HasIdentity)
        {
            throw new ArgumentException(
                "El candidato debe contener como mínimo " +
                "un artista y un título utilizables.",
                nameof(candidate));
        }

        MetadataComparisonInput referenceMetadata =
            _candidateAdapter.CreateInput(
                candidate);

        MetadataComparisonResult comparison =
            _comparisonEngine.CompareMetadata(
                localMetadata,
                referenceMetadata);

        var confidence =
            _confidenceEngine.Evaluate(
                comparison);

        return new MetadataCandidateEvaluationResult
        {
            Candidate =
                candidate,

            Comparison =
                comparison,

            Confidence =
                confidence
        };
    }

    /// <summary>
    /// Evalúa una colección y la encapsula en un resultado de lote.
    /// </summary>
    public MetadataCandidateEvaluationBatchResult EvaluateBatch(
        MetadataComparisonInput localMetadata,
        IEnumerable<MetadataCandidate> candidates)
    {
        IReadOnlyList<MetadataCandidateEvaluationResult>
            evaluations =
                EvaluateAndRank(
                    localMetadata,
                    candidates);

        return new MetadataCandidateEvaluationBatchResult
        {
            Evaluations =
                evaluations
        };
    }

    /// <summary>
    /// Devuelve el candidato mejor clasificado, cuando existe.
    ///
    /// La existencia de un resultado no implica que pueda
    /// aplicarse automáticamente. Su decisión y estado de
    /// revisión deben comprobarse por separado.
    /// </summary>
    public MetadataCandidateEvaluationResult? GetBestCandidate(
        MetadataComparisonInput localMetadata,
        IEnumerable<MetadataCandidate> candidates)
    {
        return EvaluateAndRank(
                localMetadata,
                candidates)
            .FirstOrDefault();
    }

    /// <summary>
    /// Convierte posiciones ausentes o inválidas en un valor
    /// que las sitúa después de los rangos válidos.
    /// </summary>
    private static int NormalizeSourceRank(
        int sourceRank)
    {
        return sourceRank > 0
            ? sourceRank
            : int.MaxValue;
    }
}