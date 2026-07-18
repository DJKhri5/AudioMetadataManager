namespace AudioMetadataManager.UI.Services.AudioAnalysis.Interfaces;

/// <summary>
/// Contrato común para los analizadores especializados
/// del subsistema AudioAnalysis.
///
/// Cada implementación analiza un archivo y devuelve
/// su propio tipo de resultado.
/// </summary>
/// <typeparam name="TResult">
/// Tipo de resultado producido por el analizador.
/// </typeparam>
public interface IAudioAnalyzer<TResult>
{
    /// <summary>
    /// Nombre legible del analizador.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Analiza un archivo de audio sin modificarlo.
    /// </summary>
    /// <param name="filePath">
    /// Ruta completa del archivo que será analizado.
    /// </param>
    /// <param name="cancellationToken">
    /// Permite cancelar el análisis.
    /// </param>
    Task<TResult> AnalyzeAsync(
        string filePath,
        CancellationToken cancellationToken = default);
}