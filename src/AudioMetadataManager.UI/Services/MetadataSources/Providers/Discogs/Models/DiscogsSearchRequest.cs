namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Discogs.Models;

/// <summary>
/// Representa una búsqueda musical que será enviada
/// al proveedor Discogs.
/// </summary>
public sealed class DiscogsSearchRequest
{
    /// <summary>
    /// Artista interpretado desde el archivo o sus etiquetas.
    /// </summary>
    public string? Artist { get; init; }

    /// <summary>
    /// Título principal de la pista.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Versión, mezcla o remix.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// Álbum o publicación conocida.
    /// </summary>
    public string? Album { get; init; }

    /// <summary>
    /// Año conocido, cuando esté disponible.
    /// </summary>
    public int? Year { get; init; }

    /// <summary>
    /// Número de página solicitado.
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Cantidad de resultados solicitados.
    /// Un valor nulo utiliza la configuración predeterminada.
    /// </summary>
    public int? ResultsPerPage { get; init; }

    /// <summary>
    /// Indica si existen al menos Artist o Title.
    /// </summary>
    public bool HasMinimumSearchData =>
        !string.IsNullOrWhiteSpace(Artist) ||
        !string.IsNullOrWhiteSpace(Title);

    /// <summary>
    /// Texto resumido para diagnósticos.
    /// </summary>
    public string SearchDisplay
    {
        get
        {
            List<string> parts = new();

            AddIfAvailable(
                parts,
                Artist);

            AddIfAvailable(
                parts,
                Title);

            AddIfAvailable(
                parts,
                Version);

            return parts.Count == 0
                ? "(búsqueda vacía)"
                : string.Join(
                    " - ",
                    parts);
        }
    }

    private static void AddIfAvailable(
        ICollection<string> values,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            values.Add(
                value.Trim());
        }
    }
}