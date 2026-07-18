namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Representa un bloque consecutivo de muestras PCM
/// normalizadas para su análisis.
///
/// Los valores esperados normalmente están entre:
/// -1.0 y 1.0.
/// </summary>
public class AudioSampleBlock
{
    /// <summary>
    /// Muestras PCM intercaladas por canal.
    ///
    /// Para audio estéreo:
    /// izquierda, derecha, izquierda, derecha...
    /// </summary>
    public float[] Samples { get; set; } =
        Array.Empty<float>();

    /// <summary>
    /// Posición del primer frame de este bloque dentro
    /// del archivo decodificado.
    /// </summary>
    public long StartFrame { get; set; }

    /// <summary>
    /// Cantidad de canales del bloque.
    /// </summary>
    public int Channels { get; set; }

    /// <summary>
    /// Frecuencia de muestreo del bloque.
    /// </summary>
    public int SampleRate { get; set; }

    /// <summary>
    /// Indica si este es el último bloque del archivo.
    /// </summary>
    public bool IsFinalBlock { get; set; }

    /// <summary>
    /// Cantidad de muestras individuales contenidas.
    /// </summary>
    public int SampleCount =>
        Samples.Length;

    /// <summary>
    /// Cantidad de frames contenidos en el bloque.
    ///
    /// En estéreo:
    /// 8.192 muestras representan 4.096 frames.
    /// </summary>
    public int FrameCount =>
        Channels > 0
            ? Samples.Length / Channels
            : 0;

    /// <summary>
    /// Posición temporal donde comienza este bloque.
    /// </summary>
    public TimeSpan StartTime =>
        SampleRate > 0
            ? TimeSpan.FromSeconds(
                (double)StartFrame /
                SampleRate)
            : TimeSpan.Zero;

    /// <summary>
    /// Duración aproximada del bloque.
    /// </summary>
    public TimeSpan Duration =>
        SampleRate > 0
            ? TimeSpan.FromSeconds(
                (double)FrameCount /
                SampleRate)
            : TimeSpan.Zero;

    /// <summary>
    /// Indica si el bloque contiene información utilizable.
    /// </summary>
    public bool IsValid =>
        Samples.Length > 0 &&
        Channels > 0 &&
        SampleRate > 0;
}