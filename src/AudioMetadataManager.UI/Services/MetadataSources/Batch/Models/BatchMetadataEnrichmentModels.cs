using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.Simulation.Planning.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources.Batch.Models;

/// <summary>
/// Progreso emitido durante la ejecución del enriquecimiento por lote.
/// </summary>
public sealed class BatchMetadataEnrichmentProgress
{
    public int CurrentIndex { get; init; }
    public int TotalCount { get; init; }
    public string CurrentFileName { get; init; } = string.Empty;
    public string StatusMessage { get; init; } = string.Empty;
    public double Percentage => TotalCount > 0 ? (double)CurrentIndex / TotalCount * 100.0 : 0.0;
}

/// <summary>
/// Resultado individual del análisis y enriquecimiento de un archivo.
/// </summary>
public sealed class BatchMetadataEnrichmentItemResult
{
    public AudioFile AudioFile { get; init; } = null!;
    public bool WasSuccessful { get; init; }
    public bool HasEnrichedProposals { get; init; }
    public int CandidatesFound { get; init; }
    public MetadataChangePlan? ChangePlan { get; init; }
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Resumen global del enriquecimiento por lote.
/// </summary>
public sealed class BatchMetadataEnrichmentResult
{
    public int TotalRequested { get; init; }
    public int TotalProcessed { get; init; }
    public int EnrichedCount { get; init; }
    public int UnchangedCount { get; init; }
    public int FailedCount { get; init; }
    public bool WasCancelled { get; init; }
    public TimeSpan ElapsedTime { get; init; }
    public IReadOnlyList<BatchMetadataEnrichmentItemResult> ItemResults { get; init; } = Array.Empty<BatchMetadataEnrichmentItemResult>();

    public string Summary => WasCancelled
        ? $"Enriquecimiento cancelado por el usuario. Procesados {TotalProcessed} de {TotalRequested} archivos en {ElapsedTime.TotalSeconds:F1}s."
        : $"Enriquecimiento completado: {EnrichedCount} enriquecido(s), {UnchangedCount} sin cambios, {FailedCount} errores ({TotalProcessed} archivos en {ElapsedTime.TotalSeconds:F1}s).";
}
