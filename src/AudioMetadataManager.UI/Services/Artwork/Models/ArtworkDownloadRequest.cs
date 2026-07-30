namespace AudioMetadataManager.UI.Services.Artwork.Models;

/// <summary>
/// Solicitud para descargar una imagen de carátula desde una
/// dirección propuesta por una fuente externa de metadatos.
/// </summary>
public sealed class ArtworkDownloadRequest
{
    /// <summary>
    /// Dirección de la imagen a descargar.
    /// </summary>
    public string ArtworkUrl { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si la solicitud contiene una dirección utilizable.
    /// </summary>
    public bool HasArtworkUrl =>
        !string.IsNullOrWhiteSpace(
            ArtworkUrl);
}
