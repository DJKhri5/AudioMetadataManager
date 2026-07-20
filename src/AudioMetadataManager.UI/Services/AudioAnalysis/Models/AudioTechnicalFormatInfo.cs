using System.IO;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Describe la información técnica declarada o identificada
/// desde el archivo, su contenedor y su códec.
///
/// Este modelo no representa necesariamente el audio real
/// decodificado. Para eso se utiliza AudioStreamInfo.
/// </summary>
public class AudioTechnicalFormatInfo
{
    /// <summary>
    /// Ruta completa del archivo.
    /// </summary>
    public string FilePath { get; set; } =
        string.Empty;

    /// <summary>
    /// Extensión del archivo, incluyendo el punto.
    ///
    /// Ejemplos:
    /// .mp3
    /// .flac
    /// .wav
    /// </summary>
    public string FileExtension { get; set; } =
        string.Empty;

    /// <summary>
    /// Nombre legible del contenedor.
    ///
    /// Ejemplos:
    /// MPEG Audio
    /// FLAC
    /// WAV
    /// AIFF
    /// </summary>
    public string ContainerName { get; set; } =
        string.Empty;

    /// <summary>
    /// Nombre legible del códec declarado o identificado.
    /// </summary>
    public string CodecName { get; set; } =
        string.Empty;

    /// <summary>
    /// Bitrate realmente declarado por el encabezado o
    /// contenedor, expresado en bits por segundo.
    ///
    /// Puede permanecer en cero cuando el lector actual no
    /// proporciona este dato directamente.
    /// </summary>
    public int DeclaredBitrateBitsPerSecond { get; set; }

    /// <summary>
    /// Bitrate medio estimado mediante el tamaño total del
    /// archivo y su duración decodificada.
    ///
    /// Esta medición puede incluir etiquetas, carátulas y otros
    /// datos no pertenecientes directamente al flujo de audio.
    /// </summary>
    public int EstimatedAverageBitrateBitsPerSecond { get; set; }

    /// <summary>
    /// Frecuencia de muestreo declarada por el archivo.
    /// </summary>
    public int DeclaredSampleRate { get; set; }

    /// <summary>
    /// Cantidad de canales declarada.
    /// </summary>
    public int DeclaredChannels { get; set; }

    /// <summary>
    /// Profundidad de bits declarada cuando corresponde.
    ///
    /// En formatos con pérdida puede no estar disponible.
    /// </summary>
    public int BitsPerSample { get; set; }

    /// <summary>
    /// Indica si el formato utiliza compresión con pérdida.
    /// </summary>
    public bool IsLossy { get; set; }

    /// <summary>
    /// Indica si el formato se declara sin pérdida.
    /// </summary>
    public bool IsLossless { get; set; }

    /// <summary>
    /// Indica si existe información mínima utilizable.
    /// </summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(
            FileExtension) ||
        !string.IsNullOrWhiteSpace(
            CodecName) ||
        !string.IsNullOrWhiteSpace(
            ContainerName);

    /// <summary>
    /// Nombre del archivo sin la ruta completa.
    /// </summary>
    public string FileName =>
        string.IsNullOrWhiteSpace(
            FilePath)
            ? string.Empty
            : Path.GetFileName(
                FilePath);

    /// <summary>
    /// Bitrate declarado expresado en kbps.
    /// </summary>
    public double DeclaredBitrateKbps =>
        DeclaredBitrateBitsPerSecond > 0
            ? DeclaredBitrateBitsPerSecond /
              1000.0
            : 0;

    /// <summary>
    /// Bitrate medio estimado expresado en kbps.
    /// </summary>
    public double EstimatedAverageBitrateKbps =>
        EstimatedAverageBitrateBitsPerSecond > 0
            ? EstimatedAverageBitrateBitsPerSecond /
              1000.0
            : 0;

    /// <summary>
    /// Indica si existe un bitrate declarado válido.
    /// </summary>
    public bool HasDeclaredBitrate =>
        DeclaredBitrateBitsPerSecond > 0;

    /// <summary>
    /// Indica si existe un bitrate medio estimado válido.
    /// </summary>
    public bool HasEstimatedAverageBitrate =>
        EstimatedAverageBitrateBitsPerSecond > 0;

    /// <summary>
    /// Indica si existe una frecuencia de muestreo declarada.
    /// </summary>
    public bool HasDeclaredSampleRate =>
        DeclaredSampleRate > 0;

    /// <summary>
    /// Indica si existe información de canales declarados.
    /// </summary>
    public bool HasDeclaredChannels =>
        DeclaredChannels > 0;

    /// <summary>
    /// Indica si existe profundidad de bits declarada.
    /// </summary>
    public bool HasBitsPerSample =>
        BitsPerSample > 0;

    /// <summary>
    /// Bitrate declarado en formato legible.
    /// </summary>
    public string DeclaredBitrateDisplay =>
        HasDeclaredBitrate
            ? $"{DeclaredBitrateKbps:0} kbps"
            : "Sin información";

    /// <summary>
    /// Bitrate medio estimado en formato legible.
    /// </summary>
    public string EstimatedAverageBitrateDisplay =>
        HasEstimatedAverageBitrate
            ? $"{EstimatedAverageBitrateKbps:0} kbps"
            : "Sin información";

    /// <summary>
    /// Descripción compacta para diagnóstico.
    /// </summary>
    public string Summary
    {
        get
        {
            List<string> details =
                new();

            if (!string.IsNullOrWhiteSpace(
                    FileExtension))
            {
                details.Add(
                    FileExtension.ToLowerInvariant());
            }

            if (!string.IsNullOrWhiteSpace(
                    CodecName))
            {
                details.Add(
                    CodecName);
            }

            if (HasDeclaredBitrate)
            {
                details.Add(
                    DeclaredBitrateDisplay);
            }
            else if (HasEstimatedAverageBitrate)
            {
                details.Add(
                    $"{EstimatedAverageBitrateDisplay} estimado");
            }

            if (HasDeclaredSampleRate)
            {
                details.Add(
                    $"{DeclaredSampleRate} Hz");
            }

            if (HasDeclaredChannels)
            {
                details.Add(
                    $"{DeclaredChannels} canal(es)");
            }

            return details.Count > 0
                ? string.Join(
                    " · ",
                    details)
                : "Información técnica no disponible";
        }
    }
}