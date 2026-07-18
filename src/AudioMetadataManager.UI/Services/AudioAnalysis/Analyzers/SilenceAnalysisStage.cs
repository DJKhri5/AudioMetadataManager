using AudioMetadataManager.UI.Services.AudioAnalysis.Interfaces;
using AudioMetadataManager.UI.Services.AudioAnalysis.Models;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Analyzers;

/// <summary>
/// Integra AudioSilenceAnalyzer dentro del pipeline general
/// de AudioAnalysisEngine.
///
/// Esta clase no calcula directamente el silencio.
/// Su función es ejecutar el analizador especializado y guardar
/// su resultado dentro de AudioAnalysisResult.
/// </summary>
public class SilenceAnalysisStage : IAudioAnalysisStage
{
    private readonly
        IAudioAnalyzer<AudioSilenceAnalysisResult>
        _silenceAnalyzer;

    /// <summary>
    /// Nombre legible de la etapa.
    /// </summary>
    public string Name =>
        "Análisis de silencio exterior";

    /// <summary>
    /// Primera etapa del pipeline.
    ///
    /// Los analizadores futuros podrán utilizar valores
    /// mayores: 200, 300, 400, etc.
    /// </summary>
    public int Order =>
        100;

    /// <summary>
    /// Crea la etapa utilizando un analizador de silencio.
    /// </summary>
    public SilenceAnalysisStage(
        IAudioAnalyzer<AudioSilenceAnalysisResult>
            silenceAnalyzer)
    {
        _silenceAnalyzer =
            silenceAnalyzer ??
            throw new ArgumentNullException(
                nameof(silenceAnalyzer));
    }

    /// <summary>
    /// Ejecuta el análisis y registra el resultado general
    /// dentro del contexto compartido.
    /// </summary>
    public async Task ExecuteAsync(
        AudioAnalysisContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        cancellationToken.ThrowIfCancellationRequested();

        AudioSilenceAnalysisResult silenceResult =
            await _silenceAnalyzer.AnalyzeAsync(
                context.FilePath,
                cancellationToken);

        context.AnalysisResult.Silence =
            silenceResult;

        RegisterWarnings(
            context.AnalysisResult,
            silenceResult);
    }

    /// <summary>
    /// Registra únicamente situaciones que impiden confiar
    /// en las mediciones obtenidas.
    ///
    /// La existencia de silencio NO constituye una advertencia.
    /// El silencio será utilizado posteriormente como evidencia
    /// durante la comparación con metadatos y plataformas.
    /// </summary>
    private static void RegisterWarnings(
        AudioAnalysisResult analysisResult,
        AudioSilenceAnalysisResult silenceResult)
    {
        if (silenceResult.HasError)
        {
            analysisResult.AddWarning(
                "El análisis de silencio no pudo completarse correctamente.");

            return;
        }

        if (!silenceResult.AnalysisCompleted)
        {
            analysisResult.AddWarning(
                "El análisis de silencio quedó incompleto.");
        }

        if (!silenceResult.IsReliable)
        {
            analysisResult.AddWarning(
                "Las mediciones de silencio no son suficientemente confiables para realizar comparaciones.");
        }
    }
}