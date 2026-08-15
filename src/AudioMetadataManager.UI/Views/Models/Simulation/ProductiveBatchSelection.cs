using AudioMetadataManager.UI.Services.Simulation
    .Application.Mapping;
using System.Collections.ObjectModel;
using System.IO;

namespace AudioMetadataManager.UI.Views.Models.Simulation;

/// <summary>
/// Mantiene los archivos seleccionados para una futura
/// aplicación productiva por lote.
/// </summary>
public sealed class ProductiveBatchSelection
{
    private readonly ObservableCollection<
        ProductiveBatchSelectionItem>
        _items =
            new();

    private readonly MetadataApplyRequestFactory
        _requestFactory =
            new();

    /// <summary>
    /// Elementos actualmente incluidos en la selección.
    /// </summary>
    public ReadOnlyObservableCollection<
        ProductiveBatchSelectionItem>
        Items
    {
        get;
    }

    public ProductiveBatchSelection()
    {
        Items =
            new ReadOnlyObservableCollection<
                ProductiveBatchSelectionItem>(
                    _items);
    }

    /// <summary>
    /// Cantidad de archivos incluidos.
    /// </summary>
    public int FileCount =>
        _items.Count;

    /// <summary>
    /// Cantidad total de cambios aprobados preparados.
    /// </summary>
    public int ApprovedChangeCount =>
        _items.Sum(
            item =>
                item.ApprovedChangeCount);

    /// <summary>
    /// Indica si existe al menos un archivo listo
    /// para construir una solicitud batch.
    /// </summary>
    public bool HasItems =>
        _items.Count > 0;

    /// <summary>
    /// Indica si la selección puede ejecutarse actualmente
    /// como un lote productivo válido.
    /// </summary>
    public bool IsReadyForExecution =>
        HasItems &&
        _items.All(
            item =>
                item.IsValid);

    /// <summary>
    /// Indica si la selección está completamente vacía.
    /// </summary>
    public bool IsEmpty =>
        _items.Count == 0;

    /// <summary>
    /// Indica si la selección ya no puede ejecutar
    /// ninguna solicitud productiva.
    /// </summary>
    public bool IsExecutionUnavailable =>
        !IsReadyForExecution;

    /// <summary>
    /// Agrega o actualiza un plan.
    ///
    /// Si el plan ya no contiene cambios aprobados,
    /// elimina cualquier selección anterior del mismo archivo.
    /// </summary>
    public bool AddOrReplace(
        SimulationPlanViewModel simulationPlan)
    {
        ArgumentNullException.ThrowIfNull(
            simulationPlan);

        if (string.IsNullOrWhiteSpace(
                simulationPlan.FilePath))
        {
            return false;
        }

        if (!simulationPlan.HasApprovedChanges)
        {
            Remove(
                simulationPlan.FilePath);

            return false;
        }

        var request =
            _requestFactory.Create(
                simulationPlan);

        if (!request.IsStructurallyValid)
        {
            Remove(
                simulationPlan.FilePath);

            return false;
        }

        ProductiveBatchSelectionItem newItem =
            new(
                simulationPlan,
                request);

        int existingIndex =
            FindIndex(
                simulationPlan.FilePath);

        if (existingIndex >= 0)
        {
            _items[existingIndex] =
                newItem;

            return true;
        }

        _items.Add(
            newItem);

        return true;
    }

    /// <summary>
    /// Elimina un archivo de la selección usando su ruta.
    /// </summary>
    public bool Remove(
        string filePath)
    {
        if (string.IsNullOrWhiteSpace(
                filePath))
        {
            return false;
        }

        int existingIndex =
            FindIndex(
                filePath);

        if (existingIndex < 0)
        {
            return false;
        }

        _items.RemoveAt(
            existingIndex);

        return true;
    }

    /// <summary>
    /// Elimina todos los elementos seleccionados.
    /// </summary>
    public void Clear()
    {
        _items.Clear();
    }

    /// <summary>
    /// Indica si la ruta ya forma parte de la selección.
    /// </summary>
    public bool Contains(
        string filePath)
    {
        return
            FindIndex(
                filePath) >= 0;
    }

    private int FindIndex(
        string filePath)
    {
        if (string.IsNullOrWhiteSpace(
                filePath))
        {
            return -1;
        }

        string normalizedPath;

        try
        {
            normalizedPath =
                Path.GetFullPath(
                    filePath);
        }
        catch
        {
            return -1;
        }

        for (int index = 0;
            index < _items.Count;
            index++)
        {
            string existingPath;

            try
            {
                existingPath =
                    Path.GetFullPath(
                        _items[index]
                            .FilePath);
            }
            catch
            {
                continue;
            }

            if (string.Equals(
                    existingPath,
                    normalizedPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        return -1;
    }

    /// <summary>
    /// Resumen compacto de la selección.
    /// </summary>
    public string Summary
    {
        get
        {
            if (!HasItems)
            {
                return
                    "No existen archivos seleccionados para " +
                    "aplicación productiva por lote.";
            }

            if (!IsReadyForExecution)
            {
                return
                    $"{FileCount} archivo(s) seleccionados, pero " +
                    "la selección productiva contiene elementos no válidos.";
            }

            return
                $"{FileCount} archivo(s) y " +
                $"{ApprovedChangeCount} cambio(s) aprobado(s) " +
                "seleccionados para aplicación por lote.";
        }
    }
}