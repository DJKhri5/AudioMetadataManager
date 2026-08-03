using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Coordination;

/// <summary>
/// Define la coordinación completa de una aplicación productiva
/// individual de metadatos.
///
/// El coordinador prepara una copia verificada, registra la
/// decisión de promoción, promueve opcionalmente la copia hacia
/// el archivo original y finaliza el entorno temporal.
/// </summary>
public interface IMetadataProductiveApplicationCoordinator
{
    /// <summary>
    /// Prepara una copia aislada y verificada para una posterior
    /// decisión de promoción.
    ///
    /// Este método no modifica el archivo original.
    /// </summary>
    Task<MetadataProductiveApplicationResult>
        PrepareAsync(
            MetadataApplyRequest request,
            CancellationToken cancellationToken = default);

    /// <summary>
    /// Finaliza una preparación anterior aplicando la decisión de
    /// promoción indicada.
    ///
    /// Cuando la decisión es Approved, la copia verificada puede
    /// ser promovida hacia el archivo original.
    ///
    /// Cuando la decisión es Declined, el entorno temporal debe
    /// eliminarse sin modificar el archivo original.
    /// </summary>
    Task<MetadataProductiveApplicationResult>
        CompleteAsync(
            MetadataProductiveApplicationResult
                preparedResult,
            MetadataPromotionDecision promotionDecision,
            CancellationToken cancellationToken = default);
}