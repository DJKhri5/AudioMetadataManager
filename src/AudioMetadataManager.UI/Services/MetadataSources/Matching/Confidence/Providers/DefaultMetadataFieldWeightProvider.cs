using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Providers;

/// <summary>
/// Proporciona la configuración predeterminada de ponderaciones
/// para calcular la confianza global de una coincidencia.
///
/// Esta implementación representa el perfil general utilizado
/// cuando no existe una configuración específica para una
/// fuente de metadatos.
/// </summary>
public sealed class DefaultMetadataFieldWeightProvider
    : IMetadataFieldWeightProvider
{
    private static readonly IReadOnlyDictionary<
        MetadataField,
        MetadataFieldWeight> DefaultWeights =
            new Dictionary<MetadataField, MetadataFieldWeight>
            {
                [MetadataField.Artist] = new MetadataFieldWeight
                {
                    Field = MetadataField.Artist,
                    Weight = 0.30,
                    IsCritical = true
                },

                [MetadataField.Title] = new MetadataFieldWeight
                {
                    Field = MetadataField.Title,
                    Weight = 0.30,
                    IsCritical = true
                },

                [MetadataField.Version] = new MetadataFieldWeight
                {
                    Field = MetadataField.Version,
                    Weight = 0.20,
                    IsCritical = true
                },

                [MetadataField.Album] = new MetadataFieldWeight
                {
                    Field = MetadataField.Album,
                    Weight = 0.10,
                    IsCritical = false
                },

                [MetadataField.Label] = new MetadataFieldWeight
                {
                    Field = MetadataField.Label,
                    Weight = 0.05,
                    IsCritical = false
                },

                [MetadataField.Genre] = new MetadataFieldWeight
                {
                    Field = MetadataField.Genre,
                    Weight = 0.05,
                    IsCritical = false
                }
            };

    /// <summary>
    /// Devuelve la configuración predeterminada de pesos.
    /// </summary>
    public IReadOnlyDictionary<
        MetadataField,
        MetadataFieldWeight> GetWeights()
    {
        return DefaultWeights;
    }
}