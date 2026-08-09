using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

namespace AudioMetadataManager.UI.Views.Models.Simulation;

/// <summary>
/// Representa un archivo y su solicitud productiva preparada
/// dentro de una selección para aplicación por lote.
/// </summary>
public sealed class ProductiveBatchSelectionItem
{
    public ProductiveBatchSelectionItem(
        SimulationPlanViewModel simulationPlan,
        MetadataApplyRequest applyRequest)
    {
        ArgumentNullException.ThrowIfNull(
            simulationPlan);

        ArgumentNullException.ThrowIfNull(
            applyRequest);

        SimulationPlan =
            simulationPlan;

        ApplyRequest =
            applyRequest;
    }

    /// <summary>
    /// Plan visual del que se obtuvo la solicitud.
    /// </summary>
    public SimulationPlanViewModel SimulationPlan
    {
        get;
    }

    /// <summary>
    /// Solicitud productiva preparada desde el plan.
    /// </summary>
    public MetadataApplyRequest ApplyRequest
    {
        get;
    }

    /// <summary>
    /// Identificador del plan de origen.
    /// </summary>
    public Guid PlanId =>
        SimulationPlan.PlanId;

    /// <summary>
    /// Ruta completa del archivo.
    /// </summary>
    public string FilePath =>
        ApplyRequest.FilePath;

    /// <summary>
    /// Nombre visible del archivo.
    /// </summary>
    public string FileName =>
        ApplyRequest.FileName;

    /// <summary>
    /// Cantidad de cambios productivos incluidos.
    /// </summary>
    public int ApprovedChangeCount =>
        ApplyRequest.ValidChangeCount;

    /// <summary>
    /// Indica si el elemento contiene una solicitud
    /// actualmente válida para el lote.
    /// </summary>
    public bool IsValid =>
        PlanId != Guid.Empty &&
        !string.IsNullOrWhiteSpace(
            FilePath) &&
        ApplyRequest.IsStructurallyValid;

    /// <summary>
    /// Resumen compacto para diagnóstico y futura interfaz.
    /// </summary>
    public string Summary =>
        IsValid
            ? $"{FileName}: " +
              $"{ApprovedChangeCount} cambio(s) aprobado(s)."
            : $"{FileName}: selección productiva no válida.";
}