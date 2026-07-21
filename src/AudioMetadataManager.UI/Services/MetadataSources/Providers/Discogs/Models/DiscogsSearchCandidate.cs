namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Models;

/// <summary>
/// Representa un candidato normalizado obtenido desde Discogs.
///
/// Este modelo evita que el resto de la aplicación dependa
/// directamente de la estructura JSON de la API.
/// </summary>
public sealed class DiscogsSearchCandidate
{
    /// <summary>
    /// Identificador numérico asignado por Discogs.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Tipo de recurso retornado.
    /// Ejemplos: release o master.
    /// </summary>
    public string ResourceType { get; init; } =
        string.Empty;

    /// <summary>
    /// Artista principal normalizado.
    /// </summary>
    public string? Artist { get; init; }

    /// <summary>
    /// Título principal.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Versión o mezcla detectada.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// Álbum o nombre de la publicación.
    /// </summary>
    public string? Album { get; init; }

    /// <summary>
    /// Sello discográfico principal.
    /// </summary>
    public string? Label { get; init; }

    /// <summary>
    /// Género principal.
    /// </summary>
    public string? Genre { get; init; }

    /// <summary>
    /// Estilo musical más específico.
    /// </summary>
    public string? Style { get; init; }

    /// <summary>
    /// Año de publicación.
    /// </summary>
    public int? Year { get; init; }

    /// <summary>
    /// País asociado a la publicación.
    /// </summary>
    public string? Country { get; init; }

    /// <summary>
    /// Texto de formato retornado por Discogs.
    /// </summary>
    public string? Format { get; init; }

    /// <summary>
    /// URL de la página correspondiente en Discogs.
    /// </summary>
    public Uri? DiscogsUri { get; init; }

    /// <summary>
    /// URL de una imagen de portada, cuando esté disponible.
    /// </summary>
    public Uri? CoverImageUri { get; init; }

    /// <summary>
    /// Texto original de título retornado por la búsqueda.
    /// Se conserva para diagnóstico y trazabilidad.
    /// </summary>
    public string RawTitle { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si el candidato contiene datos mínimos utilizables.
    /// </summary>
    public bool HasUsableMetadata =>
        Id > 0 &&
        (!string.IsNullOrWhiteSpace(Artist) ||
         !string.IsNullOrWhiteSpace(Title));

    /// <summary>
    /// Nombre resumido del candidato.
    /// </summary>
    public string DisplayName
    {
        get
        {
            string artist =
                string.IsNullOrWhiteSpace(Artist)
                    ? "(artista desconocido)"
                    : Artist.Trim();

            string title =
                string.IsNullOrWhiteSpace(Title)
                    ? "(título desconocido)"
                    : Title.Trim();

            return $"{artist} - {title}";
        }
    }
}