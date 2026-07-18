using AudioMetadataManager.UI.Services.AudioAnalysis.Analyzers;
using AudioMetadataManager.UI.Services.AudioAnalysis.Interfaces;
using AudioMetadataManager.UI.Services.AudioAnalysis.Models;
using AudioMetadataManager.UI.Services.AudioAnalysis.Readers;
using System.IO;

namespace AudioMetadataManager.UI.Services.AudioAnalysis;

/// <summary>
/// Coordina el pipeline de analizadores técnicos y acústicos
/// utilizados por Audio Metadata Manager.
///
/// Este motor no modifica archivos. Ejecuta las etapas
/// registradas y reúne sus resultados.
/// </summary>
public class AudioAnalysisEngine
{
    private readonly
        IReadOnlyList<IAudioAnalysisStage>
        _stages;

    /// <summary>
    /// Crea el motor con el pipeline predeterminado.
    ///
    /// Actualmente incluye:
    /// - análisis de silencio exterior;
    /// - análisis de envolvente energética.
    ///
    /// Posteriormente se agregarán huella acústica,
    /// clipping, espectro y verificación de bitrate.
    /// </summary>
    public AudioAnalysisEngine()
        : this(CreateDefaultStages())
    {
    }

    /// <summary>
    /// Crea el motor con una colección personalizada
    /// de etapas.
    ///
    /// Este constructor facilita pruebas y permite registrar
    /// analizadores futuros sin modificar el motor.
    /// </summary>
    public AudioAnalysisEngine(
        IEnumerable<IAudioAnalysisStage> stages)
    {
        ArgumentNullException.ThrowIfNull(
            stages);

        List<IAudioAnalysisStage> orderedStages =
            stages
                .Where(stage => stage is not null)
                .OrderBy(stage => stage.Order)
                .ToList();

        if (orderedStages.Count == 0)
        {
            throw new ArgumentException(
                "El pipeline debe contener al menos " +
                "una etapa de análisis.",
                nameof(stages));
        }

        _stages =
            orderedStages;
    }

    /// <summary>
    /// Analiza un archivo ejecutando ordenadamente todas
    /// las etapas registradas.
    /// </summary>
    public async Task<AudioAnalysisResult> AnalyzeAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        AudioAnalysisResult result =
            CreateInitialResult(filePath);

        AudioAnalysisContext context =
            new(
                filePath,
                result);

        try
        {
            ValidateFilePath(filePath);

            cancellationToken.ThrowIfCancellationRequested();

            foreach (IAudioAnalysisStage stage in _stages)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await stage.ExecuteAsync(
                        context,
                        cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    /*
                     * El error de una etapa especializada no debe
                     * impedir necesariamente que las etapas
                     * restantes puedan ejecutarse.
                     */
                    result.AddWarning(
                        $"La etapa \"{stage.Name}\" terminó " +
                        $"con un error: {exception.Message}");
                }
            }

            result.Summary =
                BuildSummary(result);

            result.MarkAsCompleted();

            return result;
        }
        catch (OperationCanceledException)
        {
            result.MarkAsCancelled();

            result.Summary =
                "El análisis fue cancelado antes de finalizar.";

            return result;
        }
        catch (Exception exception)
        {
            result.MarkAsFailed(
                exception.Message);

            result.Summary =
                "No fue posible completar el análisis " +
                "técnico del archivo.";

            return result;
        }
    }

    /// <summary>
    /// Construye el pipeline predeterminado.
    /// </summary>
    private static IReadOnlyList<IAudioAnalysisStage>
        CreateDefaultStages()
    {
        IAudioSampleReader sampleReader =
            new NAudioSampleReader();

        AudioSilenceAnalysisOptions silenceOptions =
            new();

        IAudioAnalyzer<AudioSilenceAnalysisResult>
            silenceAnalyzer =
                new AudioSilenceAnalyzer(
                    sampleReader,
                    silenceOptions);

        IAudioAnalysisStage silenceStage =
            new SilenceAnalysisStage(
                silenceAnalyzer);

        AudioEnvelopeAnalysisOptions envelopeOptions =
            new();

        IAudioAnalyzer<AudioEnvelopeAnalysisResult>
            envelopeAnalyzer =
                new AudioEnvelopeAnalyzer(
                    sampleReader,
                    envelopeOptions);

        IAudioAnalysisStage envelopeStage =
            new EnvelopeAnalysisStage(
                envelopeAnalyzer);

        return new List<IAudioAnalysisStage>
        {
            silenceStage,
            envelopeStage
        };
    }

    /// <summary>
    /// Crea el resultado general antes de iniciar
    /// las etapas del pipeline.
    /// </summary>
    private static AudioAnalysisResult CreateInitialResult(
        string? filePath)
    {
        string normalizedPath =
            filePath?.Trim() ??
            string.Empty;

        return new AudioAnalysisResult
        {
            FilePath =
                normalizedPath,

            FileName =
                string.IsNullOrWhiteSpace(normalizedPath)
                    ? string.Empty
                    : Path.GetFileName(normalizedPath),

            StartedAt =
                DateTime.Now,

            AnalysisCompleted =
                false,

            WasCancelled =
                false,

            HasFatalError =
                false,

            Summary =
                "Análisis técnico en proceso."
        };
    }

    /// <summary>
    /// Comprueba la ruta antes de ejecutar el pipeline.
    /// </summary>
    private static void ValidateFilePath(
        string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException(
                "La ruta del archivo de audio está vacía.",
                nameof(filePath));
        }

        if (!Path.IsPathFullyQualified(filePath))
        {
            throw new ArgumentException(
                "La ruta del archivo de audio debe ser completa.",
                nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "No se encontró el archivo que será analizado.",
                filePath);
        }
    }

    /// <summary>
    /// Construye el resumen general del pipeline.
    /// </summary>
    private static string BuildSummary(
        AudioAnalysisResult result)
    {
        AudioSilenceAnalysisResult silence =
            result.Silence;

        if (silence.HasError)
        {
            return
                "El archivo pudo abrirse, pero el análisis " +
                "de silencio terminó con un error. " +
                $"Detalle: {silence.ErrorMessage}";
        }

        if (!silence.AnalysisCompleted)
        {
            return
                "El pipeline finalizó, pero el análisis " +
                "de silencio no pudo completarse.";
        }

        List<string> details = new()
        {
            $"Duración técnica: " +
            $"{silence.TechnicalDurationDisplay}",

            $"Duración audible estimada: " +
            $"{silence.AudibleDurationDisplay}",

            $"Silencio inicial: " +
            $"{silence.LeadingSilenceDisplay}",

            $"Silencio final: " +
            $"{silence.TrailingSilenceDisplay}",

            $"Silencio exterior: " +
            $"{silence.OuterSilencePercentageDisplay}"
        };

        /*
        * Este módulo únicamente entrega mediciones.
        * La interpretación de esos datos se realizará
        * posteriormente por el motor de comparación.
        */

        details.Add(
            "Datos disponibles para comparación con otras fuentes");

        return string.Join(
            " · ",
            details);
    }
}