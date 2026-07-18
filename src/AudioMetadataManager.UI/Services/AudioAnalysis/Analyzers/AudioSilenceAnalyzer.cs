using AudioMetadataManager.UI.Services.AudioAnalysis.Interfaces;
using AudioMetadataManager.UI.Services.AudioAnalysis.Models;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Analyzers;

/// <summary>
/// Detecta silencio continuo al inicio y al final de un
/// archivo de audio decodificado.
///
/// No detecta todavía silencios internos, fades ni ruido
/// de fondo avanzado. Tampoco modifica el archivo.
/// </summary>
public class AudioSilenceAnalyzer :
    IAudioAnalyzer<AudioSilenceAnalysisResult>
{
    private readonly IAudioSampleReader _sampleReader;
    private readonly AudioSilenceAnalysisOptions _options;

    /// <summary>
    /// Nombre legible del analizador.
    /// </summary>
    public string Name =>
        "Analizador de silencio exterior";

    /// <summary>
    /// Crea el analizador utilizando un lector PCM.
    /// </summary>
    public AudioSilenceAnalyzer(
        IAudioSampleReader sampleReader,
        AudioSilenceAnalysisOptions? options = null)
    {
        _sampleReader =
            sampleReader ??
            throw new ArgumentNullException(
                nameof(sampleReader));

        _options =
            options ??
            new AudioSilenceAnalysisOptions();

        _options.Validate();
    }

    /// <summary>
    /// Analiza el silencio exterior del archivo.
    /// </summary>
    public async Task<AudioSilenceAnalysisResult> AnalyzeAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        AudioSilenceAnalysisResult result = new()
        {
            SilenceThresholdDb =
                _options.SilenceThresholdDb
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

            double silenceAmplitudeThreshold =
                ConvertDbToAmplitude(
                    _options.SilenceThresholdDb);

            int minimumAudibleFrames =
                CalculateMinimumAudibleFrames(
                    streamInfo.SampleRate,
                    _options.MinimumAudibleDuration);

            long totalProcessedFrames = 0;

            long currentAudibleRunStart = -1;
            int currentAudibleRunLength = 0;

            long? firstConfirmedAudibleFrame = null;
            long? lastConfirmedAudibleFrame = null;

            await foreach (
                AudioSampleBlock block
                in _sampleReader.ReadBlocksAsync(
                    filePath,
                    _options.FramesPerBlock,
                    cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!block.IsValid)
                {
                    continue;
                }

                AnalyzeBlock(
                    block,
                    silenceAmplitudeThreshold,
                    minimumAudibleFrames,
                    ref currentAudibleRunStart,
                    ref currentAudibleRunLength,
                    ref firstConfirmedAudibleFrame,
                    ref lastConfirmedAudibleFrame);

                long blockEndFrame =
                    block.StartFrame +
                    block.FrameCount;

                if (blockEndFrame > totalProcessedFrames)
                {
                    totalProcessedFrames =
                        blockEndFrame;
                }
            }

            /*
             * Si el archivo termina mientras existe una secuencia
             * audible, confirmamos esa última secuencia.
             */
            ConfirmPendingAudibleRun(
                minimumAudibleFrames,
                currentAudibleRunStart,
                currentAudibleRunLength,
                ref firstConfirmedAudibleFrame,
                ref lastConfirmedAudibleFrame);

            if (totalProcessedFrames <= 0)
            {
                return BuildFailure(
                    result,
                    "No se obtuvieron muestras PCM utilizables.");
            }

            TimeSpan decodedDurationFromFrames =
                FramesToTime(
                    totalProcessedFrames,
                    streamInfo.SampleRate);

            /*
             * La duración calculada desde los frames efectivamente
             * leídos tiene prioridad si es válida.
             */
            if (decodedDurationFromFrames > TimeSpan.Zero)
            {
                result.TechnicalDuration =
                    decodedDurationFromFrames;
            }

            if (!firstConfirmedAudibleFrame.HasValue ||
                !lastConfirmedAudibleFrame.HasValue)
            {
                result.LeadingSilence =
                    result.TechnicalDuration;

                result.TrailingSilence =
                    TimeSpan.Zero;

                result.AudibleDuration =
                    TimeSpan.Zero;

                result.AnalysisCompleted =
                    true;

                result.IsReliable =
                    false;

 /*
 * No se detectó contenido audible suficiente.
 *
 * El resultado podrá compararse posteriormente con
 * otras fuentes para decidir si requiere revisión.
 */

                result.RequiresManualReview =
                    false;

                result.Summary =
                    "No se detectó contenido audible continuo " +
                    "por encima del umbral configurado. " +
                    "No existen datos suficientes para realizar " +
                    "comparaciones de duración.";

                return result;
            }

            long firstAudibleFrame =
                firstConfirmedAudibleFrame.Value;

            long lastAudibleFrame =
                lastConfirmedAudibleFrame.Value;

            long audibleFrameCount =
                Math.Max(
                    0,
                    lastAudibleFrame -
                    firstAudibleFrame +
                    1);

            long trailingSilentFrames =
                Math.Max(
                    0,
                    totalProcessedFrames -
                    lastAudibleFrame -
                    1);

            result.LeadingSilence =
                FramesToTime(
                    firstAudibleFrame,
                    streamInfo.SampleRate);

            result.TrailingSilence =
                FramesToTime(
                    trailingSilentFrames,
                    streamInfo.SampleRate);

            result.AudibleDuration =
                FramesToTime(
                    audibleFrameCount,
                    streamInfo.SampleRate);

            result.AnalysisCompleted =
                true;

            result.IsReliable =
                audibleFrameCount >=
                minimumAudibleFrames;

 /*
 * El análisis de silencio ya no decide si un archivo
 * requiere revisión manual.
 *
 * Esa decisión pertenecerá al futuro motor de
 * comparación de metadatos.
 */

            result.RequiresManualReview =
                false;

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
    /// Analiza todos los frames de un bloque PCM.
    /// </summary>
    private static void AnalyzeBlock(
        AudioSampleBlock block,
        double silenceAmplitudeThreshold,
        int minimumAudibleFrames,
        ref long currentAudibleRunStart,
        ref int currentAudibleRunLength,
        ref long? firstConfirmedAudibleFrame,
        ref long? lastConfirmedAudibleFrame)
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
                    silenceAmplitudeThreshold);

            if (isAudible)
            {
                if (currentAudibleRunLength == 0)
                {
                    currentAudibleRunStart =
                        absoluteFrame;
                }

                currentAudibleRunLength++;

                if (currentAudibleRunLength >=
                    minimumAudibleFrames)
                {
                    firstConfirmedAudibleFrame ??=
                        currentAudibleRunStart;

                    lastConfirmedAudibleFrame =
                        absoluteFrame;
                }

                continue;
            }

            ConfirmPendingAudibleRun(
                minimumAudibleFrames,
                currentAudibleRunStart,
                currentAudibleRunLength,
                ref firstConfirmedAudibleFrame,
                ref lastConfirmedAudibleFrame);

            currentAudibleRunStart =
                -1;

            currentAudibleRunLength =
                0;
        }
    }

    /// <summary>
    /// Comprueba la amplitud máxima de todos los canales
    /// pertenecientes a un frame.
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
    /// Confirma una secuencia audible que alcanzó
    /// la duración mínima configurada.
    /// </summary>
    private static void ConfirmPendingAudibleRun(
        int minimumAudibleFrames,
        long currentAudibleRunStart,
        int currentAudibleRunLength,
        ref long? firstConfirmedAudibleFrame,
        ref long? lastConfirmedAudibleFrame)
    {
        if (currentAudibleRunStart < 0 ||
            currentAudibleRunLength <
            minimumAudibleFrames)
        {
            return;
        }

        long runEndFrame =
            currentAudibleRunStart +
            currentAudibleRunLength -
            1;

        firstConfirmedAudibleFrame ??=
            currentAudibleRunStart;

        lastConfirmedAudibleFrame =
            runEndFrame;
    }

    /// <summary>
    /// Convierte un nivel dBFS a amplitud lineal.
    ///
    /// Ejemplo aproximado:
    /// -20 dBFS = 0,1
    /// -40 dBFS = 0,01
    /// -60 dBFS = 0,001
    /// </summary>
    private static double ConvertDbToAmplitude(
        double decibels)
    {
        return Math.Pow(
            10,
            decibels / 20.0);
    }

    /// <summary>
    /// Calcula cuántos frames representan la duración
    /// audible mínima.
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
    /// Convierte frames PCM a tiempo.
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
    /// Construye un resultado de error controlado.
    /// </summary>
    private static AudioSilenceAnalysisResult BuildFailure(
        AudioSilenceAnalysisResult result,
        string? errorMessage)
    {
        result.AnalysisCompleted =
            false;

        result.IsReliable =
            false;

        result.RequiresManualReview =
            true;

        result.ErrorMessage =
            string.IsNullOrWhiteSpace(errorMessage)
                ? "El análisis de silencio no pudo completarse."
                : errorMessage.Trim();

        result.Summary =
            "El análisis de silencio terminó con un error.";

        return result;
    }

    /// <summary>
    /// Construye una explicación legible del resultado.
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