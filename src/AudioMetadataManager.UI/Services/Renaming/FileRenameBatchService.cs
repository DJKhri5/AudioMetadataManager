using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.Renaming.Models;

namespace AudioMetadataManager.UI.Services.Renaming;

/// <summary>
/// Servicio para coordinar la preparación, ejecución y reversión segura
/// de operaciones de renombrado de archivos por lote.
/// </summary>
public sealed class FileRenameBatchService : IFileRenameBatchService
{
    private readonly FileRenameService _renameService;
    private readonly FileRenameCollisionDetector _collisionDetector;

    public FileRenameBatchService(
        FileRenameService? renameService = null,
        FileRenameCollisionDetector? collisionDetector = null)
    {
        _collisionDetector = collisionDetector ?? new FileRenameCollisionDetector();
        _renameService = renameService ?? new FileRenameService(_collisionDetector);
    }

    /// <summary>
    /// Prepara y valida todos los archivos del lote contra el disco y colisiones intra-lote.
    /// </summary>
    public FileRenameBatchPreparationResult PrepareBatch(IEnumerable<AudioFile> files)
    {
        if (files is null)
        {
            return new FileRenameBatchPreparationResult
            {
                Items = Array.Empty<FileRenameBatchItemValidation>()
            };
        }

        var fileList = files
            .Where(f => f is not null && !string.IsNullOrWhiteSpace(f.FullPath))
            .ToList();

        var itemValidations = new List<FileRenameBatchItemValidation>(fileList.Count);

        foreach (var file in fileList)
        {
            var validation = _collisionDetector.Validate(file, fileList);
            itemValidations.Add(new FileRenameBatchItemValidation
            {
                File = file,
                Validation = validation,
                IsSelected = validation.CanProceed
            });
        }

        return new FileRenameBatchPreparationResult
        {
            Items = itemValidations
        };
    }

    /// <summary>
    /// Ejecuta físicamente el renombrado de los archivos válidos y seleccionados del lote.
    /// </summary>
    public FileRenameBatchExecutionResult ExecuteBatch(
        FileRenameBatchPreparationResult preparation,
        bool onlySelected = true)
    {
        ArgumentNullException.ThrowIfNull(preparation);

        var allFiles = preparation.Items.Select(i => i.File).ToList();
        var candidateItems = preparation.Items
            .Where(i => i.Validation.CanProceed && (!onlySelected || i.IsSelected))
            .ToList();

        int skippedCount = preparation.Items.Count - candidateItems.Count;
        var results = new List<FileRenameResult>(candidateItems.Count);

        foreach (var item in candidateItems)
        {
            // Ejecutar el renombrado individual mediante el servicio base
            var result = _renameService.Rename(item.File, allFiles);
            results.Add(result);
        }

        return new FileRenameBatchExecutionResult
        {
            ItemResults = results,
            SkippedCount = skippedCount
        };
    }

    /// <summary>
    /// Revierte en orden inverso todas las operaciones de un lote utilizando la bitácora transaccional.
    /// </summary>
    public int RollbackBatch(FileRenameBatchExecutionResult batchResult, out List<string> rollbackErrors)
    {
        ArgumentNullException.ThrowIfNull(batchResult);

        rollbackErrors = new List<string>();
        int rolledBackCount = 0;

        // Revertir en orden inverso para garantizar consistencia
        var entries = batchResult.JournalEntries.Reverse().ToList();

        foreach (var entry in entries)
        {
            if (_renameService.Rollback(entry, out string error))
            {
                rolledBackCount++;
            }
            else
            {
                rollbackErrors.Add($"Error al revertir '{entry.NewFileName}': {error}");
            }
        }

        return rolledBackCount;
    }
}
