namespace AudioMetadataManager.UI.Models;

public class AudioFile
{
    // Información del archivo
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }

    // Metadatos
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string Album { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public uint Year { get; set; }

    // Audio
    public TimeSpan Duration { get; set; }
    public int Bitrate { get; set; }
    public int SampleRate { get; set; }
    public int Channels { get; set; }

    public string DurationDisplay =>
    Duration.TotalHours >= 1
        ? Duration.ToString(@"hh\:mm\:ss")
        : Duration.ToString(@"mm\:ss");

    public string BitrateDisplay =>
        Bitrate > 0 ? $"{Bitrate} kbps" : "—";

    public string SampleRateDisplay =>
        SampleRate > 0 ? $"{SampleRate / 1000.0:0.#} kHz" : "—";

    public string FileSizeDisplay =>
        $"{FileSizeBytes / 1024d / 1024d:0.00} MB";

    // Resultado del parser
    public ParsedFileName? ParsedName { get; set; }

    // Resultado de la comparación
    public ComparisonResult? Comparison { get; set; }

    // Resultado del motor de análisis
    public AnalysisResult? Analysis { get; set; }

    // Resultado del análisis técnico de calidad
    public AudioQualityResult? QualityAnalysis { get; set; }

    // Valores para la interfaz
    public string ConfidenceScoreDisplay =>
    Analysis == null
        ? "—"
        : $"{Analysis.ConfidenceScore}%";

    public string ConfidenceLevelDisplay =>
        Analysis?.ConfidenceLevel ?? "Sin analizar";

    public string ManualReviewDisplay =>
        Analysis == null
            ? "—"
            : Analysis.RequiresManualReview
                ? "Sí"
                : "No";

    public string AnalysisSummaryDisplay =>
        Analysis?.Summary ?? string.Empty;

    // Valores de calidad para la interfaz
    public string QualityScoreDisplay =>
        QualityAnalysis == null
            ? "—"
            : $"{QualityAnalysis.QualityScore}%";

    public string QualityLevelDisplay =>
        QualityAnalysis?.QualityLevel ?? "Sin analizar";

    public string QualityReviewDisplay =>
        QualityAnalysis == null
            ? "—"
            : QualityAnalysis.RequiresManualReview
                ? "Sí"
                : "No";

    public string QualitySummaryDisplay =>
        QualityAnalysis?.Summary ?? string.Empty;

    public string ParsedArtistDisplay =>
        ParsedName?.Artist ?? string.Empty;

    public string ParsedTitleDisplay =>
        ParsedName?.Title ?? string.Empty;

    public string ParsedVersionDisplay =>
        ParsedName?.Version ?? string.Empty;

    public string ArtistMatchDisplay =>
        Comparison == null
            ? "—"
            : Comparison.ArtistMatches ? "Sí" : "No";

    public string TitleMatchDisplay =>
        Comparison == null
            ? "—"
            : Comparison.TitleMatches ? "Sí" : "No";


    // Estado
    public string Status { get; set; } = "Pendiente";
}