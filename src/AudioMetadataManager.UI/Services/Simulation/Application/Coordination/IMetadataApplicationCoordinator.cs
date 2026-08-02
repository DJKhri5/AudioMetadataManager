using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Coordination;

/// <summary>
/// Define el punto de entrada productivo para ejecutar una
/// solicitud aprobada mediante el pipeline modular de aplicación
/// de metadatos.
/// </summary>
public interface IMetadataApplicationCoordinator
{
    /// <summary>
    /// Ejecuta de forma segura una solicitud aprobada y devuelve
    /// el resultado completo, consolidado y auditable del
    /// pipeline.
    /// </summary>
    Task<MetadataApplicationPipelineResult> ExecuteAsync(
        MetadataApplyRequest request,
        CancellationToken cancellationToken = default);
}