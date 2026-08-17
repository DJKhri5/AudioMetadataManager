namespace AudioMetadataManager.UI.Services.Renaming.Models;

/// <summary>
/// Contiene el diagnóstico consolidado de la preparación de un lote de renombrado.
/// </summary>
public sealed class FileRenameBatchPreparationResult
{
    public required IReadOnlyList<FileRenameBatchItemValidation> Items { get; init; }

    public int TotalCandidatesCount => Items.Count;

    public int ReadyToRenameCount =>
        Items.Count(i => i.Validation.CanProceed);

    public int SelectedReadyCount =>
        Items.Count(i => i.IsSelected && i.Validation.CanProceed);

    public int CollisionCount =>
        Items.Count(i => i.Status is RenameValidationStatus.DestinationCollisionDisk
                                  or RenameValidationStatus.DestinationCollisionBatch);

    public int UnchangedCount =>
        Items.Count(i => i.Status == RenameValidationStatus.IdenticalNameNoOp);

    public int OtherErrorsCount =>
        Items.Count(i => !i.Validation.CanProceed &&
                         i.Status != RenameValidationStatus.IdenticalNameNoOp &&
                         i.Status != RenameValidationStatus.DestinationCollisionDisk &&
                         i.Status != RenameValidationStatus.DestinationCollisionBatch);

    public bool HasAnyReadyToRename => ReadyToRenameCount > 0;

    public string Summary =>
        $"{ReadyToRenameCount} archivo(s) listo(s) para renombrar, " +
        $"{UnchangedCount} sin cambios, " +
        $"{CollisionCount} con conflicto/colisión.";
}
