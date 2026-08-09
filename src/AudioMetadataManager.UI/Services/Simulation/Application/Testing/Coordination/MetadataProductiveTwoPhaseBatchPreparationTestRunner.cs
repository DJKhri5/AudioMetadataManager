using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Coordination;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation.Application.Testing.Infrastructure;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Comprueba de forma controlada la fase de preparación
/// productiva de un lote en dos fases.
///
/// No modifica archivos reales.
/// </summary>
public sealed class
    MetadataProductiveTwoPhaseBatchPreparationTestRunner
{
    public async Task<
        MetadataProductiveTwoPhaseBatchPreparationTestResult>
        RunAsync()
    {
        List<string> messages =
            new();

        bool nullCoordinatorWasRejected =
            false;

        try
        {
            _ =
                new MetadataProductiveTwoPhaseBatchCoordinator(
                    null!);
        }
        catch (ArgumentNullException)
        {
            nullCoordinatorWasRejected =
                true;
        }

        messages.Add(
            nullCoordinatorWasRejected
                ? "La dependencia individual nula fue rechazada."
                : "La dependencia individual nula no fue rechazada.");

        RecordingProductiveApplicationCoordinator
            controlledIndividualCoordinator =
                new();

        MetadataProductiveTwoPhaseBatchCoordinator
            coordinator =
                new(
                    controlledIndividualCoordinator);

        bool nullBatchWasRejected =
            false;

        try
        {
            await coordinator.PrepareAsync(
                null!);
        }
        catch (ArgumentNullException)
        {
            nullBatchWasRejected =
                true;
        }

        messages.Add(
            nullBatchWasRejected
                ? "La solicitud batch nula fue rechazada."
                : "La solicitud batch nula no fue rechazada.");

        MetadataApplyBatchRequest invalidBatch =
            new();

        MetadataProductiveBatchPreparationResult
            invalidResult =
                await coordinator.PrepareAsync(
                    invalidBatch);

        bool invalidBatchWasRejected =
            !invalidBatch.IsStructurallyValid &&
            invalidResult.ResultCount == 0 &&
            !invalidResult.IsReadyForDecision;

        messages.Add(
            invalidBatchWasRejected
                ? "El lote estructuralmente inválido no fue preparado."
                : "El lote estructuralmente inválido produjo un estado incorrecto.");

        MetadataApplyRequest firstRequest =
            CreateRequest(
                @"C:\AudioMetadataManager\Tests\two-phase-1.flac",
                "two-phase-1.flac");

        MetadataApplyRequest secondRequest =
            CreateRequest(
                @"C:\AudioMetadataManager\Tests\two-phase-2.flac",
                "two-phase-2.flac");

        MetadataApplyRequest thirdRequest =
            CreateRequest(
                @"C:\AudioMetadataManager\Tests\two-phase-3.flac",
                "two-phase-3.flac");

        MetadataApplyBatchRequest validBatch =
            new()
            {
                Requests =
                    new[]
                    {
                        firstRequest,
                        secondRequest,
                        thirdRequest
                    }
            };

        RecordingProductiveApplicationCoordinator
            successfulIndividualCoordinator =
                new();

        MetadataProductiveTwoPhaseBatchCoordinator
            successfulCoordinator =
                new(
                    successfulIndividualCoordinator);

        MetadataProductiveBatchPreparationResult
            successfulResult =
                await successfulCoordinator.PrepareAsync(
                    validBatch);

        bool allRequestsWerePrepared =
            successfulIndividualCoordinator
                .PrepareCallCount == 3 &&
            successfulIndividualCoordinator
                .CompleteCallCount == 0 &&
            successfulResult.ResultCount == 3 &&
            successfulResult.VerifiedPreparationCount == 3;

        messages.Add(
            allRequestsWerePrepared
                ? "Las tres solicitudes fueron preparadas sin finalizarlas."
                : "No se prepararon exactamente las tres solicitudes esperadas.");

        bool preparationsWerePending =
            successfulResult.PreparationResults.All(
                result =>
                    result.VerifiedCopyWasPrepared &&
                    result.PromotionDecision ==
                        MetadataPromotionDecision.Pending);

        messages.Add(
            preparationsWerePending
                ? "Todas las preparaciones quedaron pendientes de decisión."
                : "Una o más preparaciones no quedaron pendientes.");

        bool batchWasReadyForDecision =
            successfulResult.IsReadyForDecision &&
            !successfulResult.WasAbortedAndCleanedUp;

        messages.Add(
            batchWasReadyForDecision
                ? "El lote quedó listo para una decisión global."
                : "El lote no quedó listo para una decisión global.");

        RecordingProductiveApplicationCoordinator
            failingIndividualCoordinator =
                new()
                {
                    ThrowOnPrepareCall =
                        2
                };

        MetadataProductiveTwoPhaseBatchCoordinator
            failingCoordinator =
                new(
                    failingIndividualCoordinator);

        MetadataProductiveBatchPreparationResult
            failedResult =
                await failingCoordinator.PrepareAsync(
                    validBatch);

        bool preparationFailureStoppedBatch =
            failingIndividualCoordinator.PrepareCallCount == 2;

        messages.Add(
            preparationFailureStoppedBatch
                ? "El fallo en la segunda preparación detuvo el lote."
                : "El lote continuó después del fallo de preparación.");

        bool pendingPreparationsWereCleanedUp =
            failingIndividualCoordinator.CompleteCallCount == 1 &&
            failingIndividualCoordinator.LastPromotionDecision ==
                MetadataPromotionDecision.Declined &&
            failedResult.WasAbortedAndCleanedUp;

        messages.Add(
            pendingPreparationsWereCleanedUp
                ? "La preparación pendiente anterior fue descartada de forma segura."
                : "La preparación pendiente anterior no fue limpiada correctamente.");

        bool failedBatchWasNotReadyForDecision =
            !failedResult.IsReadyForDecision;

        messages.Add(
            failedBatchWasNotReadyForDecision
                ? "El lote fallido no quedó disponible para promoción."
                : "El lote fallido quedó incorrectamente listo para promoción.");

        return
            new MetadataProductiveTwoPhaseBatchPreparationTestResult
            {
                NullCoordinatorWasRejected =
                    nullCoordinatorWasRejected,

                NullBatchWasRejected =
                    nullBatchWasRejected,

                InvalidBatchWasRejected =
                    invalidBatchWasRejected,

                AllRequestsWerePrepared =
                    allRequestsWerePrepared,

                PreparationsWerePending =
                    preparationsWerePending,

                BatchWasReadyForDecision =
                    batchWasReadyForDecision,

                PreparationFailureStoppedBatch =
                    preparationFailureStoppedBatch,

                PendingPreparationsWereCleanedUp =
                    pendingPreparationsWereCleanedUp,

                FailedBatchWasNotReadyForDecision =
                    failedBatchWasNotReadyForDecision,

                Messages =
                    messages
            };
    }

    /// <summary>
    /// Construye una solicitud técnica válida exclusivamente
    /// para pruebas controladas de coordinación.
    /// </summary>
    private static MetadataApplyRequest CreateRequest(
        string filePath,
        string fileName)
    {
        return
            new MetadataApplyRequest
            {
                RequestId =
                    Guid.NewGuid(),

                PlanId =
                    Guid.NewGuid(),

                CreatedAtUtc =
                    DateTimeOffset.UtcNow,

                FilePath =
                    filePath,

                FileName =
                    fileName,

                RequireBackup =
                    true,

                RequirePostWriteVerification =
                    true,

                Changes =
                    new[]
                    {
                        new MetadataFieldChange
                        {
                            Field =
                                MetadataField.Genre,

                            OriginalValue =
                                string.Empty,

                            NewValue =
                                DiagnosticMetadataTestValues.CreateGenre(),

                            WasManuallyApproved =
                                true,

                            Confidence =
                                1.0,

                            SupportingSources =
                                new[]
                                {
                                    "Prueba de preparación batch en dos fases"
                                }
                        }
                    }
            };
    }
}