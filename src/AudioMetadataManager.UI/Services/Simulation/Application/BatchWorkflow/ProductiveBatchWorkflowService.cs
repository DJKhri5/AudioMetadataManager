using AudioMetadataManager.UI.Services.Simulation
    .Application.Coordination;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.BatchWorkflow;

/// <summary>
/// Orquesta el workflow productivo por lote utilizado por la
/// interfaz.
///
/// Mantiene separada la preparación de la decisión final y
/// delega la seguridad de escritura, promoción y limpieza al
/// coordinador productivo two-phase existente.
/// </summary>
public sealed class ProductiveBatchWorkflowService :
    IProductiveBatchWorkflowService
{
    private readonly IMetadataProductiveTwoPhaseBatchCoordinator
        _batchCoordinator;

    public ProductiveBatchWorkflowService()
        : this(
            new MetadataProductiveTwoPhaseBatchCoordinator())
    {
    }

    public ProductiveBatchWorkflowService(
        IMetadataProductiveTwoPhaseBatchCoordinator
            batchCoordinator)
    {
        _batchCoordinator =
            batchCoordinator ??
            throw new ArgumentNullException(
                nameof(batchCoordinator));
    }

    public async Task<ProductiveBatchPreparation> PrepareAsync(
        MetadataApplyBatchRequest batchRequest,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            batchRequest);

        cancellationToken.ThrowIfCancellationRequested();

        if (!batchRequest.IsStructurallyValid)
        {
            throw new InvalidOperationException(
                "La solicitud productiva por lote no es " +
                "estructuralmente válida.");
        }

        MetadataProductiveBatchPreparationResult
            preparationResult =
                await _batchCoordinator.PrepareAsync(
                    batchRequest,
                    cancellationToken);

        return
            new ProductiveBatchPreparation
            {
                BatchRequest =
                    batchRequest,

                PreparationResult =
                    preparationResult
            };
    }

    public async Task<MetadataProductiveBatchCompletionResult>
        CompleteAsync(
            ProductiveBatchPreparation preparation,
            MetadataPromotionDecision promotionDecision,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            preparation);

        cancellationToken.ThrowIfCancellationRequested();

        if (!preparation.IsReadyForDecision)
        {
            throw new InvalidOperationException(
                "El lote no se encuentra preparado para recibir " +
                "una decisión productiva global.");
        }

        if (promotionDecision is not
            MetadataPromotionDecision.Approved and not
            MetadataPromotionDecision.Declined)
        {
            throw new ArgumentOutOfRangeException(
                nameof(promotionDecision),
                promotionDecision,
                "La decisión productiva debe ser Approved o " +
                "Declined.");
        }

        MetadataProductiveBatchCompletionResult
            completionResult =
                await _batchCoordinator.CompleteAsync(
                    preparation.PreparationResult,
                    promotionDecision,
                    cancellationToken);

        preparation.MarkAsConsumed();

        return
            completionResult;
    }
}