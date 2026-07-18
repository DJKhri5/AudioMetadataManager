namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Representa el resultado descriptivo del análisis
/// de la envolvente energética de un archivo de audio.
///
/// Este modelo no determina si el archivo es bueno o malo
/// y tampoco recomienda modificaciones. Únicamente conserva
/// mediciones que podrán compararse posteriormente con otras
/// fuentes y analizadores.
/// </summary>
public class AudioEnvelopeAnalysisResult
    : AnalysisModuleResult
{
    /// <summary>
    /// Duración total obtenida desde los frames PCM
    /// efectivamente decodificados.
    /// </summary>
    public TimeSpan TechnicalDuration { get; set; }

    /// <summary>
    /// Momento estimado en el que comienza el contenido
    /// musical relevante.
    ///
    /// Puede ser posterior al primer sonido audible cuando
    /// existe ruido, un efecto previo o una introducción
    /// extremadamente tenue.
    /// </summary>
    public TimeSpan EstimatedMusicalStart { get; set; }

    /// <summary>
    /// Momento estimado en el que termina el contenido
    /// musical relevante.
    ///
    /// Puede ser anterior al final técnico cuando existen
    /// silencios, ruido residual o colas de reverberación.
    /// </summary>
    public TimeSpan EstimatedMusicalEnd { get; set; }

    /// <summary>
    /// Duración musical efectiva estimada entre el comienzo
    /// y el final musical.
    /// </summary>
    public TimeSpan EstimatedMusicalDuration
    {
        get
        {
            TimeSpan duration =
                EstimatedMusicalEnd -
                EstimatedMusicalStart;

            return duration < TimeSpan.Zero
                ? TimeSpan.Zero
                : duration;
        }
    }

    /// <summary>
    /// Nivel energético medio del archivo expresado en dBFS.
    /// </summary>
    public double AverageEnergyDb { get; set; }

    /// <summary>
    /// Nivel energético máximo encontrado durante el análisis,
    /// expresado en dBFS.
    /// </summary>
    public double PeakEnergyDb { get; set; }

    /// <summary>
    /// Nivel energético mínimo útil encontrado durante el
    /// análisis, expresado en dBFS.
    /// </summary>
    public double MinimumEnergyDb { get; set; }

    /// <summary>
    /// Indica que la energía aumenta progresivamente cerca
    /// del comienzo del archivo.
    ///
    /// Es una observación descriptiva, no una anomalía.
    /// </summary>
    public bool HasPossibleFadeIn { get; set; }

    /// <summary>
    /// Indica que la energía disminuye progresivamente cerca
    /// del final del archivo.
    ///
    /// Es una observación descriptiva, no una anomalía.
    /// </summary>
    public bool HasPossibleFadeOut { get; set; }

    /// <summary>
    /// Indica que después del final musical estimado existe
    /// una cola energética tenue que podría corresponder a
    /// reverberación, eco o decaimiento natural.
    /// </summary>
    public bool HasPossibleReverbTail { get; set; }

    /// <summary>
    /// Cantidad de ventanas de análisis procesadas.
    /// </summary>
    public int ProcessedWindowCount { get; set; }

    /// <summary>
    /// Duración utilizada para cada ventana de medición.
    /// </summary>
    public TimeSpan WindowDuration { get; set; }

    /// <summary>
    /// Indica si existen datos suficientes para realizar
    /// comparaciones posteriores.
    /// </summary>
    public override bool HasComparisonData =>
        AnalysisCompleted &&
        IsReliable &&
        !HasError &&
        TechnicalDuration > TimeSpan.Zero &&
        ProcessedWindowCount > 0;

    /// <summary>
    /// Duración técnica formateada.
    /// </summary>
    public string TechnicalDurationDisplay =>
        FormatTime(TechnicalDuration);

    /// <summary>
    /// Inicio musical estimado formateado.
    /// </summary>
    public string EstimatedMusicalStartDisplay =>
        FormatTime(EstimatedMusicalStart);

    /// <summary>
    /// Final musical estimado formateado.
    /// </summary>
    public string EstimatedMusicalEndDisplay =>
        FormatTime(EstimatedMusicalEnd);

    /// <summary>
    /// Duración musical estimada formateada.
    /// </summary>
    public string EstimatedMusicalDurationDisplay =>
        FormatTime(EstimatedMusicalDuration);

    /// <summary>
    /// Energía media formateada.
    /// </summary>
    public string AverageEnergyDisplay =>
        FormatDecibels(AverageEnergyDb);

    /// <summary>
    /// Energía máxima formateada.
    /// </summary>
    public string PeakEnergyDisplay =>
        FormatDecibels(PeakEnergyDb);

    /// <summary>
    /// Energía mínima útil formateada.
    /// </summary>
    public string MinimumEnergyDisplay =>
        FormatDecibels(MinimumEnergyDb);

    /// <summary>
    /// Formatea una duración con precisión de milisegundos.
    /// </summary>
    private static string FormatTime(
        TimeSpan value)
    {
        if (value < TimeSpan.Zero)
        {
            value = TimeSpan.Zero;
        }

        if (value.TotalHours >= 1)
        {
            return value.ToString(
                @"h\:mm\:ss\.fff");
        }

        return value.ToString(
            @"m\:ss\.fff");
    }

    /// <summary>
    /// Formatea un nivel energético en dBFS.
    /// </summary>
    private static string FormatDecibels(
        double value)
    {
        if (double.IsNaN(value) ||
            double.IsInfinity(value))
        {
            return "Sin información";
        }

        return $"{value:0.00} dBFS";
    }
}