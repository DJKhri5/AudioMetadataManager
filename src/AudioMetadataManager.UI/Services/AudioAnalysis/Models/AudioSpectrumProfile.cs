namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Contiene el perfil espectral acumulado de un archivo.
///
/// Este modelo conserva datos técnicos reutilizables por
/// distintos módulos sin repetir transformadas FFT ni volver
/// a leer el archivo de audio.
///
/// No determina por sí solo la calidad, el bitrate efectivo
/// ni la existencia de una transcodificación.
/// </summary>
public class AudioSpectrumProfile
{
    /// <summary>
    /// Frecuencia de muestreo del flujo PCM analizado.
    /// </summary>
    public int SampleRate { get; init; }

    /// <summary>
    /// Tamaño de la transformada FFT utilizada.
    /// </summary>
    public int FftSize { get; init; }

    /// <summary>
    /// Cantidad de ventanas FFT incorporadas al perfil.
    /// </summary>
    public int ProcessedWindowCount { get; init; }

    /// <summary>
    /// Frecuencia correspondiente a cada bin del espectro.
    ///
    /// La posición de cada elemento coincide con las listas
    /// AverageMagnitudeDb y PeakMagnitudeDb.
    /// </summary>
    public IReadOnlyList<double> FrequenciesHz { get; init; } =
        Array.Empty<double>();

    /// <summary>
    /// Magnitud espectral media acumulada para cada bin,
    /// expresada en dBFS.
    /// </summary>
    public IReadOnlyList<double> AverageMagnitudeDb { get; init; } =
        Array.Empty<double>();

    /// <summary>
    /// Magnitud máxima observada para cada bin,
    /// expresada en dBFS.
    /// </summary>
    public IReadOnlyList<double> PeakMagnitudeDb { get; init; } =
        Array.Empty<double>();

    /// <summary>
    /// Proporción de ventanas procesadas en las que cada bin
    /// superó el umbral de magnitud significativa.
    ///
    /// Los valores se encuentran entre 0 y 1.
    /// La posición coincide con FrequenciesHz.
    /// </summary>
    public IReadOnlyList<double> SignificantWindowRatios { get; init; } =
        Array.Empty<double>();

    /// <summary>
    /// Frecuencia máxima teórica representable.
    /// </summary>
    public double NyquistFrequencyHz =>
        SampleRate > 0
            ? SampleRate / 2.0
            : 0;

    /// <summary>
    /// Resolución de frecuencia entre bins consecutivos.
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
    /// Cantidad de bins positivos disponibles.
    /// </summary>
    public int BinCount =>
        FrequenciesHz.Count;

    /// <summary>
    /// Indica si el perfil contiene datos coherentes
    /// y utilizables por otros módulos.
    /// </summary>
    public bool IsValid =>
        SampleRate > 0 &&
        FftSize > 0 &&
        ProcessedWindowCount > 0 &&
        FrequenciesHz.Count > 0 &&
        FrequenciesHz.Count ==
            AverageMagnitudeDb.Count &&
        FrequenciesHz.Count ==
            PeakMagnitudeDb.Count &&
        FrequenciesHz.Count ==
            SignificantWindowRatios.Count;

    /// <summary>
    /// Obtiene el índice del bin más cercano a una frecuencia.
    /// </summary>
    public int GetNearestBinIndex(
        double frequencyHz)
    {
        if (!IsValid)
        {
            return -1;
        }

        if (frequencyHz <= 0)
        {
            return 0;
        }

        double clampedFrequency =
            Math.Min(
                frequencyHz,
                NyquistFrequencyHz);

        int index =
            (int)Math.Round(
                clampedFrequency /
                FrequencyResolutionHz);

        return Math.Clamp(
            index,
            0,
            BinCount - 1);
    }

    /// <summary>
    /// Obtiene la magnitud media del bin más cercano
    /// a una frecuencia determinada.
    /// </summary>
    public double GetAverageMagnitudeDb(
        double frequencyHz)
    {
        int index =
            GetNearestBinIndex(
                frequencyHz);

        return index >= 0
            ? AverageMagnitudeDb[index]
            : double.NaN;
    }

    /// <summary>
    /// Obtiene la magnitud máxima del bin más cercano
    /// a una frecuencia determinada.
    /// </summary>
    public double GetPeakMagnitudeDb(
        double frequencyHz)
    {
        int index =
            GetNearestBinIndex(
                frequencyHz);

        return index >= 0
            ? PeakMagnitudeDb[index]
            : double.NaN;
    }

    /// <summary>
    /// Obtiene la proporción de ventanas significativas
    /// correspondiente al bin más cercano a una frecuencia.
    /// </summary>
    public double GetSignificantWindowRatio(
        double frequencyHz)
    {
        int index =
            GetNearestBinIndex(
                frequencyHz);

        return index >= 0
            ? SignificantWindowRatios[index]
            : double.NaN;
    }

    /// <summary>
    /// Obtiene la energía media máxima existente dentro
    /// de un rango de frecuencias.
    /// </summary>
    public double GetMaximumAverageMagnitudeDb(
        double minimumFrequencyHz,
        double maximumFrequencyHz)
    {
        if (!IsValid ||
            maximumFrequencyHz <
            minimumFrequencyHz)
        {
            return double.NaN;
        }

        int startIndex =
            GetNearestBinIndex(
                minimumFrequencyHz);

        int endIndex =
            GetNearestBinIndex(
                maximumFrequencyHz);

        if (startIndex < 0 ||
            endIndex < 0)
        {
            return double.NaN;
        }

        double maximum =
            double.NegativeInfinity;

        for (int index = startIndex;
            index <= endIndex;
            index++)
        {
            if (AverageMagnitudeDb[index] >
                maximum)
            {
                maximum =
                    AverageMagnitudeDb[index];
            }
        }

        return double.IsNegativeInfinity(maximum)
            ? double.NaN
            : maximum;
    }
}