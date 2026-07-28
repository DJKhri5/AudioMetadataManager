using System.IO;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Context;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Base;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.PipelineStages;

/// <summary>
/// Ejecuta pruebas estructurales sobre la infraestructura común
/// de etapas del pipeline.
///
/// No accede al sistema de archivos y no ejecuta escritores.
/// </summary>
public sealed class MetadataApplicationStageBaseTestRunner
{
    public async Task<MetadataApplicationStageBaseTestResult>
        RunAsync()
    {
        List<string> messages =
            new();

        bool successfulStageWasRegistered =
            false;

        bool failedStageWasRegistered =
            false;

        bool duplicateExecutionWasRejected =
            false;

        bool cancelledStageWasRegistered =
            false;

        bool executionMetadataWasPreserved =
            false;

        MetadataApplyRequest successRequest =
            CreateRequest(
                "stage-success.mp3");

        MetadataApplicationContext successContext =
            new(
                successRequest);

        SuccessfulTestStage successStage =
            new();

        await successStage.ExecuteAsync(
            successContext);

        MetadataApplicationStageResult?
            successfulResult =
                successContext.StageResults
                    .SingleOrDefault();

        successfulStageWasRegistered =
            successfulResult is not null &&
            successfulResult.Stage ==
                MetadataApplicationStage.Validation &&
            successfulResult.Status ==
                MetadataApplicationStageStatus.Completed &&
            successfulResult.Message ==
                "Etapa ficticia completada correctamente.";

        executionMetadataWasPreserved =
            successStage.Name ==
                "Successful test stage" &&
            successStage.ExecutionOrder ==
                100 &&
            successfulResult is not null &&
            successfulResult.StartedAtUtc != default &&
            successfulResult.CompletedAtUtc != default &&
            successfulResult.ElapsedTime >= TimeSpan.Zero;

        messages.Add(
            successfulStageWasRegistered
                ? "La etapa correcta fue registrada."
                : "La etapa correcta no fue registrada como se " +
                  "esperaba.");

        messages.Add(
            executionMetadataWasPreserved
                ? "Los metadatos de ejecución fueron preservados."
                : "Los metadatos de ejecución no coinciden.");

        try
        {
            await successStage.ExecuteAsync(
                successContext);

            messages.Add(
                "La ejecución duplicada fue permitida.");
        }
        catch (InvalidOperationException)
        {
            duplicateExecutionWasRejected =
                true;

            messages.Add(
                "La ejecución duplicada fue rechazada.");
        }

        MetadataApplicationContext failureContext =
            new(
                CreateRequest(
                    "stage-failure.mp3"));

        FailingTestStage failingStage =
            new();

        await failingStage.ExecuteAsync(
            failureContext);

        MetadataApplicationStageResult?
            failedResult =
                failureContext.StageResults
                    .SingleOrDefault();

        failedStageWasRegistered =
            failedResult is not null &&
            failedResult.Stage ==
                MetadataApplicationStage.MetadataWrite &&
            failedResult.Status ==
                MetadataApplicationStageStatus.Failed &&
            failedResult.Details.Any(
                detail =>
                    detail.Contains(
                        "Error controlado de prueba",
                        StringComparison.Ordinal));

        messages.Add(
            failedStageWasRegistered
                ? "La excepción fue transformada en un resultado " +
                  "fallido."
                : "La excepción no fue registrada correctamente.");

        using CancellationTokenSource
            cancellationTokenSource =
                new();

        cancellationTokenSource.Cancel();

        MetadataApplicationContext cancellationContext =
            new(
                CreateRequest(
                    "stage-cancelled.mp3"),
                cancellationTokenSource.Token);

        CancelledTestStage cancelledStage =
            new();

        try
        {
            await cancelledStage.ExecuteAsync(
                cancellationContext);
        }
        catch (OperationCanceledException)
        {
            // Resultado esperado.
        }

        MetadataApplicationStageResult?
            cancelledResult =
                cancellationContext.StageResults
                    .SingleOrDefault();

        cancelledStageWasRegistered =
            cancelledResult is not null &&
            cancelledResult.Stage ==
                MetadataApplicationStage.Backup &&
            cancelledResult.Status ==
                MetadataApplicationStageStatus.Cancelled;

        messages.Add(
            cancelledStageWasRegistered
                ? "La cancelación fue registrada correctamente."
                : "La cancelación no produjo el resultado " +
                  "esperado.");

        return new MetadataApplicationStageBaseTestResult
        {
            SuccessfulStageWasRegistered =
                successfulStageWasRegistered,

            FailedStageWasRegistered =
                failedStageWasRegistered,

            DuplicateExecutionWasRejected =
                duplicateExecutionWasRejected,

            CancelledStageWasRegistered =
                cancelledStageWasRegistered,

            ExecutionMetadataWasPreserved =
                executionMetadataWasPreserved,

            Messages =
                messages.ToArray()
        };
    }

    private static MetadataApplyRequest CreateRequest(
        string fileName)
    {
        return new MetadataApplyRequest
        {
            PlanId =
                Guid.NewGuid(),

            FilePath =
                Path.Combine(
                    @"C:\Tests",
                    fileName),

            FileName =
                fileName,

            Changes =
                Array.Empty<MetadataFieldChange>()
        };
    }

    private sealed class SuccessfulTestStage :
        MetadataApplicationStageBase
    {
        public override MetadataApplicationStage Stage =>
            MetadataApplicationStage.Validation;

        public override string Name =>
            "Successful test stage";

        public override int ExecutionOrder =>
            100;

        protected override Task<MetadataApplicationStageExecution>
            ExecuteCoreAsync(
                MetadataApplicationContext context)
        {
            return Task.FromResult(
                Completed(
                    "Etapa ficticia completada correctamente.",
                    new[]
                    {
                        "Detalle estructural de prueba."
                    }));
        }
    }

    private sealed class FailingTestStage :
        MetadataApplicationStageBase
    {
        public override MetadataApplicationStage Stage =>
            MetadataApplicationStage.MetadataWrite;

        public override string Name =>
            "Failing test stage";

        public override int ExecutionOrder =>
            200;

        protected override Task<MetadataApplicationStageExecution>
            ExecuteCoreAsync(
                MetadataApplicationContext context)
        {
            throw new InvalidOperationException(
                "Error controlado de prueba.");
        }
    }

    private sealed class CancelledTestStage :
        MetadataApplicationStageBase
    {
        public override MetadataApplicationStage Stage =>
            MetadataApplicationStage.Backup;

        public override string Name =>
            "Cancelled test stage";

        public override int ExecutionOrder =>
            150;

        protected override Task<MetadataApplicationStageExecution>
            ExecuteCoreAsync(
                MetadataApplicationContext context)
        {
            context.ThrowIfCancellationRequested();

            return Task.FromResult(
                Completed(
                    "Esta etapa no debería completarse."));
        }
    }
}