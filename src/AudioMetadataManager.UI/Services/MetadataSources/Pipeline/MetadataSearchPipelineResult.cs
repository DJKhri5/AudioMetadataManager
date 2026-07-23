using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Pipeline.Strategy;
using AudioMetadataManager.UI.Services.MetadataSources.Pipeline;
using AudioMetadataManager.UI.Services.MetadataSources.Pipeline.Execution;


namespace AudioMetadataManager.UI.Services.MetadataSources.Pipeline;

/// <summary>
/// Contiene el resultado completo de una ejecución del pipeline
/// de búsqueda de metadatos.
/// </summary>
public sealed class MetadataSearchPipelineResult
{
    /// <summary>
    /// Contexto que originó esta ejecución.
    /// </summary>
    public MetadataSearchContext Context { get; init; } =
        new();

    /// <summary>
    /// Resultados individuales entregados por las fuentes.
    /// </summary>
    public IReadOnlyList<MetadataSearchResult>
        SourceResults
    { get; init; } =
            Array.Empty<MetadataSearchResult>();

    /// <summary>
    /// Intentos ejecutados por la estrategia, en su orden real.
    /// </summary>
    public IReadOnlyList<MetadataSearchAttempt>
        Attempts
    { get; init; } =
            Array.Empty<MetadataSearchAttempt>();

    /// <summary>
    /// Razón por la que se detuvo la ejecución escalonada.
    /// </summary>
    public MetadataSearchStopReason StopReason { get; init; } =
        MetadataSearchStopReason.None;

    /// <summary>
    /// Momento UTC en que comenzó la ejecución.
    /// </summary>
    public DateTimeOffset StartedAtUtc { get; init; }

    /// <summary>
    /// Momento UTC en que terminó la ejecución.
    /// </summary>
    public DateTimeOffset CompletedAtUtc { get; init; }

    /// <summary>
    /// Duración total del pipeline.
    /// </summary>
    public TimeSpan ElapsedTime { get; init; }

    /// <summary>
    /// Indica si el pipeline pudo ejecutar su flujo general.
    ///
    /// Una fuente individual todavía puede haber fallado.
    /// </summary>
    public bool ExecutionSucceeded { get; init; }

    /// <summary>
    /// Error general del pipeline, cuando exista.
    /// </summary>
    public string ErrorMessage { get; init; } =
        string.Empty;

    /// <summary>
    /// Todos los candidatos utilizables obtenidos desde las
    /// diferentes plataformas.
    ///
    /// Se conserva primero el orden de las fuentes y después
    /// el orden original informado por cada una.
    /// </summary>
    public IReadOnlyList<MetadataCandidate> Candidates =>
        Attempts.Count > 0
            ? Attempts
                .SelectMany(
                    attempt =>
                        attempt.Candidates)
                .Where(
                    candidate =>
                        candidate.HasIdentity)
                .ToArray()
            : SourceResults
                .SelectMany(
                    result =>
                        result.Candidates)
                .Where(
                    candidate =>
                        candidate.HasIdentity)
                .ToArray();

    /// <summary>
    /// Fuentes cuya operación terminó correctamente,
    /// tengan o no candidatos.
    /// </summary>
    public IReadOnlyList<MetadataSearchResult>
        SuccessfulSourceResults =>
            SourceResults
                .Where(
                    result =>
                        result.WasSuccessful)
                .ToArray();

    /// <summary>
    /// Fuentes que terminaron con error.
    /// </summary>
    public IReadOnlyList<MetadataSearchResult>
        FailedSourceResults =>
            SourceResults
                .Where(
                    result =>
                        result.HasError)
                .ToArray();

    /// <summary>
    /// Cantidad de fuentes procesadas.
    /// </summary>
    public int ProcessedSourceCount =>
        SourceResults.Count;

    /// <summary>
    /// Cantidad de fuentes que respondieron correctamente.
    /// </summary>
    public int SuccessfulSourceCount =>
        SuccessfulSourceResults.Count;

    /// <summary>
    /// Cantidad de fuentes que terminaron con error.
    /// </summary>
    public int FailedSourceCount =>
        FailedSourceResults.Count;

    /// <summary>
    /// Cantidad total de candidatos utilizables.
    /// </summary>
    public int CandidateCount =>
        Candidates.Count;

    /// <summary>
    /// Cantidad de variantes que fueron ejecutadas.
    /// </summary>
    public int AttemptCount =>
        Attempts.Count;

    /// <summary>
    /// Último intento realizado.
    /// </summary>
    public MetadataSearchAttempt? LastAttempt =>
        Attempts.LastOrDefault();

    /// <summary>
    /// Consulta que finalmente produjo candidatos.
    /// </summary>
    public MetadataSearchQuery? SuccessfulQuery =>
        Attempts
            .FirstOrDefault(
                attempt =>
                    attempt.HasCandidates)
            ?.Query;

    /// <summary>
    /// Indica si existe al menos un candidato utilizable.
    /// </summary>
    public bool HasCandidates =>
        CandidateCount > 0;

    /// <summary>
    /// Indica si existe al menos una fuente que requiere
    /// aprobación manual para sus resultados.
    /// </summary>
    public bool ContainsManualApprovalCandidates =>
        SourceResults.Any(
            result =>
                result.RequiresManualApproval &&
                result.HasCandidates);

    /// <summary>
    /// Indica si ocurrió un fallo general del pipeline.
    /// </summary>
    public bool HasPipelineError =>
        !ExecutionSucceeded ||
        !string.IsNullOrWhiteSpace(
            ErrorMessage);

    /// <summary>
    /// Resumen legible de la ejecución.
    /// </summary>
    public string Summary
    {
        get
        {
            if (HasPipelineError)
            {
                return string.IsNullOrWhiteSpace(
                    ErrorMessage)
                        ? "El pipeline de metadatos no pudo completarse."
                        : ErrorMessage;
            }

            return
                $"Intentos ejecutados: {AttemptCount}. " +
                $"Fuentes procesadas: {ProcessedSourceCount}. " +
                $"Fuentes correctas: {SuccessfulSourceCount}. " +
                $"Fuentes con error: {FailedSourceCount}. " +
                $"Candidatos utilizables: {CandidateCount}. " +
                $"Detención: {StopReason}. " +
                $"Duración total: {ElapsedTime.TotalMilliseconds:0} ms.";
        }
    }
}