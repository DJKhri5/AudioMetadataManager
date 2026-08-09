using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Coordination;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation.Application.Testing.Infrastructure;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Comprueba la segunda fase de la coordinación productiva
/// batch sin modificar archivos reales.
/// </summary>
public sealed class
    MetadataProductiveTwoPhaseBatchCompletionTestRunner
{
    public async Task<
        MetadataProductiveTwoPhaseBatchCompletionTestResult>
        RunAsync()
    {
        List<string> messages =
            new();

        RecordingProductiveApplicationCoordinator
            validationIndividualCoordinator =
                new();

        MetadataProductiveTwoPhaseBatchCoordinator
            validationCoordinator =
                new(
                    validationIndividualCoordinator);

        bool nullPreparationWasRejected =
            false;

        try
        {
            await validationCoordinator.CompleteAsync(
                null!,
                MetadataPromotionDecision.Declined);
        }
        catch (ArgumentNullException)
        {
            nullPreparationWasRejected =
                true;
        }

        messages.Add(
            nullPreparationWasRejected
                ? "La preparación nula fue rechazada."
                : "La preparación nula no fue rechazada.");

        MetadataProductiveBatchPreparationResult
            invalidPreparation =
                new();

        bool unsupportedDecisionWasRejected =
            false;

        try
        {
            await validationCoordinator.CompleteAsync(
                invalidPreparation,
                MetadataPromotionDecision.Pending);
        }
        catch (ArgumentOutOfRangeException)
        {
            unsupportedDecisionWasRejected =
                true;
        }

        messages.Add(
            unsupportedDecisionWasRejected
                ? "La decisión global no admitida fue rechazada."
                : "La decisión global no admitida no fue rechazada.");

        bool invalidPreparationWasRejected =
            false;

        try
        {
            await validationCoordinator.CompleteAsync(
                invalidPreparation,
                MetadataPromotionDecision.Declined);
        }
        catch (InvalidOperationException)
        {
            invalidPreparationWasRejected =
                true;
        }

        messages.Add(
            invalidPreparationWasRejected
                ? "La preparación no lista fue rechazada."
                : "La preparación no lista no fue rechazada.");

        MetadataApplyBatchRequest batchRequest =
            CreateBatchRequest();

        RecordingProductiveApplicationCoordinator
            declinedIndividualCoordinator =
                new();

        MetadataProductiveTwoPhaseBatchCoordinator
            declinedCoordinator =
                new(
                    declinedIndividualCoordinator);

        MetadataProductiveBatchPreparationResult
            declinedPreparation =
                await declinedCoordinator.PrepareAsync(
                    batchRequest);

        MetadataProductiveBatchCompletionResult
            declinedResult =
                await declinedCoordinator.CompleteAsync(
                    declinedPreparation,
                    MetadataPromotionDecision.Declined);

        bool declinedCompletedAllPreparations =
            declinedResult.DecisionResultCount == 3 &&
            declinedIndividualCoordinator.CompleteCallCount == 3;

        messages.Add(
            declinedCompletedAllPreparations
                ? "Declined finalizó las tres preparaciones."
                : "Declined no finalizó todas las preparaciones.");

        bool declinedWasForwardedToAll =
            declinedIndividualCoordinator
                .PromotionDecisions.Count == 3 &&
            declinedIndividualCoordinator
                .PromotionDecisions.All(
                    decision =>
                        decision ==
                        MetadataPromotionDecision.Declined);

        messages.Add(
            declinedWasForwardedToAll
                ? "Declined fue reenviado a todo el lote."
                : "Declined no fue reenviado correctamente.");

        bool declinedBatchWasSuccessful =
            declinedResult.WasSuccessful &&
            declinedResult.DecisionResults.All(
                result =>
                    result.WasSafelyDeclined);

        messages.Add(
            declinedBatchWasSuccessful
                ? "El lote Declined terminó correctamente."
                : "El lote Declined no terminó correctamente.");

        RecordingProductiveApplicationCoordinator
            approvedIndividualCoordinator =
                new();

        MetadataProductiveTwoPhaseBatchCoordinator
            approvedCoordinator =
                new(
                    approvedIndividualCoordinator);

        MetadataProductiveBatchPreparationResult
            approvedPreparation =
                await approvedCoordinator.PrepareAsync(
                    batchRequest);

        MetadataProductiveBatchCompletionResult
            approvedResult =
                await approvedCoordinator.CompleteAsync(
                    approvedPreparation,
                    MetadataPromotionDecision.Approved);

        bool approvedCompletedAllPreparations =
            approvedResult.DecisionResultCount == 3 &&
            approvedIndividualCoordinator.CompleteCallCount == 3;

        messages.Add(
            approvedCompletedAllPreparations
                ? "Approved finalizó las tres preparaciones."
                : "Approved no finalizó todas las preparaciones.");

        bool approvedWasForwardedToAll =
            approvedIndividualCoordinator
                .PromotionDecisions.Count == 3 &&
            approvedIndividualCoordinator
                .PromotionDecisions.All(
                    decision =>
                        decision ==
                        MetadataPromotionDecision.Approved);

        messages.Add(
            approvedWasForwardedToAll
                ? "Approved fue reenviado a todo el lote."
                : "Approved no fue reenviado correctamente.");

        bool approvedBatchWasSuccessful =
            approvedResult.WasSuccessful &&
            approvedResult.DecisionResults.All(
                result =>
                    result.WasSuccessfullyPromoted);

        messages.Add(
            approvedBatchWasSuccessful
                ? "El lote Approved terminó correctamente."
                : "El lote Approved no terminó correctamente.");

        RecordingProductiveApplicationCoordinator
            failingApprovedIndividualCoordinator =
                new()
                {
                    ThrowOnCompleteCall =
                        2
                };

        MetadataProductiveTwoPhaseBatchCoordinator
            failingApprovedCoordinator =
                new(
                    failingApprovedIndividualCoordinator);

        MetadataProductiveBatchPreparationResult
            failingApprovedPreparation =
                await failingApprovedCoordinator.PrepareAsync(
                    batchRequest);

        MetadataProductiveBatchCompletionResult
            failingApprovedResult =
                await failingApprovedCoordinator.CompleteAsync(
                    failingApprovedPreparation,
                    MetadataPromotionDecision.Approved);

        bool approvedFailureStoppedFurtherPromotion =
            failingApprovedResult.DecisionResultCount == 2 &&
            failingApprovedResult.DecisionResults[0]
                .WasSuccessfullyPromoted &&
            !string.IsNullOrWhiteSpace(
                failingApprovedResult.DecisionResults[1]
                    .ErrorMessage);

        messages.Add(
            approvedFailureStoppedFurtherPromotion
                ? "El segundo fallo Approved detuvo nuevas promociones."
                : "El fallo Approved no detuvo correctamente el lote.");

        bool remainingPreparationsWereDeclined =
            failingApprovedResult.CleanupResultCount == 2 &&
            failingApprovedResult.CleanupWasSuccessful &&
            failingApprovedIndividualCoordinator
                .PromotionDecisions.Count(
                    decision =>
                        decision ==
                        MetadataPromotionDecision.Declined) == 2;

        messages.Add(
            remainingPreparationsWereDeclined
                ? "Las preparaciones pendientes fueron descartadas."
                : "Las preparaciones pendientes no fueron limpiadas.");

        bool failedApprovedBatchWasNotSuccessful =
            !failingApprovedResult.WasSuccessful;

        messages.Add(
            failedApprovedBatchWasNotSuccessful
                ? "El lote Approved fallido no declaró éxito."
                : "El lote Approved fallido declaró un estado incorrecto.");

        using CancellationTokenSource
            cancellationSource =
                new();

        cancellationSource.Cancel();

        RecordingProductiveApplicationCoordinator
            cancellationIndividualCoordinator =
                new();

        MetadataProductiveTwoPhaseBatchCoordinator
            cancellationCoordinator =
                new(
                    cancellationIndividualCoordinator);

        MetadataProductiveBatchPreparationResult
            cancellationPreparation =
                await cancellationCoordinator.PrepareAsync(
                    batchRequest);

        bool cancellationCleanedPendingPreparations =
            false;

        try
        {
            await cancellationCoordinator.CompleteAsync(
                cancellationPreparation,
                MetadataPromotionDecision.Approved,
                cancellationSource.Token);
        }
        catch (OperationCanceledException)
        {
            cancellationCleanedPendingPreparations =
                cancellationIndividualCoordinator
                    .CompleteCallCount == 3 &&
                cancellationIndividualCoordinator
                    .PromotionDecisions.All(
                        decision =>
                            decision ==
                            MetadataPromotionDecision.Declined);
        }

        messages.Add(
            cancellationCleanedPendingPreparations
                ? "La cancelación descartó todas las preparaciones pendientes."
                : "La cancelación no limpió correctamente las preparaciones.");

        return
            new MetadataProductiveTwoPhaseBatchCompletionTestResult
            {
                NullPreparationWasRejected =
                    nullPreparationWasRejected,

                UnsupportedDecisionWasRejected =
                    unsupportedDecisionWasRejected,

                InvalidPreparationWasRejected =
                    invalidPreparationWasRejected,

                DeclinedCompletedAllPreparations =
                    declinedCompletedAllPreparations,

                DeclinedWasForwardedToAll =
                    declinedWasForwardedToAll,

                DeclinedBatchWasSuccessful =
                    declinedBatchWasSuccessful,

                ApprovedCompletedAllPreparations =
                    approvedCompletedAllPreparations,

                ApprovedWasForwardedToAll =
                    approvedWasForwardedToAll,

                ApprovedBatchWasSuccessful =
                    approvedBatchWasSuccessful,

                ApprovedFailureStoppedFurtherPromotion =
                    approvedFailureStoppedFurtherPromotion,

                RemainingPreparationsWereDeclined =
                    remainingPreparationsWereDeclined,

                FailedApprovedBatchWasNotSuccessful =
                    failedApprovedBatchWasNotSuccessful,

                CancellationCleanedPendingPreparations =
                    cancellationCleanedPendingPreparations,

                Messages =
                    messages
            };
    }

    private static MetadataApplyBatchRequest
        CreateBatchRequest()
    {
        return
            new MetadataApplyBatchRequest
            {
                Requests =
                    new[]
                    {
                        CreateRequest(
                            @"C:\AudioMetadataManager\Tests\completion-1.flac",
                            "completion-1.flac"),

                        CreateRequest(
                            @"C:\AudioMetadataManager\Tests\completion-2.flac",
                            "completion-2.flac"),

                        CreateRequest(
                            @"C:\AudioMetadataManager\Tests\completion-3.flac",
                            "completion-3.flac")
                    }
            };
    }

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
                                    "Prueba de finalización batch en dos fases"
                                }
                        }
                    }
            };
    }
}