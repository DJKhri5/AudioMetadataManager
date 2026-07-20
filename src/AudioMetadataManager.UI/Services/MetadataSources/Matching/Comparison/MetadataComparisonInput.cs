namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison;

/// <summary>
/// Representa un conjunto normalizado de metadatos que será
/// entregado al motor central de comparación.
///
/// El mismo modelo puede representar información procedente
/// del archivo local, del nombre analizado, de las etiquetas
/// internas o de una fuente externa.
///
/// Este objeto solamente transporta información. No compara
/// valores ni modifica archivos.
/// </summary>
public sealed class MetadataComparisonInput
{
    /// <summary>
    /// Nombre descriptivo de la fuente de los datos.
    ///
    /// Ejemplos:
    /// Archivo local
    /// Nombre del archivo
    /// Etiquetas internas
    /// Discogs
    /// Beatport
    /// Spotify
    /// SoundCloud
    /// </summary>
    public string SourceName { get; init; } =
        string.Empty;

    /// <summary>
    /// Artista principal o conjunto de artistas.
    ///
    /// Los conectores originales, como &, feat., vs y x,
    /// deben conservarse en este valor.
    /// </summary>
    public string? Artist { get; init; }

    /// <summary>
    /// Título principal de la pista.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Versión, mezcla o edición de la pista.
    ///
    /// Ejemplos:
    /// Original Mix
    /// Extended Mix
    /// Radio Edit
    /// Will Atkinson Remix
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// Álbum, lanzamiento o recopilación asociada.
    /// </summary>
    public string? Album { get; init; }

    /// <summary>
    /// Género musical informado por la fuente.
    /// </summary>
    public string? Genre { get; init; }

    /// <summary>
    /// Sello discográfico informado por la fuente.
    /// </summary>
    public string? Label { get; init; }

    /// <summary>
    /// Indica si el conjunto contiene al menos un campo
    /// de metadatos utilizable.
    /// </summary>
    public bool HasAnyMetadata =>
        !string.IsNullOrWhiteSpace(Artist) ||
        !string.IsNullOrWhiteSpace(Title) ||
        !string.IsNullOrWhiteSpace(Version) ||
        !string.IsNullOrWhiteSpace(Album) ||
        !string.IsNullOrWhiteSpace(Genre) ||
        !string.IsNullOrWhiteSpace(Label);

    /// <summary>
    /// Nombre de fuente preparado para mostrarse en informes.
    /// </summary>
    public string SourceDisplayName =>
        string.IsNullOrWhiteSpace(SourceName)
            ? "Fuente sin identificar"
            : SourceName.Trim();
}