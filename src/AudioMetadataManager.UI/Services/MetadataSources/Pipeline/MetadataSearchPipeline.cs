using System.Diagnostics;
using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Pipeline.Execution;
using AudioMetadataManager.UI.Services.MetadataSources
    .Pipeline.Strategy;

namespace AudioMetadataManager.UI.Services.MetadataSources.Pipeline;

/// <summary>
/// Coordina la ejecución escalonada de búsquedas externas.
///
/// El pipeline genera variantes mediante una estrategia,
/// ejecuta las fuentes registradas y decide si debe continuar
/// o detenerse.
/// </summary>
public sealed class MetadataSearchPipeline
{
    private readonly MetadataSourceManager
        _sourceManager;

    private readonly IMetadataSearchStrategy
        _searchStrategy;

    /// <summary>
    /// Crea el pipeline con la estrategia predeterminada.
    /// </summary>
    public MetadataSearchPipeline(
        MetadataSourceManager sourceManager)
        : this(
            sourceManager,
            new DefaultMetadataSearchStrategy())
    {
    }

    /// <summary>
    /// Crea el pipeline con una estrategia personalizada.
    /// </summary>
    public MetadataSearchPipeline(
        MetadataSourceManager sourceManager,
        IMetadataSearchStrategy searchStrategy)
    {
        _sourceManager =
            sourceManager ??
            throw new ArgumentNullException(
                nameof(sourceManager));

        _searchStrategy =
            searchStrategy ??
            throw new ArgumentNullException(
                nameof(searchStrategy));
    }

    /// <summary>
    /// Ejecuta el pipeline a partir de una solicitud común.
    /// </summary>
    public Task<MetadataSearchPipelineResult> ExecuteAsync(
        MetadataSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        MetadataSearchContext context =
            new()
            {
                Request =
                    request
            };

        return ExecuteAsync(
            context,
            cancellationToken);
    }

    /// <summary>
    /// Ejecuta el pipeline utilizando un contexto existente.
    /// </summary>
    public async Task<MetadataSearchPipelineResult> ExecuteAsync(
        MetadataSearchContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        cancellationToken.ThrowIfCancellationRequested();

        DateTimeOffset startedAtUtc =
            DateTimeOffset.UtcNow;

        Stopwatch pipelineStopwatch =
            Stopwatch.StartNew();

        if (!context.HasSearchableIdentity)
        {
            pipelineStopwatch.Stop();

            return CreatePipelineFailure(
                context,
                startedAtUtc,
                pipelineStopwatch.Elapsed,
                MetadataSearchStopReason.NoValidQueries,
                "La solicitud no contiene una identidad " +
                "musical suficiente para iniciar el pipeline.");
        }

        try
        {
            IReadOnlyList<MetadataSearchQuery> queries =
                _searchStrategy.BuildQueries(
                    context.Request);

            if (queries.Count == 0)
            {
                pipelineStopwatch.Stop();

                return CreatePipelineFailure(
                    context,
                    startedAtUtc,
                    pipelineStopwatch.Elapsed,
                    MetadataSearchStopReason.NoValidQueries,
                    "La estrategia no produjo consultas válidas.");
            }

            List<MetadataSearchAttempt> attempts =
                new();

            MetadataSearchStopReason stopReason =
                MetadataSearchStopReason.QueriesExhausted;

            int attemptNumber =
                0;

            foreach (MetadataSearchQuery query in queries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                attemptNumber++;

                MetadataSearchAttempt attempt =
                    await ExecuteAttemptAsync(
                        attemptNumber,
                        query,
                        context.Request,
                        cancellationToken);

                attempts.Add(
                    attempt);

                if (attempt.HasCandidates)
                {
                    stopReason =
                        MetadataSearchStopReason.CandidatesFound;

                    break;
                }

                MetadataSearchStopReason? blockingReason =
                    GetBlockingStopReason(
                        attempt.Outcome);

                if (blockingReason.HasValue)
                {
                    stopReason =
                        blockingReason.Value;

                    break;
                }
            }

            pipelineStopwatch.Stop();

            IReadOnlyList<MetadataSearchResult> finalSourceResults =
                attempts
                    .LastOrDefault()
                    ?.SourceResults ??
                Array.Empty<MetadataSearchResult>();

            return new MetadataSearchPipelineResult
            {
                Context =
                    context,

                Attempts =
                    attempts,

                SourceResults =
                    finalSourceResults,

                StopReason =
                    stopReason,

                StartedAtUtc =
                    startedAtUtc,

                CompletedAtUtc =
                    DateTimeOffset.UtcNow,

                ElapsedTime =
                    pipelineStopwatch.Elapsed,

                ExecutionSucceeded =
                    true
            };
        }
        catch (OperationCanceledException)
        {
            pipelineStopwatch.Stop();
            throw;
        }
        catch (Exception exception)
        {
            pipelineStopwatch.Stop();

            return CreatePipelineFailure(
                context,
                startedAtUtc,
                pipelineStopwatch.Elapsed,
                MetadataSearchStopReason.UnexpectedFailure,
                "Ocurrió un error general durante el pipeline " +
                "de búsqueda de metadatos: " +
                exception.Message);
        }
    }

    /// <summary>
    /// Ejecuta una única variante contra todas las fuentes.
    /// </summary>
    private async Task<MetadataSearchAttempt>
        ExecuteAttemptAsync(
            int attemptNumber,
            MetadataSearchQuery query,
            MetadataSearchRequest originalRequest,
            CancellationToken cancellationToken)
    {
        DateTimeOffset startedAtUtc =
            DateTimeOffset.UtcNow;

        Stopwatch stopwatch =
            Stopwatch.StartNew();

        MetadataSearchRequest attemptRequest =
            query.CreateRequestFrom(
                originalRequest);

        IReadOnlyList<MetadataSearchResult> sourceResults =
            await _sourceManager.SearchAllAsync(
                attemptRequest,
                cancellationToken);

        stopwatch.Stop();

        MetadataSearchAttemptOutcome outcome =
            DetermineOutcome(
                sourceResults);

        int candidateCount =
            sourceResults
                .SelectMany(
                    result =>
                        result.Candidates)
                .Count(
                    candidate =>
                        candidate.HasIdentity);

        string message =
            BuildAttemptMessage(
                outcome,
                candidateCount);

        return new MetadataSearchAttempt
        {
            AttemptNumber =
                attemptNumber,

            Query =
                query,

            SourceResults =
                sourceResults,

            StartedAtUtc =
                startedAtUtc,

            CompletedAtUtc =
                DateTimeOffset.UtcNow,

            ElapsedTime =
                stopwatch.Elapsed,

            Outcome =
                outcome,

            Message =
                message
        };
    }

    private static MetadataSearchAttemptOutcome DetermineOutcome(
        IReadOnlyList<MetadataSearchResult> sourceResults)
    {
        if (sourceResults
            .SelectMany(
                result =>
                    result.Candidates)
            .Any(
                candidate =>
                    candidate.HasIdentity))
        {
            return MetadataSearchAttemptOutcome.CandidatesFound;
        }

        if (sourceResults.Any(
                result =>
                    result.Status is
                        MetadataSourceStatus.AuthenticationRequired or
                        MetadataSourceStatus.AuthenticationFailed))
        {
            return MetadataSearchAttemptOutcome.AuthenticationFailure;
        }

        if (sourceResults.Any(
                result =>
                    result.Status ==
                    MetadataSourceStatus.RateLimited))
        {
            return MetadataSearchAttemptOutcome.RateLimited;
        }

        if (sourceResults.Any(
                result =>
                    result.Status ==
                    MetadataSourceStatus.NetworkError))
        {
            return MetadataSearchAttemptOutcome.NetworkFailure;
        }

        if (sourceResults.Any(
                result =>
                    result.Status ==
                    MetadataSourceStatus.InvalidResponse))
        {
            return MetadataSearchAttemptOutcome.InvalidResponse;
        }

        if (sourceResults.Any(
                result =>
                    result.Status ==
                    MetadataSourceStatus.UnexpectedError))
        {
            return MetadataSearchAttemptOutcome.UnexpectedFailure;
        }

        if (sourceResults.Any(
                result =>
                    result.Status ==
                    MetadataSourceStatus.InvalidRequest))
        {
            return MetadataSearchAttemptOutcome.InvalidRequest;
        }

        bool allUnavailable =
            sourceResults.Count > 0 &&
            sourceResults.All(
                result =>
                    result.Status ==
                    MetadataSourceStatus.InvalidConfiguration);

        if (allUnavailable)
        {
            return MetadataSearchAttemptOutcome.SourcesUnavailable;
        }

        return MetadataSearchAttemptOutcome.NoCandidates;
    }

    private static MetadataSearchStopReason?
        GetBlockingStopReason(
            MetadataSearchAttemptOutcome outcome)
    {
        return outcome switch
        {
            MetadataSearchAttemptOutcome.AuthenticationFailure =>
                MetadataSearchStopReason.AuthenticationFailure,

            MetadataSearchAttemptOutcome.RateLimited =>
                MetadataSearchStopReason.RateLimited,

            MetadataSearchAttemptOutcome.NetworkFailure =>
                MetadataSearchStopReason.NetworkFailure,

            MetadataSearchAttemptOutcome.InvalidResponse =>
                MetadataSearchStopReason.InvalidResponse,

            MetadataSearchAttemptOutcome.UnexpectedFailure =>
                MetadataSearchStopReason.UnexpectedFailure,

            _ =>
                null
        };
    }

    private static string BuildAttemptMessage(
        MetadataSearchAttemptOutcome outcome,
        int candidateCount)
    {
        return outcome switch
        {
            MetadataSearchAttemptOutcome.CandidatesFound =>
                $"El intento encontró {candidateCount} " +
                "candidato(s) utilizables.",

            MetadataSearchAttemptOutcome.NoCandidates =>
                "Las fuentes respondieron, pero no encontraron " +
                "candidatos utilizables.",

            MetadataSearchAttemptOutcome.SourcesUnavailable =>
                "Ninguna fuente se encuentra disponible.",

            MetadataSearchAttemptOutcome.InvalidRequest =>
                "La consulta generada no fue aceptada.",

            MetadataSearchAttemptOutcome.AuthenticationFailure =>
                "Una fuente rechazó o requiere credenciales.",

            MetadataSearchAttemptOutcome.RateLimited =>
                "Una fuente alcanzó temporalmente su límite.",

            MetadataSearchAttemptOutcome.NetworkFailure =>
                "Ocurrió un problema de red durante la consulta.",

            MetadataSearchAttemptOutcome.InvalidResponse =>
                "Una fuente devolvió una respuesta inválida.",

            MetadataSearchAttemptOutcome.UnexpectedFailure =>
                "Ocurrió un error inesperado durante el intento.",

            _ =>
                "El intento terminó sin una conclusión definida."
        };
    }

    private static MetadataSearchPipelineResult
        CreatePipelineFailure(
            MetadataSearchContext context,
            DateTimeOffset startedAtUtc,
            TimeSpan elapsedTime,
            MetadataSearchStopReason stopReason,
            string errorMessage)
    {
        return new MetadataSearchPipelineResult
        {
            Context =
                context,

            StopReason =
                stopReason,

            StartedAtUtc =
                startedAtUtc,

            CompletedAtUtc =
                DateTimeOffset.UtcNow,

            ElapsedTime =
                elapsedTime,

            ExecutionSucceeded =
                false,

            ErrorMessage =
                errorMessage
        };
    }
}