using AudioMetadataManager.UI.Services.Simulation
    .Application.Context;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Contracts;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Execution;

/// <summary>
/// Ejecuta una colección ordenada de etapas sobre un único
/// MetadataApplicationContext.
///
/// El ejecutor no conoce la implementación interna de las
/// etapas. Únicamente controla su orden y las condiciones de
/// detención.
/// </summary>
public sealed class MetadataApplicationPipelineExecutor
{
    private readonly IReadOnlyList<IMetadataApplicationStage>
        _stages;

    private readonly MetadataApplicationPipelineOptions
        _options;

    /// <summary>
    /// Crea el ejecutor utilizando la configuración
    /// predeterminada.
    /// </summary>
    public MetadataApplicationPipelineExecutor(
        IEnumerable<IMetadataApplicationStage> stages)
        : this(
            stages,
            MetadataApplicationPipelineOptions.Default)
    {
    }

    /// <summary>
    /// Crea el ejecutor con las etapas y opciones indicadas.
    /// </summary>
    public MetadataApplicationPipelineExecutor(
        IEnumerable<IMetadataApplicationStage> stages,
        MetadataApplicationPipelineOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            stages);

        _options =
            options ??
            throw new ArgumentNullException(
                nameof(options));

        IMetadataApplicationStage[] stageArray =
            stages.ToArray();

        ValidateStages(
            stageArray,
            _options);

        _stages =
            stageArray
                .OrderBy(
                    stage =>
                        stage.ExecutionOrder)
                .ThenBy(
                    stage =>
                        stage.Stage)
                .ToArray();
    }

    /// <summary>
    /// Etapas ordenadas que serán ejecutadas.
    /// </summary>
    public IReadOnlyList<IMetadataApplicationStage>
        Stages =>
            _stages;

    /// <summary>
    /// Configuración utilizada por el ejecutor.
    /// </summary>
    public MetadataApplicationPipelineOptions Options =>
        _options;

    /// <summary>
    /// Ejecuta secuencialmente las etapas registradas.
    /// </summary>
    /// <param name="context">
    /// Contexto compartido de la ejecución.
    /// </param>
    /// <param name="progress">
    /// Receptor opcional de actualizaciones de progreso, reportadas
    /// después de que cada etapa registra su resultado.
    /// </param>
    public async Task<MetadataApplicationPipelineExecutionResult>
        ExecuteAsync(
            MetadataApplicationContext context,
            IProgress<MetadataApplicationProgress>? progress = null)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        if (context.IsCompleted)
        {
            throw new InvalidOperationException(
                "No es posible ejecutar etapas sobre un contexto " +
                "que ya fue finalizado.");
        }

        int executedStageCount =
            0;

        MetadataApplicationStage stoppedAtStage =
            MetadataApplicationStage.None;

        string stopMessage =
            string.Empty;

        foreach (IMetadataApplicationStage stage in _stages)
        {
            context.ThrowIfCancellationRequested();

            await stage.ExecuteAsync(
                context);

            executedStageCount++;

            MetadataApplicationStageResult?
                stageResult =
                    context.StageResults
                        .SingleOrDefault(
                            result =>
                                result.Stage ==
                                stage.Stage);

            if (stageResult is null)
            {
                stoppedAtStage =
                    stage.Stage;

                stopMessage =
                    $"La etapa {stage.Name} no registró un " +
                    "resultado auditable.";

                break;
            }

            progress?.Report(
                new MetadataApplicationProgress
                {
                    Stage =
                        stage.Stage,

                    Percentage =
                        executedStageCount *
                        100.0 /
                        _stages.Count,

                    Message =
                        stageResult.Message,

                    FileName =
                        context.Request.FileName
                });

            if (_options.StopOnCancellation &&
                stageResult.Status ==
                    MetadataApplicationStageStatus.Cancelled)
            {
                stoppedAtStage =
                    stage.Stage;

                stopMessage =
                    $"La etapa {stage.Name} fue cancelada.";

                break;
            }

            if (_options.StopOnBlockingFailure &&
                stageResult.IsBlockingFailure)
            {
                stoppedAtStage =
                    stage.Stage;

                stopMessage =
                    $"La etapa {stage.Name} terminó con un fallo " +
                    "bloqueante.";

                break;
            }

            if (_options.StopOnSkippedStage &&
                stageResult.Status ==
                    MetadataApplicationStageStatus.Skipped)
            {
                stoppedAtStage =
                    stage.Stage;

                stopMessage =
                    $"La etapa {stage.Name} fue omitida.";

                break;
            }
        }

        bool allStagesWereExecuted =
            executedStageCount ==
            _stages.Count;

        bool hasBlockingFailure =
            context.StageResults.Any(
                result =>
                    result.IsBlockingFailure);

        bool wasCancelled =
            context.StageResults.Any(
                result =>
                    result.Status ==
                    MetadataApplicationStageStatus.Cancelled);

        if (_options.CompleteContextAutomatically &&
            allStagesWereExecuted &&
            !hasBlockingFailure &&
            !wasCancelled &&
            !context.IsCompleted)
        {
            context.Complete();
        }

        return new MetadataApplicationPipelineExecutionResult
        {
            Context =
                context,

            RegisteredStageCount =
                _stages.Count,

            ExecutedStageCount =
                executedStageCount,

            StoppedAtStage =
                stoppedAtStage,

            StopMessage =
                stopMessage
        };
    }

    private static void ValidateStages(
        IReadOnlyList<IMetadataApplicationStage> stages,
        MetadataApplicationPipelineOptions options)
    {
        if (stages.Count == 0)
        {
            throw new ArgumentException(
                "El ejecutor requiere al menos una etapa.",
                nameof(stages));
        }

        if (stages.Any(
                stage =>
                    stage is null))
        {
            throw new ArgumentException(
                "La colección contiene una etapa nula.",
                nameof(stages));
        }

        IGrouping<MetadataApplicationStage,
            IMetadataApplicationStage>?
            duplicateIdentity =
                stages
                    .GroupBy(
                        stage =>
                            stage.Stage)
                    .FirstOrDefault(
                        group =>
                            group.Count() > 1);

        if (duplicateIdentity is not null)
        {
            throw new ArgumentException(
                $"La identidad de etapa " +
                $"{duplicateIdentity.Key} está registrada más " +
                "de una vez.",
                nameof(stages));
        }

        if (stages.Any(
                stage =>
                    stage.Stage ==
                    MetadataApplicationStage.None))
        {
            throw new ArgumentException(
                "Todas las etapas deben tener una identidad " +
                "funcional válida.",
                nameof(stages));
        }

        if (stages.Any(
                stage =>
                    string.IsNullOrWhiteSpace(
                        stage.Name)))
        {
            throw new ArgumentException(
                "Todas las etapas deben tener un nombre legible.",
                nameof(stages));
        }

        if (!options.RejectDuplicateExecutionOrder)
        {
            return;
        }

        IGrouping<int, IMetadataApplicationStage>?
            duplicateOrder =
                stages
                    .GroupBy(
                        stage =>
                            stage.ExecutionOrder)
                    .FirstOrDefault(
                        group =>
                            group.Count() > 1);

        if (duplicateOrder is not null)
        {
            throw new ArgumentException(
                $"El orden de ejecución " +
                $"{duplicateOrder.Key} está asignado a más de " +
                "una etapa.",
                nameof(stages));
        }
    }
}