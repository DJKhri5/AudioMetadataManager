using AudioMetadataManager.UI.Services.AudioAnalysis.Algorithms;
using AudioMetadataManager.UI.Services.AudioAnalysis.Interfaces;
using AudioMetadataManager.UI.Services.AudioAnalysis.Models;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Analyzers;

/// <summary>
/// Analiza la envolvente energética mediante una lectura
/// PCM independiente.
///
/// Esta clase mantiene la compatibilidad con IAudioAnalyzer,
/// pero delega toda la lógica matemática en
/// AudioEnvelopeAlgorithm.
/// </summary>
public class AudioEnvelopeAnalyzer :
    IAudioAnalyzer<AudioEnvelopeAnalysisResult>
{
    private readonly IAudioSampleReader _sampleReader;
    private readonly AudioEnvelopeAnalysisOptions _options;

    /// <summary>
    /// Nombre legible del analizador.
    /// </summary>
    public string Name =>
        "Analizador de envolvente energética";

    /// <summary>
    /// Crea el analizador utilizando un lector PCM.
    /// </summary>
    public AudioEnvelopeAnalyzer(
        IAudioSampleReader sampleReader,
        AudioEnvelopeAnalysisOptions? options = null)
    {
        _sampleReader =
            sampleReader ??
            throw new ArgumentNullException(
                nameof(sampleReader));

        _options =
            options ??
            new AudioEnvelopeAnalysisOptions();

        _options.Validate();
    }

    /// <summary>
    /// Abre el archivo, lee sus bloques PCM y los entrega
    /// al algoritmo reutilizable de envolvente.
    /// </summary>
    public async Task<AudioEnvelopeAnalysisResult> AnalyzeAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        AudioEnvelopeAlgorithm algorithm =
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

            int framesPerBlock =
                CalculateFramesPerBlock(
                    streamInfo.SampleRate,
                    _options.WindowDuration);

            await foreach (
                AudioSampleBlock block
                in _sampleReader.ReadBlocksAsync(
                    filePath,
                    framesPerBlock,
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

    /// <summary>
    /// Calcula el tamaño de bloque utilizado por la lectura
    /// independiente.
    ///
    /// Se mantiene equivalente al comportamiento previo,
    /// donde cada bloque utilizaba la duración configurada
    /// para una ventana RMS.
    /// </summary>
    private static int CalculateFramesPerBlock(
        int sampleRate,
        TimeSpan windowDuration)
    {
        if (sampleRate <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sampleRate),
                sampleRate,
                "La frecuencia de muestreo debe ser mayor que cero.");
        }

        if (windowDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(windowDuration),
                windowDuration,
                "La duración de ventana debe ser mayor que cero.");
        }

        double frames =
            sampleRate *
            windowDuration.TotalSeconds;

        return Math.Max(
            1,
            (int)Math.Round(frames));
    }
}