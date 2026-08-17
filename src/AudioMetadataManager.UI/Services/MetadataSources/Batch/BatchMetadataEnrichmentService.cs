using System.Diagnostics;
using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services;
using AudioMetadataManager.UI.Services.MetadataSources.Batch.Models;
using AudioMetadataManager.UI.Services.MetadataSources.Consensus.Engine;
using AudioMetadataManager.UI.Services.MetadataSources.Matching;
using AudioMetadataManager.UI.Services.MetadataSources.Matching.Candidates;
using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources.Pipeline;
using AudioMetadataManager.UI.Services.Simulation;
using AudioMetadataManager.UI.Services.Simulation.Planning.Decision;
using AudioMetadataManager.UI.Services.Simulation.Planning.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources.Batch;

/// <summary>
/// Implementación del servicio de enriquecimiento de metadatos online por lote.
/// </summary>
public sealed class BatchMetadataEnrichmentService : IBatchMetadataEnrichmentService
{
    private readonly FileNameParserService _parserService;
    private readonly SimulationPlanToRenamingSynchronizer _synchronizer;

    public BatchMetadataEnrichmentService(
        FileNameParserService? parserService = null,
        SimulationPlanToRenamingSynchronizer? synchronizer = null)
    {
        _parserService = parserService ?? new FileNameParserService();
        _synchronizer = synchronizer ?? new SimulationPlanToRenamingSynchronizer();
    }

    public async Task<BatchMetadataEnrichmentResult> EnrichBatchAsync(
        IReadOnlyList<AudioFile> files,
        IProgress<BatchMetadataEnrichmentProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (files == null || files.Count == 0)
        {
            return new BatchMetadataEnrichmentResult
            {
                TotalRequested = 0,
                TotalProcessed = 0,
                ElapsedTime = TimeSpan.Zero
            };
        }

        var stopwatch = Stopwatch.StartNew();
        var itemResults = new List<BatchMetadataEnrichmentItemResult>(files.Count);
        bool wasCancelled = false;

        var sourceManager = MetadataSourceFactory.CreateDefault();
        var pipeline = new MetadataSearchPipeline(sourceManager);
        var localMetadataFactory = new LocalMetadataComparisonInputFactory();
        var candidateEvaluationEngine = new MetadataCandidateEvaluationEngine();
        var consensusOrchestrator = new MetadataConsensusOrchestrator();
        var changeDecisionEngine = new MetadataChangeDecisionEngine();

        int processedCount = 0;
        int enrichedCount = 0;
        int unchangedCount = 0;
        int failedCount = 0;

        for (int i = 0; i < files.Count; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                wasCancelled = true;
                break;
            }

            var file = files[i];

            progress?.Report(new BatchMetadataEnrichmentProgress
            {
                CurrentIndex = i + 1,
                TotalCount = files.Count,
                CurrentFileName = file.FileName,
                StatusMessage = $"Consultando proveedores online ({i + 1}/{files.Count})..."
            });

            try
            {
                var parsedFileName = file.ParsedName ?? _parserService.Parse(file);
                file.ParsedName = parsedFileName;

                var searchRequest = new MetadataSearchRequest
                {
                    FileName = file.FileName,
                    ParsedArtist = parsedFileName.Artist,
                    ParsedTitle = parsedFileName.Title,
                    ParsedVersion = parsedFileName.Version,
                    TaggedArtist = file.Artist,
                    TaggedTitle = file.Title,
                    TaggedAlbum = file.Album,
                    TaggedYear = file.Year,
                    Duration = file.Duration
                };

                var searchContext = new MetadataSearchContext(searchRequest);
                var pipelineResult = await pipeline.ExecuteAsync(searchContext, cancellationToken).ConfigureAwait(false);

                var localMetadata = localMetadataFactory.Create(file, parsedFileName);
                var candidateBatch = candidateEvaluationEngine.EvaluateBatch(localMetadata, pipelineResult.Candidates);
                var consensusResult = consensusOrchestrator.Evaluate(candidateBatch);
                var changePlan = changeDecisionEngine.BuildPlan(file, consensusResult);

                _synchronizer.Synchronize(file, changePlan);

                bool hasEnriched = file.Simulation?.HasChanges == true;
                if (hasEnriched)
                {
                    enrichedCount++;
                }
                else
                {
                    unchangedCount++;
                }

                itemResults.Add(new BatchMetadataEnrichmentItemResult
                {
                    AudioFile = file,
                    WasSuccessful = true,
                    HasEnrichedProposals = hasEnriched,
                    CandidatesFound = pipelineResult.Candidates.Count,
                    ChangePlan = changePlan,
                    Message = hasEnriched ? "Propuestas canónicas generadas." : "Sin cambios propuestos."
                });
            }
            catch (OperationCanceledException)
            {
                wasCancelled = true;
                break;
            }
            catch (Exception ex)
            {
                failedCount++;
                itemResults.Add(new BatchMetadataEnrichmentItemResult
                {
                    AudioFile = file,
                    WasSuccessful = false,
                    HasEnrichedProposals = false,
                    Message = $"Error al procesar: {ex.Message}"
                });
            }

            processedCount++;
        }

        stopwatch.Stop();

        return new BatchMetadataEnrichmentResult
        {
            TotalRequested = files.Count,
            TotalProcessed = processedCount,
            EnrichedCount = enrichedCount,
            UnchangedCount = unchangedCount,
            FailedCount = failedCount,
            WasCancelled = wasCancelled,
            ElapsedTime = stopwatch.Elapsed,
            ItemResults = itemResults
        };
    }
}
