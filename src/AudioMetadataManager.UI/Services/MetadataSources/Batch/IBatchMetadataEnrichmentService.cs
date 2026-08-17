using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.MetadataSources.Batch.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources.Batch;

/// <summary>
/// Contrato para el servicio de análisis y enriquecimiento de metadatos online por lote.
/// </summary>
public interface IBatchMetadataEnrichmentService
{
    Task<BatchMetadataEnrichmentResult> EnrichBatchAsync(
        IReadOnlyList<AudioFile> files,
        IProgress<BatchMetadataEnrichmentProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
