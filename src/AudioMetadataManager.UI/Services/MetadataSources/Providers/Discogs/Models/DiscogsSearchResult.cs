namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Models;

/// <summary>
/// Contiene el resultado completo de una búsqueda realizada
/// mediante Discogs.
/// </summary>
public sealed class DiscogsSearchResult
{
    /// <summary>
    /// Estado general de la operación.
    /// </summary>
    public DiscogsProviderStatus Status { get; init; } =
        DiscogsProviderStatus.Unknown;

    /// <summary>
    /// Solicitud que originó este resultado.
    /// </summary>
    public DiscogsSearchRequest? Request { get; init; }

    /// <summary>
    /// Candidatos normalizados obtenidos.
    /// </summary>
    public IReadOnlyList<DiscogsSearchCandidate>
        Candidates
    { get; init; } =
            Array.Empty<DiscogsSearchCandidate>();

    /// <summary>
    /// Número de página procesada.
    /// </summary>
    public int Page { get; init; }

    /// <summary>
    /// Cantidad total de páginas reportada.
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Cantidad total de resultados reportada.
    /// </summary>
    public int TotalResults { get; init; }

    /// <summary>
    /// Mensaje descriptivo para interfaz o diagnóstico.
    /// </summary>
    public string Message { get; init; } =
        string.Empty;

    /// <summary>
    /// Código HTTP, cuando la operación alcanzó el servidor.
    /// </summary>
    public int? HttpStatusCode { get; init; }

    /// <summary>
    /// Cantidad restante de solicitudes informada por Discogs,
    /// cuando ese dato esté disponible.
    /// </summary>
    public int? RemainingRequests { get; init; }

    /// <summary>
    /// Momento UTC en que se produjo el resultado.
    /// </summary>
    public DateTimeOffset RetrievedAtUtc { get; init; } =
        DateTimeOffset.UtcNow;

    /// <summary>
    /// Indica si la operación terminó correctamente.
    /// </summary>
    public bool IsSuccess =>
        Status == DiscogsProviderStatus.Success ||
        Status == DiscogsProviderStatus.NoResults;

    /// <summary>
    /// Indica si existen candidatos utilizables.
    /// </summary>
    public bool HasCandidates =>
        Candidates.Count > 0;

    /// <summary>
    /// Construye un resultado para una solicitud inválida.
    /// </summary>
    public static DiscogsSearchResult InvalidRequest(
        DiscogsSearchRequest? request,
        string message)
    {
        return new DiscogsSearchResult
        {
            Status =
                DiscogsProviderStatus.InvalidRequest,
            Request =
                request,
            Message =
                message
        };
    }

    /// <summary>
    /// Construye un resultado para una configuración inválida.
    /// </summary>
    public static DiscogsSearchResult InvalidConfiguration(
        DiscogsSearchRequest? request,
        string message)
    {
        return new DiscogsSearchResult
        {
            Status =
                DiscogsProviderStatus.InvalidConfiguration,
            Request =
                request,
            Message =
                message
        };
    }
}