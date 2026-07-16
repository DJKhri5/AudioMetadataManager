namespace AudioMetadataManager.UI.Models;

public class AudioQualityResult
{
    public int QualityScore { get; set; }

    public string QualityLevel { get; set; } = "Sin analizar";

    public bool RequiresManualReview { get; set; } = true;

    public bool HasValidDuration { get; set; }

    public bool HasValidSampleRate { get; set; }

    public bool HasValidChannels { get; set; }

    public bool HasPlausibleBitrate { get; set; }

    public bool SpectralAnalysisCompleted { get; set; }

    public string Summary { get; set; } = string.Empty;
}