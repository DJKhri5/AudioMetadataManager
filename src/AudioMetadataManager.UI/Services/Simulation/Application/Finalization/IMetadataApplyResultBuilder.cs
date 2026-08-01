using AudioMetadataManager.UI.Services.Simulation
    .Application.Context;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Finalization;

/// <summary>
/// Define el contrato para construir el resultado final,
/// consolidado y auditable de una aplicación de metadatos.
/// </summary>
public interface IMetadataApplyResultBuilder
{
    /// <summary>
    /// Construye el resultado final utilizando el estado
    /// acumulado en el contexto del pipeline.
    /// </summary>
    MetadataApplyResult Build(
        MetadataApplicationContext context);
}