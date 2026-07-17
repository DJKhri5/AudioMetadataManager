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