namespace AudioMetadataManager.UI.Services.Artwork.Models;

/// <summary>
/// Contiene el resultado completo de obtener e incrustar la
/// carátula de una pista.
/// </summary>
public sealed class TrackArtworkResult
{
    /// <summary>
    /// Estado general de la operación.
    /// </summary>
    public TrackArtworkStatus Status { get; init; } =
        TrackArtworkStatus.Unknown;

    /// <summary>
    /// Ruta del archivo procesado.
    /// </summary>
    public string FilePath { get; init; } =
        string.Empty;

    /// <summary>
    /// Resultado de la descarga de la imagen.
    /// Queda nulo cuando la solicitud fue inválida antes de
    /// intentar la descarga.
    /// </summary>
    public ArtworkDownloadResult? DownloadResult { get; init; }

    /// <summary>
    /// Resultado de la incrustación en el archivo.
    /// Queda nulo cuando la descarga falló y la incrustación
    /// nunca llegó a intentarse.
    /// </summary>
    public ArtworkEmbedResult? EmbedResult { get; init; }

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
    /// Indica si la operación terminó correctamente.
    /// </summary>
    public bool IsSuccess =>
        Status == TrackArtworkStatus.Success;

    /// <summary>
    /// Construye un resultado para una solicitud inválida.
    /// </summary>
    public static TrackArtworkResult InvalidRequest(
        string filePath,
        string message)
    {
        return new TrackArtworkResult
        {
            Status =
                TrackArtworkStatus.InvalidRequest,

            FilePath =
                filePath,

            Message =
                message
        };
    }
}
