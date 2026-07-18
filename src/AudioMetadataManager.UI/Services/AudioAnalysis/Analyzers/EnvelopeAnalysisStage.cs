using AudioMetadataManager.UI.Services.AudioAnalysis.Interfaces;
using AudioMetadataManager.UI.Services.AudioAnalysis.Models;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Analyzers;

/// <summary>
/// Integra AudioEnvelopeAnalyzer dentro del pipeline general
/// de AudioAnalysisEngine.
///
/// Esta etapa no calcula directamente la envolvente.
/// Su responsabilidad es ejecutar el analizador especializado,
/// guardar el resultado y registrar únicamente problemas de
/// ejecución o confiabilidad.
/// </summary>
public class EnvelopeAnalysisStage : IAudioAnalysisStage
{
    private readonly
        IAudioAnalyzer<AudioEnvelopeAnalysisResult>
        _envelopeAnalyzer;

    /// <summary>
    /// Nombre legible de la etapa.
    /// </summary>
    public string Name =>
        "Análisis de envolvente energética";

    /// <summary>
    /// Orden de ejecución dentro del pipeline.
    ///
    /// Se ejecuta después del análisis de silencio.
    /// </summary>
    public int Order =>
        200;

    /// <summary>
    /// Crea la etapa utilizando un analizador
    /// de envolvente energética.
    /// </summary>
    public EnvelopeAnalysisStage(
        IAudioAnalyzer<AudioEnvelopeAnalysisResult>
            envelopeAnalyzer)
    {
        _envelopeAnalyzer =
            envelopeAnalyzer ??
            throw new ArgumentNullException(
                nameof(envelopeAnalyzer));
    }

    /// <summary>
    /// Ejecuta el análisis de envolvente y guarda
    /// el resultado en el contexto compartido.
    /// </summary>
    public async Task ExecuteAsync(
        AudioAnalysisContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        cancellationToken.ThrowIfCancellationRequested();

        AudioEnvelopeAnalysisResult envelopeResult =
            await _envelopeAnalyzer.AnalyzeAsync(
                context.FilePath,
                cancellationToken);

        context.AnalysisResult.Envelope =
            envelopeResult;

        RegisterWarnings(
            context.AnalysisResult,
            envelopeResult);
    }

    /// <summary>
    /// Registra únicamente situaciones que impiden utilizar
    /// las mediciones de envolvente con suficiente confianza.
    ///
    /// La presencia de fade-in, fade-out o cola de
    /// reverberación no constituye una advertencia.
    /// </summary>
    private static void RegisterWarnings(
        AudioAnalysisResult analysisResult,
        AudioEnvelopeAnalysisResult envelopeResult)
    {
        if (envelopeResult.HasError)
        {
            analysisResult.AddWarning(
                "El análisis de envolvente energética no pudo " +
                "completarse correctamente.");

            return;
        }

        if (!envelopeResult.AnalysisCompleted)
        {
            analysisResult.AddWarning(
                "El análisis de envolvente energética " +
                "quedó incompleto.");
        }

        if (!envelopeResult.IsReliable)
        {
            analysisResult.AddWarning(
                "Las mediciones de envolvente energética " +
                "no son suficientemente confiables para " +
                "realizar comparaciones.");
        }
    }
}