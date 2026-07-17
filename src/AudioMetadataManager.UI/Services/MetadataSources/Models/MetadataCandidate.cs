namespace AudioMetadataManager.UI.Services.MetadataSources.Models;

/// <summary>
/// Representa una posible coincidencia obtenida desde una
/// plataforma externa de metadatos musicales.
/// </summary>
public class MetadataCandidate
{
    /// <summary>
    /// Plataforma que entregó el resultado.
    /// Ejemplos: Discogs, Beatport, Spotify o SoundCloud.
    /// </summary>
    public string SourceName { get; set; } = string.Empty;

    /// <summary>
    /// Identificador interno que la plataforma asigna
    /// al lanzamiento, pista o resultado.
    /// </summary>
    public string SourceId { get; set; } = string.Empty;

    /// <summary>
    /// Dirección o referencia externa del resultado.
    /// Se conserva como texto para auditoría y navegación futura.
    /// </summary>
    public string SourceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Artista o crédito principal informado por la plataforma.
    /// </summary>
    public string Artist { get; set; } = string.Empty;

    /// <summary>
    /// Título oficial de la pista.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Versión, remix, mezcla o edición de la pista.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del álbum, sencillo, EP o lanzamiento.
    /// </summary>
    public string ReleaseTitle { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del sello discográfico.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Género principal informado por la fuente.
    /// </summary>
    public string Genre { get; set; } = string.Empty;

    /// <summary>
    /// Año del lanzamiento.
    /// Cero representa un año no informado.
    /// </summary>
    public uint Year { get; set; }

    /// <summary>
    /// Duración de la pista, cuando la plataforma la entrega.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Dirección de la carátula propuesta.
    /// No se descargará ni aplicará automáticamente.
    /// </summary>
    public string ArtworkUrl { get; set; } = string.Empty;

    /// <summary>
    /// Posición original del resultado dentro de la respuesta
    /// de la plataforma. Un número menor suele representar
    /// una coincidencia mostrada antes por la fuente.
    /// </summary>
    public int SourceRank { get; set; }

    /// <summary>
    /// Indica si la plataforma entregó como mínimo
    /// un artista y un título utilizables.
    /// </summary>
    public bool HasIdentity =>
        !string.IsNullOrWhiteSpace(Artist) &&
        !string.IsNullOrWhiteSpace(Title);

    /// <summary>
    /// Indica si existe una duración que pueda compararse
    /// con la duración técnica del archivo local.
    /// </summary>
    public bool HasDuration =>
        Duration > TimeSpan.Zero;

    /// <summary>
    /// Indica si el candidato contiene una carátula.
    /// </summary>
    public bool HasArtwork =>
        !string.IsNullOrWhiteSpace(ArtworkUrl);

    /// <summary>
    /// Nombre completo de la pista para presentación,
    /// registros y comparaciones visuales.
    /// </summary>
    public string DisplayName
    {
        get
        {
            string versionPart =
                string.IsNullOrWhiteSpace(Version)
                    ? string.Empty
                    : $" ({Version.Trim()})";

            if (string.IsNullOrWhiteSpace(Artist))
            {
                return $"{Title.Trim()}{versionPart}".Trim();
            }

            if (string.IsNullOrWhiteSpace(Title))
            {
                return Artist.Trim();
            }

            return
                $"{Artist.Trim()} - " +
                $"{Title.Trim()}" +
                $"{versionPart}";
        }
    }

    /// <summary>
    /// Identificador legible de la procedencia del resultado.
    /// </summary>
    public string SourceDisplay
    {
        get
        {
            if (string.IsNullOrWhiteSpace(SourceId))
            {
                return SourceName;
            }

            return $"{SourceName} · {SourceId}";
        }
    }
}