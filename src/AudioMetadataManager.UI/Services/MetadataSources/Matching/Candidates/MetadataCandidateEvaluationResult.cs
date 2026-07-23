using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Candidates;

/// <summary>
/// Contiene la evaluación completa de un candidato externo
/// frente a la identidad local de una pista.
///
/// Conserva el candidato original, la comparación técnica
/// y la evaluación de confianza para permitir ranking,
/// diagnósticos y revisión manual.
/// </summary>
public sealed class MetadataCandidateEvaluationResult
{
    /// <summary>
    /// Candidato externo que fue evaluado.
    /// </summary>
    public MetadataCandidate Candidate { get; init; } =
        new();

    /// <summary>
    /// Resultado campo por campo generado por el motor
    /// de comparación.
    /// </summary>
    public MetadataComparisonResult Comparison { get; init; } =
        new();

    /// <summary>
    /// Resultado ponderado producido por el motor
    /// de confianza.
    /// </summary>
    public MetadataConfidenceResult Confidence { get; init; } =
        new();

    /// <summary>
    /// Posición original entregada por la plataforma.
    /// </summary>
    public int OriginalSourceRank =>
        Candidate.SourceRank;

    /// <summary>
    /// Prioridad de la decisión para ordenar candidatos.
    ///
    /// Una coincidencia aceptada siempre debe situarse antes que
    /// otra que requiera revisión, aunque su similitud técnica
    /// entre los campos comparables sea ligeramente inferior.
    /// </summary>
    public int DecisionPriority =>
        Confidence.Decision switch
        {
            MetadataComparisonDecision.Accepted =>
                50,

            MetadataComparisonDecision.AcceptedWithReview =>
                40,

            MetadataComparisonDecision.ManualReviewRequired =>
                30,

            MetadataComparisonDecision.InsufficientData =>
                20,

            MetadataComparisonDecision.Rejected =>
                10,

            _ =>
                0
        };

    /// <summary>
    /// Puntuación principal utilizada para ordenar candidatos.
    ///
    /// La confianza global ya incorpora similitud,
    /// cobertura, pesos y reglas críticas.
    /// </summary>
    public double RankingScore =>
        Math.Clamp(
            Confidence.ConfidenceScore,
            0,
            1);

    /// <summary>
    /// Confianza preparada para mostrar en la interfaz.
    /// </summary>
    public string RankingScoreDisplay =>
        $"{RankingScore * 100:0.00}%";

    /// <summary>
    /// Indica si el candidato requiere revisión manual.
    /// </summary>
    public bool RequiresManualReview =>
        Confidence.RequiresManualReview;

    /// <summary>
    /// Indica si existe algún conflicto técnico entre
    /// la identidad local y el candidato externo.
    /// </summary>
    public bool HasConflicts =>
        Comparison.HasConflicts;

    /// <summary>
    /// Indica si el candidato contiene la identidad mínima
    /// necesaria para ser evaluado.
    /// </summary>
    public bool IsUsable =>
        Candidate.HasIdentity;

    /// <summary>
    /// Nombre completo preparado para registros e interfaz.
    /// </summary>
    public string DisplayName =>
        Candidate.DisplayName;

    /// <summary>
    /// Procedencia preparada para registros e interfaz.
    /// </summary>
    public string SourceDisplay =>
        Candidate.SourceDisplay;

    /// <summary>
    /// Resumen compacto de la evaluación.
    /// </summary>
    public string Summary =>
        $"{SourceDisplay}: {DisplayName}. " +
        $"Confianza: {RankingScoreDisplay}. " +
        $"Conflictos: {(HasConflicts ? "Sí" : "No")}. " +
        $"Revisión manual: " +
        $"{(RequiresManualReview ? "Sí" : "No")}.";
}