using AudioMetadataManager.UI.Services.AudioAnalysis.Interfaces;
using AudioMetadataManager.UI.Services.AudioAnalysis.Models;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Analyzers;

/// <summary>
/// Analiza la evolución energética de un archivo de audio
/// decodificado mediante ventanas RMS.
///
/// Este analizador solamente produce mediciones descriptivas.
/// No modifica el archivo ni recomienda recortes.
/// </summary>
public class AudioEnvelopeAnalyzer :
    IAudioAnalyzer<AudioEnvelopeAnalysisResult>
{
    private readonly IAudioSampleReader _sampleReader;
    private readonly AudioEnvelopeAnalysisOptions _options;

    /// <summary>
    /// Nombre legible del analizador.
    /// </summary>
    public string Name =>
        "Analizador de envolvente energética";

    /// <summary>
    /// Crea el analizador utilizando un lector PCM.
    /// </summary>
    public AudioEnvelopeAnalyzer(
        IAudioSampleReader sampleReader,
        AudioEnvelopeAnalysisOptions? options = null)
    {
        _sampleReader =
            sampleReader ??
            throw new ArgumentNullException(
                nameof(sampleReader));

        _options =
            options ??
            new AudioEnvelopeAnalysisOptions();

        _options.Validate();
    }

    /// <summary>
    /// Analiza la envolvente energética del archivo.
    /// </summary>
    public async Task<AudioEnvelopeAnalysisResult> AnalyzeAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        AudioEnvelopeAnalysisResult result = new()
        {
            WindowDuration =
                _options.WindowDuration
        };

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            AudioStreamInfo streamInfo =
                await _sampleReader.ReadInfoAsync(
                    filePath,
                    cancellationToken);

            if (!streamInfo.IsValid)
            {
                return BuildFailure(
                    result,
                    "La información del flujo PCM no es válida.");
            }

            result.TechnicalDuration =
                streamInfo.DecodedDuration;

            int windowFrames =
                CalculateWindowFrames(
                    streamInfo.SampleRate,
                    _options.WindowDuration);

            int hopFrames =
                CalculateHopFrames(
                    windowFrames,
                    _options.WindowOverlap);

            List<double> energyWindows =
                await CalculateEnergyWindowsAsync(
                    filePath,
                    windowFrames,
                    hopFrames,
                    cancellationToken);

            result.ProcessedWindowCount =
                energyWindows.Count;

            if (energyWindows.Count == 0)
            {
                return BuildFailure(
                    result,
                    "No se obtuvieron ventanas energéticas utilizables.");
            }

            result.AverageEnergyDb =
                CalculateAverageEnergyDb(
                    energyWindows);

            result.PeakEnergyDb =
                energyWindows.Max();

            result.MinimumEnergyDb =
                energyWindows.Min();

            int? firstMusicalWindow =
                FindFirstConfirmedWindow(
                    energyWindows,
                    _options.EnergyThresholdDb,
                    _options.MinimumConsecutiveWindows);

            int? lastMusicalWindow =
                FindLastConfirmedWindow(
                    energyWindows,
                    _options.EnergyThresholdDb,
                    _options.MinimumConsecutiveWindows);

            if (firstMusicalWindow.HasValue &&
                lastMusicalWindow.HasValue &&
                lastMusicalWindow.Value >=
                firstMusicalWindow.Value)
            {
                result.EstimatedMusicalStart =
                    WindowIndexToTime(
                        firstMusicalWindow.Value,
                        hopFrames,
                        streamInfo.SampleRate);

                result.EstimatedMusicalEnd =
                    WindowIndexToTime(
                        lastMusicalWindow.Value + 1,
                        hopFrames,
                        streamInfo.SampleRate);

                if (result.EstimatedMusicalEnd >
                    result.TechnicalDuration)
                {
                    result.EstimatedMusicalEnd =
                        result.TechnicalDuration;
                }
            }

            result.HasPossibleFadeIn =
                _options.DetectFadeIn &&
                DetectIncreasingTrend(
                    energyWindows,
                    fromStart: true);

            result.HasPossibleFadeOut =
                _options.DetectFadeOut &&
                DetectDecreasingTrend(
                    energyWindows,
                    fromEnd: true);

            result.HasPossibleReverbTail =
                _options.DetectReverbTail &&
                DetectPossibleReverbTail(
                    energyWindows,
                    lastMusicalWindow,
                    _options.EnergyThresholdDb);

            result.AnalysisCompleted =
                true;

            result.IsReliable =
                firstMusicalWindow.HasValue &&
                lastMusicalWindow.HasValue &&
                result.ProcessedWindowCount >=
                _options.MinimumConsecutiveWindows;

            result.Summary =
                BuildSummary(result);

            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return BuildFailure(
                result,
                exception.Message);
        }
    }

    /// <summary>
    /// Calcula las ventanas energéticas RMS expresadas
    /// en dBFS.
    /// </summary>
    private async Task<List<double>>
        CalculateEnergyWindowsAsync(
            string filePath,
            int windowFrames,
            int hopFrames,
            CancellationToken cancellationToken)
    {
        List<double> energyWindows =
            new();

        List<float> pendingSamples =
            new();

        int channels = 0;

        await foreach (
            AudioSampleBlock block
            in _sampleReader.ReadBlocksAsync(
                filePath,
                windowFrames,
                cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!block.IsValid)
            {
                continue;
            }

            channels =
                block.Channels;

            pendingSamples.AddRange(
                block.Samples);

            int windowSamples =
                checked(windowFrames * channels);

            int hopSamples =
                checked(hopFrames * channels);

            while (pendingSamples.Count >=
                windowSamples)
            {
                double energyDb =
                    CalculateWindowEnergyDb(
                        pendingSamples,
                        windowSamples);

                energyWindows.Add(
                    energyDb);

                pendingSamples.RemoveRange(
                    0,
                    Math.Min(
                        hopSamples,
                        pendingSamples.Count));
            }
        }

        return energyWindows;
    }

    /// <summary>
    /// Calcula la energía RMS de una ventana y la convierte
    /// a dBFS.
    /// </summary>
    private static double CalculateWindowEnergyDb(
        IReadOnlyList<float> samples,
        int sampleCount)
    {
        if (sampleCount <= 0)
        {
            return -120.0;
        }

        double sumSquares = 0;

        for (int index = 0;
            index < sampleCount;
            index++)
        {
            double sample =
                samples[index];

            sumSquares +=
                sample * sample;
        }

        double rms =
            Math.Sqrt(
                sumSquares /
                sampleCount);

        if (rms <= 0)
        {
            return -120.0;
        }

        return Math.Max(
            -120.0,
            20.0 * Math.Log10(rms));
    }

    /// <summary>
    /// Calcula el promedio energético en dominio lineal
    /// y lo convierte nuevamente a dBFS.
    /// </summary>
    private static double CalculateAverageEnergyDb(
        IReadOnlyCollection<double> energyWindows)
    {
        if (energyWindows.Count == 0)
        {
            return -120.0;
        }

        double linearSum = 0;

        foreach (double energyDb in energyWindows)
        {
            linearSum +=
                Math.Pow(
                    10,
                    energyDb / 20.0);
        }

        double averageLinear =
            linearSum /
            energyWindows.Count;

        if (averageLinear <= 0)
        {
            return -120.0;
        }

        return 20.0 *
            Math.Log10(
                averageLinear);
    }

    /// <summary>
    /// Busca la primera región energética confirmada.
    /// </summary>
    private static int? FindFirstConfirmedWindow(
        IReadOnlyList<double> windows,
        double thresholdDb,
        int minimumConsecutiveWindows)
    {
        int runLength = 0;

        for (int index = 0;
            index < windows.Count;
            index++)
        {
            if (windows[index] >= thresholdDb)
            {
                runLength++;

                if (runLength >=
                    minimumConsecutiveWindows)
                {
                    return
                        index -
                        minimumConsecutiveWindows +
                        1;
                }
            }
            else
            {
                runLength = 0;
            }
        }

        return null;
    }

    /// <summary>
    /// Busca la última región energética confirmada.
    /// </summary>
    private static int? FindLastConfirmedWindow(
        IReadOnlyList<double> windows,
        double thresholdDb,
        int minimumConsecutiveWindows)
    {
        int runLength = 0;

        for (int index = windows.Count - 1;
            index >= 0;
            index--)
        {
            if (windows[index] >= thresholdDb)
            {
                runLength++;

                if (runLength >=
                    minimumConsecutiveWindows)
                {
                    return
                        index +
                        minimumConsecutiveWindows -
                        1;
                }
            }
            else
            {
                runLength = 0;
            }
        }

        return null;
    }

    /// <summary>
    /// Detecta una tendencia energética ascendente
    /// al comienzo del archivo.
    /// </summary>
    private static bool DetectIncreasingTrend(
        IReadOnlyList<double> windows,
        bool fromStart)
    {
        if (!fromStart ||
            windows.Count < 6)
        {
            return false;
        }

        int count =
            Math.Min(
                20,
                windows.Count);

        int increases = 0;

        for (int index = 1;
            index < count;
            index++)
        {
            if (windows[index] >
                windows[index - 1])
            {
                increases++;
            }
        }

        return increases >=
            (count - 1) * 0.70;
    }

    /// <summary>
    /// Detecta una tendencia energética descendente
    /// al final del archivo.
    /// </summary>
    private static bool DetectDecreasingTrend(
        IReadOnlyList<double> windows,
        bool fromEnd)
    {
        if (!fromEnd ||
            windows.Count < 6)
        {
            return false;
        }

        int start =
            Math.Max(
                1,
                windows.Count - 20);

        int decreases = 0;
        int comparisons = 0;

        for (int index = start;
            index < windows.Count;
            index++)
        {
            comparisons++;

            if (windows[index] <
                windows[index - 1])
            {
                decreases++;
            }
        }

        return comparisons > 0 &&
            decreases >= comparisons * 0.70;
    }

    /// <summary>
    /// Detecta una posible cola energética tenue después
    /// de la región musical principal.
    /// </summary>
    private static bool DetectPossibleReverbTail(
        IReadOnlyList<double> windows,
        int? lastMusicalWindow,
        double thresholdDb)
    {
        if (!lastMusicalWindow.HasValue)
        {
            return false;
        }

        int start =
            lastMusicalWindow.Value + 1;

        if (start >= windows.Count)
        {
            return false;
        }

        for (int index = start;
            index < windows.Count;
            index++)
        {
            double value =
                windows[index];

            if (value < thresholdDb &&
                value > -90.0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Calcula la cantidad de frames de una ventana.
    /// </summary>
    private static int CalculateWindowFrames(
        int sampleRate,
        TimeSpan windowDuration)
    {
        double frames =
            sampleRate *
            windowDuration.TotalSeconds;

        return Math.Max(
            1,
            (int)Math.Round(frames));
    }

    /// <summary>
    /// Calcula el desplazamiento entre ventanas.
    /// </summary>
    private static int CalculateHopFrames(
        int windowFrames,
        double overlap)
    {
        double hop =
            windowFrames *
            (1.0 - overlap);

        return Math.Max(
            1,
            (int)Math.Round(hop));
    }

    /// <summary>
    /// Convierte un índice de ventana a tiempo.
    /// </summary>
    private static TimeSpan WindowIndexToTime(
        int windowIndex,
        int hopFrames,
        int sampleRate)
    {
        if (windowIndex <= 0 ||
            hopFrames <= 0 ||
            sampleRate <= 0)
        {
            return TimeSpan.Zero;
        }

        long frames =
            (long)windowIndex *
            hopFrames;

        return TimeSpan.FromSeconds(
            (double)frames /
            sampleRate);
    }

    /// <summary>
    /// Construye un resultado de error controlado.
    /// </summary>
    private static AudioEnvelopeAnalysisResult BuildFailure(
        AudioEnvelopeAnalysisResult result,
        string? errorMessage)
    {
        result.AnalysisCompleted =
            false;

        result.IsReliable =
            false;

        result.ErrorMessage =
            string.IsNullOrWhiteSpace(errorMessage)
                ? "El análisis de envolvente no pudo completarse."
                : errorMessage.Trim();

        result.Summary =
            "El análisis de envolvente terminó con un error.";

        return result;
    }

    /// <summary>
    /// Construye un resumen descriptivo.
    /// </summary>
    private static string BuildSummary(
        AudioEnvelopeAnalysisResult result)
    {
        List<string> details = new()
        {
            $"Duración técnica: " +
            $"{result.TechnicalDurationDisplay}",

            $"Inicio musical estimado: " +
            $"{result.EstimatedMusicalStartDisplay}",

            $"Final musical estimado: " +
            $"{result.EstimatedMusicalEndDisplay}",

            $"Duración musical estimada: " +
            $"{result.EstimatedMusicalDurationDisplay}",

            $"Energía media: " +
            $"{result.AverageEnergyDisplay}",

            $"Energía máxima: " +
            $"{result.PeakEnergyDisplay}",

            $"Ventanas procesadas: " +
            $"{result.ProcessedWindowCount}"
        };

        if (result.HasPossibleFadeIn)
        {
            details.Add(
                "Posible aumento progresivo de energía al inicio");
        }

        if (result.HasPossibleFadeOut)
        {
            details.Add(
                "Posible disminución progresiva de energía al final");
        }

        if (result.HasPossibleReverbTail)
        {
            details.Add(
                "Posible cola energética tenue después del final musical");
        }

        if (result.HasComparisonData)
        {
            details.Add(
                "Datos disponibles para comparación con otras fuentes");
        }

        return string.Join(
            " · ",
            details);
    }
}