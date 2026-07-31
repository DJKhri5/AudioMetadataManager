namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Models;

/// <summary>
/// Contiene el resultado completo de una búsqueda realizada
/// mediante Spotify.
/// </summary>
public sealed class SpotifySearchResult
{
    /// <summary>
    /// Estado general de la operación.
    /// </summary>
    public SpotifyProviderStatus Status { get; init; } =
        SpotifyProviderStatus.Unknown;

    /// <summary>
    /// Solicitud que originó este resultado.
    /// </summary>
    public SpotifySearchRequest? Request { get; init; }

    /// <summary>
    /// Candidatos normalizados obtenidos.
    /// </summary>
    public IReadOnlyList<SpotifySearchCandidate>
        Candidates
    { get; init; } =
            Array.Empty<SpotifySearchCandidate>();

    /// <summary>
    /// Cantidad total de resultados reportada por Spotify.
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
    /// Momento UTC en que se produjo el resultado.
    /// </summary>
    public DateTimeOffset RetrievedAtUtc { get; init; } =
        DateTimeOffset.UtcNow;

    /// <summary>
    /// Indica si la operación terminó correctamente.
    /// </summary>
    public bool IsSuccess =>
        Status == SpotifyProviderStatus.Success ||
        Status == SpotifyProviderStatus.NoResults;

    /// <summary>
    /// Indica si existen candidatos utilizables.
    /// </summary>
    public bool HasCandidates =>
        Candidates.Count > 0;

    /// <summary>
    /// Construye un resultado para una solicitud inválida.
    /// </summary>
    public static SpotifySearchResult InvalidRequest(
        SpotifySearchRequest? request,
        string message)
    {
        return new SpotifySearchResult
        {
            Status =
                SpotifyProviderStatus.InvalidRequest,
            Request =
                request,
            Message =
                message
        };
    }

    /// <summary>
    /// Construye un resultado para una configuración inválida.
    /// </summary>
    public static SpotifySearchResult InvalidConfiguration(
        SpotifySearchRequest? request,
        string message)
    {
        return new SpotifySearchResult
        {
            Status =
                SpotifyProviderStatus.InvalidConfiguration,
            Request =
                request,
            Message =
                message
        };
    }
}
