namespace AudioMetadataManager.UI.Services.Artwork.Models;

/// <summary>
/// Contiene el resultado completo de incrustar una imagen de
/// carátula en un archivo de audio.
/// </summary>
public sealed class ArtworkEmbedResult
{
    /// <summary>
    /// Estado general de la operación.
    /// </summary>
    public ArtworkEmbedStatus Status { get; init; } =
        ArtworkEmbedStatus.Unknown;

    /// <summary>
    /// Ruta del archivo modificado.
    /// </summary>
    public string FilePath { get; init; } =
        string.Empty;

    /// <summary>
    /// Cantidad de imágenes incrustadas antes de la operación.
    /// </summary>
    public int PictureCountBefore { get; init; }

    /// <summary>
    /// Cantidad de imágenes incrustadas después de la operación.
    /// </summary>
    public int PictureCountAfter { get; init; }

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
        Status == ArtworkEmbedStatus.Success;

    /// <summary>
    /// Construye un resultado para una solicitud inválida.
    /// </summary>
    public static ArtworkEmbedResult InvalidRequest(
        string filePath,
        string message)
    {
        return new ArtworkEmbedResult
        {
            Status =
                ArtworkEmbedStatus.InvalidRequest,

            FilePath =
                filePath,

            Message =
                message
        };
    }
}
