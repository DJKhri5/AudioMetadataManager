namespace AudioMetadataManager.UI.Services.Artwork.Models;

/// <summary>
/// Describe el resultado general de una descarga de imagen de
/// carátula.
/// </summary>
public enum ArtworkDownloadStatus
{
    /// <summary>
    /// La operación todavía no fue ejecutada.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// La imagen se descargó correctamente.
    /// </summary>
    Success = 1,

    /// <summary>
    /// La solicitud no contiene una URL válida.
    /// </summary>
    InvalidRequest = 2,

    /// <summary>
    /// Ocurrió un problema de conexión o el servidor respondió
    /// con un estado HTTP de error.
    /// </summary>
    NetworkError = 3,

    /// <summary>
    /// El servidor respondió con un tipo de contenido no admitido
    /// como imagen de carátula.
    /// </summary>
    UnexpectedContentType = 4,

    /// <summary>
    /// La imagen supera el tamaño máximo configurado.
    /// </summary>
    TooLarge = 5,

    /// <summary>
    /// El servidor respondió correctamente, pero sin contenido.
    /// </summary>
    EmptyContent = 6,

    /// <summary>
    /// La operación fue cancelada por el usuario.
    /// </summary>
    Cancelled = 7,

    /// <summary>
    /// Ocurrió un error no clasificado.
    /// </summary>
    UnexpectedError = 8
}
