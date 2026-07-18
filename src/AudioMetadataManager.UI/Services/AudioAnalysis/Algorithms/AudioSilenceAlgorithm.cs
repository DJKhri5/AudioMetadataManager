using AudioMetadataManager.UI.Services.AudioAnalysis.Models;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Algorithms;

/// <summary>
/// Contiene el algoritmo reutilizable para analizar el
/// silencio técnico exterior de un flujo PCM.
///
/// Esta clase no abre archivos, no decodifica audio y no
/// modifica el resultado general del pipeline.
///
/// Puede ser utilizada tanto por un analizador independiente
/// como por un procesador conectado a la lectura PCM
/// compartida.
/// </summary>
public class AudioSilenceAlgorithm
{
    private readonly AudioSilenceAnalysisOptions _options;

    private AudioSilenceAnalysisResult _result =
        new();

    private AudioStreamInfo? _streamInfo;

    private double _silenceAmplitudeThreshold;
    private int _minimumAudibleFrames;

    private long _totalProcessedFrames;

    private long _currentAudibleRunStart = -1;
    private int _currentAudibleRunLength;

    private long? _firstConfirmedAudibleFrame;
    private long? _lastConfirmedAudibleFrame;

    private bool _isInitialized;
    private bool _isCompleted;

    /// <summary>
    /// Crea el algoritmo con la configuración indicada.
    /// </summary>
    public AudioSilenceAlgorithm(
        AudioSilenceAnalysisOptions? options = null)
    {
        _options =
            options ??
            new AudioSilenceAnalysisOptions();

        _options.Validate();
    }

    /// <summary>
    /// Resultado acumulado por el algoritmo.
    /// </summary>
    public AudioSilenceAnalysisResult Result =>
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
            new AudioSilenceAnalysisResult
            {
                SilenceThresholdDb =
                    _options.SilenceThresholdDb,

                TechnicalDuration =
                    streamInfo.DecodedDuration
            };

        _silenceAmplitudeThreshold =
            ConvertDbToAmplitude(
                _options.SilenceThresholdDb);

        _minimumAudibleFrames =
            CalculateMinimumAudibleFrames(
                streamInfo.SampleRate,
                _options.MinimumAudibleDuration);

        _totalProcessedFrames = 0;

        _currentAudibleRunStart = -1;
        _currentAudibleRunLength = 0;

        _firstConfirmedAudibleFrame = null;
        _lastConfirmedAudibleFrame = null;

        _isInitialized = true;
        _isCompleted = false;
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

        AnalyzeBlock(
            block);

        long blockEndFrame =
            block.StartFrame +
            block.FrameCount;

        if (blockEndFrame >
            _totalProcessedFrames)
        {
            _totalProcessedFrames =
                blockEndFrame;
        }
    }

    /// <summary>
    /// Finaliza los cálculos y devuelve el resultado.
    /// </summary>
    public AudioSilenceAnalysisResult Complete()
    {
        EnsureReadyForProcessing();

        AudioStreamInfo streamInfo =
            _streamInfo!;

        ConfirmPendingAudibleRun();

        if (_totalProcessedFrames <= 0)
        {
            return Fail(
                "No se obtuvieron muestras PCM utilizables.");
        }

        TimeSpan decodedDurationFromFrames =
            FramesToTime(
                _totalProcessedFrames,
                streamInfo.SampleRate);

        /*
         * La duración calculada desde los frames realmente
         * procesados tiene prioridad cuando es válida.
         */
        if (decodedDurationFromFrames >
            TimeSpan.Zero)
        {
            _result.TechnicalDuration =
                decodedDurationFromFrames;
        }

        if (!_firstConfirmedAudibleFrame.HasValue ||
            !_lastConfirmedAudibleFrame.HasValue)
        {
            _result.LeadingSilence =
                _result.TechnicalDuration;

            _result.TrailingSilence =
                TimeSpan.Zero;

            _result.AudibleDuration =
                TimeSpan.Zero;

            _result.AnalysisCompleted =
                true;

            _result.IsReliable =
                false;

            _result.RequiresManualReview =
                false;

            _result.Summary =
                "No se detectó contenido audible continuo " +
                "por encima del umbral configurado. " +
                "No existen datos suficientes para realizar " +
                "comparaciones de duración.";

            _isCompleted =
                true;

            return _result;
        }

        long firstAudibleFrame =
            _firstConfirmedAudibleFrame.Value;

        long lastAudibleFrame =
            _lastConfirmedAudibleFrame.Value;

        long audibleFrameCount =
            Math.Max(
                0,
                lastAudibleFrame -
                firstAudibleFrame +
                1);

        long trailingSilentFrames =
            Math.Max(
                0,
                _totalProcessedFrames -
                lastAudibleFrame -
                1);

        _result.LeadingSilence =
            FramesToTime(
                firstAudibleFrame,
                streamInfo.SampleRate);

        _result.TrailingSilence =
            FramesToTime(
                trailingSilentFrames,
                streamInfo.SampleRate);

        _result.AudibleDuration =
            FramesToTime(
                audibleFrameCount,
                streamInfo.SampleRate);

        _result.AnalysisCompleted =
            true;

        _result.IsReliable =
            audibleFrameCount >=
            _minimumAudibleFrames;

        _result.RequiresManualReview =
            false;

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
    public AudioSilenceAnalysisResult Fail(
        string? errorMessage)
    {
        _result.AnalysisCompleted =
            false;

        _result.IsReliable =
            false;

        _result.RequiresManualReview =
            false;

        _result.ErrorMessage =
            string.IsNullOrWhiteSpace(errorMessage)
                ? "El análisis de silencio no pudo completarse."
                : errorMessage.Trim();

        _result.Summary =
            "El análisis de silencio terminó con un error.";

        _isCompleted =
            true;

        return _result;
    }

    /// <summary>
    /// Analiza todos los frames de un bloque PCM.
    /// </summary>
    private void AnalyzeBlock(
        AudioSampleBlock block)
    {
        for (
            int frameIndex = 0;
            frameIndex < block.FrameCount;
            frameIndex++)
        {
            long absoluteFrame =
                block.StartFrame +
                frameIndex;

            bool isAudible =
                IsFrameAudible(
                    block,
                    frameIndex,
                    _silenceAmplitudeThreshold);

            if (isAudible)
            {
                if (_currentAudibleRunLength == 0)
                {
                    _currentAudibleRunStart =
                        absoluteFrame;
                }

                _currentAudibleRunLength++;

                if (_currentAudibleRunLength >=
                    _minimumAudibleFrames)
                {
                    _firstConfirmedAudibleFrame ??=
                        _currentAudibleRunStart;

                    _lastConfirmedAudibleFrame =
                        absoluteFrame;
                }

                continue;
            }

            ConfirmPendingAudibleRun();

            _currentAudibleRunStart =
                -1;

            _currentAudibleRunLength =
                0;
        }
    }

    /// <summary>
    /// Comprueba si al menos uno de los canales de un frame
    /// supera el umbral técnico de silencio.
    /// </summary>
    private static bool IsFrameAudible(
        AudioSampleBlock block,
        int frameIndex,
        double silenceAmplitudeThreshold)
    {
        int firstSampleIndex =
            frameIndex *
            block.Channels;

        for (
            int channelIndex = 0;
            channelIndex < block.Channels;
            channelIndex++)
        {
            int sampleIndex =
                firstSampleIndex +
                channelIndex;

            if (sampleIndex >=
                block.Samples.Length)
            {
                break;
            }

            double amplitude =
                Math.Abs(
                    block.Samples[sampleIndex]);

            if (amplitude >
                silenceAmplitudeThreshold)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Confirma una secuencia audible cuando alcanzó
    /// la duración mínima configurada.
    /// </summary>
    private void ConfirmPendingAudibleRun()
    {
        if (_currentAudibleRunStart < 0 ||
            _currentAudibleRunLength <
            _minimumAudibleFrames)
        {
            return;
        }

        long runEndFrame =
            _currentAudibleRunStart +
            _currentAudibleRunLength -
            1;

        _firstConfirmedAudibleFrame ??=
            _currentAudibleRunStart;

        _lastConfirmedAudibleFrame =
            runEndFrame;
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
                "El algoritmo de silencio no fue inicializado.");
        }

        if (_isCompleted)
        {
            throw new InvalidOperationException(
                "El algoritmo de silencio ya fue finalizado.");
        }
    }

    /// <summary>
    /// Convierte un nivel dBFS a amplitud lineal.
    /// </summary>
    private static double ConvertDbToAmplitude(
        double decibels)
    {
        return Math.Pow(
            10,
            decibels / 20.0);
    }

    /// <summary>
    /// Calcula la cantidad mínima de frames audibles.
    /// </summary>
    private static int CalculateMinimumAudibleFrames(
        int sampleRate,
        TimeSpan minimumAudibleDuration)
    {
        double frames =
            sampleRate *
            minimumAudibleDuration.TotalSeconds;

        return Math.Max(
            1,
            (int)Math.Ceiling(frames));
    }

    /// <summary>
    /// Convierte una cantidad de frames PCM a tiempo.
    /// </summary>
    private static TimeSpan FramesToTime(
        long frameCount,
        int sampleRate)
    {
        if (frameCount <= 0 ||
            sampleRate <= 0)
        {
            return TimeSpan.Zero;
        }

        return TimeSpan.FromSeconds(
            (double)frameCount /
            sampleRate);
    }

    /// <summary>
    /// Construye un resumen descriptivo.
    /// </summary>
    private static string BuildSummary(
        AudioSilenceAnalysisResult result)
    {
        List<string> details = new()
        {
            $"Duración técnica: " +
            $"{result.TechnicalDurationDisplay}",

            $"Duración audible estimada: " +
            $"{result.AudibleDurationDisplay}",

            $"Silencio inicial: " +
            $"{result.LeadingSilenceDisplay}",

            $"Silencio final: " +
            $"{result.TrailingSilenceDisplay}",

            $"Silencio exterior total: " +
            $"{result.TotalOuterSilenceDisplay}"
        };

        if (result.HasLeadingSilence)
        {
            details.Add(
                "Se detectó silencio técnico al inicio");
        }

        if (result.HasTrailingSilence)
        {
            details.Add(
                "Se detectó silencio técnico al final");
        }

        if (!result.IsReliable)
        {
            details.Add(
                "Las mediciones no son suficientemente confiables");
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