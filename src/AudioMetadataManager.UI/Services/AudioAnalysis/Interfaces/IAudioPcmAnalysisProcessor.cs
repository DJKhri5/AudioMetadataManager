using AudioMetadataManager.UI.Services.AudioAnalysis.Models;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Interfaces;

/// <summary>
/// Representa un módulo que analiza bloques PCM ya
/// decodificados por el coordinador del pipeline.
///
/// Los procesadores no abren el archivo de audio y no
/// realizan lecturas independientes desde el disco.
/// Todos reciben los mismos bloques PCM durante una única
/// pasada por el archivo.
/// </summary>
public interface IAudioPcmAnalysisProcessor
{
    /// <summary>
    /// Nombre legible del procesador.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Orden en que el procesador será preparado,
    /// ejecutado y finalizado.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Prepara el procesador antes de recibir el primer
    /// bloque PCM.
    /// </summary>
    /// <param name="context">
    /// Contexto compartido del análisis.
    /// </param>
    /// <param name="streamInfo">
    /// Información técnica del flujo PCM.
    /// </param>
    void Initialize(
        AudioAnalysisContext context,
        AudioStreamInfo streamInfo);

    /// <summary>
    /// Procesa uno de los bloques PCM pertenecientes
    /// a la lectura compartida.
    /// </summary>
    /// <param name="context">
    /// Contexto compartido del análisis.
    /// </param>
    /// <param name="block">
    /// Bloque PCM decodificado que también será entregado
    /// a los demás procesadores registrados.
    /// </param>
    void ProcessBlock(
        AudioAnalysisContext context,
        AudioSampleBlock block);

    /// <summary>
    /// Finaliza los cálculos después de recibir el último
    /// bloque PCM y guarda el resultado especializado
    /// dentro del contexto compartido.
    /// </summary>
    void Complete(
        AudioAnalysisContext context);

    /// <summary>
    /// Registra un fallo controlado producido por este
    /// procesador sin interrumpir necesariamente a los demás.
    /// </summary>
    void Fail(
        AudioAnalysisContext context,
        string? errorMessage);
}