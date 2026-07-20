namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Representa una medición objetiva de la extensión y la
/// caída de la zona superior del espectro.
///
/// Este modelo no determina por sí mismo si el archivo está
/// transcodificado ni si su origen es una fuente con pérdida.
///
/// Sus datos pueden ser reutilizados por distintas reglas del
/// motor de calidad sin volver a procesar el archivo ni
/// ejecutar una segunda FFT.
/// </summary>
public class AudioSpectrumCutoffMeasurement
{
    /// <summary>
    /// Indica si la medición pudo completarse.
    /// </summary>
    public bool MeasurementCompleted { get; init; }

    /// <summary>
    /// Indica si los datos disponibles permiten utilizar
    /// esta medición en comparaciones posteriores.
    /// </summary>
    public bool IsReliable { get; init; }

    /// <summary>
    /// Frecuencia de Nyquist correspondiente al archivo.
    /// </summary>
    public double NyquistFrequencyHz { get; init; }

    /// <summary>
    /// Frecuencia superior hasta la cual se observó contenido
    /// espectral significativo, aunque no necesariamente
    /// persistente.
    /// </summary>
    public double HighestSignificantFrequencyHz { get; init; }

    /// <summary>
    /// Frecuencia superior hasta la cual se observó contenido
    /// espectral persistente.
    /// </summary>
    public double HighestPersistentFrequencyHz { get; init; }

    /// <summary>
    /// Frecuencia superior hasta la cual se observó contenido
    /// con persistencia fuerte.
    /// </summary>
    public double HighestStrongPersistentFrequencyHz { get; init; }

    /// <summary>
    /// Frecuencia estimada de caída superior ya calculada por
    /// el análisis espectral.
    /// </summary>
    public double EstimatedCutoffFrequencyHz { get; init; }

    /// <summary>
    /// Diferencia entre Nyquist y la frecuencia estimada de
    /// caída superior.
    /// </summary>
    public double CutoffDistanceFromNyquistHz =>
        HasNyquistFrequency &&
        HasEstimatedCutoffFrequency
            ? Math.Max(
                0,
                NyquistFrequencyHz -
                EstimatedCutoffFrequencyHz)
            : 0;

    /// <summary>
    /// Proporción de la frecuencia de Nyquist alcanzada por
    /// la caída superior estimada.
    /// </summary>
    public double NyquistCoverageRatio =>
        HasNyquistFrequency &&
        HasEstimatedCutoffFrequency
            ? Math.Clamp(
                EstimatedCutoffFrequencyHz /
                NyquistFrequencyHz,
                0,
                1)
            : 0;

    /// <summary>
    /// Indica si existe una frecuencia de Nyquist válida.
    /// </summary>
    public bool HasNyquistFrequency =>
        IsPositiveFinite(
            NyquistFrequencyHz);

    /// <summary>
    /// Indica si existe una frecuencia significativa válida.
    /// </summary>
    public bool HasHighestSignificantFrequency =>
        IsPositiveFinite(
            HighestSignificantFrequencyHz);

    /// <summary>
    /// Indica si existe una frecuencia persistente válida.
    /// </summary>
    public bool HasHighestPersistentFrequency =>
        IsPositiveFinite(
            HighestPersistentFrequencyHz);

    /// <summary>
    /// Indica si existe una frecuencia con persistencia
    /// fuerte válida.
    /// </summary>
    public bool HasHighestStrongPersistentFrequency =>
        IsPositiveFinite(
            HighestStrongPersistentFrequencyHz);

    /// <summary>
    /// Indica si existe una frecuencia estimada de caída
    /// superior válida.
    /// </summary>
    public bool HasEstimatedCutoffFrequency =>
        IsPositiveFinite(
            EstimatedCutoffFrequencyHz);

    /// <summary>
    /// Indica si existe información mínima utilizable.
    /// </summary>
    public bool HasComparisonData =>
        MeasurementCompleted &&
        IsReliable &&
        HasNyquistFrequency &&
        (
            HasEstimatedCutoffFrequency ||
            HasHighestPersistentFrequency ||
            HasHighestStrongPersistentFrequency);

    /// <summary>
    /// Frecuencia estimada de caída en formato legible.
    /// </summary>
    public string EstimatedCutoffFrequencyDisplay =>
        FormatFrequency(
            EstimatedCutoffFrequencyHz);

    /// <summary>
    /// Frecuencia persistente superior en formato legible.
    /// </summary>
    public string HighestPersistentFrequencyDisplay =>
        FormatFrequency(
            HighestPersistentFrequencyHz);

    /// <summary>
    /// Frecuencia con persistencia fuerte en formato legible.
    /// </summary>
    public string HighestStrongPersistentFrequencyDisplay =>
        FormatFrequency(
            HighestStrongPersistentFrequencyHz);

    /// <summary>
    /// Cobertura de Nyquist en formato porcentual.
    /// </summary>
    public string NyquistCoverageDisplay =>
        HasNyquistFrequency &&
        HasEstimatedCutoffFrequency
            ? $"{NyquistCoverageRatio * 100.0:0.00}%"
            : "Sin información";

    /// <summary>
    /// Comprueba si una frecuencia es positiva y finita.
    /// </summary>
    private static bool IsPositiveFinite(
        double value)
    {
        return value > 0 &&
            !double.IsNaN(
                value) &&
            !double.IsInfinity(
                value);
    }

    /// <summary>
    /// Formatea una frecuencia para mostrarla de manera
    /// legible.
    /// </summary>
    private static string FormatFrequency(
        double frequencyHz)
    {
        if (!IsPositiveFinite(
                frequencyHz))
        {
            return "Sin información";
        }

        if (frequencyHz >= 1000)
        {
            return
                $"{frequencyHz / 1000.0:0.00} kHz";
        }

        return
            $"{frequencyHz:0.00} Hz";
    }
}