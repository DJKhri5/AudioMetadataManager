namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.Models;

/// <summary>
/// Contiene una instantánea de sólo lectura de las etiquetas y
/// propiedades detectadas en un archivo MP3 mediante
/// TagLibSharp.
/// </summary>
public sealed class TagLibMp3InspectionResult
{
    /// <summary>
    /// Ruta del archivo inspeccionado.
    /// </summary>
    public string FilePath { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si TagLibSharp pudo abrir y leer el archivo.
    /// </summary>
    public bool WasSuccessful { get; init; }

    /// <summary>
    /// Título almacenado en las etiquetas.
    /// </summary>
    public string Title { get; init; } =
        string.Empty;

    /// <summary>
    /// Artistas principales.
    /// </summary>
    public IReadOnlyList<string> Performers { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Artistas del álbum.
    /// </summary>
    public IReadOnlyList<string> AlbumArtists { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Nombre del álbum o lanzamiento.
    /// </summary>
    public string Album { get; init; } =
        string.Empty;

    /// <summary>
    /// Géneros almacenados.
    /// </summary>
    public IReadOnlyList<string> Genres { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Año almacenado.
    /// </summary>
    public uint Year { get; init; }

    /// <summary>
    /// Número de pista.
    /// </summary>
    public uint Track { get; init; }

    /// <summary>
    /// Total de pistas indicado.
    /// </summary>
    public uint TrackCount { get; init; }

    /// <summary>
    /// Número de disco.
    /// </summary>
    public uint Disc { get; init; }

    /// <summary>
    /// Total de discos indicado.
    /// </summary>
    public uint DiscCount { get; init; }

    /// <summary>
    /// Comentario general.
    /// </summary>
    public string Comment { get; init; } =
        string.Empty;

    /// <summary>
    /// Cantidad de imágenes incrustadas.
    /// </summary>
    public int EmbeddedPictureCount { get; init; }

    /// <summary>
    /// Indica si el archivo contiene al menos una imagen.
    /// </summary>
    public bool HasEmbeddedPictures =>
        EmbeddedPictureCount > 0;

    /// <summary>
    /// Duración técnica detectada.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Bitrate informado por TagLibSharp.
    /// </summary>
    public int AudioBitrateKbps { get; init; }

    /// <summary>
    /// Frecuencia de muestreo.
    /// </summary>
    public int AudioSampleRateHz { get; init; }

    /// <summary>
    /// Número de canales.
    /// </summary>
    public int AudioChannels { get; init; }

    /// <summary>
    /// Tipos de etiquetas encontrados en el archivo.
    /// </summary>
    public string TagTypes { get; init; } =
        string.Empty;

    /// <summary>
    /// Mensajes producidos durante la inspección.
    /// </summary>
    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Artistas combinados para presentación.
    /// </summary>
    public string PerformersDisplay =>
        Performers.Count == 0
            ? "(sin información)"
            : string.Join(
                ", ",
                Performers);

    /// <summary>
    /// Géneros combinados para presentación.
    /// </summary>
    public string GenresDisplay =>
        Genres.Count == 0
            ? "(sin información)"
            : string.Join(
                ", ",
                Genres);

    /// <summary>
    /// Resumen compacto.
    /// </summary>
    public string Summary
    {
        get
        {
            if (!WasSuccessful)
            {
                return
                    "TagLibSharp no pudo inspeccionar el " +
                    "archivo MP3.";
            }

            return
                $"MP3 inspeccionado correctamente. " +
                $"Título: {DisplayValue(Title)}. " +
                $"Artista: {PerformersDisplay}. " +
                $"Imágenes incrustadas: " +
                $"{EmbeddedPictureCount}.";
        }
    }

    private static string DisplayValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "(sin información)"
            : value.Trim();
    }
}