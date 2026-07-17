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
    public int BitsPerSample { get; set; }
    public string CodecName { get; set; } = string.Empty;

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

    // Resultado de la simulación de cambios
    public FileSimulationResult? Simulation { get; set; }

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

    public string SimulationStatusDisplay =>
    Simulation == null
        ? "Sin simulación"
        : Simulation.HasChanges
            ? $"{Simulation.ChangeCount} cambio(s)"
            : "Sin cambios";

    public string ProposedFileNameDisplay =>
        Simulation?.ProposedFileName ?? string.Empty;

    public string CanApplyAutomaticallyDisplay =>
        Simulation == null
            ? "—"
            : Simulation.CanApplyAutomatically
                ? "Sí"
                : "No";

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

    public string CodecDisplay =>
    string.IsNullOrWhiteSpace(QualityAnalysis?.CodecName)
        ? "Sin determinar"
        : QualityAnalysis.CodecName;

    public string CompressionTypeDisplay =>
        QualityAnalysis?.CompressionType ?? "Desconocida";

    public string BitrateModeDisplay =>
        QualityAnalysis?.BitrateMode ?? "Sin determinar";

    public string BitsPerSampleDisplay =>
        QualityAnalysis == null || QualityAnalysis.BitsPerSample <= 0
            ? "—"
            : $"{QualityAnalysis.BitsPerSample} bits";

    public string LosslessDisplay =>
        QualityAnalysis == null
            ? "—"
            : QualityAnalysis.IsLossless
                ? "Sí"
                : "No";

    // Estado
    public string Status { get; set; } = "Pendiente";
}