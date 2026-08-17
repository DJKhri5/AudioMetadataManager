namespace AudioMetadataManager.UI.Services.Renaming.Models;

/// <summary>
/// Representa el resultado detallado de la validación de renombrado para un archivo.
/// </summary>
public class RenameValidationResult
{
    public RenameValidationStatus Status { get; set; }

    public string CurrentFilePath { get; set; } = string.Empty;

    public string CurrentFileName { get; set; } = string.Empty;

    public string ProposedFileName { get; set; } = string.Empty;

    public string SanitizedFileName { get; set; } = string.Empty;

    public string TargetFilePath { get; set; } = string.Empty;

    public bool CanProceed => Status == RenameValidationStatus.ReadyToRename;

    public bool IsNoOp => Status == RenameValidationStatus.IdenticalNameNoOp;

    public bool HasCollision =>
        Status == RenameValidationStatus.DestinationCollisionDisk ||
        Status == RenameValidationStatus.DestinationCollisionBatch;

    public string Message { get; set; } = string.Empty;

    public string DiagnosticDetail { get; set; } = string.Empty;
}
