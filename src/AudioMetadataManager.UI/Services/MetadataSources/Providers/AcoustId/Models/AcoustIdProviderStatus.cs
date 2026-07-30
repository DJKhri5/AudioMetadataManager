namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.AcoustId.Models;

/// <summary>
/// Describe el resultado general de una operación realizada
/// mediante el proveedor AcoustID.
/// </summary>
public enum AcoustIdProviderStatus
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
    /// La consulta terminó correctamente, pero no encontró
    /// grabaciones asociadas a la huella.
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
    /// AcoustID rechazó la clave de cliente proporcionada.
    /// </summary>
    AuthenticationFailed = 5,

    /// <summary>
    /// Se alcanzó temporalmente un límite de solicitudes.
    /// </summary>
    RateLimited = 6,

    /// <summary>
    /// Ocurrió un problema de conexión o transporte.
    /// </summary>
    NetworkError = 7,

    /// <summary>
    /// La respuesta recibida no pudo interpretarse.
    /// </summary>
    InvalidResponse = 8,

    /// <summary>
    /// Ocurrió un error no clasificado.
    /// </summary>
    UnexpectedError = 9
}
