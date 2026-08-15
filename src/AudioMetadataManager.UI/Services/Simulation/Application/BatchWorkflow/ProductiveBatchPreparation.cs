using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.BatchWorkflow;

/// <summary>
/// Representa el resultado de la primera fase del workflow
/// productivo por lote.
///
/// Una preparación puede recibir una única decisión productiva.
/// Una vez consumida no puede reutilizarse.
/// </summary>
public sealed class ProductiveBatchPreparation
{
    private bool _wasConsumed;

    public required MetadataApplyBatchRequest BatchRequest
    {
        get;
        init;
    }

    public required MetadataProductiveBatchPreparationResult
        PreparationResult
    {
        get;
        init;
    }

    /// <summary>
    /// Indica si esta preparación ya recibió su decisión
    /// productiva definitiva.
    /// </summary>
    public bool WasConsumed =>
        _wasConsumed;

    /// <summary>
    /// Indica si el lote se encuentra preparado para recibir
    /// una decisión global Approved o Declined.
    /// </summary>
    public bool IsReadyForDecision =>
        !_wasConsumed &&
        BatchRequest.IsStructurallyValid &&
        PreparationResult.IsReadyForDecision;

    /// <summary>
    /// Marca esta preparación como consumida.
    ///
    /// La operación es irreversible para esta instancia.
    /// </summary>
    internal void MarkAsConsumed()
    {
        if (_wasConsumed)
        {
            throw new InvalidOperationException(
                "La preparación productiva por lote ya fue " +
                "consumida.");
        }

        _wasConsumed =
            true;
    }

    public string Summary =>
        _wasConsumed
            ? "La preparación productiva por lote ya recibió " +
              "su decisión definitiva."
            : PreparationResult.Summary;
}