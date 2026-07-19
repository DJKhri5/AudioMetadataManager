namespace AudioMetadataManager.UI.Services.AudioAnalysis.Models;

/// <summary>
/// Identifica las regiones frecuenciales utilizadas para
/// construir el perfil tonal de un archivo de audio.
///
/// Estos valores describen zonas del espectro y no determinan
/// por sí mismos la calidad del archivo.
/// </summary>
public enum AudioFrequencyBand
{
    /// <summary>
    /// Región inferior del espectro.
    /// Aproximadamente desde 20 Hz hasta 60 Hz.
    /// </summary>
    SubBass = 0,

    /// <summary>
    /// Región principal de graves.
    /// Aproximadamente desde 60 Hz hasta 250 Hz.
    /// </summary>
    Bass = 1,

    /// <summary>
    /// Región de medios bajos.
    /// Aproximadamente desde 250 Hz hasta 500 Hz.
    /// </summary>
    LowMidrange = 2,

    /// <summary>
    /// Región central del espectro audible.
    /// Aproximadamente desde 500 Hz hasta 2 kHz.
    /// </summary>
    Midrange = 3,

    /// <summary>
    /// Región de medios altos.
    /// Aproximadamente desde 2 kHz hasta 6 kHz.
    /// </summary>
    UpperMidrange = 4,

    /// <summary>
    /// Región de agudos.
    /// Aproximadamente desde 6 kHz hasta 12 kHz.
    /// </summary>
    Treble = 5,

    /// <summary>
    /// Región superior o de aire.
    /// Aproximadamente desde 12 kHz hasta la frecuencia
    /// de Nyquist disponible.
    /// </summary>
    Air = 6
}