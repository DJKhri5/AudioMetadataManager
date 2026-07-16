namespace AudioMetadataManager.UI.Models;

public class AnalysisResult
{
    public int ConfidenceScore { get; set; }

    public string ConfidenceLevel { get; set; } = "Sin analizar";

    public bool RequiresManualReview { get; set; } = true;

    public bool ArtistReliable { get; set; }

    public bool TitleReliable { get; set; }

    public bool VersionDetected { get; set; }

    public string Summary { get; set; } = string.Empty;
}