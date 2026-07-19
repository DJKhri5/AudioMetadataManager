namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Contiene las mediciones espectrales obtenidas para una
/// banda de frecuencias concreta.
///
/// Este modelo es descriptivo. No clasifica la calidad del
/// audio ni genera recomendaciones por sí mismo.
/// </summary>
public class AudioFrequencyBandMeasurement
{
    /// <summary>
    /// Definición utilizada para delimitar la banda.
    /// </summary>
    public AudioFrequencyBandDefinition Definition { get; init; } =
        new();

    /// <summary>
    /// Cantidad de bins FFT incluidos en la medición.
    /// </summary>
    public int BinCount { get; init; }

    /// <summary>
    /// Magnitud media acumulada dentro de la banda,
    /// expresada en dBFS.
    /// </summary>
    public double AverageMagnitudeDb { get; init; }

    /// <summary>
    /// Magnitud máxima observada dentro de la banda,
    /// expresada en dBFS.
    /// </summary>
    public double PeakMagnitudeDb { get; init; }

    /// <summary>
    /// Frecuencia del bin que presentó la mayor magnitud
    /// media dentro de esta banda.
    /// </summary>
    public double DominantFrequencyHz { get; init; }

    /// <summary>
    /// Proporción media de ventanas significativas entre
    /// todos los bins incluidos en la banda.
    ///
    /// Los valores se encuentran entre 0 y 1.
    /// </summary>
    public double AveragePersistenceRatio { get; init; }

    /// <summary>
    /// Mayor proporción de persistencia observada entre
    /// los bins de la banda.
    ///
    /// Los valores se encuentran entre 0 y 1.
    /// </summary>
    public double PeakPersistenceRatio { get; init; }

    /// <summary>
    /// Proporción de la energía espectral total que
    /// corresponde a esta banda.
    ///
    /// Los valores se encuentran entre 0 y 1.
    /// </summary>
    public double TotalEnergyRatio { get; init; }

    /// <summary>
    /// Indica si la medición contiene datos utilizables.
    /// </summary>
    public bool IsValid =>
        Definition.IsValid &&
        BinCount > 0 &&
        IsFinite(AverageMagnitudeDb) &&
        IsFinite(PeakMagnitudeDb) &&
        IsFinite(DominantFrequencyHz) &&
        DominantFrequencyHz >=
            Definition.MinimumFrequencyHz &&
        DominantFrequencyHz <=
            Definition.MaximumFrequencyHz &&
        AveragePersistenceRatio >= 0 &&
        AveragePersistenceRatio <= 1 &&
        PeakPersistenceRatio >= 0 &&
        PeakPersistenceRatio <= 1 &&
        TotalEnergyRatio >= 0 &&
        TotalEnergyRatio <= 1;

    /// <summary>
    /// Nombre legible de la banda.
    /// </summary>
    public string DisplayName =>
        string.IsNullOrWhiteSpace(
            Definition.DisplayName)
            ? Definition.Band.ToString()
            : Definition.DisplayName.Trim();

    /// <summary>
    /// Rango frecuencial en formato legible.
    /// </summary>
    public string FrequencyRangeDisplay =>
        $"{FormatFrequency(Definition.MinimumFrequencyHz)} – " +
        $"{FormatFrequency(Definition.MaximumFrequencyHz)}";

    /// <summary>
    /// Magnitud media en formato legible.
    /// </summary>
    public string AverageMagnitudeDisplay =>
        FormatDecibels(
            AverageMagnitudeDb);

    /// <summary>
    /// Magnitud máxima en formato legible.
    /// </summary>
    public string PeakMagnitudeDisplay =>
        FormatDecibels(
            PeakMagnitudeDb);

    /// <summary>
    /// Frecuencia dominante en formato legible.
    /// </summary>
    public string DominantFrequencyDisplay =>
        FormatFrequency(
            DominantFrequencyHz);

    /// <summary>
    /// Persistencia media expresada como porcentaje.
    /// </summary>
    public string AveragePersistenceDisplay =>
        FormatPercentage(
            AveragePersistenceRatio);

    /// <summary>
    /// Persistencia máxima expresada como porcentaje.
    /// </summary>
    public string PeakPersistenceDisplay =>
        FormatPercentage(
            PeakPersistenceRatio);

    /// <summary>
    /// Participación de energía expresada como porcentaje.
    /// </summary>
    public string TotalEnergyRatioDisplay =>
        FormatPercentage(
            TotalEnergyRatio);

    /// <summary>
    /// Comprueba que un valor sea finito.
    /// </summary>
    private static bool IsFinite(
        double value)
    {
        return !double.IsNaN(value) &&
            !double.IsInfinity(value);
    }

    /// <summary>
    /// Formatea una frecuencia en Hz o kHz.
    /// </summary>
    private static string FormatFrequency(
        double frequencyHz)
    {
        if (!IsFinite(frequencyHz) ||
            frequencyHz < 0)
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

    /// <summary>
    /// Formatea una magnitud espectral.
    /// </summary>
    private static string FormatDecibels(
        double value)
    {
        return IsFinite(value)
            ? $"{value:0.00} dBFS"
            : "Sin información";
    }

    /// <summary>
    /// Formatea una proporción comprendida entre 0 y 1.
    /// </summary>
    private static string FormatPercentage(
        double ratio)
    {
        if (!IsFinite(ratio))
        {
            return "Sin información";
        }

        double clampedRatio =
            Math.Clamp(
                ratio,
                0,
                1);

        return
            $"{clampedRatio * 100.0:0.00}%";
    }
}