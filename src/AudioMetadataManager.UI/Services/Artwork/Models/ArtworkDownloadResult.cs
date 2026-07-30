namespace AudioMetadataManager.UI.Services.Artwork.Models;

/// <summary>
/// Contiene el resultado completo de una descarga de imagen de
/// carátula.
/// </summary>
public sealed class ArtworkDownloadResult
{
    /// <summary>
    /// Estado general de la operación.
    /// </summary>
    public ArtworkDownloadStatus Status { get; init; } =
        ArtworkDownloadStatus.Unknown;

    /// <summary>
    /// Dirección solicitada.
    /// </summary>
    public string SourceUrl { get; init; } =
        string.Empty;

    /// <summary>
    /// Bytes de la imagen descargada.
    /// </summary>
    public byte[] ImageBytes { get; init; } =
        Array.Empty<byte>();

    /// <summary>
    /// Tipo de contenido informado por el servidor.
    /// </summary>
    public string MimeType { get; init; } =
        string.Empty;

    /// <summary>
    /// Mensaje descriptivo para interfaz o diagnóstico.
    /// </summary>
    public string Message { get; init; } =
        string.Empty;

    /// <summary>
    /// Momento UTC en que se produjo el resultado.
    /// </summary>
    public DateTimeOffset RetrievedAtUtc { get; init; } =
        DateTimeOffset.UtcNow;

    /// <summary>
    /// Indica si la descarga contiene una imagen utilizable.
    /// </summary>
    public bool HasImageBytes =>
        ImageBytes.Length > 0;

    /// <summary>
    /// Indica si la operación terminó correctamente.
    /// </summary>
    public bool IsSuccess =>
        Status == ArtworkDownloadStatus.Success &&
        HasImageBytes;

    /// <summary>
    /// Construye un resultado para una solicitud inválida.
    /// </summary>
    public static ArtworkDownloadResult InvalidRequest(
        string sourceUrl,
        string message)
    {
        return new ArtworkDownloadResult
        {
            Status =
                ArtworkDownloadStatus.InvalidRequest,

            SourceUrl =
                sourceUrl,

            Message =
                message
        };
    }
}
