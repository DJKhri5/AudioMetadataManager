using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.BatchWorkflow;

/// <summary>
/// Representa el resultado de la primera fase del workflow
/// productivo por lote.
/// </summary>
public sealed class ProductiveBatchPreparation
{
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
    /// Indica si el lote se encuentra preparado para recibir
    /// una decisión global Approved o Declined.
    /// </summary>
    public bool IsReadyForDecision =>
        BatchRequest.IsStructurallyValid &&
        PreparationResult.IsReadyForDecision;

    public string Summary =>
        PreparationResult.Summary;
}