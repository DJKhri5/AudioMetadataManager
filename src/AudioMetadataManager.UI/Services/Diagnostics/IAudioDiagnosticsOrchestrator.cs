using AudioMetadataManager.UI.Views.Models.Simulation;

namespace AudioMetadataManager.UI.Services.Diagnostics;

/// <summary>
/// Contrato para la orquestación del diagnóstico técnico completo
/// de un archivo de audio y de las pruebas estructurales asociadas.
/// </summary>
public interface IAudioDiagnosticsOrchestrator
{
    /// <summary>
    /// Ejecuta la secuencia completa de diagnóstico y pruebas
    /// estructurales sobre el archivo indicado, reportando el
    /// progreso a través de log.
    /// </summary>
    Task RunFullDiagnosticAsync(
        string filePath,
        ProductiveBatchSelection currentSelection,
        Action<string> log,
        CancellationToken cancellationToken = default);
}
