using AudioMetadataManager.UI.Services.AudioAnalysis.Interfaces;
using AudioMetadataManager.UI.Services.AudioAnalysis.Models;
using AudioMetadataManager.UI.Services.AudioAnalysis.Readers;
using System.IO;

namespace AudioMetadataManager.UI.Services.AudioAnalysis;

/// <summary>
/// Coordina una única lectura PCM del archivo y distribuye
/// cada bloque decodificado entre todos los procesadores
/// registrados.
///
/// Este coordinador no interpreta las mediciones ni modifica
/// el archivo de audio.
/// </summary>
public class AudioPcmAnalysisCoordinator
{
    private readonly IAudioSampleReader _sampleReader;
    private readonly TechnicalFormatReader _technicalFormatReader;
    private readonly IReadOnlyList<IAudioPcmAnalysisProcessor>
        _processors;
    private readonly int _framesPerBlock;

    /// <summary>
    /// Crea el coordinador de lectura PCM compartida.
    /// </summary>
    public AudioPcmAnalysisCoordinator(
        IAudioSampleReader sampleReader,
        IEnumerable<IAudioPcmAnalysisProcessor> processors,
        int framesPerBlock = 4096)
    {
        _sampleReader =
            sampleReader ??
            throw new ArgumentNullException(
                nameof(sampleReader));

        _technicalFormatReader =
            new TechnicalFormatReader();

        ArgumentNullException.ThrowIfNull(
            processors);

        List<IAudioPcmAnalysisProcessor> orderedProcessors =
            processors
                .Where(processor => processor is not null)
                .OrderBy(processor => processor.Order)
                .ToList();

        if (orderedProcessors.Count == 0)
        {
            throw new ArgumentException(
                "Debe registrarse al menos un procesador PCM.",
                nameof(processors));
        }

        if (framesPerBlock <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(framesPerBlock),
                framesPerBlock,
                "La cantidad de frames por bloque debe ser mayor que cero.");
        }

        _processors =
            orderedProcessors;

        _framesPerBlock =
            framesPerBlock;
    }

    /// <summary>
    /// Abre el archivo, obtiene la información técnica
    /// y distribuye todos los bloques PCM mediante una sola
    /// lectura secuencial.
    /// </summary>
    public async Task ExecuteAsync(
        AudioAnalysisContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        cancellationToken.ThrowIfCancellationRequested();

        AudioStreamInfo streamInfo =
            await _sampleReader.ReadInfoAsync(
                context.FilePath,
                cancellationToken);

        if (!streamInfo.IsValid)
        {
            FailAll(
                context,
                "La información del flujo PCM no es válida.");

            return;
        }

        context.StreamInfo =
            streamInfo;

        AudioTechnicalFormatInfo technicalFormat;

        try
        {
            technicalFormat =
                _technicalFormatReader.Read(
                    context.FilePath);
        }
        catch (Exception exception)
        {
            context.AnalysisResult.AddWarning(
                "No fue posible leer todas las propiedades " +
                $"técnicas declaradas: {exception.Message}");

            technicalFormat =
                BuildTechnicalFormatInfo(
                    context.FilePath,
                    streamInfo);
        }

        context.TechnicalFormatInfo =
            technicalFormat;

        context.AnalysisResult.TechnicalFormat =
            technicalFormat;

        context.SetData(
            technicalFormat);

        List<IAudioPcmAnalysisProcessor> activeProcessors =
            InitializeProcessors(
                context,
                streamInfo);

        if (activeProcessors.Count == 0)
        {
            context.AnalysisResult.AddWarning(
                "Ningún procesador PCM pudo inicializarse.");

            return;
        }

        await foreach (
            AudioSampleBlock block
            in _sampleReader.ReadBlocksAsync(
                context.FilePath,
                _framesPerBlock,
                cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!block.IsValid)
            {
                continue;
            }

            ProcessBlock(
                context,
                block,
                activeProcessors);
        }

        CompleteProcessors(
            context,
            activeProcessors);
    }

    /// <summary>
    /// Inicializa todos los procesadores y conserva únicamente
    /// aquellos que pudieron prepararse correctamente.
    /// </summary>
    private List<IAudioPcmAnalysisProcessor>
        InitializeProcessors(
            AudioAnalysisContext context,
            AudioStreamInfo streamInfo)
    {
        List<IAudioPcmAnalysisProcessor> activeProcessors =
            new();

        foreach (
            IAudioPcmAnalysisProcessor processor
            in _processors)
        {
            try
            {
                processor.Initialize(
                    context,
                    streamInfo);

                activeProcessors.Add(
                    processor);
            }
            catch (Exception exception)
            {
                processor.Fail(
                    context,
                    exception.Message);

                context.AnalysisResult.AddWarning(
                    $"El procesador PCM \"{processor.Name}\" " +
                    $"no pudo inicializarse: {exception.Message}");
            }
        }

        return activeProcessors;
    }

    /// <summary>
    /// Distribuye un bloque PCM entre los procesadores activos.
    /// </summary>
    private static void ProcessBlock(
        AudioAnalysisContext context,
        AudioSampleBlock block,
        List<IAudioPcmAnalysisProcessor> activeProcessors)
    {
        for (int index = activeProcessors.Count - 1;
            index >= 0;
            index--)
        {
            IAudioPcmAnalysisProcessor processor =
                activeProcessors[index];

            try
            {
                processor.ProcessBlock(
                    context,
                    block);
            }
            catch (Exception exception)
            {
                processor.Fail(
                    context,
                    exception.Message);

                context.AnalysisResult.AddWarning(
                    $"El procesador PCM \"{processor.Name}\" " +
                    $"terminó con un error: {exception.Message}");

                activeProcessors.RemoveAt(
                    index);
            }
        }
    }

    /// <summary>
    /// Finaliza todos los procesadores que permanecieron
    /// activos hasta el final de la lectura.
    /// </summary>
    private static void CompleteProcessors(
        AudioAnalysisContext context,
        IReadOnlyList<IAudioPcmAnalysisProcessor>
            activeProcessors)
    {
        foreach (
            IAudioPcmAnalysisProcessor processor
            in activeProcessors)
        {
            try
            {
                processor.Complete(
                    context);
            }
            catch (Exception exception)
            {
                processor.Fail(
                    context,
                    exception.Message);

                context.AnalysisResult.AddWarning(
                    $"El procesador PCM \"{processor.Name}\" " +
                    $"no pudo finalizar correctamente: " +
                    $"{exception.Message}");
            }
        }
    }

    /// <summary>
    /// Construye la información técnica declarada del archivo.
    ///
    /// Esta información proviene del archivo y del contenedor.
    /// No representa necesariamente el audio realmente
    /// decodificado.
    /// </summary>
    private static AudioTechnicalFormatInfo BuildTechnicalFormatInfo(
        string filePath,
        AudioStreamInfo streamInfo)
    {
        FileInfo file =
            new(filePath);

        long fileSizeBytes =
            file.Exists
                ? file.Length
                : 0;

        int bitrate = 0;

        if (fileSizeBytes > 0 &&
            streamInfo.DecodedDuration.TotalSeconds > 0)
        {
            bitrate =
                (int)Math.Round(
                    fileSizeBytes * 8.0 /
                    streamInfo.DecodedDuration.TotalSeconds);
        }

        string extension =
            Path.GetExtension(filePath)
                .ToLowerInvariant();

        bool isLossless =
            extension is ".flac" or
            ".wav" or
            ".aif" or
            ".aiff";

        return new AudioTechnicalFormatInfo
        {
            FilePath =
                filePath,

            FileExtension =
                extension,

            ContainerName =
                extension.TrimStart('.')
                    .ToUpperInvariant(),

            CodecName =
                streamInfo.CodecName,

            DeclaredBitrateBitsPerSecond =
                0,

            EstimatedAverageBitrateBitsPerSecond =
                bitrate,

            DeclaredSampleRate =
                streamInfo.SampleRate,

            DeclaredChannels =
                streamInfo.Channels,

            BitsPerSample =
                0,

            IsLossless =
                isLossless,

            IsLossy =
                !isLossless
        };
    }

    /// <summary>
    /// Registra el mismo fallo en todos los procesadores.
    /// </summary>
    private void FailAll(
        AudioAnalysisContext context,
        string? errorMessage)
    {
        foreach (
            IAudioPcmAnalysisProcessor processor
            in _processors)
        {
            processor.Fail(
                context,
                errorMessage);
        }
    }
}