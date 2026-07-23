namespace AudioMetadataManager.UI.Services.MetadataSources
    .Pipeline.Execution;

/// <summary>
/// Explica por qué el pipeline dejó de ejecutar variantes
/// adicionales de búsqueda.
/// </summary>
public enum MetadataSearchStopReason
{
    /// <summary>
    /// Todavía no existe una razón de detención.
    /// </summary>
    None = 0,

    /// <summary>
    /// Se encontraron candidatos utilizables.
    /// </summary>
    CandidatesFound = 1,

    /// <summary>
    /// Se ejecutaron todas las consultas disponibles.
    /// </summary>
    QueriesExhausted = 2,

    /// <summary>
    /// La estrategia no produjo consultas válidas.
    /// </summary>
    NoValidQueries = 3,

    /// <summary>
    /// Se detectó un problema de autenticación.
    /// </summary>
    AuthenticationFailure = 4,

    /// <summary>
    /// Se alcanzó el límite del servicio externo.
    /// </summary>
    RateLimited = 5,

    /// <summary>
    /// Se produjo un problema de red o transporte.
    /// </summary>
    NetworkFailure = 6,

    /// <summary>
    /// La fuente devolvió una respuesta inválida.
    /// </summary>
    InvalidResponse = 7,

    /// <summary>
    /// Ocurrió un error inesperado que impide continuar.
    /// </summary>
    UnexpectedFailure = 8
}