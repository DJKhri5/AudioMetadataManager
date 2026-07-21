namespace AudioMetadataManager.UI.Services.MetadataSources.Models;

/// <summary>
/// Describe de forma común el resultado de una operación
/// realizada mediante cualquier fuente externa de metadatos.
/// </summary>
public enum MetadataSourceStatus
{
    /// <summary>
    /// La operación todavía no se ha ejecutado.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// La búsqueda terminó correctamente y encontró candidatos.
    /// </summary>
    Success = 1,

    /// <summary>
    /// La búsqueda terminó correctamente, pero no encontró
    /// candidatos utilizables.
    /// </summary>
    NoResults = 2,

    /// <summary>
    /// La solicitud no contiene información suficiente.
    /// </summary>
    InvalidRequest = 3,

    /// <summary>
    /// La fuente no está configurada correctamente.
    /// </summary>
    InvalidConfiguration = 4,

    /// <summary>
    /// La fuente requiere credenciales o autenticación.
    /// </summary>
    AuthenticationRequired = 5,

    /// <summary>
    /// Las credenciales configuradas fueron rechazadas.
    /// </summary>
    AuthenticationFailed = 6,

    /// <summary>
    /// Se alcanzó temporalmente el límite de solicitudes.
    /// </summary>
    RateLimited = 7,

    /// <summary>
    /// Ocurrió un error de conexión o transporte.
    /// </summary>
    NetworkError = 8,

    /// <summary>
    /// La respuesta externa no pudo interpretarse.
    /// </summary>
    InvalidResponse = 9,

    /// <summary>
    /// La operación fue cancelada.
    /// </summary>
    Cancelled = 10,

    /// <summary>
    /// Ocurrió un error no clasificado.
    /// </summary>
    UnexpectedError = 11
}