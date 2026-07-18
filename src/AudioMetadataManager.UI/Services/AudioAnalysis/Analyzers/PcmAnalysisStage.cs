using AudioMetadataManager.UI.Services.AudioAnalysis.Interfaces;
using AudioMetadataManager.UI.Services.AudioAnalysis.Models;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Analyzers;

/// <summary>
/// Ejecuta la lectura PCM compartida dentro del pipeline
/// general de AudioAnalysisEngine.
///
/// Esta etapa abre y decodifica el archivo una sola vez,
/// distribuyendo cada bloque entre todos los procesadores
/// registrados.
/// </summary>
public class PcmAnalysisStage : IAudioAnalysisStage
{
    private readonly AudioPcmAnalysisCoordinator _coordinator;

    /// <summary>
    /// Nombre legible de la etapa.
    /// </summary>
    public string Name =>
        "Análisis PCM compartido";

    /// <summary>
    /// Orden de ejecución dentro del pipeline.
    /// </summary>
    public int Order =>
        100;

    /// <summary>
    /// Crea la etapa utilizando el coordinador PCM.
    /// </summary>
    public PcmAnalysisStage(
        AudioPcmAnalysisCoordinator coordinator)
    {
        _coordinator =
            coordinator ??
            throw new ArgumentNullException(
                nameof(coordinator));
    }

    /// <summary>
    /// Ejecuta la lectura y los procesadores PCM.
    /// </summary>
    public async Task ExecuteAsync(
        AudioAnalysisContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        cancellationToken.ThrowIfCancellationRequested();

        await _coordinator.ExecuteAsync(
            context,
            cancellationToken);

        RegisterWarnings(
            context.AnalysisResult);
    }

    /// <summary>
    /// Registra advertencias globales únicamente cuando
    /// alguno de los módulos no pudo generar mediciones
    /// utilizables.
    /// </summary>
    private static void RegisterWarnings(
        AudioAnalysisResult analysisResult)
    {
        AudioSilenceAnalysisResult silence =
            analysisResult.Silence;

        if (silence.HasError)
        {
            analysisResult.AddWarning(
                "El análisis de silencio no pudo completarse correctamente.");
        }
        else if (!silence.AnalysisCompleted)
        {
            analysisResult.AddWarning(
                "El análisis de silencio quedó incompleto.");
        }
        else if (!silence.IsReliable)
        {
            analysisResult.AddWarning(
                "Las mediciones de silencio no son suficientemente confiables para realizar comparaciones.");
        }

        AudioEnvelopeAnalysisResult envelope =
            analysisResult.Envelope;

        if (envelope.HasError)
        {
            analysisResult.AddWarning(
                "El análisis de envolvente energética no pudo completarse correctamente.");
        }
        else if (!envelope.AnalysisCompleted)
        {
            analysisResult.AddWarning(
                "El análisis de envolvente energética quedó incompleto.");
        }
        else if (!envelope.IsReliable)
        {
            analysisResult.AddWarning(
                "Las mediciones de envolvente energética no son suficientemente confiables para realizar comparaciones.");
        }
    }
}