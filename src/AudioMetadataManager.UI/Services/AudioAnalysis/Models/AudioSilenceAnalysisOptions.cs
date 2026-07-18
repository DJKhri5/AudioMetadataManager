namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Configuración utilizada por AudioSilenceAnalyzer.
///
/// Los valores predeterminados son conservadores.
/// El programa no recorta ni modifica audio automáticamente.
/// </summary>
public class AudioSilenceAnalysisOptions
{
    /// <summary>
    /// Nivel máximo considerado silencio, expresado en dBFS.
    ///
    /// -50 dBFS permite ignorar ruido digital muy bajo,
    /// pero evita clasificar automáticamente como silencio
    /// una introducción musical claramente audible.
    /// </summary>
    public double SilenceThresholdDb { get; set; } =
        -50.0;

    /// <summary>
    /// Tiempo mínimo durante el cual debe existir audio
    /// sobre el umbral para confirmar que comenzó o terminó
    /// el contenido audible.
    ///
    /// Evita que un clic aislado sea interpretado como música.
    /// </summary>
    public TimeSpan MinimumAudibleDuration { get; set; } =
        TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Silencio inicial a partir del cual se recomendará
    /// una revisión manual.
    /// </summary>
    public TimeSpan SuspiciousLeadingSilenceLimit { get; set; } =
        TimeSpan.FromSeconds(3);

    /// <summary>
    /// Silencio final a partir del cual se recomendará
    /// una revisión manual.
    /// </summary>
    public TimeSpan SuspiciousTrailingSilenceLimit { get; set; } =
        TimeSpan.FromSeconds(5);

    /// <summary>
    /// Tamaño de los bloques solicitados al lector PCM.
    /// </summary>
    public int FramesPerBlock { get; set; } =
        4096;

    /// <summary>
    /// Comprueba que la configuración pueda utilizarse.
    /// </summary>
    public void Validate()
    {
        if (double.IsNaN(SilenceThresholdDb) ||
            double.IsInfinity(SilenceThresholdDb) ||
            SilenceThresholdDb >= 0 ||
            SilenceThresholdDb < -120)
        {
            throw new ArgumentOutOfRangeException(
                nameof(SilenceThresholdDb),
                SilenceThresholdDb,
                "El umbral de silencio debe estar entre " +
                "-120 y 0 dBFS.");
        }

        if (MinimumAudibleDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MinimumAudibleDuration),
                "La duración audible mínima debe ser " +
                "mayor que cero.");
        }

        if (SuspiciousLeadingSilenceLimit < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(SuspiciousLeadingSilenceLimit),
                "El límite de silencio inicial no puede " +
                "ser negativo.");
        }

        if (SuspiciousTrailingSilenceLimit < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(SuspiciousTrailingSilenceLimit),
                "El límite de silencio final no puede " +
                "ser negativo.");
        }

        if (FramesPerBlock <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(FramesPerBlock),
                "La cantidad de frames por bloque debe ser " +
                "mayor que cero.");
        }
    }
}