namespace AudioMetadataManager.UI.Services.MetadataSources.Models;

/// <summary>
/// Representa los datos disponibles para buscar una canción
/// en una fuente externa como Discogs, Beatport, Spotify
/// o SoundCloud.
/// </summary>
public class MetadataSearchRequest
{
    /// <summary>
    /// Nombre completo del archivo, incluida su extensión.
    /// Se conserva para auditoría y trazabilidad.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Artista obtenido desde el nombre del archivo.
    /// Generalmente será el primer candidato de búsqueda.
    /// </summary>
    public string ParsedArtist { get; set; } = string.Empty;

    /// <summary>
    /// Título obtenido desde el nombre del archivo.
    /// </summary>
    public string ParsedTitle { get; set; } = string.Empty;

    /// <summary>
    /// Versión, remix o edición obtenida desde el nombre.
    /// Ejemplos: Extended Mix, Original Mix o Radio Edit.
    /// </summary>
    public string ParsedVersion { get; set; } = string.Empty;

    /// <summary>
    /// Artista almacenado actualmente en las etiquetas.
    /// Puede estar vacío o ser diferente al resultado del parser.
    /// </summary>
    public string TaggedArtist { get; set; } = string.Empty;

    /// <summary>
    /// Título almacenado actualmente en las etiquetas.
    /// </summary>
    public string TaggedTitle { get; set; } = string.Empty;

    /// <summary>
    /// Álbum o lanzamiento almacenado en las etiquetas.
    /// </summary>
    public string TaggedAlbum { get; set; } = string.Empty;

    /// <summary>
    /// Año declarado en las etiquetas del archivo.
    /// El valor cero representa año desconocido.
    /// </summary>
    public uint TaggedYear { get; set; }

    /// <summary>
    /// Duración técnica del archivo.
    /// Permitirá comparar resultados externos y descartar
    /// canciones distintas con nombres similares.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Indica si el parser consiguió obtener como mínimo
    /// un artista y un título utilizables.
    /// </summary>
    public bool HasParsedIdentity =>
        !string.IsNullOrWhiteSpace(ParsedArtist) &&
        !string.IsNullOrWhiteSpace(ParsedTitle);

    /// <summary>
    /// Indica si las etiquetas contienen artista y título
    /// suficientes para utilizarlos como búsqueda alternativa.
    /// </summary>
    public bool HasTaggedIdentity =>
        !string.IsNullOrWhiteSpace(TaggedArtist) &&
        !string.IsNullOrWhiteSpace(TaggedTitle);

    /// <summary>
    /// Texto principal de búsqueda.
    /// Prioriza los datos interpretados desde el nombre.
    /// Si no existen, utiliza las etiquetas actuales.
    /// </summary>
    public string PrimaryQuery
    {
        get
        {
            if (HasParsedIdentity)
            {
                return BuildQuery(
                    ParsedArtist,
                    ParsedTitle,
                    ParsedVersion);
            }

            if (HasTaggedIdentity)
            {
                return BuildQuery(
                    TaggedArtist,
                    TaggedTitle,
                    string.Empty);
            }

            return string.Empty;
        }
    }

    /// <summary>
    /// Búsqueda alternativa basada en las etiquetas actuales.
    /// Solo se devuelve cuando difiere de la consulta principal.
    /// </summary>
    public string AlternativeQuery
    {
        get
        {
            if (!HasTaggedIdentity)
            {
                return string.Empty;
            }

            string taggedQuery =
                BuildQuery(
                    TaggedArtist,
                    TaggedTitle,
                    string.Empty);

            return string.Equals(
                    taggedQuery,
                    PrimaryQuery,
                    StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : taggedQuery;
        }
    }

    private static string BuildQuery(
        string artist,
        string title,
        string version)
    {
        List<string> parts = new();

        if (!string.IsNullOrWhiteSpace(artist))
        {
            parts.Add(artist.Trim());
        }

        if (!string.IsNullOrWhiteSpace(title))
        {
            parts.Add(title.Trim());
        }

        if (!string.IsNullOrWhiteSpace(version))
        {
            parts.Add(version.Trim());
        }

        return string.Join(" ", parts);
    }
}