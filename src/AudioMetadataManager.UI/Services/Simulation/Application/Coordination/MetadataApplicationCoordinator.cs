using AudioMetadataManager.UI.Services.Simulation
    .Application.Context;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Composition;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Execution;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Coordination;

/// <summary>
/// Coordina productivamente la ejecución completa del pipeline
/// modular de aplicación de metadatos.
/// </summary>
public sealed class MetadataApplicationCoordinator :
    IMetadataApplicationCoordinator
{
    private readonly
        Func<MetadataApplicationPipelineExecutor>
        _executorFactory;

    /// <summary>
    /// Crea el coordinador con la composición predeterminada
    /// del pipeline.
    /// </summary>
    public MetadataApplicationCoordinator()
        : this(
            MetadataApplicationPipelineFactory.CreateDefault)
    {
    }

    /// <summary>
    /// Crea el coordinador con una fábrica de ejecutores
    /// proporcionada.
    /// </summary>
    public MetadataApplicationCoordinator(
        Func<MetadataApplicationPipelineExecutor>
            executorFactory)
    {
        _executorFactory =
            executorFactory ??
            throw new ArgumentNullException(
                nameof(executorFactory));
    }

    /// <inheritdoc />
    public async Task<MetadataApplicationPipelineResult>
        ExecuteAsync(
            MetadataApplyRequest request,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        MetadataApplicationContext context =
            new(
                request,
                cancellationToken);

        try
        {
            cancellationToken
                .ThrowIfCancellationRequested();

            MetadataApplicationPipelineExecutor executor =
                _executorFactory() ??
                throw new InvalidOperationException(
                    "La fábrica no produjo un ejecutor del " +
                    "pipeline.");

            MetadataApplicationPipelineExecutionResult
                executionResult =
                    await executor.ExecuteAsync(
                        context);

            FinalizeContext(
                context,
                executionResult);
        }
        catch (OperationCanceledException)
        {
            if (!context.IsCompleted)
            {
                context.Stop(
                    MetadataApplicationStopReason.Cancelled,
                    "La ejecución productiva fue cancelada.");
            }
        }
        catch (Exception exception)
        {
            if (!context.IsCompleted)
            {
                context.Stop(
                    MetadataApplicationStopReason
                        .UnexpectedError,
                    exception.Message);
            }
        }

        return context.BuildResult();
    }

    private static void FinalizeContext(
        MetadataApplicationContext context,
        MetadataApplicationPipelineExecutionResult
            executionResult)
    {
        if (context.IsCompleted)
        {
            return;
        }

        if (executionResult.ExecutionWasSuccessful &&
            context.ApplyResult is not null)
        {
            context.Complete();
            return;
        }

        MetadataApplicationStopReason stopReason =
            DetermineStopReason(
                executionResult);

        string stopMessage =
            string.IsNullOrWhiteSpace(
                executionResult.StopMessage)
                ? "El pipeline no pudo completar la solicitud."
                : executionResult.StopMessage;

        context.Stop(
            stopReason,
            stopMessage);
    }

    private static MetadataApplicationStopReason
        DetermineStopReason(
            MetadataApplicationPipelineExecutionResult
                executionResult)
    {
        if (executionResult.WasCancelled)
        {
            return MetadataApplicationStopReason.Cancelled;
        }

        return executionResult.StoppedAtStage switch
        {
            MetadataApplicationStage.Validation =>
                MetadataApplicationStopReason
                    .ValidationFailed,

            MetadataApplicationStage.Backup =>
                MetadataApplicationStopReason
                    .BackupFailed,

            MetadataApplicationStage.MetadataWrite =>
                MetadataApplicationStopReason
                    .MetadataWriteFailed,

            MetadataApplicationStage
                .PostWriteVerification =>
                    MetadataApplicationStopReason
                        .VerificationFailed,

            _ =>
                MetadataApplicationStopReason
                    .UnexpectedError
        };
    }
}