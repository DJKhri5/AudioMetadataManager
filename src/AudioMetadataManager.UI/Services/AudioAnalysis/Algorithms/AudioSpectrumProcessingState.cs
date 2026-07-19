using AudioMetadataManager.UI.Services.AudioAnalysis.Models;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Algorithms;

/// <summary>
/// Conserva el estado temporal utilizado durante el
/// procesamiento espectral de un flujo PCM.
///
/// Esta clase no abre archivos, no ejecuta decisiones de
/// calidad y no publica resultados en el contexto.
///
/// Su responsabilidad es mantener buffers, acumuladores y
/// contadores reutilizables durante una única ejecución del
/// análisis espectral.
/// </summary>
public class AudioSpectrumProcessingState
{
    /// <summary>
    /// Inicializa el estado para una configuración espectral.
    /// </summary>
    public AudioSpectrumProcessingState(
        AudioStreamInfo streamInfo,
        AudioSpectrumAnalysisOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            streamInfo);

        ArgumentNullException.ThrowIfNull(
            options);

        if (!streamInfo.IsValid)
        {
            throw new ArgumentException(
                "La información del flujo PCM no es válida.",
                nameof(streamInfo));
        }

        options.Validate();

        SampleRate =
            streamInfo.SampleRate;

        Channels =
            streamInfo.Channels;

        FftSize =
            options.FftSize;

        HopSize =
            CalculateHopSize(
                options.FftSize,
                options.WindowOverlap);

        PositiveBinCount =
            options.FftSize / 2 + 1;

        PendingMonoSamples =
            new List<float>(
                options.FftSize * 2);

        WindowBuffer =
            new double[options.FftSize];

        AverageMagnitudeLinearSums =
            new double[PositiveBinCount];

        PeakMagnitudeLinear =
            new double[PositiveBinCount];

        SignificantWindowCounts =
            new int[PositiveBinCount];

        FrequenciesHz =
            BuildFrequencyAxis(
                SampleRate,
                FftSize,
                PositiveBinCount);
    }

    /// <summary>
    /// Frecuencia de muestreo del flujo PCM.
    /// </summary>
    public int SampleRate { get; }

    /// <summary>
    /// Cantidad de canales del flujo PCM.
    /// </summary>
    public int Channels { get; }

    /// <summary>
    /// Tamaño de la FFT.
    /// </summary>
    public int FftSize { get; }

    /// <summary>
    /// Desplazamiento entre ventanas consecutivas.
    /// </summary>
    public int HopSize { get; }

    /// <summary>
    /// Cantidad de bins positivos, incluyendo DC y Nyquist.
    /// </summary>
    public int PositiveBinCount { get; }

    /// <summary>
    /// Muestras mono pendientes de formar una ventana FFT.
    ///
    /// Los canales del bloque PCM se combinan antes de
    /// almacenarse aquí.
    /// </summary>
    public List<float> PendingMonoSamples { get; }

    /// <summary>
    /// Buffer temporal utilizado para construir una ventana
    /// y aplicar posteriormente una función de ventana.
    /// </summary>
    public double[] WindowBuffer { get; }

    /// <summary>
    /// Acumulación lineal de magnitudes por bin.
    ///
    /// La conversión final a dBFS se realizará únicamente
    /// al completar el análisis.
    /// </summary>
    public double[] AverageMagnitudeLinearSums { get; }

    /// <summary>
    /// Magnitud lineal máxima detectada por cada bin.
    /// </summary>
    public double[] PeakMagnitudeLinear { get; }

    /// <summary>
    /// Cantidad de ventanas procesadas en las que cada bin
    /// superó el umbral de energía significativa.
    ///
    /// Esta medición permite diferenciar contenido persistente
    /// de ruido o artefactos aislados.
    /// </summary>
    public int[] SignificantWindowCounts { get; }

    /// <summary>
    /// Frecuencia correspondiente a cada bin positivo.
    /// </summary>
    public double[] FrequenciesHz { get; }

    /// <summary>
    /// Cantidad total de ventanas completas encontradas
    /// en el flujo PCM.
    /// </summary>
    public long AvailableWindowCount { get; set; }

    /// <summary>
    /// Cantidad de ventanas que fueron realmente procesadas.
    /// </summary>
    public int ProcessedWindowCount { get; set; }

    /// <summary>
    /// Cantidad de ventanas omitidas por la configuración
    /// de muestreo espectral.
    /// </summary>
    public int SkippedWindowCount { get; set; }

    /// <summary>
    /// Índice secuencial de la próxima ventana disponible.
    /// </summary>
    public long NextWindowIndex { get; set; }

    /// <summary>
    /// Cantidad total de frames PCM recibidos.
    /// </summary>
    public long TotalReceivedFrames { get; set; }

    /// <summary>
    /// Indica si el estado contiene una cantidad válida
    /// de información procesada.
    /// </summary>
    public bool HasProcessedData =>
        ProcessedWindowCount > 0;

    /// <summary>
    /// Duración aproximada de cada ventana FFT.
    /// </summary>
    public TimeSpan WindowDuration =>
        SampleRate > 0
            ? TimeSpan.FromSeconds(
                (double)FftSize /
                SampleRate)
            : TimeSpan.Zero;

    /// <summary>
    /// Resolución frecuencial de cada bin.
    /// </summary>
    public double FrequencyResolutionHz =>
        SampleRate > 0 &&
        FftSize > 0
            ? (double)SampleRate /
                FftSize
            : 0;

    /// <summary>
    /// Restablece los acumuladores de procesamiento,
    /// conservando la configuración inicial.
    /// </summary>
    public void Reset()
    {
        PendingMonoSamples.Clear();

        Array.Clear(
            WindowBuffer);

        Array.Clear(
            AverageMagnitudeLinearSums);

        Array.Clear(
            PeakMagnitudeLinear);

        Array.Clear(
            SignificantWindowCounts);

        AvailableWindowCount = 0;
        ProcessedWindowCount = 0;
        SkippedWindowCount = 0;
        NextWindowIndex = 0;
        TotalReceivedFrames = 0;
    }

    /// <summary>
    /// Determina si una ventana debe analizarse según la
    /// cantidad configurada de ventanas intermedias omitidas.
    /// </summary>
    public bool ShouldProcessWindow(
        int skippedWindowsBetweenAnalyses)
    {
        if (skippedWindowsBetweenAnalyses <= 0)
        {
            return true;
        }

        long interval =
            skippedWindowsBetweenAnalyses + 1L;

        return NextWindowIndex %
            interval == 0;
    }

    /// <summary>
    /// Calcula el desplazamiento entre ventanas FFT.
    /// </summary>
    private static int CalculateHopSize(
        int fftSize,
        double overlap)
    {
        double hop =
            fftSize *
            (1.0 - overlap);

        return Math.Max(
            1,
            (int)Math.Round(hop));
    }

    /// <summary>
    /// Construye el eje de frecuencias correspondiente
    /// a los bins positivos de la FFT.
    /// </summary>
    private static double[] BuildFrequencyAxis(
        int sampleRate,
        int fftSize,
        int binCount)
    {
        double[] frequencies =
            new double[binCount];

        double resolution =
            (double)sampleRate /
            fftSize;

        for (int index = 0;
            index < binCount;
            index++)
        {
            frequencies[index] =
                index *
                resolution;
        }

        return frequencies;
    }
}