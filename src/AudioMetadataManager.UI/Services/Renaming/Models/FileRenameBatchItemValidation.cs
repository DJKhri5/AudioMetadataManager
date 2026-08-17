using AudioMetadataManager.UI.Models;

namespace AudioMetadataManager.UI.Services.Renaming.Models;

/// <summary>
/// Representa el estado de validación y selección de un archivo individual dentro de un lote de renombrado.
/// </summary>
public sealed class FileRenameBatchItemValidation
{
    public required AudioFile File { get; init; }

    public required RenameValidationResult Validation { get; init; }

    /// <summary>
    /// Indica si este archivo está marcado para ser procesado durante la ejecución del lote.
    /// </summary>
    public bool IsSelected { get; set; } = true;

    public string FileName => File.FileName;

    public string FullPath => File.FullPath;

    public string ProposedFileName => Validation.ProposedFileName;

    public string SanitizedFileName => Validation.SanitizedFileName;

    public bool CanRename => Validation.CanProceed;

    public RenameValidationStatus Status => Validation.Status;

    public string StatusMessage => Validation.Message;

    public string StatusDisplay => Status switch
    {
        RenameValidationStatus.ReadyToRename => "Listo para renombrar",
        RenameValidationStatus.IdenticalNameNoOp => "Sin cambios",
        RenameValidationStatus.DestinationCollisionDisk => "Colisión en disco",
        RenameValidationStatus.DestinationCollisionBatch => "Colisión en lote",
        RenameValidationStatus.PathTooLong => "Ruta demasiado larga",
        RenameValidationStatus.SourceFileLocked => "Archivo en uso",
        RenameValidationStatus.SourceFileNotFound => "Archivo no encontrado",
        _ => "No ejecutable"
    };

    public string StatusBadgeColor => Status switch
    {
        RenameValidationStatus.ReadyToRename => "#166534", // Verde
        RenameValidationStatus.IdenticalNameNoOp => "#6B7280", // Gris
        RenameValidationStatus.DestinationCollisionDisk => "#DC2626", // Rojo
        RenameValidationStatus.DestinationCollisionBatch => "#EA580C", // Naranja
        RenameValidationStatus.SourceFileLocked => "#D97706", // Ámbar
        _ => "#DC2626"
    };

    public string StatusBadgeBackground => Status switch
    {
        RenameValidationStatus.ReadyToRename => "#DCFCE7",
        RenameValidationStatus.IdenticalNameNoOp => "#F3F4F6",
        RenameValidationStatus.DestinationCollisionDisk => "#FEE2E2",
        RenameValidationStatus.DestinationCollisionBatch => "#FFEDD5",
        RenameValidationStatus.SourceFileLocked => "#FEF3C7",
        _ => "#FEE2E2"
    };
}
