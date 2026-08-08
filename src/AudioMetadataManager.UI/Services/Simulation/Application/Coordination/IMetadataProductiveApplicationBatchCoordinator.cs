using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Coordination;

/// <summary>
/// Define la coordinación productiva de un lote de solicitudes
/// de aplicación de metadatos.
///
/// El coordinador por lote reutiliza la coordinación productiva
/// individual para cada solicitud válida y conserva un resultado
/// consolidado del procesamiento completo.
///
/// La decisión de promoción debe ser explícita.
/// </summary>
public interface IMetadataProductiveApplicationBatchCoordinator
{
    /// <summary>
    /// Procesa las solicitudes válidas contenidas en un lote
    /// mediante la coordinación productiva individual.
    ///
    /// La decisión indicada se aplica de forma explícita al
    /// procesamiento productivo del lote.
    ///
    /// La implementación debe respetar la cancelación solicitada
    /// y devolver un resultado consolidado incluso cuando el lote
    /// no pueda completarse totalmente.
    /// </summary>
    Task<MetadataApplyBatchResult>
        ExecuteAsync(
            MetadataApplyBatchRequest batchRequest,
            MetadataPromotionDecision promotionDecision,
            CancellationToken cancellationToken = default);
}