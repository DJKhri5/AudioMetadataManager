using AudioMetadataManager.UI.Services.AudioAnalysis.Algorithms;
using AudioMetadataManager.UI.Services.AudioAnalysis.Interfaces;
using AudioMetadataManager.UI.Services.AudioAnalysis.Models;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Analyzers;

/// <summary>
/// Analiza el silencio exterior de un archivo mediante
/// una lectura PCM independiente.
///
/// Esta clase conserva la compatibilidad con el contrato
/// IAudioAnalyzer, pero delega toda la lógica matemática
/// en AudioSilenceAlgorithm.
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
    /// Abre el archivo, lee sus bloques PCM y los entrega
    /// al algoritmo reutilizable de silencio.
    /// </summary>
    public async Task<AudioSilenceAnalysisResult> AnalyzeAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        AudioSilenceAlgorithm algorithm =
            new(
                _options);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            AudioStreamInfo streamInfo =
                await _sampleReader.ReadInfoAsync(
                    filePath,
                    cancellationToken);

            algorithm.Initialize(
                streamInfo);

            await foreach (
                AudioSampleBlock block
                in _sampleReader.ReadBlocksAsync(
                    filePath,
                    _options.FramesPerBlock,
                    cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                algorithm.ProcessBlock(
                    block);
            }

            return algorithm.Complete();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return algorithm.Fail(
                exception.Message);
        }
    }
}