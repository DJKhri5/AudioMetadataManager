using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.Renaming.Models;

namespace AudioMetadataManager.UI.Services.Renaming;

public interface IFileRenameBatchService
{
    /// <summary>
    /// Prepara y valida todos los archivos del lote contra el disco y colisiones intra-lote.
    /// </summary>
    FileRenameBatchPreparationResult PrepareBatch(IEnumerable<AudioFile> files);

    /// <summary>
    /// Ejecuta físicamente el renombrado de los archivos válidos y seleccionados del lote.
    /// </summary>
    FileRenameBatchExecutionResult ExecuteBatch(FileRenameBatchPreparationResult preparation, bool onlySelected = true);

    /// <summary>
    /// Revierte en orden inverso todas las operaciones de un lote utilizando la bitácora transaccional.
    /// </summary>
    int RollbackBatch(FileRenameBatchExecutionResult batchResult, out List<string> rollbackErrors);
}
