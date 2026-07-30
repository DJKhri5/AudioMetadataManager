namespace AudioMetadataManager.UI.Services.Artwork.Models;

/// <summary>
/// Solicitud para obtener e incrustar la carátula de una pista a
/// partir de una dirección propuesta por una fuente externa de
/// metadatos.
/// </summary>
public sealed class TrackArtworkRequest
{
    /// <summary>
    /// Ruta del archivo de audio a modificar.
    /// </summary>
    public string FilePath { get; init; } =
        string.Empty;

    /// <summary>
    /// Ruta de un respaldo ya creado y verificado del archivo
    /// original.
    /// </summary>
    public string VerifiedBackupPath { get; init; } =
        string.Empty;

    /// <summary>
    /// Dirección de la imagen de carátula a descargar.
    /// </summary>
    public string ArtworkUrl { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si la nueva carátula debe reemplazar todas las
    /// imágenes existentes.
    /// </summary>
    public bool ReplaceExisting { get; init; } =
        true;

    /// <summary>
    /// Indica si la solicitud contiene todos los datos
    /// obligatorios.
    /// </summary>
    public bool IsStructurallyValid =>
        !string.IsNullOrWhiteSpace(FilePath) &&
        !string.IsNullOrWhiteSpace(VerifiedBackupPath) &&
        !string.IsNullOrWhiteSpace(ArtworkUrl);
}
