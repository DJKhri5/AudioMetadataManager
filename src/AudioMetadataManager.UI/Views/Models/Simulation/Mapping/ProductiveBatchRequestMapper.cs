using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Views.Models.Simulation;

namespace AudioMetadataManager.UI.Views.Models.Simulation.Mapping;

/// <summary>
/// Convierte la selección productiva mantenida por la interfaz
/// en una solicitud de aplicación productiva por lote.
/// </summary>
public sealed class ProductiveBatchRequestMapper
{
    public MetadataApplyBatchRequest Map(
        ProductiveBatchSelection selection)
    {
        ArgumentNullException.ThrowIfNull(
            selection);

        if (!selection.HasItems)
        {
            throw new InvalidOperationException(
                "No existen archivos seleccionados para " +
                "construir una solicitud productiva por lote.");
        }

        ProductiveBatchSelectionItem[]
            selectedItems =
                selection.Items
                    .ToArray();

        if (selectedItems.Any(
                item =>
                    item is null ||
                    !item.IsValid))
        {
            throw new InvalidOperationException(
                "La selección productiva contiene uno o más " +
                "elementos no válidos.");
        }

        MetadataApplyRequest[] requests =
            selectedItems
                .Select(
                    item =>
                        item.ApplyRequest)
                .ToArray();

        MetadataApplyBatchRequest batchRequest =
            new()
            {
                Requests =
                    requests
            };

        if (!batchRequest.IsStructurallyValid)
        {
            throw new InvalidOperationException(
                "La selección no pudo convertirse en una " +
                "solicitud productiva por lote válida.");
        }

        return batchRequest;
    }
}