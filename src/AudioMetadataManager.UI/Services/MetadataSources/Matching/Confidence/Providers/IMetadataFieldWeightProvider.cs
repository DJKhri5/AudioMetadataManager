using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Providers;

/// <summary>
/// Define el contrato para obtener la configuración de pesos
/// utilizada por el motor de confianza de metadatos.
///
/// El motor depende de esta abstracción y no de una
/// implementación concreta.
/// </summary>
public interface IMetadataFieldWeightProvider
{
    /// <summary>
    /// Obtiene los pesos configurados, indexados mediante
    /// identificadores fuertes de campos.
    /// </summary>
    IReadOnlyDictionary<MetadataField, MetadataFieldWeight> GetWeights();
}