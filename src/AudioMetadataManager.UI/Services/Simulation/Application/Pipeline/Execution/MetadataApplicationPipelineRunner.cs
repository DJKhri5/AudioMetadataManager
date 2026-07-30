using AudioMetadataManager.UI.Services.Simulation
    .Application.Context;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Composition;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Execution;

/// <summary>
/// Punto de entrada de un solo uso para ejecutar el pipeline
/// modular de aplicación de metadatos sobre una única solicitud.
///
/// Crea el contexto, ejecuta las etapas registradas, finaliza el
/// contexto sin importar el desenlace (éxito, fallo bloqueante o
/// cancelación) y devuelve el resultado consolidado. Sin esta
/// clase, cada consumidor tendría que reproducir la misma lógica
/// de cierre del contexto que MetadataApplicationContext exige
/// antes de construir su resultado.
/// </summary>
public sealed class MetadataApplicationPipelineRunner
{
    private readonly MetadataApplicationPipelineExecutor
        _executor;

    /// <summary>
    /// Crea el runner utilizando la composición predeterminada
    /// del pipeline.
    /// </summary>
    public MetadataApplicationPipelineRunner()
        : this(
            MetadataApplicationPipelineFactory.CreateDefault())
    {
    }

    /// <summary>
    /// Crea el runner con un ejecutor proporcionado.
    /// </summary>
    public MetadataApplicationPipelineRunner(
        MetadataApplicationPipelineExecutor executor)
    {
        _executor =
            executor ??
            throw new ArgumentNullException(
                nameof(executor));
    }

    /// <summary>
    /// Ejecuta el pipeline completo sobre la solicitud indicada.
    /// </summary>
    public async Task<MetadataApplicationPipelineResult> RunAsync(
        MetadataApplyRequest request,
        IProgress<MetadataApplicationProgress>? progress = null,
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
            await _executor.ExecuteAsync(
                context,
                progress);
        }
        catch (OperationCanceledException)
        {
            FinalizeIfNeeded(
                context,
                MetadataApplicationStopReason.Cancelled,
                "La ejecución fue cancelada.");

            return context.BuildResult();
        }

        FinalizeIfNeeded(
            context,
            DetermineStopReason(
                context),
            "El pipeline se detuvo antes de completar todas las " +
            "etapas obligatorias.");

        return context.BuildResult();
    }

    private static void FinalizeIfNeeded(
        MetadataApplicationContext context,
        MetadataApplicationStopReason stopReason,
        string message)
    {
        if (context.IsCompleted)
        {
            return;
        }

        context.Stop(
            stopReason,
            message);
    }

    private static MetadataApplicationStopReason DetermineStopReason(
        MetadataApplicationContext context)
    {
        if (context.StageResults.Any(
                result =>
                    result.Status ==
                    MetadataApplicationStageStatus.Cancelled))
        {
            return MetadataApplicationStopReason.Cancelled;
        }

        MetadataApplicationStageResult? blockingFailure =
            context.StageResults
                .LastOrDefault(
                    result =>
                        result.IsBlockingFailure);

        if (blockingFailure is null)
        {
            return MetadataApplicationStopReason.UnexpectedError;
        }

        return blockingFailure.Stage switch
        {
            MetadataApplicationStage.Validation =>
                MetadataApplicationStopReason.ValidationFailed,

            MetadataApplicationStage.Backup =>
                MetadataApplicationStopReason.BackupFailed,

            MetadataApplicationStage.MetadataWrite =>
                MetadataApplicationStopReason.MetadataWriteFailed,

            MetadataApplicationStage.PostWriteVerification =>
                MetadataApplicationStopReason.VerificationFailed,

            _ =>
                MetadataApplicationStopReason.UnexpectedError
        };
    }
}
