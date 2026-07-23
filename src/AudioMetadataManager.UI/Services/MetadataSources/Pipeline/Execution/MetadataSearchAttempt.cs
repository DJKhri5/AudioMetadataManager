using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Pipeline.Strategy;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Pipeline.Execution;

/// <summary>
/// Registra un intento concreto realizado por el pipeline.
///
/// Contiene la consulta utilizada, los resultados de las
/// fuentes, la duración y la conclusión del intento.
/// </summary>
public sealed class MetadataSearchAttempt
{
    /// <summary>
    /// Número correlativo del intento dentro de la ejecución.
    /// </summary>
    public int AttemptNumber { get; init; }

    /// <summary>
    /// Variante de búsqueda ejecutada.
    /// </summary>
    public MetadataSearchQuery Query { get; init; } =
        new();

    /// <summary>
    /// Resultados entregados por las fuentes registradas.
    /// </summary>
    public IReadOnlyList<MetadataSearchResult>
        SourceResults
    { get; init; } =
            Array.Empty<MetadataSearchResult>();

    /// <summary>
    /// Momento UTC en que comenzó el intento.
    /// </summary>
    public DateTimeOffset StartedAtUtc { get; init; }

    /// <summary>
    /// Momento UTC en que terminó el intento.
    /// </summary>
    public DateTimeOffset CompletedAtUtc { get; init; }

    /// <summary>
    /// Duración total del intento.
    /// </summary>
    public TimeSpan ElapsedTime { get; init; }

    /// <summary>
    /// Resultado general del intento.
    /// </summary>
    public MetadataSearchAttemptOutcome Outcome { get; init; } =
        MetadataSearchAttemptOutcome.Unknown;

    /// <summary>
    /// Explicación legible del resultado.
    /// </summary>
    public string Message { get; init; } =
        string.Empty;

    /// <summary>
    /// Candidatos utilizables encontrados durante el intento.
    /// </summary>
    public IReadOnlyList<MetadataCandidate> Candidates =>
        SourceResults
            .SelectMany(
                result =>
                    result.Candidates)
            .Where(
                candidate =>
                    candidate.HasIdentity)
            .ToArray();

    /// <summary>
    /// Cantidad de candidatos utilizables.
    /// </summary>
    public int CandidateCount =>
        Candidates.Count;

    /// <summary>
    /// Indica si el intento encontró candidatos.
    /// </summary>
    public bool HasCandidates =>
        CandidateCount > 0;

    /// <summary>
    /// Indica si este resultado justifica detener el pipeline.
    /// </summary>
    public bool IsBlockingFailure =>
        Outcome is
            MetadataSearchAttemptOutcome.AuthenticationFailure or
            MetadataSearchAttemptOutcome.RateLimited or
            MetadataSearchAttemptOutcome.NetworkFailure or
            MetadataSearchAttemptOutcome.InvalidResponse or
            MetadataSearchAttemptOutcome.UnexpectedFailure;

    /// <summary>
    /// Resumen preparado para diagnósticos.
    /// </summary>
    public string Summary =>
        $"Intento {AttemptNumber}: " +
        $"{Query.DisplayText}. " +
        $"Resultado: {Outcome}. " +
        $"Candidatos: {CandidateCount}. " +
        $"Duración: {ElapsedTime.TotalMilliseconds:0} ms.";
}