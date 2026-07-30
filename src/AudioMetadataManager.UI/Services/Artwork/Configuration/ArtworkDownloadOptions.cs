namespace AudioMetadataManager.UI.Services.Artwork.Configuration;

/// <summary>
/// Configuración necesaria para descargar imágenes de carátula
/// desde direcciones propuestas por fuentes externas de metadatos.
/// </summary>
public sealed class ArtworkDownloadOptions
{
    /// <summary>
    /// Tamaño máximo permitido para una imagen descargada.
    /// Protege contra respuestas anormalmente grandes.
    /// </summary>
    public long MaxSizeBytes { get; init; } =
        10 * 1024 * 1024;

    /// <summary>
    /// Tiempo máximo permitido para la descarga.
    /// </summary>
    public TimeSpan RequestTimeout { get; init; } =
        TimeSpan.FromSeconds(15);

    /// <summary>
    /// Tipos de contenido aceptados como imagen de carátula.
    /// </summary>
    public IReadOnlySet<string> AllowedMimeTypes { get; init; } =
        new HashSet<string>(
            StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png"
        };

    /// <summary>
    /// Indica si existe una configuración mínima utilizable.
    /// </summary>
    public bool IsValid =>
        MaxSizeBytes > 0 &&
        RequestTimeout > TimeSpan.Zero &&
        AllowedMimeTypes.Count > 0;
}
