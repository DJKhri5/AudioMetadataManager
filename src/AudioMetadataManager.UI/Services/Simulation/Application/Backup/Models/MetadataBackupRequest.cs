using System.IO;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Backup.Models;

/// <summary>
/// Contiene toda la información necesaria para solicitar una
/// copia de seguridad de un archivo original.
/// </summary>
public sealed class MetadataBackupRequest
{
    /// <summary>
    /// Identificador único de la solicitud de respaldo.
    /// </summary>
    public Guid BackupRequestId { get; init; } =
        Guid.NewGuid();

    /// <summary>
    /// Identificador de la solicitud de aplicación relacionada.
    /// </summary>
    public Guid ApplyRequestId { get; init; }

    /// <summary>
    /// Identificador del plan de simulación.
    /// </summary>
    public Guid PlanId { get; init; }

    /// <summary>
    /// Momento UTC en que se creó la solicitud.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; init; } =
        DateTimeOffset.UtcNow;

    /// <summary>
    /// Ruta completa del archivo que debe respaldarse.
    /// </summary>
    public string SourceFilePath { get; init; } =
        string.Empty;

    /// <summary>
    /// Nombre del archivo para diagnósticos.
    /// </summary>
    public string FileName { get; init; } =
        string.Empty;

    /// <summary>
    /// Ruta base de la biblioteca musical.
    ///
    /// Más adelante permitirá conservar la estructura relativa
    /// de carpetas durante operaciones por lotes.
    /// </summary>
    public string LibraryRootPath { get; init; } =
        string.Empty;

    /// <summary>
    /// Ruta de respaldo solicitada de forma explícita.
    ///
    /// Normalmente quedará vacía y será resuelta por el motor.
    /// </summary>
    public string RequestedBackupRootPath { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si esta solicitud contiene la identidad mínima
    /// necesaria.
    /// </summary>
    public bool IsStructurallyValid =>
        BackupRequestId != Guid.Empty &&
        ApplyRequestId != Guid.Empty &&
        PlanId != Guid.Empty &&
        !string.IsNullOrWhiteSpace(
            SourceFilePath);

    /// <summary>
    /// Ruta normalizada del archivo original.
    /// </summary>
    public string NormalizedSourceFilePath =>
        string.IsNullOrWhiteSpace(
            SourceFilePath)
                ? string.Empty
                : Path.GetFullPath(
                    SourceFilePath.Trim());

    /// <summary>
    /// Nombre utilizable del archivo.
    /// </summary>
    public string EffectiveFileName =>
        !string.IsNullOrWhiteSpace(
            FileName)
                ? FileName.Trim()
                : Path.GetFileName(
                    NormalizedSourceFilePath);

    /// <summary>
    /// Resumen compacto.
    /// </summary>
    public string Summary =>
        $"{EffectiveFileName}: solicitud de respaldo " +
        $"asociada al plan {PlanId}.";
}