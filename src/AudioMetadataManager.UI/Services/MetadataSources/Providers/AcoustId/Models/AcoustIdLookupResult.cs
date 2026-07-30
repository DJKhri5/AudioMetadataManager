namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Models;

/// <summary>
/// Contiene el resultado completo de una consulta realizada
/// mediante AcoustID.
/// </summary>
public sealed class AcoustIdLookupResult
{
    /// <summary>
    /// Estado general de la operación.
    /// </summary>
    public AcoustIdProviderStatus Status { get; init; } =
        AcoustIdProviderStatus.Unknown;

    /// <summary>
    /// Solicitud que originó este resultado.
    /// </summary>
    public AcoustIdLookupRequest? Request { get; init; }

    /// <summary>
    /// Grabaciones normalizadas obtenidas, ordenadas según
    /// la confianza informada por AcoustID.
    /// </summary>
    public IReadOnlyList<AcoustIdRecordingCandidate>
        Candidates
    { get; init; } =
            Array.Empty<AcoustIdRecordingCandidate>();

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
        Status == AcoustIdProviderStatus.Success ||
        Status == AcoustIdProviderStatus.NoResults;

    /// <summary>
    /// Indica si existen grabaciones utilizables.
    /// </summary>
    public bool HasCandidates =>
        Candidates.Count > 0;

    /// <summary>
    /// Mejor coincidencia según la confianza informada
    /// por AcoustID.
    /// </summary>
    public AcoustIdRecordingCandidate? BestCandidate =>
        Candidates
            .OrderByDescending(candidate => candidate.Score)
            .FirstOrDefault();

    /// <summary>
    /// Construye un resultado para una solicitud inválida.
    /// </summary>
    public static AcoustIdLookupResult InvalidRequest(
        AcoustIdLookupRequest? request,
        string message)
    {
        return new AcoustIdLookupResult
        {
            Status =
                AcoustIdProviderStatus.InvalidRequest,
            Request =
                request,
            Message =
                message
        };
    }

    /// <summary>
    /// Construye un resultado para una configuración inválida.
    /// </summary>
    public static AcoustIdLookupResult InvalidConfiguration(
        AcoustIdLookupRequest? request,
        string message)
    {
        return new AcoustIdLookupResult
        {
            Status =
                AcoustIdProviderStatus.InvalidConfiguration,
            Request =
                request,
            Message =
                message
        };
    }
}
