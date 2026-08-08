using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Infrastructure;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Ejecuta comprobaciones estructurales controladas sobre
/// MetadataApplyBatchResult.
///
/// No modifica archivos ni ejecuta el pipeline productivo.
/// </summary>
public sealed class MetadataApplyBatchResultTestRunner
{
    public MetadataApplyBatchResultTestResult Run()
    {
        List<string> messages = new();

        DateTime startedAtUtc =
            DateTime.UtcNow;

        DateTime finishedAtUtc =
            startedAtUtc.AddSeconds(
                5);

        Guid batchId =
            Guid.NewGuid();

        MetadataApplyBatchResult emptyResult =
            new()
            {
                BatchId =
                    Guid.NewGuid(),

                StartedAtUtc =
                    startedAtUtc,

                FinishedAtUtc =
                    finishedAtUtc
            };

        bool emptyResultWasRejected =
            emptyResult.TotalCount == 0 &&
            !emptyResult.WasSuccessful;

        messages.Add(
            emptyResultWasRejected
                ? "El resultado vacío fue rechazado correctamente."
                : "El resultado vacío no fue rechazado correctamente.");

        MetadataProductiveApplicationResult
            successfulIndividualResult =
                CreateSuccessfulResult();

        MetadataProductiveApplicationResult
            secondSuccessfulIndividualResult =
                CreateSuccessfulResult();

        MetadataApplyBatchResult
            successfulBatch =
                new()
                {
                    BatchId =
                        batchId,

                    StartedAtUtc =
                        startedAtUtc,

                    FinishedAtUtc =
                        finishedAtUtc,

                    Results =
                        new[]
                        {
                            successfulIndividualResult,
                            secondSuccessfulIndividualResult
                        },

                    Messages =
                        new[]
                        {
                            "Mensaje estructural del lote."
                        }
                };

        bool successfulResultsWereCounted =
            successfulBatch.TotalCount == 2 &&
            successfulBatch.SuccessfulCount == 2;

        messages.Add(
            successfulResultsWereCounted
                ? "Los resultados correctos fueron contabilizados."
                : "Los resultados correctos no fueron contabilizados correctamente.");

        bool successfulBatchWasDetected =
            successfulBatch.WasSuccessful &&
            successfulBatch.FailedCount == 0;

        messages.Add(
            successfulBatchWasDetected
                ? "El lote completamente correcto fue detectado."
                : "El lote completamente correcto no fue detectado.");

        MetadataProductiveApplicationResult
            failedIndividualResult =
                new();

        MetadataApplyBatchResult
            partialFailureBatch =
                new()
                {
                    BatchId =
                        Guid.NewGuid(),

                    StartedAtUtc =
                        startedAtUtc,

                    FinishedAtUtc =
                        finishedAtUtc,

                    Results =
                        new[]
                        {
                            successfulIndividualResult,
                            failedIndividualResult
                        }
                };

        bool failedResultsWereCounted =
            partialFailureBatch.TotalCount == 2 &&
            partialFailureBatch.SuccessfulCount == 1 &&
            partialFailureBatch.FailedCount == 1;

        messages.Add(
            failedResultsWereCounted
                ? "Los resultados no satisfactorios fueron contabilizados."
                : "Los resultados no satisfactorios no fueron contabilizados correctamente.");

        bool partialFailureWasDetected =
            !partialFailureBatch.WasSuccessful;

        messages.Add(
            partialFailureWasDetected
                ? "El fallo parcial del lote fue detectado."
                : "El fallo parcial del lote no fue detectado.");

        bool batchIdentityWasPreserved =
            successfulBatch.BatchId ==
            batchId;

        messages.Add(
            batchIdentityWasPreserved
                ? "La identidad del lote fue preservada."
                : "La identidad del lote no fue preservada.");

        bool timesWerePreserved =
            successfulBatch.StartedAtUtc ==
                startedAtUtc &&
            successfulBatch.FinishedAtUtc ==
                finishedAtUtc;

        messages.Add(
            timesWerePreserved
                ? "Los tiempos del lote fueron preservados."
                : "Los tiempos del lote no fueron preservados.");

        bool durationWasCalculated =
            successfulBatch.Duration ==
            TimeSpan.FromSeconds(
                5);

        messages.Add(
            durationWasCalculated
                ? "La duración del lote fue calculada correctamente."
                : "La duración del lote no fue calculada correctamente.");

        bool messagesWerePreserved =
            successfulBatch.Messages.Count == 1 &&
            successfulBatch.Messages[0] ==
                "Mensaje estructural del lote.";

        messages.Add(
            messagesWerePreserved
                ? "Los mensajes del lote fueron preservados."
                : "Los mensajes del lote no fueron preservados.");

        bool summaryWasGenerated =
            !string.IsNullOrWhiteSpace(
                successfulBatch.Summary) &&
            !string.IsNullOrWhiteSpace(
                partialFailureBatch.Summary);

        messages.Add(
            summaryWasGenerated
                ? "Los resúmenes del lote fueron generados."
                : "Los resúmenes del lote no fueron generados.");

        return
            new MetadataApplyBatchResultTestResult
            {
                EmptyResultWasRejected =
                    emptyResultWasRejected,

                SuccessfulResultsWereCounted =
                    successfulResultsWereCounted,

                FailedResultsWereCounted =
                    failedResultsWereCounted,

                SuccessfulBatchWasDetected =
                    successfulBatchWasDetected,

                PartialFailureWasDetected =
                    partialFailureWasDetected,

                BatchIdentityWasPreserved =
                    batchIdentityWasPreserved,

                TimesWerePreserved =
                    timesWerePreserved,

                DurationWasCalculated =
                    durationWasCalculated,

                MessagesWerePreserved =
                    messagesWerePreserved,

                SummaryWasGenerated =
                    summaryWasGenerated,

                Messages =
                    messages
            };
    }

    private static MetadataProductiveApplicationResult
    CreateSuccessfulResult()
    {
        FileIsolationContext isolationContext =
            new()
            {
                OriginalFilePath =
                    @"C:\test\original.flac",

                OriginalFileName =
                    "original.flac",

                WorkingCopyPath =
                    @"C:\test\working.flac",

                WorkingBackupPath =
                    @"C:\test\backup.flac",

                TestDirectoryPath =
                    @"C:\test",

                OriginalHashBefore =
                    "ORIGINAL_HASH",

                WorkingCopyHashBefore =
                    "WORKING_HASH_BEFORE",

                WorkingBackupHash =
                    "WORKING_HASH_BEFORE"
            };

        FileIsolationVerificationResult
            isolationVerification =
                new()
                {
                    Context =
                        isolationContext,

                    OriginalHashAfter =
                        "ORIGINAL_HASH",

                    WorkingCopyHashAfter =
                        "WORKING_HASH_AFTER"
                };

        MetadataApplyResult applyResult =
            new()
            {
                Status =
                    MetadataApplyStatus.Completed
            };

        MetadataApplicationStageResult
            stageResult =
                new()
                {
                    Stage =
                        MetadataApplicationStage.Finalization,

                    Status =
                        MetadataApplicationStageStatus.Completed
                };

        MetadataApplicationPipelineResult
            pipelineResult =
                new()
                {
                    StopReason =
                        MetadataApplicationStopReason.Completed,

                    ApplyResult =
                        applyResult,

                    StageResults =
                        new[]
                        {
                        stageResult
                        }
                };

        MetadataApplicationIsolatedExecutionResult
            isolatedExecutionResult =
                new()
                {
                    IsolationContext =
                        isolationContext,

                    PipelineResult =
                        pipelineResult,

                    IsolationVerification =
                        isolationVerification,

                    EnvironmentWasPreserved =
                        true
                };

        MetadataApplicationPromotionResult
            promotionResult =
                new()
                {
                    InputsWereValidated =
                        true,

                    ProductiveBackupWasCreated =
                        true,

                    ProductiveBackupWasVerified =
                        true,

                    ReplacementWasExecuted =
                        true,

                    PromotedFileWasVerified =
                        true,

                    RollbackWasAttempted =
                        false
                };

        return
            new MetadataProductiveApplicationResult
            {
                IsolatedExecutionResult =
                    isolatedExecutionResult,

                PromotionDecision =
                    MetadataPromotionDecision.Approved,

                PromotionResult =
                    promotionResult,

                FinalCleanupWasAttempted =
                    true,

                FinalCleanupWasSuccessful =
                    true
            };
    }
}