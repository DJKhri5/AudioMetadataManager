namespace AudioMetadataManager.UI.Services.MetadataSources.Models;

/// <summary>
/// Representa el resultado completo de una búsqueda realizada
/// en una fuente externa de metadatos musicales.
/// </summary>
public class MetadataSearchResult
{
    /// <summary>
    /// Plataforma que realizó la búsqueda.
    /// Ejemplos: Discogs, Beatport, Spotify o SoundCloud.
    /// </summary>
    public string SourceName { get; set; } = string.Empty;

    /// <summary>
    /// Estado normalizado de la operación, independiente de la
    /// plataforma que realizó la búsqueda.
    /// </summary>
    public MetadataSourceStatus Status { get; set; } =
        MetadataSourceStatus.Unknown;

    /// <summary>
    /// Consulta exacta enviada a la plataforma.
    /// Se conserva para auditoría y diagnóstico.
    /// </summary>
    public string QueryUsed { get; set; } = string.Empty;

    /// <summary>
    /// Indica si la consulta terminó correctamente,
    /// aunque no haya encontrado candidatos.
    /// </summary>
    public bool WasSuccessful { get; set; }

    /// <summary>
    /// Mensaje de error o diagnóstico.
    /// Queda vacío cuando la búsqueda terminó correctamente.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Duración total de la consulta externa.
    /// Permitirá medir rendimiento y detectar fuentes lentas.
    /// </summary>
    public TimeSpan ElapsedTime { get; set; }

    /// <summary>
    /// Indica si los candidatos de esta fuente deben ser
    /// confirmados manualmente antes de utilizarse.
    ///
    /// SoundCloud utilizará siempre true.
    /// </summary>
    public bool RequiresManualApproval { get; set; }

    /// <summary>
    /// Código HTTP recibido desde la plataforma, cuando exista.
    /// </summary>
    public int? HttpStatusCode { get; set; }

    /// <summary>
    /// Cantidad de solicitudes restantes informada por el servicio,
    /// cuando ese dato esté disponible.
    /// </summary>
    public int? RemainingRequests { get; set; }

    /// <summary>
    /// Cantidad total de resultados comunicada por la plataforma,
    /// aunque sólo se haya descargado una página.
    /// </summary>
    public int ExternalTotalResults { get; set; }

    /// <summary>
    /// Momento UTC en que se obtuvo la respuesta externa.
    /// </summary>
    public DateTimeOffset RetrievedAtUtc { get; set; } =
        DateTimeOffset.UtcNow;

    /// <summary>
    /// Candidatos devueltos por la plataforma.
    /// </summary>
    public List<MetadataCandidate> Candidates { get; set; } = new();

    /// <summary>
    /// Indica si la búsqueda devolvió al menos un candidato
    /// con artista y título utilizables.
    /// </summary>
    public bool HasCandidates =>
        Candidates.Any(candidate => candidate.HasIdentity);

    /// <summary>
    /// Cantidad total de candidatos recibidos.
    /// </summary>
    public int CandidateCount =>
        Candidates.Count;

    /// <summary>
    /// Primer candidato válido según el orden entregado
    /// por la plataforma.
    /// Todavía no implica que sea la mejor coincidencia.
    /// </summary>
    public MetadataCandidate? FirstValidCandidate =>
        Candidates.FirstOrDefault(candidate => candidate.HasIdentity);

    /// <summary>
    /// Indica si la búsqueda terminó con un error.
    /// </summary>
    public bool HasError =>
        Status is
            MetadataSourceStatus.InvalidRequest or
            MetadataSourceStatus.InvalidConfiguration or
            MetadataSourceStatus.AuthenticationRequired or
            MetadataSourceStatus.AuthenticationFailed or
            MetadataSourceStatus.RateLimited or
            MetadataSourceStatus.NetworkError or
            MetadataSourceStatus.InvalidResponse or
            MetadataSourceStatus.UnexpectedError ||
        !WasSuccessful ||
        !string.IsNullOrWhiteSpace(ErrorMessage);

    /// <summary>
    /// Resumen legible del resultado de la consulta.
    /// </summary>
    public string Summary
    {
        get
        {
            if (HasError)
            {
                return string.IsNullOrWhiteSpace(ErrorMessage)
                    ? $"{SourceName}: la búsqueda no pudo completarse."
                    : $"{SourceName}: {ErrorMessage}";
            }

            if (!HasCandidates)
            {
                return
                    $"{SourceName}: no se encontraron coincidencias utilizables.";
            }

            return
                $"{SourceName}: {CandidateCount} candidato(s) encontrado(s).";
        }
    }
}