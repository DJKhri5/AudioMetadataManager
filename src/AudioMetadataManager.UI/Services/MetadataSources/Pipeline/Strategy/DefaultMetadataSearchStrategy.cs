using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Pipeline.Strategy;

/// <summary>
/// Genera una secuencia escalonada de consultas, comenzando
/// por la identidad más precisa y ampliando progresivamente
/// sólo cuando sea necesario.
/// </summary>
public sealed class DefaultMetadataSearchStrategy
    : IMetadataSearchStrategy
{
    public string Name =>
        "Estrategia predeterminada escalonada";

    /// <inheritdoc />
    public IReadOnlyList<MetadataSearchQuery> BuildQueries(
        MetadataSearchRequest request)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        List<MetadataSearchQuery> candidates =
            new();

        AddParsedIdentityWithVersion(
            candidates,
            request);

        AddParsedIdentityWithoutVersion(
            candidates,
            request);

        AddTaggedIdentity(
            candidates,
            request);

        AddParsedTitleOnly(
            candidates,
            request);

        AddTaggedTitleOnly(
            candidates,
            request);

        HashSet<string> knownKeys =
            new(
                StringComparer.OrdinalIgnoreCase);

        return candidates
            .Where(
                query =>
                    query.IsValid)
            .OrderBy(
                query =>
                    query.Priority)
            .Where(
                query =>
                    knownKeys.Add(
                        query.DeduplicationKey))
            .ToArray();
    }

    private static void AddParsedIdentityWithVersion(
        ICollection<MetadataSearchQuery> queries,
        MetadataSearchRequest request)
    {
        if (!request.HasParsedIdentity ||
            string.IsNullOrWhiteSpace(
                request.ParsedVersion))
        {
            return;
        }

        queries.Add(
            new MetadataSearchQuery
            {
                Kind =
                    MetadataSearchQueryKind
                        .ParsedIdentityWithVersion,

                Priority =
                    10,

                Artist =
                    Normalize(
                        request.ParsedArtist),

                Title =
                    Normalize(
                        request.ParsedTitle),

                Version =
                    Normalize(
                        request.ParsedVersion),

                Album =
                    Normalize(
                        request.TaggedAlbum),

                Year =
                    request.TaggedYear,

                Reason =
                    "Consulta precisa basada en artista, " +
                    "título y versión interpretados desde " +
                    "el nombre del archivo."
            });
    }

    private static void AddParsedIdentityWithoutVersion(
        ICollection<MetadataSearchQuery> queries,
        MetadataSearchRequest request)
    {
        if (!request.HasParsedIdentity)
        {
            return;
        }

        queries.Add(
            new MetadataSearchQuery
            {
                Kind =
                    MetadataSearchQueryKind
                        .ParsedIdentityWithoutVersion,

                Priority =
                    20,

                Artist =
                    Normalize(
                        request.ParsedArtist),

                Title =
                    Normalize(
                        request.ParsedTitle),

                Version =
                    string.Empty,

                Album =
                    Normalize(
                        request.TaggedAlbum),

                Year =
                    request.TaggedYear,

                Reason =
                    "Consulta de respaldo basada en artista " +
                    "y título interpretados, omitiendo la " +
                    "versión para ampliar la búsqueda."
            });
    }

    private static void AddTaggedIdentity(
        ICollection<MetadataSearchQuery> queries,
        MetadataSearchRequest request)
    {
        if (!request.HasTaggedIdentity)
        {
            return;
        }

        queries.Add(
            new MetadataSearchQuery
            {
                Kind =
                    MetadataSearchQueryKind.TaggedIdentity,

                Priority =
                    30,

                Artist =
                    Normalize(
                        request.TaggedArtist),

                Title =
                    Normalize(
                        request.TaggedTitle),

                Version =
                    string.Empty,

                Album =
                    Normalize(
                        request.TaggedAlbum),

                Year =
                    request.TaggedYear,

                Reason =
                    "Consulta alternativa basada en las " +
                    "etiquetas actuales del archivo."
            });
    }

    private static void AddParsedTitleOnly(
        ICollection<MetadataSearchQuery> queries,
        MetadataSearchRequest request)
    {
        if (string.IsNullOrWhiteSpace(
                request.ParsedTitle))
        {
            return;
        }

        queries.Add(
            new MetadataSearchQuery
            {
                Kind =
                    MetadataSearchQueryKind.ParsedTitleOnly,

                Priority =
                    40,

                Artist =
                    string.Empty,

                Title =
                    Normalize(
                        request.ParsedTitle),

                Version =
                    string.Empty,

                Album =
                    string.Empty,

                Year =
                    0,

                Reason =
                    "Consulta amplia basada únicamente en el " +
                    "título interpretado. Debe utilizarse " +
                    "como último recurso."
            });
    }

    private static void AddTaggedTitleOnly(
        ICollection<MetadataSearchQuery> queries,
        MetadataSearchRequest request)
    {
        if (string.IsNullOrWhiteSpace(
                request.TaggedTitle))
        {
            return;
        }

        queries.Add(
            new MetadataSearchQuery
            {
                Kind =
                    MetadataSearchQueryKind.TaggedTitleOnly,

                Priority =
                    50,

                Artist =
                    string.Empty,

                Title =
                    Normalize(
                        request.TaggedTitle),

                Version =
                    string.Empty,

                Album =
                    string.Empty,

                Year =
                    0,

                Reason =
                    "Consulta amplia basada únicamente en el " +
                    "título almacenado en las etiquetas."
            });
    }

    private static string Normalize(
        string value)
    {
        return string.IsNullOrWhiteSpace(
            value)
                ? string.Empty
                : value.Trim();
    }
}