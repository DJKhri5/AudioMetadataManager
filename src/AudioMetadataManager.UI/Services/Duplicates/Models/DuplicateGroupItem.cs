using AudioMetadataManager.UI.Models;

namespace AudioMetadataManager.UI.Services.Duplicates.Models;

/// <summary>
/// Representa un archivo individual dentro de un grupo de duplicados,
/// enriquecido con su puntuación relativa de calidad y diagnóstico comparativo.
/// </summary>
public sealed class DuplicateGroupItem
{
    public required AudioFile File { get; init; }

    /// <summary>
    /// Puntuación técnica de calidad calculada (Lossless > Alto Bitrate > Frecuencia).
    /// </summary>
    public int QualityScore { get; init; }

    /// <summary>
    /// Indica si este archivo es la versión de mayor calidad detectada dentro de su grupo.
    /// </summary>
    public bool IsBestQualityCandidate { get; set; }

    /// <summary>
    /// Etiqueta visual para orientar la decisión del usuario.
    /// </summary>
    public string QualityBadge =>
        IsBestQualityCandidate
            ? "Mejor versión recomendada"
            : "Copia redundante / menor calidad";

    public string FileName => File.FileName;

    public string FullPath => File.FullPath;

    public string Extension => (File.Extension ?? string.Empty).ToUpperInvariant();

    public int BitrateKbps => File.Bitrate;

    public int SampleRateHz => File.SampleRate;

    public string BitrateDisplay =>
        BitrateKbps > 0 ? $"{BitrateKbps} kbps" : "Desconocido";

    public string SampleRateDisplay =>
        SampleRateHz > 0 ? $"{SampleRateHz:N0} Hz" : "Desconocido";

    public string DurationDisplay =>
        File.Duration > TimeSpan.Zero ? File.Duration.ToString(@"mm\:ss") : "--:--";

    public long FileSizeBytes => File.FileSizeBytes;

    public string FileSizeDisplay
    {
        get
        {
            double mb = FileSizeBytes / (1024.0 * 1024.0);
            return $"{mb:F2} MB";
        }
    }

    public string FormatTier =>
        File.QualityAnalysis?.IsLossless == true ? "Sin pérdida (Lossless)" : "Con compresión (Lossy)";
}
