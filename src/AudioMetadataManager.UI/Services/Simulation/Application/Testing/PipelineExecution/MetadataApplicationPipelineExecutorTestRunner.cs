using System.IO;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Context;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Execution;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Base;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Stages.Contracts;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.PipelineExecution;

/// <summary>
/// Ejecuta pruebas estructurales sobre el ejecutor genérico del
/// pipeline.
///
/// No accede a archivos reales ni ejecuta escritores.
/// </summary>
public sealed class MetadataApplicationPipelineExecutorTestRunner
{
    public async Task<MetadataApplicationPipelineExecutorTestResult>
        RunAsync()
    {
        List<string> messages =
            new();

        bool stagesWereOrderedCorrectly =
            false;

        bool completeExecutionSucceeded =
            false;

        bool blockingFailureStoppedExecution =
            false;

        bool duplicateIdentityWasRejected =
            false;

        bool duplicateOrderWasRejectedWhenConfigured =
            false;

        bool automaticCompletionWorked =
            false;

        bool contextWasPreserved =
            false;

        IMetadataApplicationStage[] unorderedStages =
        {
            new TestStage(
                MetadataApplicationStage.Finalization,
                "Finalization test stage",
                500,
                MetadataApplicationStageStatus.Completed),

            new TestStage(
                MetadataApplicationStage.Validation,
                "Validation test stage",
                100,
                MetadataApplicationStageStatus.Completed),

            new TestStage(
                MetadataApplicationStage.Backup,
                "Backup test stage",
                200,
                MetadataApplicationStageStatus.Completed)
        };

        MetadataApplicationPipelineExecutor orderedExecutor =
            new(
                unorderedStages);

        MetadataApplicationStage[] orderedIdentities =
            orderedExecutor.Stages
                .Select(
                    stage =>
                        stage.Stage)
                .ToArray();

        stagesWereOrderedCorrectly =
            orderedIdentities.SequenceEqual(
                new[]
                {
                    MetadataApplicationStage.Validation,
                    MetadataApplicationStage.Backup,
                    MetadataApplicationStage.Finalization
                });

        messages.Add(
            stagesWereOrderedCorrectly
                ? "Las etapas fueron ordenadas correctamente."
                : "El orden de las etapas no coincide con lo " +
                  "esperado.");

        MetadataApplicationContext completeContext =
            new(
                CreateRequest(
                    "executor-complete.mp3"));

        MetadataApplicationPipelineExecutionResult
            completeExecutionResult =
                await orderedExecutor.ExecuteAsync(
                    completeContext);

        completeExecutionSucceeded =
            completeExecutionResult.ExecutionWasSuccessful &&
            completeExecutionResult.AllStagesWereExecuted &&
            completeExecutionResult.ExecutedStageCount == 3 &&
            completeContext.StageResults.Count == 3;

        contextWasPreserved =
            ReferenceEquals(
                completeExecutionResult.Context,
                completeContext);

        messages.Add(
            completeExecutionSucceeded
                ? "La ejecución completa terminó correctamente."
                : "La ejecución completa no produjo el resultado " +
                  "esperado.");

        messages.Add(
            contextWasPreserved
                ? "El ejecutor conservó la misma instancia del " +
                  "contexto."
                : "El ejecutor reemplazó la instancia del contexto.");

        IMetadataApplicationStage[] failureStages =
        {
            new TestStage(
                MetadataApplicationStage.Validation,
                "Validation before failure",
                100,
                MetadataApplicationStageStatus.Completed),

            new TestStage(
                MetadataApplicationStage.MetadataWrite,
                "Blocking failure stage",
                200,
                MetadataApplicationStageStatus.Failed),

            new TestStage(
                MetadataApplicationStage.Finalization,
                "Stage after failure",
                300,
                MetadataApplicationStageStatus.Completed)
        };

        MetadataApplicationPipelineExecutor failureExecutor =
            new(
                failureStages);

        MetadataApplicationContext failureContext =
            new(
                CreateRequest(
                    "executor-failure.mp3"));

        MetadataApplicationPipelineExecutionResult
            failureExecutionResult =
                await failureExecutor.ExecuteAsync(
                    failureContext);

        blockingFailureStoppedExecution =
            failureExecutionResult.WasStoppedEarly &&
            failureExecutionResult.ExecutedStageCount == 2 &&
            failureExecutionResult.StoppedAtStage ==
                MetadataApplicationStage.MetadataWrite &&
            failureExecutionResult.HasBlockingFailure &&
            !failureContext.HasStage(
                MetadataApplicationStage.Finalization);

        messages.Add(
            blockingFailureStoppedExecution
                ? "El fallo bloqueante detuvo correctamente la " +
                  "ejecución."
                : "El ejecutor no se detuvo correctamente ante " +
                  "el fallo bloqueante.");

        try
        {
            _ =
                new MetadataApplicationPipelineExecutor(
                    new IMetadataApplicationStage[]
                    {
                        new TestStage(
                            MetadataApplicationStage.Validation,
                            "First duplicate identity",
                            100,
                            MetadataApplicationStageStatus.Completed),

                        new TestStage(
                            MetadataApplicationStage.Validation,
                            "Second duplicate identity",
                            200,
                            MetadataApplicationStageStatus.Completed)
                    });

            messages.Add(
                "El ejecutor permitió identidades duplicadas.");
        }
        catch (ArgumentException)
        {
            duplicateIdentityWasRejected =
                true;

            messages.Add(
                "Las identidades duplicadas fueron rechazadas.");
        }

        try
        {
            MetadataApplicationPipelineOptions strictOrderOptions =
                new()
                {
                    RejectDuplicateExecutionOrder =
                        true
                };

            _ =
                new MetadataApplicationPipelineExecutor(
                    new IMetadataApplicationStage[]
                    {
                        new TestStage(
                            MetadataApplicationStage.Validation,
                            "First duplicate order",
                            100,
                            MetadataApplicationStageStatus.Completed),

                        new TestStage(
                            MetadataApplicationStage.Backup,
                            "Second duplicate order",
                            100,
                            MetadataApplicationStageStatus.Completed)
                    },
                    strictOrderOptions);

            messages.Add(
                "El ejecutor permitió órdenes duplicados con la " +
                "opción estricta activada.");
        }
        catch (ArgumentException)
        {
            duplicateOrderWasRejectedWhenConfigured =
                true;

            messages.Add(
                "Los órdenes duplicados fueron rechazados con la " +
                "opción estricta.");
        }

        MetadataApplicationPipelineOptions automaticOptions =
            new()
            {
                CompleteContextAutomatically =
                    true
            };

        MetadataApplicationPipelineExecutor automaticExecutor =
            new(
                new IMetadataApplicationStage[]
                {
                    new TestStage(
                        MetadataApplicationStage.Validation,
                        "Automatic completion stage",
                        100,
                        MetadataApplicationStageStatus.Completed)
                },
                automaticOptions);

        MetadataApplicationContext automaticContext =
            new(
                CreateRequest(
                    "executor-automatic.mp3"));

        await automaticExecutor.ExecuteAsync(
            automaticContext);

        automaticCompletionWorked =
            automaticContext.IsCompleted &&
            automaticContext.StopReason ==
                MetadataApplicationStopReason.Completed;

        messages.Add(
            automaticCompletionWorked
                ? "La finalización automática funcionó " +
                  "correctamente."
                : "La finalización automática no produjo el " +
                  "estado esperado.");

        return new MetadataApplicationPipelineExecutorTestResult
        {
            StagesWereOrderedCorrectly =
                stagesWereOrderedCorrectly,

            CompleteExecutionSucceeded =
                completeExecutionSucceeded,

            BlockingFailureStoppedExecution =
                blockingFailureStoppedExecution,

            DuplicateIdentityWasRejected =
                duplicateIdentityWasRejected,

            DuplicateOrderWasRejectedWhenConfigured =
                duplicateOrderWasRejectedWhenConfigured,

            AutomaticCompletionWorked =
                automaticCompletionWorked,

            ContextWasPreserved =
                contextWasPreserved,

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

    private sealed class TestStage :
        MetadataApplicationStageBase
    {
        private readonly MetadataApplicationStage
            _stage;

        private readonly string
            _name;

        private readonly int
            _executionOrder;

        private readonly MetadataApplicationStageStatus
            _status;

        public TestStage(
            MetadataApplicationStage stage,
            string name,
            int executionOrder,
            MetadataApplicationStageStatus status)
        {
            _stage =
                stage;

            _name =
                name;

            _executionOrder =
                executionOrder;

            _status =
                status;
        }

        public override MetadataApplicationStage Stage =>
            _stage;

        public override string Name =>
            _name;

        public override int ExecutionOrder =>
            _executionOrder;

        protected override Task<MetadataApplicationStageExecution>
            ExecuteCoreAsync(
                MetadataApplicationContext context)
        {
            MetadataApplicationStageExecution execution =
                _status switch
                {
                    MetadataApplicationStageStatus.Completed =>
                        Completed(
                            $"{Name} completed."),

                    MetadataApplicationStageStatus
                        .CompletedWithWarnings =>
                            CompletedWithWarnings(
                                $"{Name} completed with warnings."),

                    MetadataApplicationStageStatus.Failed =>
                        Failed(
                            $"{Name} failed."),

                    MetadataApplicationStageStatus.Skipped =>
                        Skipped(
                            $"{Name} was skipped."),

                    _ =>
                        Failed(
                            $"{Name} received an unsupported " +
                            "test status.")
                };

            return Task.FromResult(
                execution);
        }
    }
}