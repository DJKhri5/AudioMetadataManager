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

    // Información técnica avanzada
    public string CodecName { get; set; } = string.Empty;

    public string CompressionType { get; set; } = "Desconocida";

    public bool IsLossless { get; set; }

    public int BitsPerSample { get; set; }

    public string BitrateMode { get; set; } = "Sin determinar";

    public bool HasTechnicalWarnings { get; set; }

    public List<string> TechnicalWarnings { get; set; } = new();

    public string Summary { get; set; } = string.Empty;
}