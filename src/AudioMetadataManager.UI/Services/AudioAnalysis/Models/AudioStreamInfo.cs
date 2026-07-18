using System.IO;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Describe las características técnicas del flujo PCM
/// que será entregado a los analizadores.
///
/// Esta información debe corresponder al audio realmente
/// decodificado, no solamente a lo declarado por el contenedor.
/// </summary>
public class AudioStreamInfo
{
    /// <summary>
    /// Ruta completa del archivo de origen.
    /// </summary>
    public string FilePath { get; set; } =
        string.Empty;

    /// <summary>
    /// Frecuencia de muestreo del audio decodificado.
    ///
    /// Ejemplos:
    /// 44100
    /// 48000
    /// 96000
    /// </summary>
    public int SampleRate { get; set; }

    /// <summary>
    /// Cantidad de canales del audio decodificado.
    ///
    /// Ejemplos:
    /// 1 = mono
    /// 2 = estéreo
    /// </summary>
    public int Channels { get; set; }

    /// <summary>
    /// Cantidad total de frames PCM cuando puede determinarse.
    ///
    /// Un frame contiene una muestra por cada canal.
    /// En estéreo, un frame contiene dos muestras.
    /// </summary>
    public long TotalFrames { get; set; }

    /// <summary>
    /// Duración técnica calculada desde el flujo decodificado.
    /// </summary>
    public TimeSpan DecodedDuration { get; set; }

    /// <summary>
    /// Nombre legible del codec o decodificador.
    ///
    /// Ejemplos:
    /// MPEG Layer III
    /// FLAC
    /// PCM / WAV
    /// </summary>
    public string CodecName { get; set; } =
        string.Empty;

    /// <summary>
    /// Indica si la información técnica es suficiente
    /// para iniciar los analizadores.
    /// </summary>
    public bool IsValid =>
        SampleRate > 0 &&
        Channels > 0;

    /// <summary>
    /// Cantidad aproximada de muestras por segundo,
    /// considerando todos los canales.
    /// </summary>
    public long SamplesPerSecond =>
        IsValid
            ? (long)SampleRate * Channels
            : 0;

    /// <summary>
    /// Nombre del archivo sin la ruta completa.
    /// </summary>
    public string FileName =>
        string.IsNullOrWhiteSpace(FilePath)
            ? string.Empty
            : Path.GetFileName(FilePath);

    /// <summary>
    /// Descripción compacta para registros y diagnóstico.
    /// </summary>
    public string Summary =>
        IsValid
            ? $"{SampleRate} Hz · {Channels} canal(es) · " +
              $"{FormatDuration(DecodedDuration)}"
            : "Información de audio no válida";

    private static string FormatDuration(
        TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
        {
            duration = TimeSpan.Zero;
        }

        if (duration.TotalHours >= 1)
        {
            return duration.ToString(
                @"h\:mm\:ss\.fff");
        }

        return duration.ToString(
            @"m\:ss\.fff");
    }
}