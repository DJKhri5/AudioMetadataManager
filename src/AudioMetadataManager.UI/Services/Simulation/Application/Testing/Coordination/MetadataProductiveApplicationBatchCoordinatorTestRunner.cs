using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Coordination;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation.Application.Testing.Infrastructure;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Ejecuta comprobaciones estructurales controladas sobre
/// MetadataProductiveApplicationBatchCoordinator.
///
/// No modifica archivos y no ejecuta solicitudes productivas
/// individuales.
/// </summary>
public sealed class MetadataProductiveApplicationBatchCoordinatorTestRunner
{
    public async Task<
        MetadataProductiveApplicationBatchCoordinatorTestResult>
        RunAsync()
    {
        List<string> messages =
            new();

        bool nullCoordinatorWasRejected =
            false;

        try
        {
            _ =
                new MetadataProductiveApplicationBatchCoordinator(
                    null!);
        }
        catch (ArgumentNullException)
        {
            nullCoordinatorWasRejected =
                true;
        }

        messages.Add(
            nullCoordinatorWasRejected
                ? "La dependencia individual nula fue rechazada correctamente."
                : "La dependencia individual nula no fue rechazada.");

        RecordingProductiveApplicationCoordinator
            individualCoordinator =
                new();

        MetadataProductiveApplicationBatchCoordinator
            coordinator =
                new(
                    individualCoordinator);

        bool nullBatchWasRejected =
            false;

        try
        {
            await coordinator.ExecuteAsync(
                null!,
                MetadataPromotionDecision.Declined);
        }
        catch (ArgumentNullException)
        {
            nullBatchWasRejected =
                true;
        }

        messages.Add(
            nullBatchWasRejected
                ? "La solicitud por lote nula fue rechazada correctamente."
                : "La solicitud por lote nula no fue rechazada.");

        MetadataApplyBatchRequest batchRequest =
            new();

        bool preCancellationWasRespected =
            false;

        using (
            CancellationTokenSource cancellationSource =
                new())
        {
            cancellationSource.Cancel();

            try
            {
                await coordinator.ExecuteAsync(
                    batchRequest,
                    MetadataPromotionDecision.Declined,
                    cancellationSource.Token);
            }
            catch (OperationCanceledException)
            {
                preCancellationWasRespected =
                    true;
            }
        }

        messages.Add(
            preCancellationWasRespected
                ? "La cancelación previa fue respetada correctamente."
                : "La cancelación previa no fue respetada.");

        MetadataApplyBatchRequest invalidBatchRequest =
            new();

        MetadataApplyBatchResult invalidBatchResult =
            await coordinator.ExecuteAsync(
                invalidBatchRequest,
                MetadataPromotionDecision.Declined);

        bool invalidBatchWasRejected =
            !invalidBatchRequest.IsStructurallyValid &&
            invalidBatchResult.TotalCount == 0 &&
            invalidBatchResult.Messages.Count > 0;

        messages.Add(
            invalidBatchWasRejected
                ? "El lote estructuralmente inválido fue rechazado."
                : "El lote estructuralmente inválido no fue rechazado.");

        Guid controlledBatchId =
            Guid.NewGuid();

        MetadataApplyRequest controlledRequest =
            new()
            {
                RequestId =
                    Guid.NewGuid(),

                PlanId =
                    Guid.NewGuid(),

                CreatedAtUtc =
                    DateTime.UtcNow,

                FilePath =
                    @"C:\AudioMetadataManager\Tests\batch-test.flac",

                FileName =
                    "batch-test.flac",

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
                            "Prueba estructural del coordinador batch"
                        }
                    }
                }
            };

        MetadataApplyBatchRequest controlledBatchRequest =
            new()
            {
                BatchId =
                    controlledBatchId,

                Requests =
                    new[]
                    {
                controlledRequest
                    }
            };

        MetadataApplyRequest
            failFastSecondRequest =
                new()
                {
                    RequestId =
                        Guid.NewGuid(),

                    PlanId =
                        controlledRequest.PlanId,

                    CreatedAtUtc =
                        controlledRequest.CreatedAtUtc,

                    FilePath =
                        @"C:\AudioMetadataManager\Tests\batch-fail-fast-2.flac",

                    FileName =
                        "batch-fail-fast-2.flac",

                    Changes =
                        controlledRequest.Changes,

                    RequireBackup =
                        controlledRequest.RequireBackup,

                    RequirePostWriteVerification =
                        controlledRequest.RequirePostWriteVerification
                };

        MetadataApplyRequest
            failFastThirdRequest =
                new()
                {
                    RequestId =
                        Guid.NewGuid(),

                    PlanId =
                        controlledRequest.PlanId,

                    CreatedAtUtc =
                        controlledRequest.CreatedAtUtc,

                    FilePath =
                        @"C:\AudioMetadataManager\Tests\batch-fail-fast-3.flac",

                    FileName =
                        "batch-fail-fast-3.flac",

                    Changes =
                        controlledRequest.Changes,

                    RequireBackup =
                        controlledRequest.RequireBackup,

                    RequirePostWriteVerification =
                        controlledRequest.RequirePostWriteVerification
                };

        MetadataApplyBatchRequest
            failFastBatchRequest =
                new()
                {
                    Requests =
                        new[]
                        {
                    controlledRequest,
                    failFastSecondRequest,
                    failFastThirdRequest
                        }
                };

        RecordingProductiveApplicationCoordinator
            failFastIndividualCoordinator =
                new()
                {
                    ThrowOnPrepareCall =
                        2
                };

        MetadataProductiveApplicationBatchCoordinator
            failFastBatchCoordinator =
                new(
                    failFastIndividualCoordinator);

        MetadataApplyBatchResult
            failFastResult =
                await failFastBatchCoordinator.ExecuteAsync(
                    failFastBatchRequest,
                    MetadataPromotionDecision.Declined);

        bool failFastStoppedAfterSecondPrepare =
            failFastIndividualCoordinator.PrepareCallCount == 2 &&
            failFastIndividualCoordinator.CompleteCallCount == 1;

        messages.Add(
            failFastStoppedAfterSecondPrepare
                ? "El lote se detuvo inmediatamente cuando falló la " +
                  "segunda preparación."
                : "El lote no respetó el comportamiento fail-fast esperado.");

        bool partialFailureResultWasReturned =
            failFastResult is not null;

        messages.Add(
            partialFailureResultWasReturned
                ? "El fallo parcial produjo un resultado batch auditable."
                : "El fallo parcial no produjo un resultado batch auditable.");

        bool partialFailureWasPreserved =
            failFastResult is not null &&
            failFastResult.Results.Count == 2 &&
            failFastResult.Results[1].ErrorMessage ==
                "Fallo simulado durante PrepareAsync.";

        messages.Add(
            partialFailureWasPreserved
                ? "El resultado batch conservó la ejecución previa y el " +
                  "fallo que detuvo el lote."
                : "El resultado batch no conservó correctamente el fallo parcial.");

        RecordingProductiveApplicationCoordinator
            completeExceptionIndividualCoordinator =
                new()
                {
                    ThrowOnCompleteCall =
                        2
                };

        MetadataProductiveApplicationBatchCoordinator
            completeExceptionBatchCoordinator =
                new(
                    completeExceptionIndividualCoordinator);

        MetadataApplyBatchResult
            completeExceptionResult =
                await completeExceptionBatchCoordinator.ExecuteAsync(
                    failFastBatchRequest,
                    MetadataPromotionDecision.Declined);

        bool failFastStoppedAfterSecondComplete =
            completeExceptionIndividualCoordinator.PrepareCallCount == 2 &&
            completeExceptionIndividualCoordinator.CompleteCallCount == 2;

        messages.Add(
            failFastStoppedAfterSecondComplete
                ? "El lote se detuvo inmediatamente cuando falló la " +
                  "segunda finalización."
                : "El lote no se detuvo correctamente tras el fallo " +
                  "de la segunda finalización.");

        bool completeExceptionWasPreserved =
            completeExceptionResult.Results.Count == 2 &&
            completeExceptionResult.Results[1].ErrorMessage ==
                "Fallo simulado durante CompleteAsync.";

        messages.Add(
            completeExceptionWasPreserved
                ? "El lote conservó correctamente la excepción producida " +
                  "durante CompleteAsync."
                : "El lote no conservó correctamente la excepción de " +
                  "CompleteAsync.");

        RecordingProductiveApplicationCoordinator
            returnedPrepareFailureIndividualCoordinator =
                new()
                {
                    ReturnPrepareErrorOnCall =
                        2
                };

        MetadataProductiveApplicationBatchCoordinator
            returnedPrepareFailureBatchCoordinator =
                new(
                    returnedPrepareFailureIndividualCoordinator);

        MetadataApplyBatchResult
            returnedPrepareFailureResult =
                await returnedPrepareFailureBatchCoordinator.ExecuteAsync(
                    failFastBatchRequest,
                    MetadataPromotionDecision.Declined);

        bool returnedPrepareFailureStoppedBatch =
            returnedPrepareFailureIndividualCoordinator.PrepareCallCount == 2 &&
            returnedPrepareFailureIndividualCoordinator.CompleteCallCount == 1 &&
            returnedPrepareFailureResult.Results.Count == 2 &&
            returnedPrepareFailureResult.Results[1].ErrorMessage ==
                "Fallo controlado durante PrepareAsync.";

        messages.Add(
            returnedPrepareFailureStoppedBatch
                ? "El lote detectó y detuvo correctamente un fallo " +
                  "devuelto por PrepareAsync."
                : "El lote no procesó correctamente el fallo devuelto " +
                  "por PrepareAsync.");

        RecordingProductiveApplicationCoordinator
            returnedCompleteFailureIndividualCoordinator =
                new()
                {
                    ReturnCompleteErrorOnCall =
                        2
                };

        MetadataProductiveApplicationBatchCoordinator
            returnedCompleteFailureBatchCoordinator =
                new(
                    returnedCompleteFailureIndividualCoordinator);

        MetadataApplyBatchResult
            returnedCompleteFailureResult =
                await returnedCompleteFailureBatchCoordinator.ExecuteAsync(
                    failFastBatchRequest,
                    MetadataPromotionDecision.Declined);

        bool returnedCompleteFailureStoppedBatch =
            returnedCompleteFailureIndividualCoordinator.PrepareCallCount == 2 &&
            returnedCompleteFailureIndividualCoordinator.CompleteCallCount == 2 &&
            returnedCompleteFailureResult.Results.Count == 2 &&
            returnedCompleteFailureResult.Results[1].ErrorMessage ==
                "Fallo controlado durante CompleteAsync.";

        messages.Add(
            returnedCompleteFailureStoppedBatch
                ? "El lote detectó y detuvo correctamente un fallo " +
                  "devuelto por CompleteAsync."
                : "El lote no procesó correctamente el fallo devuelto " +
                  "por CompleteAsync.");

        bool remainingRequestsWereReported =
            completeExceptionResult.Messages.Any(
                message =>
                    message.Contains(
                        "1 solicitud(es) no fueron ejecutadas",
                        StringComparison.Ordinal));

        messages.Add(
            remainingRequestsWereReported
                ? "El lote registró correctamente las solicitudes que " +
                  "quedaron sin ejecutar."
                : "El lote no registró correctamente las solicitudes " +
                  "restantes sin ejecutar.");

        using CancellationTokenSource
            midBatchCancellationSource =
                new();

        RecordingProductiveApplicationCoordinator
            midBatchCancellationIndividualCoordinator =
                new()
                {
                    CancellationSource =
                        midBatchCancellationSource,

                    CancelAfterCompleteCall =
                        1
                };

        MetadataProductiveApplicationBatchCoordinator
            midBatchCancellationCoordinator =
                new(
                    midBatchCancellationIndividualCoordinator);

        bool midBatchCancellationWasRespected;

        try
        {
            await midBatchCancellationCoordinator.ExecuteAsync(
                failFastBatchRequest,
                MetadataPromotionDecision.Declined,
                midBatchCancellationSource.Token);

            midBatchCancellationWasRespected =
                false;
        }
        catch (OperationCanceledException)
        {
            midBatchCancellationWasRespected =
                midBatchCancellationSource
                    .IsCancellationRequested &&
                midBatchCancellationIndividualCoordinator
                    .PrepareCallCount == 1 &&
                midBatchCancellationIndividualCoordinator
                    .CompleteCallCount == 1;
        }

        messages.Add(
            midBatchCancellationWasRespected
                ? "La cancelación iniciada después de la primera " +
                  "solicitud detuvo correctamente el resto del lote."
                : "La cancelación durante un lote iniciado no fue " +
                  "respetada correctamente.");

        RecordingProductiveApplicationCoordinator
            multiApprovedIndividualCoordinator =
                new();

        MetadataProductiveApplicationBatchCoordinator
            multiApprovedBatchCoordinator =
                new(
                    multiApprovedIndividualCoordinator);

        MetadataApplyBatchResult
            multiApprovedResult =
                await multiApprovedBatchCoordinator.ExecuteAsync(
                    failFastBatchRequest,
                    MetadataPromotionDecision.Approved);

        bool multiApprovedBatchWasExecuted =
            multiApprovedIndividualCoordinator.PrepareCallCount == 3 &&
            multiApprovedIndividualCoordinator.CompleteCallCount == 3 &&
            multiApprovedResult.Results.Count == 3;

        messages.Add(
            multiApprovedBatchWasExecuted
                ? "El lote Approved procesó correctamente sus tres " +
                  "solicitudes."
                : "El lote Approved no procesó correctamente todas sus " +
                  "solicitudes.");

        bool multiApprovedDecisionWasForwarded =
            multiApprovedIndividualCoordinator
                .PromotionDecisions.Count == 3 &&
            multiApprovedIndividualCoordinator
                .PromotionDecisions.All(
                    decision =>
                        decision ==
                        MetadataPromotionDecision.Approved);

        messages.Add(
            multiApprovedDecisionWasForwarded
                ? "La decisión Approved fue reenviada a todas las " +
                  "solicitudes del lote."
                : "La decisión Approved no fue reenviada correctamente " +
                  "a todas las solicitudes.");

        RecordingProductiveApplicationCoordinator
            approvedFailureIndividualCoordinator =
                new()
                {
                    ThrowOnCompleteCall =
                        2
                };

        MetadataProductiveApplicationBatchCoordinator
            approvedFailureBatchCoordinator =
                new(
                    approvedFailureIndividualCoordinator);

        MetadataApplyBatchResult
            approvedFailureResult =
                await approvedFailureBatchCoordinator.ExecuteAsync(
                    failFastBatchRequest,
                    MetadataPromotionDecision.Approved);

        bool approvedFailureStoppedBatch =
            approvedFailureIndividualCoordinator.PrepareCallCount == 2 &&
            approvedFailureIndividualCoordinator.CompleteCallCount == 2 &&
            approvedFailureResult.Results.Count == 2;

        messages.Add(
            approvedFailureStoppedBatch
                ? "El lote Approved se detuvo correctamente cuando " +
                  "falló la segunda finalización."
                : "El lote Approved continuó después de un fallo que " +
                  "debía detenerlo.");

        bool approvedFailurePreservedPreviousResult =
            approvedFailureResult.Results.Count == 2 &&
            approvedFailureResult.Results[0]
                .PromotionDecision ==
                    MetadataPromotionDecision.Approved &&
            string.IsNullOrWhiteSpace(
                approvedFailureResult.Results[0]
                    .ErrorMessage) &&
            approvedFailureResult.Results[1]
                .ErrorMessage ==
                    "Fallo simulado durante CompleteAsync.";

        messages.Add(
            approvedFailurePreservedPreviousResult
                ? "El fallo Approved conservó el resultado completado " +
                  "anterior sin intentar revertirlo a nivel de lote."
                : "El fallo Approved no conservó correctamente el " +
                  "resultado completado anterior.");

        MetadataApplyBatchResult controlledResult =
            await coordinator.ExecuteAsync(
                controlledBatchRequest,
                MetadataPromotionDecision.Declined);

        bool controlledResultWasCreated =
            controlledResult is not null;

        messages.Add(
            controlledResultWasCreated
                ? "El resultado controlado fue creado."
                : "El resultado controlado no fue creado.");

        if (controlledResult is null)
        {
            return
                new MetadataProductiveApplicationBatchCoordinatorTestResult
                {
                    NullCoordinatorWasRejected =
                        nullCoordinatorWasRejected,

                    NullBatchWasRejected =
                        nullBatchWasRejected,

                    PreCancellationWasRespected =
                        preCancellationWasRespected,

                    ControlledResultWasCreated =
                        false,

                    InvalidBatchWasRejected =
                        invalidBatchWasRejected,

                    MessagesWereRecorded =
                        messages.Count > 0,

                    Messages =
                        messages
                };
        }

        bool batchIdentityWasCreated =
            controlledResult.BatchId !=
            Guid.Empty;

        messages.Add(
            batchIdentityWasCreated
                ? "La identidad del lote fue creada."
                : "La identidad del lote no fue creada.");

        bool timesWereRecorded =
            controlledResult.StartedAtUtc !=
                default &&
            controlledResult.FinishedAtUtc !=
                default &&
            controlledResult.FinishedAtUtc >=
                controlledResult.StartedAtUtc;

        messages.Add(
            timesWereRecorded
                ? "Los tiempos del lote fueron registrados."
                : "Los tiempos del lote no fueron registrados correctamente.");

        bool emptyResultWasNotSuccessful =
            invalidBatchResult.TotalCount == 0 &&
            !invalidBatchResult.WasSuccessful;

        messages.Add(
            emptyResultWasNotSuccessful
                ? "El resultado vacío no declaró éxito falso."
                : "El resultado vacío declaró un estado incorrecto.");

        bool batchIdentityWasPreserved =
            controlledResult.BatchId ==
            controlledBatchId;

        messages.Add(
            batchIdentityWasPreserved
                ? "La identidad original del lote fue preservada."
                : "La identidad original del lote no fue preservada.");

        bool validRequestsWereInspected =
            controlledBatchRequest.IsStructurallyValid &&
            controlledBatchRequest.ValidRequestCount == 1 &&
            string.Join(
                Environment.NewLine,
                controlledResult.Messages)
                .Contains(
                    controlledRequest.FilePath,
                    StringComparison.OrdinalIgnoreCase);

        messages.Add(
            validRequestsWereInspected
                ? "Todas las solicitudes válidas fueron inspeccionadas."
                : "Las solicitudes válidas no fueron inspeccionadas correctamente.");

        bool productiveResultsWereCreated =
            controlledResult.TotalCount == 1 &&
            controlledResult.Results.Count == 1;

        messages.Add(
            productiveResultsWereCreated
                ? "Se creó un resultado productivo por la solicitud válida."
                : "No se creó exactamente un resultado productivo por la solicitud válida.");

        bool individualPrepareWasCalledOnce =
            individualCoordinator.PrepareCallCount == 1;

        messages.Add(
            individualPrepareWasCalledOnce
                ? "PrepareAsync fue ejecutado exactamente una vez."
                : "PrepareAsync no fue ejecutado exactamente una vez.");

        bool individualCompleteWasCalledOnce =
            individualCoordinator.CompleteCallCount == 1;

        messages.Add(
            individualCompleteWasCalledOnce
                ? "CompleteAsync fue ejecutado exactamente una vez."
                : "CompleteAsync no fue ejecutado exactamente una vez.");

        bool declinedDecisionWasForwarded =
            individualCoordinator.LastPromotionDecision ==
                MetadataPromotionDecision.Declined &&
            controlledResult.Results.Count == 1 &&
            controlledResult.Results[0].PromotionWasDeclined;

        messages.Add(
            declinedDecisionWasForwarded
                ? "La decisión Declined fue enviada correctamente al coordinador individual."
                : "La decisión Declined no fue enviada correctamente al coordinador individual.");

        RecordingProductiveApplicationCoordinator
            approvedIndividualCoordinator =
                new();

        MetadataProductiveApplicationBatchCoordinator
            approvedCoordinator =
                new(
                    approvedIndividualCoordinator);

        MetadataApplyBatchResult approvedResult =
            await approvedCoordinator.ExecuteAsync(
                controlledBatchRequest,
                MetadataPromotionDecision.Approved);

        bool approvedResultsWereCreated =
            approvedResult.TotalCount == 1 &&
            approvedResult.Results.Count == 1;

        messages.Add(
            approvedResultsWereCreated
                ? "La ejecución Approved creó exactamente un resultado productivo."
                : "La ejecución Approved no creó exactamente un resultado productivo.");

        bool approvedCompleteWasCalledOnce =
            approvedIndividualCoordinator.CompleteCallCount == 1;

        messages.Add(
            approvedCompleteWasCalledOnce
                ? "CompleteAsync fue ejecutado exactamente una vez para Approved."
                : "CompleteAsync no fue ejecutado exactamente una vez para Approved.");

        bool approvedDecisionWasForwarded =
            approvedIndividualCoordinator.LastPromotionDecision ==
                MetadataPromotionDecision.Approved &&
            approvedResult.Results.Count == 1 &&
            approvedResult.Results[0].PromotionWasApproved;

        messages.Add(
            approvedDecisionWasForwarded
                ? "La decisión Approved fue enviada correctamente al coordinador individual."
                : "La decisión Approved no fue enviada correctamente al coordinador individual.");

        RecordingProductiveApplicationCoordinator
            unsupportedDecisionCoordinator =
                new();

        MetadataProductiveApplicationBatchCoordinator
            unsupportedDecisionBatchCoordinator =
                new(
                    unsupportedDecisionCoordinator);

        bool unsupportedDecisionWasRejected;

        try
        {
            await unsupportedDecisionBatchCoordinator.ExecuteAsync(
                controlledBatchRequest,
                MetadataPromotionDecision.Pending);

            unsupportedDecisionWasRejected =
                false;
        }
        catch (InvalidOperationException)
        {
            unsupportedDecisionWasRejected =
                unsupportedDecisionCoordinator.PrepareCallCount == 0 &&
                unsupportedDecisionCoordinator.CompleteCallCount == 0;
        }

        messages.Add(
            unsupportedDecisionWasRejected
                ? "La decisión no admitida fue rechazada antes de ejecutar el coordinador individual."
                : "La decisión no admitida no fue rechazada correctamente.");

        bool messagesWereRecorded =
            controlledResult.Messages.Count > 0;

        messages.Add(
            messagesWereRecorded
                ? "Los mensajes controlados fueron registrados."
                : "Los mensajes controlados no fueron registrados.");

        return
            new MetadataProductiveApplicationBatchCoordinatorTestResult
            {
                NullCoordinatorWasRejected =
                    nullCoordinatorWasRejected,

                NullBatchWasRejected =
                    nullBatchWasRejected,

                PreCancellationWasRespected =
                    preCancellationWasRespected,

                ControlledResultWasCreated =
                    controlledResultWasCreated,

                BatchIdentityWasCreated =
                    batchIdentityWasCreated,

                TimesWereRecorded =
                    timesWereRecorded,

                EmptyResultWasNotSuccessful =
                    emptyResultWasNotSuccessful,

                BatchIdentityWasPreserved =
                    batchIdentityWasPreserved,

                InvalidBatchWasRejected =
                    invalidBatchWasRejected,

                ValidRequestsWereInspected =
                    validRequestsWereInspected,

                ProductiveResultsWereCreated =
                    productiveResultsWereCreated,

                IndividualPrepareWasCalledOnce =
                    individualPrepareWasCalledOnce,

                IndividualCompleteWasCalledOnce =
                    individualCompleteWasCalledOnce,

                DeclinedDecisionWasForwarded =
                    declinedDecisionWasForwarded,

                ApprovedResultsWereCreated =
                    approvedResultsWereCreated,

                ApprovedCompleteWasCalledOnce =
                    approvedCompleteWasCalledOnce,

                ApprovedDecisionWasForwarded =
                    approvedDecisionWasForwarded,

                UnsupportedDecisionWasRejected =
                    unsupportedDecisionWasRejected,

                FailFastStoppedAfterSecondPrepare =
                    failFastStoppedAfterSecondPrepare,

                PartialFailureResultWasReturned =
                    partialFailureResultWasReturned,

                PartialFailureWasPreserved =
                    partialFailureWasPreserved,

                FailFastStoppedAfterSecondComplete =
                    failFastStoppedAfterSecondComplete,

                CompleteExceptionWasPreserved =
                    completeExceptionWasPreserved,

                ReturnedPrepareFailureStoppedBatch =
                    returnedPrepareFailureStoppedBatch,

                ReturnedCompleteFailureStoppedBatch =
                    returnedCompleteFailureStoppedBatch,

                RemainingRequestsWereReported =
                    remainingRequestsWereReported,

                MidBatchCancellationWasRespected =
                    midBatchCancellationWasRespected,

                MultiApprovedBatchWasExecuted =
                    multiApprovedBatchWasExecuted,

                MultiApprovedDecisionWasForwarded =
                    multiApprovedDecisionWasForwarded,

                ApprovedFailureStoppedBatch =
                    approvedFailureStoppedBatch,

                ApprovedFailurePreservedPreviousResult =
                    approvedFailurePreservedPreviousResult,

                MessagesWereRecorded =
                    messagesWereRecorded,

                Messages =
                    messages
            };
    }
}