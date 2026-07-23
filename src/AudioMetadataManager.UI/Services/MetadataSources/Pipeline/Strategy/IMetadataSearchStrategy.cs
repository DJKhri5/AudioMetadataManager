using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Pipeline.Strategy;

/// <summary>
/// Define el contrato para generar variantes ordenadas
/// de búsqueda desde una solicitud de metadatos.
/// </summary>
public interface IMetadataSearchStrategy
{
    /// <summary>
    /// Nombre legible de la estrategia.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Genera consultas ordenadas, válidas y sin duplicados.
    /// </summary>
    IReadOnlyList<MetadataSearchQuery> BuildQueries(
        MetadataSearchRequest request);
}