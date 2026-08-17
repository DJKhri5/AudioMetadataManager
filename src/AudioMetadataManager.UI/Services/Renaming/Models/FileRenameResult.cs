namespace AudioMetadataManager.UI.Services.Renaming.Models;

/// <summary>
/// Resultado de una operación física de renombrado seguro.
/// </summary>
public class FileRenameResult
{
    public bool WasSuccessful { get; set; }

    public string OriginalFilePath { get; set; } = string.Empty;

    public string NewFilePath { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string NewFileName { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public RenameJournalEntry? JournalEntry { get; set; }

    public bool WasRolledBack { get; set; }
}
