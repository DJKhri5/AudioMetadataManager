using AudioMetadataManager.UI.Services.AudioAnalysis.Models;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Algorithms;

/// <summary>
/// Contiene el algoritmo reutilizable para analizar la
/// envolvente energética de un flujo PCM mediante ventanas RMS.
///
/// Esta clase no abre archivos, no decodifica audio y no
/// modifica directamente el resultado general del pipeline.
///
/// Puede utilizarse tanto desde un analizador independiente
/// como desde un procesador conectado a la lectura PCM
/// compartida.
/// </summary>
public class AudioEnvelopeAlgorithm
{
    private readonly AudioEnvelopeAnalysisOptions _options;

    private AudioEnvelopeAnalysisResult _result =
        new();

    private AudioStreamInfo? _streamInfo;

    private readonly List<double> _energyWindows =
        new();

    private readonly List<float> _pendingSamples =
        new();

    private int _windowFrames;
    private int _hopFrames;
    private int _channels;

    private bool _isInitialized;
    private bool _isCompleted;

    /// <summary>
    /// Crea el algoritmo con la configuración indicada.
    /// </summary>
    public AudioEnvelopeAlgorithm(
        AudioEnvelopeAnalysisOptions? options = null)
    {
        _options =
            options ??
            new AudioEnvelopeAnalysisOptions();

        _options.Validate();
    }

    /// <summary>
    /// Resultado acumulado por el algoritmo.
    /// </summary>
    public AudioEnvelopeAnalysisResult Result =>
        _result;

    /// <summary>
    /// Prepara el algoritmo antes de recibir bloques PCM.
    /// </summary>
    public void Initialize(
        AudioStreamInfo streamInfo)
    {
        ArgumentNullException.ThrowIfNull(
            streamInfo);

        if (!streamInfo.IsValid)
        {
            throw new ArgumentException(
                "La información del flujo PCM no es válida.",
                nameof(streamInfo));
        }

        _streamInfo =
            streamInfo;

        _result =
            new AudioEnvelopeAnalysisResult
            {
                TechnicalDuration =
                    streamInfo.DecodedDuration,

                WindowDuration =
                    _options.WindowDuration
            };

        _windowFrames =
            CalculateWindowFrames(
                streamInfo.SampleRate,
                _options.WindowDuration);

        _hopFrames =
            CalculateHopFrames(
                _windowFrames,
                _options.WindowOverlap);

        _channels =
            streamInfo.Channels;

        _energyWindows.Clear();
        _pendingSamples.Clear();

        _isInitialized =
            true;

        _isCompleted =
            false;
    }

    /// <summary>
    /// Procesa un bloque PCM previamente decodificado.
    /// </summary>
    public void ProcessBlock(
        AudioSampleBlock block)
    {
        ArgumentNullException.ThrowIfNull(
            block);

        EnsureReadyForProcessing();

        if (!block.IsValid)
        {
            return;
        }

        if (block.Channels != _channels)
        {
            throw new InvalidOperationException(
                "La cantidad de canales del bloque PCM " +
                "no coincide con la información del flujo.");
        }

        _pendingSamples.AddRange(
            block.Samples);

        int windowSamples =
            checked(
                _windowFrames *
                _channels);

        int hopSamples =
            checked(
                _hopFrames *
                _channels);

        while (_pendingSamples.Count >=
            windowSamples)
        {
            double energyDb =
                CalculateWindowEnergyDb(
                    _pendingSamples,
                    windowSamples);

            _energyWindows.Add(
                energyDb);

            _pendingSamples.RemoveRange(
                0,
                Math.Min(
                    hopSamples,
                    _pendingSamples.Count));
        }
    }

    /// <summary>
    /// Finaliza los cálculos y devuelve el resultado.
    /// </summary>
    public AudioEnvelopeAnalysisResult Complete()
    {
        EnsureReadyForProcessing();

        AudioStreamInfo streamInfo =
            _streamInfo!;

        _result.ProcessedWindowCount =
            _energyWindows.Count;

        if (_energyWindows.Count == 0)
        {
            return Fail(
                "No se obtuvieron ventanas energéticas utilizables.");
        }

        _result.AverageEnergyDb =
            CalculateAverageEnergyDb(
                _energyWindows);

        _result.PeakEnergyDb =
            _energyWindows.Max();

        _result.MinimumEnergyDb =
            _energyWindows.Min();

        int? firstMusicalWindow =
            FindFirstConfirmedWindow(
                _energyWindows,
                _options.EnergyThresholdDb,
                _options.MinimumConsecutiveWindows);

        int? lastMusicalWindow =
            FindLastConfirmedWindow(
                _energyWindows,
                _options.EnergyThresholdDb,
                _options.MinimumConsecutiveWindows);

        if (firstMusicalWindow.HasValue &&
            lastMusicalWindow.HasValue &&
            lastMusicalWindow.Value >=
            firstMusicalWindow.Value)
        {
            _result.EstimatedMusicalStart =
                WindowIndexToTime(
                    firstMusicalWindow.Value,
                    _hopFrames,
                    streamInfo.SampleRate);

            _result.EstimatedMusicalEnd =
                WindowIndexToTime(
                    lastMusicalWindow.Value + 1,
                    _hopFrames,
                    streamInfo.SampleRate);

            if (_result.EstimatedMusicalEnd >
                _result.TechnicalDuration)
            {
                _result.EstimatedMusicalEnd =
                    _result.TechnicalDuration;
            }
        }

        _result.HasPossibleFadeIn =
            _options.DetectFadeIn &&
            DetectIncreasingTrend(
                _energyWindows);

        _result.HasPossibleFadeOut =
            _options.DetectFadeOut &&
            DetectDecreasingTrend(
                _energyWindows);

        _result.HasPossibleReverbTail =
            _options.DetectReverbTail &&
            DetectPossibleReverbTail(
                _energyWindows,
                lastMusicalWindow,
                _options.EnergyThresholdDb);

        _result.AnalysisCompleted =
            true;

        _result.IsReliable =
            firstMusicalWindow.HasValue &&
            lastMusicalWindow.HasValue &&
            _result.ProcessedWindowCount >=
            _options.MinimumConsecutiveWindows;

        _result.Summary =
            BuildSummary(
                _result);

        _isCompleted =
            true;

        return _result;
    }

    /// <summary>
    /// Construye un resultado de error controlado.
    /// </summary>
    public AudioEnvelopeAnalysisResult Fail(
        string? errorMessage)
    {
        _result.AnalysisCompleted =
            false;

        _result.IsReliable =
            false;

        _result.ErrorMessage =
            string.IsNullOrWhiteSpace(errorMessage)
                ? "El análisis de envolvente no pudo completarse."
                : errorMessage.Trim();

        _result.Summary =
            "El análisis de envolvente terminó con un error.";

        _isCompleted =
            true;

        return _result;
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
            if (windows[index] >=
                thresholdDb)
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
            if (windows[index] >=
                thresholdDb)
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
        IReadOnlyList<double> windows)
    {
        if (windows.Count < 6)
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
        IReadOnlyList<double> windows)
    {
        if (windows.Count < 6)
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
            decreases >=
                comparisons * 0.70;
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
    /// Comprueba que el algoritmo esté preparado para
    /// procesar o finalizar bloques.
    /// </summary>
    private void EnsureReadyForProcessing()
    {
        if (!_isInitialized ||
            _streamInfo is null)
        {
            throw new InvalidOperationException(
                "El algoritmo de envolvente no fue inicializado.");
        }

        if (_isCompleted)
        {
            throw new InvalidOperationException(
                "El algoritmo de envolvente ya fue finalizado.");
        }
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