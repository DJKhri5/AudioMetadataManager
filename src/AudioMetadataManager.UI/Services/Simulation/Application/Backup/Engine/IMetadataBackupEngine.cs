using AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.Engine;

/// <summary>
/// Define el contrato para crear y verificar respaldos antes
/// de modificar los metadatos de un archivo.
/// </summary>
public interface IMetadataBackupEngine
{
    /// <summary>
    /// Crea y verifica una copia de seguridad.
    /// </summary>
    Task<MetadataBackupResult> CreateBackupAsync(
        MetadataBackupRequest request,
        IProgress<MetadataBackupProgress>? progress = null,
        CancellationToken cancellationToken = default);
}