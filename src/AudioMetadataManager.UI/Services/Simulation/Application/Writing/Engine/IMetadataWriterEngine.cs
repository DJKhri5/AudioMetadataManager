using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Engine;

/// <summary>
/// Define el contrato para ejecutar una solicitud técnica de
/// escritura de metadatos.
/// </summary>
public interface IMetadataWriterEngine
{
    /// <summary>
    /// Ejecuta la escritura mediante el escritor compatible con
    /// el formato del archivo.
    /// </summary>
    Task<MetadataWriteResult> WriteAsync(
        MetadataWriteRequest request,
        CancellationToken cancellationToken = default);
}