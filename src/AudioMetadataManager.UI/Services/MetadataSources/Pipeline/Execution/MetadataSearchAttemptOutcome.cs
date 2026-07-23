namespace AudioMetadataManager.UI.Services.MetadataSources
    .Pipeline.Execution;

/// <summary>
/// Describe el resultado general de un intento individual
/// ejecutado por el pipeline.
/// </summary>
public enum MetadataSearchAttemptOutcome
{
    /// <summary>
    /// El intento todavía no ha sido evaluado.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// El intento terminó correctamente y encontró candidatos.
    /// </summary>
    CandidatesFound = 1,

    /// <summary>
    /// El intento terminó correctamente, pero ninguna fuente
    /// encontró candidatos utilizables.
    /// </summary>
    NoCandidates = 2,

    /// <summary>
    /// Una o más fuentes no estaban configuradas.
    /// </summary>
    SourcesUnavailable = 3,

    /// <summary>
    /// La solicitud generada no fue aceptada.
    /// </summary>
    InvalidRequest = 4,

    /// <summary>
    /// Una fuente requiere autenticación o rechazó
    /// las credenciales configuradas.
    /// </summary>
    AuthenticationFailure = 5,

    /// <summary>
    /// Se alcanzó el límite temporal de solicitudes.
    /// </summary>
    RateLimited = 6,

    /// <summary>
    /// Ocurrió un problema de red, transporte o timeout.
    /// </summary>
    NetworkFailure = 7,

    /// <summary>
    /// Una respuesta externa no pudo interpretarse.
    /// </summary>
    InvalidResponse = 8,

    /// <summary>
    /// Ocurrió un error inesperado.
    /// </summary>
    UnexpectedFailure = 9
}