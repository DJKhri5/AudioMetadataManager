namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Spotify.Models;

/// <summary>
/// Representa un candidato normalizado obtenido desde Spotify.
///
/// Este modelo evita que el resto de la aplicación dependa
/// directamente de la estructura JSON de la API.
/// </summary>
public sealed class SpotifySearchCandidate
{
    /// <summary>
    /// Identificador de la pista en Spotify.
    /// </summary>
    public string Id { get; init; } =
        string.Empty;

    /// <summary>
    /// Artistas acreditados, combinados.
    /// </summary>
    public string? Artist { get; init; }

    /// <summary>
    /// Título de la pista.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Nombre del álbum o publicación.
    /// </summary>
    public string? Album { get; init; }

    /// <summary>
    /// Fecha de lanzamiento del álbum, tal como la informa
    /// Spotify (puede ser sólo el año).
    /// </summary>
    public string? ReleaseDate { get; init; }

    /// <summary>
    /// Duración de la pista.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Popularidad informada por Spotify, entre 0 y 100.
    /// </summary>
    public int Popularity { get; init; }

    /// <summary>
    /// Dirección de la carátula del álbum, cuando está
    /// disponible.
    /// </summary>
    public string? ArtworkUrl { get; init; }

    /// <summary>
    /// Dirección pública de la pista en Spotify.
    /// </summary>
    public Uri? SpotifyUri { get; init; }

    /// <summary>
    /// Posición original del resultado dentro de la respuesta
    /// de Spotify.
    /// </summary>
    public int SourceRank { get; init; }

    /// <summary>
    /// Indica si el candidato contiene datos mínimos utilizables.
    /// </summary>
    public bool HasUsableMetadata =>
        !string.IsNullOrWhiteSpace(Id) &&
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
