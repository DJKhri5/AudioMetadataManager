namespace AudioMetadataManager.UI.Models;

public class FileSimulationResult
{
    public string OriginalFileName { get; set; } = string.Empty;

    public string ProposedFileName { get; set; } = string.Empty;

    public List<SimulationChange> Changes { get; set; } = new();

    public bool RequiresManualReview { get; set; }

    public bool CanApplyAutomatically { get; set; }

    public int ConfidenceScore { get; set; }

    public string Summary { get; set; } = string.Empty;

    public bool HasChanges =>
        Changes.Any(change => change.HasChange);

    public int ChangeCount =>
        Changes.Count(change => change.HasChange);
}