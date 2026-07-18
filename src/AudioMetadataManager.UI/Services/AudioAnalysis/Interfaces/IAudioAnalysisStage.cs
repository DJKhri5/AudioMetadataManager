using AudioMetadataManager.UI.Services.AudioAnalysis.Models;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Interfaces;

/// <summary>
/// Representa una etapa ejecutable dentro del pipeline
/// de AudioAnalysisEngine.
///
/// Cada etapa analiza un aspecto concreto del archivo y agrega
/// sus resultados al contexto compartido del análisis.
/// </summary>
public interface IAudioAnalysisStage
{
    /// <summary>
    /// Nombre legible de la etapa.
    ///
    /// Se utiliza en registros, informes y mensajes de error.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Orden de ejecución dentro del pipeline.
    ///
    /// Los valores menores se ejecutan primero.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Ejecuta la etapa sin modificar el archivo de audio.
    /// </summary>
    /// <param name="context">
    /// Contexto compartido que contiene la ruta, el resultado
    /// general y los datos producidos por otras etapas.
    /// </param>
    /// <param name="cancellationToken">
    /// Permite cancelar el análisis.
    /// </param>
    Task ExecuteAsync(
        AudioAnalysisContext context,
        CancellationToken cancellationToken = default);
}