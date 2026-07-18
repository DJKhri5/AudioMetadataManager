namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Representa las mediciones descriptivas obtenidas durante
/// el análisis espectral de un archivo de audio.
///
/// Este modelo no determina por sí solo la calidad del archivo
/// ni concluye que exista una transcodificación. Sus datos
/// serán utilizados posteriormente por módulos comparativos,
/// especialmente por el detector de bitrate efectivo.
/// </summary>
public class AudioSpectrumAnalysisResult
    : AnalysisModuleResult
{
    /// <summary>
    /// Duración técnica del audio decodificado.
    /// </summary>
    public TimeSpan TechnicalDuration { get; set; }

    /// <summary>
    /// Frecuencia de muestreo del flujo PCM.
    /// </summary>
    public int SampleRate { get; set; }

    /// <summary>
    /// Frecuencia máxima teórica representable según
    /// el teorema de Nyquist.
    /// </summary>
    public double NyquistFrequencyHz =>
        SampleRate > 0
            ? SampleRate / 2.0
            : 0;

    /// <summary>
    /// Frecuencia máxima en la que se detectó contenido
    /// energético significativo.
    ///
    /// Este valor no constituye por sí solo una prueba
    /// de corte espectral o transcodificación.
    /// </summary>
    public double HighestSignificantFrequencyHz { get; set; }

    /// <summary>
    /// Frecuencia aproximada donde comienza una disminución
    /// persistente de energía en la zona superior del espectro.
    /// </summary>
    public double EstimatedHighFrequencyRolloffHz { get; set; }

    /// <summary>
    /// Nivel energético medio del espectro expresado en dBFS.
    /// </summary>
    public double AverageSpectrumEnergyDb { get; set; }

    /// <summary>
    /// Nivel energético máximo detectado en el espectro,
    /// expresado en dBFS.
    /// </summary>
    public double PeakSpectrumEnergyDb { get; set; }

    /// <summary>
    /// Cantidad total de ventanas FFT procesadas.
    /// </summary>
    public int ProcessedWindowCount { get; set; }

    /// <summary>
    /// Tamaño de la transformada FFT utilizada.
    /// </summary>
    public int FftSize { get; set; }

    /// <summary>
    /// Duración aproximada de cada ventana FFT.
    /// </summary>
    public TimeSpan WindowDuration { get; set; }

    /// <summary>
    /// Resolución frecuencial obtenida por cada bin FFT.
    /// </summary>
    public double FrequencyResolutionHz
    {
        get
        {
            if (SampleRate <= 0 ||
                FftSize <= 0)
            {
                return 0;
            }

            return
                (double)SampleRate /
                FftSize;
        }
    }

    /// <summary>
    /// Indica si el espectro contiene mediciones suficientes
    /// para participar en comparaciones posteriores.
    /// </summary>
    public override bool HasComparisonData =>
        AnalysisCompleted &&
        IsReliable &&
        !HasError &&
        SampleRate > 0 &&
        FftSize > 0 &&
        ProcessedWindowCount > 0;

    /// <summary>
    /// Duración técnica formateada.
    /// </summary>
    public string TechnicalDurationDisplay =>
        FormatDuration(
            TechnicalDuration);

    /// <summary>
    /// Frecuencia de Nyquist formateada.
    /// </summary>
    public string NyquistFrequencyDisplay =>
        FormatFrequency(
            NyquistFrequencyHz);

    /// <summary>
    /// Frecuencia significativa más alta formateada.
    /// </summary>
    public string HighestSignificantFrequencyDisplay =>
        FormatFrequency(
            HighestSignificantFrequencyHz);

    /// <summary>
    /// Caída superior estimada formateada.
    /// </summary>
    public string EstimatedHighFrequencyRolloffDisplay =>
        FormatFrequency(
            EstimatedHighFrequencyRolloffHz);

    /// <summary>
    /// Resolución frecuencial formateada.
    /// </summary>
    public string FrequencyResolutionDisplay =>
        FrequencyResolutionHz > 0
            ? $"{FrequencyResolutionHz:0.00} Hz"
            : "Sin información";

    /// <summary>
    /// Energía espectral media formateada.
    /// </summary>
    public string AverageSpectrumEnergyDisplay =>
        FormatDecibels(
            AverageSpectrumEnergyDb);

    /// <summary>
    /// Energía espectral máxima formateada.
    /// </summary>
    public string PeakSpectrumEnergyDisplay =>
        FormatDecibels(
            PeakSpectrumEnergyDb);

    /// <summary>
    /// Formatea una duración con precisión de milisegundos.
    /// </summary>
    private static string FormatDuration(
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
    /// Formatea una frecuencia en Hz o kHz.
    /// </summary>
    private static string FormatFrequency(
        double value)
    {
        if (double.IsNaN(value) ||
            double.IsInfinity(value) ||
            value <= 0)
        {
            return "Sin información";
        }

        if (value >= 1000)
        {
            return
                $"{value / 1000.0:0.00} kHz";
        }

        return $"{value:0.00} Hz";
    }

    /// <summary>
    /// Formatea un valor energético en dBFS.
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