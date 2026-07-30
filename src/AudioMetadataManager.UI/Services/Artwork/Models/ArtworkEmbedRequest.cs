namespace AudioMetadataManager.UI.Services.Artwork.Models;

/// <summary>
/// Solicitud para incrustar una imagen de carátula en un archivo
/// de audio local mediante TagLibSharp.
///
/// Exige un respaldo ya verificado del archivo original, siguiendo
/// el mismo principio de seguridad que el resto del pipeline de
/// escritura de metadatos: nunca se escribe sin una copia previa.
/// </summary>
public sealed class ArtworkEmbedRequest
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
    /// Bytes de la imagen a incrustar.
    /// </summary>
    public byte[] ImageBytes { get; init; } =
        Array.Empty<byte>();

    /// <summary>
    /// Tipo de contenido de la imagen (por ejemplo, "image/jpeg").
    /// </summary>
    public string MimeType { get; init; } =
        string.Empty;

    /// <summary>
    /// Descripción opcional de la imagen.
    /// </summary>
    public string Description { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si la nueva carátula debe reemplazar todas las
    /// imágenes existentes.
    ///
    /// Cuando es falso, la imagen se agrega junto a las existentes.
    /// </summary>
    public bool ReplaceExisting { get; init; } =
        true;

    /// <summary>
    /// Indica si la solicitud contiene una imagen utilizable.
    /// </summary>
    public bool HasImageBytes =>
        ImageBytes.Length > 0;

    /// <summary>
    /// Indica si la solicitud contiene todos los datos
    /// obligatorios para una incrustación segura.
    /// </summary>
    public bool IsStructurallyValid =>
        !string.IsNullOrWhiteSpace(FilePath) &&
        !string.IsNullOrWhiteSpace(VerifiedBackupPath) &&
        !string.IsNullOrWhiteSpace(MimeType) &&
        HasImageBytes;
}
