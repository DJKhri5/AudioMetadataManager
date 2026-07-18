using AudioMetadataManager.UI.Services.AudioAnalysis.Interfaces;
using AudioMetadataManager.UI.Services.AudioAnalysis.Models;
using NAudio.Wave;
using System.IO;
using System.Runtime.CompilerServices;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Readers;

/// <summary>
/// Implementación de IAudioSampleReader basada en NAudio.
///
/// Decodifica los archivos compatibles y entrega muestras PCM
/// normalizadas de 32 bits flotantes, intercaladas por canal.
/// </summary>
public class NAudioSampleReader : IAudioSampleReader
{
    /// <summary>
    /// Lee la información técnica del flujo realmente
    /// decodificable por NAudio.
    ///
    /// No carga el archivo completo en memoria.
    /// </summary>
    public async Task<AudioStreamInfo> ReadInfoAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ValidateFilePath(filePath);

        cancellationToken.ThrowIfCancellationRequested();

        /*
         * La apertura y lectura de información se ejecutan en
         * segundo plano para no bloquear la interfaz WPF.
         */
        return await Task.Run(
            () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                using AudioFileReader reader =
                    new(filePath);

                cancellationToken.ThrowIfCancellationRequested();

                WaveFormat format =
                    reader.WaveFormat;

                TimeSpan decodedDuration =
                    reader.TotalTime;

                long totalFrames =
                    CalculateTotalFrames(
                        decodedDuration,
                        format.SampleRate);

                return new AudioStreamInfo
                {
                    FilePath =
                        filePath,

                    SampleRate =
                        format.SampleRate,

                    Channels =
                        format.Channels,

                    TotalFrames =
                        totalFrames,

                    DecodedDuration =
                        decodedDuration,

                    CodecName =
                        GetCodecName(filePath)
                };
            },
            cancellationToken);
    }

    /// <summary>
    /// Lee progresivamente bloques de muestras PCM float.
    ///
    /// Las muestras se entregan intercaladas por canal:
    /// izquierda, derecha, izquierda, derecha...
    /// </summary>
    public async IAsyncEnumerable<AudioSampleBlock>
        ReadBlocksAsync(
            string filePath,
            int framesPerBlock = 4096,
            [EnumeratorCancellation]
            CancellationToken cancellationToken = default)
    {
        ValidateFilePath(filePath);
        ValidateFramesPerBlock(framesPerBlock);

        cancellationToken.ThrowIfCancellationRequested();

        using AudioFileReader reader =
            new(filePath);

        /*
         * AudioFileReader implementa ISampleProvider y entrega
         * muestras de 32 bits flotantes.
         */
        ISampleProvider sampleProvider =
            reader;

        int channels =
            sampleProvider.WaveFormat.Channels;

        int sampleRate =
            sampleProvider.WaveFormat.SampleRate;

        if (channels <= 0)
        {
            throw new InvalidDataException(
                "El flujo decodificado no contiene " +
                "una cantidad válida de canales.");
        }

        if (sampleRate <= 0)
        {
            throw new InvalidDataException(
                "El flujo decodificado no contiene " +
                "una frecuencia de muestreo válida.");
        }

        int samplesPerBlock =
            checked(framesPerBlock * channels);

        float[] readBuffer =
            new float[samplesPerBlock];

        long startFrame = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int samplesRead =
                sampleProvider.Read(
                    readBuffer,
                    0,
                    readBuffer.Length);

            if (samplesRead <= 0)
            {
                yield break;
            }

            /*
             * Un lector puede devolver una cantidad menor que
             * el tamaño del buffer en el último bloque.
             *
             * Copiamos únicamente las muestras válidas para que
             * los analizadores no procesen posiciones sobrantes.
             */
            float[] blockSamples =
                new float[samplesRead];

            Array.Copy(
                readBuffer,
                blockSamples,
                samplesRead);

            int framesRead =
                samplesRead / channels;

            bool isFinalBlock =
                samplesRead < readBuffer.Length ||
                reader.Position >= reader.Length;

            AudioSampleBlock block = new()
            {
                Samples =
                    blockSamples,

                StartFrame =
                    startFrame,

                Channels =
                    channels,

                SampleRate =
                    sampleRate,

                IsFinalBlock =
                    isFinalBlock
            };

            yield return block;

            startFrame +=
                framesRead;

            if (isFinalBlock)
            {
                yield break;
            }

            /*
             * Entregamos temporalmente el control para que una
             * operación extensa no monopolice el hilo llamador.
             *
             * El AudioAnalysisEngine se ejecutará posteriormente
             * mediante una tarea de trabajo en segundo plano.
             */
            await Task.Yield();
        }
    }

    /// <summary>
    /// Comprueba que la ruta recibida sea utilizable.
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
                "La ruta del archivo debe ser completa.",
                nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "No se encontró el archivo de audio.",
                filePath);
        }
    }

    /// <summary>
    /// Comprueba que el tamaño de bloque solicitado sea válido.
    /// </summary>
    private static void ValidateFramesPerBlock(
        int framesPerBlock)
    {
        if (framesPerBlock <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(framesPerBlock),
                framesPerBlock,
                "La cantidad de frames por bloque debe " +
                "ser mayor que cero.");
        }

        /*
         * Evita solicitudes accidentales que podrían reservar
         * bloques de memoria excesivamente grandes.
         */
        if (framesPerBlock > 1_048_576)
        {
            throw new ArgumentOutOfRangeException(
                nameof(framesPerBlock),
                framesPerBlock,
                "La cantidad de frames por bloque es " +
                "excesivamente grande.");
        }
    }

    /// <summary>
    /// Calcula aproximadamente la cantidad total de frames
    /// utilizando la duración decodificada y el sample rate.
    /// </summary>
    private static long CalculateTotalFrames(
        TimeSpan duration,
        int sampleRate)
    {
        if (duration <= TimeSpan.Zero ||
            sampleRate <= 0)
        {
            return 0;
        }

        double frames =
            duration.TotalSeconds *
            sampleRate;

        if (frames >= long.MaxValue)
        {
            return long.MaxValue;
        }

        return Math.Max(
            0,
            (long)Math.Round(frames));
    }

    /// <summary>
    /// Obtiene un nombre legible del codec o contenedor
    /// según la extensión del archivo.
    ///
    /// Esta información todavía es descriptiva. Más adelante
    /// podremos obtener el codec real desde el decodificador.
    /// </summary>
    private static string GetCodecName(
        string filePath)
    {
        string extension =
            Path.GetExtension(filePath)
                .Trim()
                .ToLowerInvariant();

        return extension switch
        {
            ".mp3" =>
                "MPEG Layer III",

            ".wav" =>
                "PCM / WAV",

            ".aif" or ".aiff" =>
                "AIFF",

            ".flac" =>
                "FLAC",

            ".m4a" =>
                "AAC / ALAC",

            ".aac" =>
                "AAC",

            ".wma" =>
                "Windows Media Audio",

            ".ogg" =>
                "Ogg Vorbis",

            ".opus" =>
                "Opus",

            _ =>
                "Codec sin identificar"
        };
    }
}