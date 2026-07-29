using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Verification.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Verification.Engine;

/// <summary>
/// Define el contrato para verificar los metadatos persistidos
/// después de una operación de escritura.
/// </summary>
public interface IMetadataWriterVerificationEngine
{
    /// <summary>
    /// Reabre el archivo y compara los valores persistidos con
    /// los cambios que debían aplicarse.
    /// </summary>
    MetadataVerificationResult Verify(
        string? filePath,
        IEnumerable<MetadataFieldChange>? changes,
        int pictureCountBefore);
}