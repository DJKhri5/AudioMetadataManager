namespace AudioMetadataManager.UI.Services.Renaming.Models;

/// <summary>
/// Contiene el resultado global tras ejecutar un lote de renombrado seguro.
/// </summary>
public sealed class FileRenameBatchExecutionResult
{
    public required IReadOnlyList<FileRenameResult> ItemResults { get; init; }

    public int SucceededCount =>
        ItemResults.Count(r => r.WasSuccessful);

    public int FailedCount =>
        ItemResults.Count(r => !r.WasSuccessful);

    public int SkippedCount { get; init; }

    public bool WasFullySuccessful =>
        FailedCount == 0 && SucceededCount > 0;

    public IReadOnlyList<RenameJournalEntry> JournalEntries =>
        ItemResults
            .Where(r => r.JournalEntry is not null)
            .Select(r => r.JournalEntry!)
            .ToList();

    public string Summary =>
        $"Lote de renombrado finalizado: {SucceededCount} archivo(s) renombrado(s) exitosamente, " +
        $"{FailedCount} con error, {SkippedCount} omitido(s).";
}
