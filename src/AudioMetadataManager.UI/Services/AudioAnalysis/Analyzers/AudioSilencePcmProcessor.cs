using AudioMetadataManager.UI.Services.AudioAnalysis.Algorithms;
using AudioMetadataManager.UI.Services.AudioAnalysis.Interfaces;
using AudioMetadataManager.UI.Services.AudioAnalysis.Models;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Analyzers;

/// <summary>
/// Adapta AudioSilenceAlgorithm al flujo PCM compartido
/// administrado por AudioPcmAnalysisCoordinator.
///
/// Este procesador no abre ni decodifica el archivo.
/// Toda la lógica matemática reside en AudioSilenceAlgorithm.
/// </summary>
public class AudioSilencePcmProcessor :
    IAudioPcmAnalysisProcessor
{
    private readonly AudioSilenceAlgorithm _algorithm;

    /// <summary>
    /// Nombre legible del procesador.
    /// </summary>
    public string Name =>
        "Procesador PCM de silencio exterior";

    /// <summary>
    /// Orden de ejecución dentro de la lectura compartida.
    /// </summary>
    public int Order =>
        100;

    /// <summary>
    /// Crea el procesador con la configuración indicada.
    /// </summary>
    public AudioSilencePcmProcessor(
        AudioSilenceAnalysisOptions? options = null)
    {
        _algorithm =
            new AudioSilenceAlgorithm(
                options);
    }

    /// <summary>
    /// Prepara el algoritmo antes de recibir bloques PCM.
    /// </summary>
    public void Initialize(
        AudioAnalysisContext context,
        AudioStreamInfo streamInfo)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        ArgumentNullException.ThrowIfNull(
            streamInfo);

        _algorithm.Initialize(
            streamInfo);

        context.AnalysisResult.Silence =
            _algorithm.Result;
    }

    /// <summary>
    /// Entrega un bloque PCM al algoritmo reutilizable.
    /// </summary>
    public void ProcessBlock(
        AudioAnalysisContext context,
        AudioSampleBlock block)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        ArgumentNullException.ThrowIfNull(
            block);

        _algorithm.ProcessBlock(
            block);
    }

    /// <summary>
    /// Finaliza el algoritmo y guarda su resultado.
    /// </summary>
    public void Complete(
        AudioAnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        AudioSilenceAnalysisResult result =
            _algorithm.Complete();

        context.AnalysisResult.Silence =
            result;

        context.SetData(
            result);
    }


    /// <summary>
    /// Registra un fallo controlado del algoritmo.
    /// </summary>
    public void Fail(
        AudioAnalysisContext context,
        string? errorMessage)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        AudioSilenceAnalysisResult result =
            _algorithm.Fail(
                errorMessage);

        context.AnalysisResult.Silence =
            result;

        context.SetData(
            result);
    }
}