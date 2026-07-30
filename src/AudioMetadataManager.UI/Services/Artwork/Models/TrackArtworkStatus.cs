namespace AudioMetadataManager.UI.Services.Artwork.Models;

/// <summary>
/// Describe el resultado general de obtener e incrustar la
/// carátula de una pista, combinando descarga y escritura.
/// </summary>
public enum TrackArtworkStatus
{
    /// <summary>
    /// La operación todavía no fue ejecutada.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// La carátula se descargó e incrustó correctamente.
    /// </summary>
    Success = 1,

    /// <summary>
    /// La solicitud no contiene todos los datos obligatorios.
    /// </summary>
    InvalidRequest = 2,

    /// <summary>
    /// Falló la descarga de la imagen.
    /// </summary>
    DownloadFailed = 3,

    /// <summary>
    /// La imagen se descargó, pero no pudo incrustarse en el
    /// archivo.
    /// </summary>
    EmbedFailed = 4,

    /// <summary>
    /// La operación fue cancelada por el usuario.
    /// </summary>
    Cancelled = 5
}
