using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.BatchWorkflow;

/// <summary>
/// Define el workflow productivo por lote utilizado por
/// la interfaz.
///
/// El servicio no conoce controles WPF ni solicita decisiones
/// al usuario.
/// </summary>
public interface IProductiveBatchWorkflowService
{
    /// <summary>
    /// Prepara completamente un lote sin modificar todavía
    /// los archivos originales.
    /// </summary>
    Task<ProductiveBatchPreparation> PrepareAsync(
        MetadataApplyBatchRequest batchRequest,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Aplica una única decisión global a un lote previamente
    /// preparado.
    /// </summary>
    Task<MetadataProductiveBatchCompletionResult> CompleteAsync(
        ProductiveBatchPreparation preparation,
        MetadataPromotionDecision promotionDecision,
        CancellationToken cancellationToken = default);
}