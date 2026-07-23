namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Candidates;

/// <summary>
/// Representa el resultado completo de evaluar y ordenar una
/// colección de candidatos externos.
/// </summary>
public sealed class MetadataCandidateEvaluationBatchResult
{
    /// <summary>
    /// Evaluaciones ordenadas de mejor a peor coincidencia.
    /// </summary>
    public IReadOnlyList<MetadataCandidateEvaluationResult>
        Evaluations
    { get; init; } =
            Array.Empty<MetadataCandidateEvaluationResult>();

    /// <summary>
    /// Candidato mejor clasificado, cuando existe.
    /// </summary>
    public MetadataCandidateEvaluationResult? BestCandidate =>
        Evaluations.FirstOrDefault();

    /// <summary>
    /// Cantidad de candidatos evaluados.
    /// </summary>
    public int EvaluatedCandidateCount =>
        Evaluations.Count;

    /// <summary>
    /// Indica si existe al menos un candidato evaluado.
    /// </summary>
    public bool HasEvaluations =>
        EvaluatedCandidateCount > 0;

    /// <summary>
    /// Cantidad de candidatos que requieren revisión manual.
    /// </summary>
    public int ManualReviewCount =>
        Evaluations.Count(
            evaluation =>
                evaluation.RequiresManualReview);

    /// <summary>
    /// Indica si el mejor candidato todavía requiere revisión.
    /// </summary>
    public bool BestCandidateRequiresManualReview =>
        BestCandidate?.RequiresManualReview ??
        false;

    /// <summary>
    /// Resumen compacto del lote.
    /// </summary>
    public string Summary
    {
        get
        {
            if (BestCandidate is null)
            {
                return
                    "No existen candidatos utilizables para evaluar.";
            }

            return
                $"Candidatos evaluados: " +
                $"{EvaluatedCandidateCount}. " +
                $"Mejor coincidencia: " +
                $"{BestCandidate.DisplayName}. " +
                $"Confianza: " +
                $"{BestCandidate.RankingScoreDisplay}. " +
                $"Revisión manual: " +
                $"{(BestCandidateRequiresManualReview ? "Sí" : "No")}.";
        }
    }
}