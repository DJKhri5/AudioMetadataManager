namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Models;

/// <summary>
/// Describe el resultado general de una operación realizada
/// mediante el proveedor Discogs.
/// </summary>
public enum DiscogsProviderStatus
{
    /// <summary>
    /// La operación todavía no fue ejecutada.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// La operación terminó correctamente.
    /// </summary>
    Success = 1,

    /// <summary>
    /// La búsqueda terminó correctamente, pero no encontró
    /// candidatos.
    /// </summary>
    NoResults = 2,

    /// <summary>
    /// La solicitud no contiene información suficiente.
    /// </summary>
    InvalidRequest = 3,

    /// <summary>
    /// La configuración del proveedor no es válida.
    /// </summary>
    InvalidConfiguration = 4,

    /// <summary>
    /// La operación requiere autenticación.
    /// </summary>
    AuthenticationRequired = 5,

    /// <summary>
    /// Discogs rechazó las credenciales proporcionadas.
    /// </summary>
    AuthenticationFailed = 6,

    /// <summary>
    /// Se alcanzó temporalmente un límite de solicitudes.
    /// </summary>
    RateLimited = 7,

    /// <summary>
    /// Ocurrió un problema de conexión o transporte.
    /// </summary>
    NetworkError = 8,

    /// <summary>
    /// La respuesta recibida no pudo interpretarse.
    /// </summary>
    InvalidResponse = 9,

    /// <summary>
    /// Ocurrió un error no clasificado.
    /// </summary>
    UnexpectedError = 10
}