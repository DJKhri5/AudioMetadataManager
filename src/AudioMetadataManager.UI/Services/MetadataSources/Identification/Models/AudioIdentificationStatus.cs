namespace AudioMetadataManager.UI.Services.MetadataSources
    .Identification.Models;

/// <summary>
/// Describe el resultado general de un intento de identificación
/// automática de una pista, combinando Chromaprint y AcoustID.
/// </summary>
public enum AudioIdentificationStatus
{
    /// <summary>
    /// La operación todavía no fue ejecutada.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Se generó la huella y AcoustID devolvió al menos una
    /// grabación utilizable.
    /// </summary>
    Success = 1,

    /// <summary>
    /// Se generó la huella correctamente, pero AcoustID no
    /// encontró grabaciones asociadas.
    /// </summary>
    NoMatchFound = 2,

    /// <summary>
    /// La solicitud no contiene una ruta de archivo utilizable.
    /// </summary>
    InvalidRequest = 3,

    /// <summary>
    /// Falló la generación local de la huella acústica.
    /// </summary>
    FingerprintFailed = 4,

    /// <summary>
    /// La huella se generó correctamente, pero la consulta a
    /// AcoustID falló.
    /// </summary>
    LookupFailed = 5,

    /// <summary>
    /// La operación fue cancelada por el usuario.
    /// </summary>
    Cancelled = 6
}
