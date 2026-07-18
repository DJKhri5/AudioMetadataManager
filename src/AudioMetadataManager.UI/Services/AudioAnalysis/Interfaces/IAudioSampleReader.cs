using AudioMetadataManager.UI.Services.AudioAnalysis.Models;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Interfaces;

/// <summary>
/// Define el contrato utilizado para obtener información
/// técnica y muestras PCM desde un archivo de audio.
///
/// Los analizadores dependerán de esta interfaz y no de una
/// biblioteca específica como NAudio, FFmpeg o BASS.
/// </summary>
public interface IAudioSampleReader
{
    /// <summary>
    /// Lee la información técnica del flujo de audio
    /// realmente decodificable.
    ///
    /// Este método no debe cargar todas las muestras
    /// del archivo en memoria.
    /// </summary>
    /// <param name="filePath">
    /// Ruta completa del archivo de audio.
    /// </param>
    /// <param name="cancellationToken">
    /// Permite cancelar la operación.
    /// </param>
    Task<AudioStreamInfo> ReadInfoAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Decodifica el archivo progresivamente y entrega
    /// bloques consecutivos de muestras PCM normalizadas.
    ///
    /// Las muestras deben estar intercaladas por canal.
    /// Para estéreo:
    /// izquierda, derecha, izquierda, derecha...
    /// </summary>
    /// <param name="filePath">
    /// Ruta completa del archivo de audio.
    /// </param>
    /// <param name="framesPerBlock">
    /// Cantidad objetivo de frames por bloque.
    ///
    /// El valor predeterminado de 4096 ofrece un equilibrio
    /// razonable entre memoria y rendimiento.
    /// </param>
    /// <param name="cancellationToken">
    /// Permite detener la lectura cuando el usuario cancela
    /// el análisis o cierra el proyecto.
    /// </param>
    IAsyncEnumerable<AudioSampleBlock> ReadBlocksAsync(
        string filePath,
        int framesPerBlock = 4096,
        CancellationToken cancellationToken = default);
}