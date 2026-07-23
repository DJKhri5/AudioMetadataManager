using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources.Pipeline;

/// <summary>
/// Contiene el contexto completo de una ejecución del pipeline
/// de búsqueda de metadatos.
///
/// Permite conservar trazabilidad entre la solicitud original,
/// las fuentes consultadas y el resultado final.
/// </summary>
public sealed class MetadataSearchContext
{
    public MetadataSearchContext()
    {
    }

    public MetadataSearchContext(
        MetadataSearchRequest request)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        Request = request;
    }

    /// <summary>
    /// Identificador único de la ejecución.
    /// </summary>
    public Guid ExecutionId { get; init; } =
        Guid.NewGuid();

    /// <summary>
    /// Solicitud original construida desde el archivo,
    /// las etiquetas y el parser.
    /// </summary>
    public MetadataSearchRequest Request { get; init; } =
        new();

    /// <summary>
    /// Momento UTC en que se creó el contexto.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; init; } =
        DateTimeOffset.UtcNow;

    /// <summary>
    /// Indica si el contexto contiene información suficiente
    /// para iniciar una búsqueda externa.
    /// </summary>
    public bool HasSearchableIdentity =>
        Request.HasParsedIdentity ||
        Request.HasTaggedIdentity;

    /// <summary>
    /// Consulta principal preparada para diagnósticos.
    /// </summary>
    public string PrimaryQueryDisplay =>
        string.IsNullOrWhiteSpace(
            Request.PrimaryQuery)
                ? "(consulta principal no disponible)"
                : Request.PrimaryQuery;

    /// <summary>
    /// Consulta alternativa preparada para diagnósticos.
    /// </summary>
    public string AlternativeQueryDisplay =>
        string.IsNullOrWhiteSpace(
            Request.AlternativeQuery)
                ? "(consulta alternativa no disponible)"
                : Request.AlternativeQuery;

    /// <summary>
    /// Nombre del archivo asociado a esta ejecución.
    /// </summary>
    public string FileDisplayName =>
        string.IsNullOrWhiteSpace(
            Request.FileName)
                ? "(archivo sin identificar)"
                : Request.FileName;
}