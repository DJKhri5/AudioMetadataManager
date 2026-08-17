namespace AudioMetadataManager.UI.Services.Renaming.Models;

/// <summary>
/// Entrada de la bitácora transaccional para auditar y revertir operaciones de renombrado.
/// </summary>
public class RenameJournalEntry
{
    public Guid OperationId { get; set; } = Guid.NewGuid();

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    public string OriginalFilePath { get; set; } = string.Empty;

    public string RenamedFilePath { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string NewFileName { get; set; } = string.Empty;

    public string FileSha256 { get; set; } = string.Empty;

    public bool WasRolledBack { get; set; }

    public DateTime? RollbackTimestampUtc { get; set; }
}
