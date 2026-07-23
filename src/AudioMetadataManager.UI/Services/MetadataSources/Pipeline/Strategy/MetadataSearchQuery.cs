using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Pipeline.Strategy;

/// <summary>
/// Representa una variante concreta de búsqueda generada
/// desde la solicitud original.
///
/// Cada consulta conserva su origen, prioridad y explicación
/// para permitir diagnósticos y decisiones posteriores.
/// </summary>
public sealed class MetadataSearchQuery
{
    /// <summary>
    /// Tipo de consulta generada.
    /// </summary>
    public MetadataSearchQueryKind Kind { get; init; } =
        MetadataSearchQueryKind.Unknown;

    /// <summary>
    /// Posición de ejecución dentro de la estrategia.
    ///
    /// Un número menor indica que debe intentarse antes.
    /// </summary>
    public int Priority { get; init; }

    /// <summary>
    /// Artista que se enviará a las fuentes externas.
    /// </summary>
    public string Artist { get; init; } =
        string.Empty;

    /// <summary>
    /// Título que se enviará a las fuentes externas.
    /// </summary>
    public string Title { get; init; } =
        string.Empty;

    /// <summary>
    /// Versión, mezcla o remix incluido en esta variante.
    /// </summary>
    public string Version { get; init; } =
        string.Empty;

    /// <summary>
    /// Álbum o lanzamiento utilizado como dato complementario.
    /// </summary>
    public string Album { get; init; } =
        string.Empty;

    /// <summary>
    /// Año opcional utilizado como filtro.
    /// </summary>
    public uint Year { get; init; }

    /// <summary>
    /// Explicación legible de por qué se generó esta consulta.
    /// </summary>
    public string Reason { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si la consulta contiene al menos un título
    /// utilizable.
    /// </summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(
            Title);

    /// <summary>
    /// Indica si contiene una identidad completa de artista
    /// y título.
    /// </summary>
    public bool HasFullIdentity =>
        !string.IsNullOrWhiteSpace(Artist) &&
        !string.IsNullOrWhiteSpace(Title);

    /// <summary>
    /// Texto preparado para registros y diagnósticos.
    /// </summary>
    public string DisplayText
    {
        get
        {
            List<string> parts =
                new();

            AddIfAvailable(
                parts,
                Artist);

            AddIfAvailable(
                parts,
                Title);

            if (!string.IsNullOrWhiteSpace(
                    Version))
            {
                parts.Add(
                    $"({Version.Trim()})");
            }

            return parts.Count == 0
                ? "(consulta vacía)"
                : string.Join(
                    " - ",
                    parts);
        }
    }

    /// <summary>
    /// Clave normalizada utilizada para detectar consultas
    /// equivalentes dentro de una misma estrategia.
    /// </summary>
    public string DeduplicationKey =>
        string.Join(
            "|",
            NormalizeForKey(Artist),
            NormalizeForKey(Title),
            NormalizeForKey(Version),
            NormalizeForKey(Album),
            Year.ToString());

    /// <summary>
    /// Convierte esta variante en una solicitud común que puede
    /// ser entregada a MetadataSourceManager.
    /// </summary>
    public MetadataSearchRequest CreateRequestFrom(
        MetadataSearchRequest originalRequest)
    {
        ArgumentNullException.ThrowIfNull(
            originalRequest);

        return new MetadataSearchRequest
        {
            FileName =
                originalRequest.FileName,

            ParsedArtist =
                Artist,

            ParsedTitle =
                Title,

            ParsedVersion =
                Version,

            TaggedArtist =
                string.Empty,

            TaggedTitle =
                string.Empty,

            TaggedAlbum =
                Album,

            TaggedYear =
                Year,

            Duration =
                originalRequest.Duration
        };
    }

    private static void AddIfAvailable(
        ICollection<string> values,
        string value)
    {
        if (!string.IsNullOrWhiteSpace(
                value))
        {
            values.Add(
                value.Trim());
        }
    }

    private static string NormalizeForKey(
        string value)
    {
        if (string.IsNullOrWhiteSpace(
                value))
        {
            return string.Empty;
        }

        return string.Join(
                " ",
                value
                    .Trim()
                    .ToUpperInvariant()
                    .Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries));
    }
}