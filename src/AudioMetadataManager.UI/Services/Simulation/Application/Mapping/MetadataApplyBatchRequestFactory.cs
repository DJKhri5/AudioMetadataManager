using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Views.Models.Simulation;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Mapping;

/// <summary>
/// Convierte una selección productiva multiarchivo en una
/// solicitud técnica de aplicación por lote.
/// </summary>
public sealed class MetadataApplyBatchRequestFactory
{
    /// <summary>
    /// Construye una solicitud batch usando solamente
    /// elementos productivos válidos.
    /// </summary>
    public MetadataApplyBatchRequest Create(
        ProductiveBatchSelection selection)
    {
        ArgumentNullException.ThrowIfNull(
            selection);

        IReadOnlyList<MetadataApplyRequest>
            requests =
                selection.Items
                    .Where(
                        item =>
                            item is not null &&
                            item.IsValid)
                    .Select(
                        item =>
                            item.ApplyRequest)
                    .ToArray();

        return new MetadataApplyBatchRequest
        {
            Requests =
                requests
        };
    }
}