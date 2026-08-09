using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Coordination;

/// <summary>
/// Define una coordinación productiva por lote separada en
/// preparación y decisión global.
/// </summary>
public interface IMetadataProductiveTwoPhaseBatchCoordinator
{
    /// <summary>
    /// Prepara secuencialmente todas las solicitudes válidas
    /// sin promover todavía ningún archivo original.
    /// </summary>
    Task<MetadataProductiveBatchPreparationResult>
        PrepareAsync(
            MetadataApplyBatchRequest batchRequest,
            CancellationToken cancellationToken = default);

    /// <summary>
    /// Finaliza todas las preparaciones verificadas utilizando
    /// una única decisión global Approved o Declined.
    ///
    /// Si una finalización falla, las preparaciones posteriores
    /// que sigan pendientes son descartadas mediante Declined.
    /// </summary>
    Task<MetadataProductiveBatchCompletionResult>
        CompleteAsync(
            MetadataProductiveBatchPreparationResult
                preparationResult,
            MetadataPromotionDecision promotionDecision,
            CancellationToken cancellationToken = default);
}