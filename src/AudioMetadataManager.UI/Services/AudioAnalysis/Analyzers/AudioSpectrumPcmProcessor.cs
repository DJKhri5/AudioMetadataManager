using AudioMetadataManager.UI.Services.AudioAnalysis.Algorithms;
using AudioMetadataManager.UI.Services.AudioAnalysis.Interfaces;
using AudioMetadataManager.UI.Services.AudioAnalysis.Models;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Analyzers;

/// <summary>
/// Adapta AudioSpectrumAlgorithm al flujo PCM compartido
/// administrado por AudioPcmAnalysisCoordinator.
///
/// Este procesador no abre ni decodifica el archivo.
/// Publica tanto el resultado resumido como el perfil
/// espectral reutilizable dentro de AudioAnalysisContext.
/// </summary>
public class AudioSpectrumPcmProcessor :
    IAudioPcmAnalysisProcessor
{
    private readonly AudioSpectrumAlgorithm _algorithm;
    private readonly AudioToneProfileCalculator
        _toneProfileCalculator;

    /// <summary>
    /// Nombre legible del procesador.
    /// </summary>
    public string Name =>
        "Procesador PCM de análisis espectral";

    /// <summary>
    /// Orden de ejecución dentro de la lectura compartida.
    /// </summary>
    public int Order =>
        300;

    /// <summary>
    /// Crea el procesador con la configuración indicada.
    /// </summary>
    public AudioSpectrumPcmProcessor(
    AudioSpectrumAnalysisOptions? options = null)
    {
        _algorithm =
            new AudioSpectrumAlgorithm(
                options);

        _toneProfileCalculator =
            new AudioToneProfileCalculator();
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

        context.AnalysisResult.Spectrum =
            _algorithm.Result;
    }

    /// <summary>
    /// Entrega un bloque PCM al algoritmo espectral.
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
    /// Finaliza el análisis y publica sus dos salidas:
    /// resultado resumido y perfil espectral reutilizable.
    /// </summary>
    public void Complete(
        AudioAnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        AudioSpectrumAnalysisResult result =
            _algorithm.Complete();

        AudioSpectrumProfile profile =
            _algorithm.Profile;

        AudioToneProfile toneProfile =
            _toneProfileCalculator.Calculate(
            profile);

        context.AnalysisResult.Spectrum =
            result;

        context.AnalysisResult.ToneProfile =
            toneProfile;

        context.SetData(
            result);

        context.SetData(
            profile);

        context.SetData(
            toneProfile);
    }

    /// <summary>
    /// Registra un fallo controlado y publica el estado
    /// resultante para que otros módulos puedan consultarlo.
    /// </summary>
    public void Fail(
    AudioAnalysisContext context,
    string? errorMessage)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        AudioSpectrumAnalysisResult result =
            _algorithm.Fail(
                errorMessage);

        AudioSpectrumProfile profile =
            _algorithm.Profile;

        context.AnalysisResult.Spectrum =
            result;

        context.SetData(
            result);

        context.SetData(
            profile);
    }
}