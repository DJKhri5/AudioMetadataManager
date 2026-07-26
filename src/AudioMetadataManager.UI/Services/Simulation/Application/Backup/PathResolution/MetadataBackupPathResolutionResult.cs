namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.PathResolution;

/// <summary>
/// Contiene el resultado de resolver la ubicación final de un
/// respaldo.
/// </summary>
public sealed class MetadataBackupPathResolutionResult
{
    /// <summary>
    /// Ruta raíz seleccionada para los respaldos.
    /// </summary>
    public string RootBackupDirectory { get; init; } =
        string.Empty;

    /// <summary>
    /// Carpeta final que contendrá el archivo.
    /// </summary>
    public string BackupDirectoryPath { get; init; } =
        string.Empty;

    /// <summary>
    /// Ruta completa final del respaldo.
    /// </summary>
    public string BackupFilePath { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si fue necesario modificar el nombre para evitar
    /// una colisión.
    /// </summary>
    public bool UsedUniqueFileName { get; init; }

    /// <summary>
    /// Indica si la ruta pudo resolverse correctamente.
    /// </summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(
            RootBackupDirectory) &&
        !string.IsNullOrWhiteSpace(
            BackupDirectoryPath) &&
        !string.IsNullOrWhiteSpace(
            BackupFilePath);

    /// <summary>
    /// Resumen legible.
    /// </summary>
    public string Summary =>
        IsValid
            ? $"Ruta de respaldo resuelta: " +
              $"{BackupFilePath}."
            : "No fue posible resolver una ruta de respaldo.";
}