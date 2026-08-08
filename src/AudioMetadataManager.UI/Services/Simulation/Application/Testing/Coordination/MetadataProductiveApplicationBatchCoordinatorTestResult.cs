namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Conserva el resultado de las comprobaciones estructurales
/// del coordinador productivo por lote.
/// </summary>
public sealed class MetadataProductiveApplicationBatchCoordinatorTestResult
{
    public bool NullCoordinatorWasRejected { get; init; }

    public bool NullBatchWasRejected { get; init; }

    public bool PreCancellationWasRespected { get; init; }

    public bool ControlledResultWasCreated { get; init; }

    public bool BatchIdentityWasCreated { get; init; }

    public bool TimesWereRecorded { get; init; }

    public bool EmptyResultWasNotSuccessful { get; init; }

    public bool BatchIdentityWasPreserved { get; init; }

    public bool InvalidBatchWasRejected { get; init; }

    public bool ValidRequestsWereInspected { get; init; }

    public bool ProductiveResultsWereCreated { get; init; }

    public bool IndividualPrepareWasCalledOnce { get; init; }

    public bool IndividualCompleteWasCalledOnce { get; init; }

    public bool DeclinedDecisionWasForwarded { get; init; }

    public bool ApprovedResultsWereCreated { get; init; }

    public bool ApprovedCompleteWasCalledOnce { get; init; }

    public bool ApprovedDecisionWasForwarded { get; init; }

    public bool UnsupportedDecisionWasRejected { get; init; }

    /// <summary>
    /// Indica si el lote se detuvo inmediatamente después
    /// del fallo simulado en la segunda preparación.
    /// </summary>
    public bool FailFastStoppedAfterSecondPrepare { get; init; }

    /// <summary>
    /// Indica si el fallo parcial produjo un resultado batch
    /// utilizable en lugar de propagar la excepción.
    /// </summary>
    public bool PartialFailureResultWasReturned { get; init; }

    /// <summary>
    /// Indica si el resultado batch conservó tanto la ejecución
    /// completada como el fallo que detuvo el lote.
    /// </summary>
    public bool PartialFailureWasPreserved { get; init; }

    /// <summary>
    /// Indica si una excepción durante la segunda finalización
    /// detuvo inmediatamente el lote.
    /// </summary>
    public bool FailFastStoppedAfterSecondComplete { get; init; }

    /// <summary>
    /// Indica si la excepción producida durante CompleteAsync fue
    /// conservada dentro del resultado batch.
    /// </summary>
    public bool CompleteExceptionWasPreserved { get; init; }

    /// <summary>
    /// Indica si un fallo devuelto normalmente por PrepareAsync fue
    /// detectado y detuvo el lote.
    /// </summary>
    public bool ReturnedPrepareFailureStoppedBatch { get; init; }

    /// <summary>
    /// Indica si un fallo devuelto normalmente por CompleteAsync fue
    /// detectado y detuvo el lote.
    /// </summary>
    public bool ReturnedCompleteFailureStoppedBatch { get; init; }

    /// <summary>
    /// Indica si las solicitudes restantes sin ejecutar fueron
    /// registradas de forma auditable.
    /// </summary>
    public bool RemainingRequestsWereReported { get; init; }

    /// <summary>
    /// Indica si una cancelación solicitada después de completar
    /// la primera solicitud detuvo inmediatamente el resto del lote.
    /// </summary>
    public bool MidBatchCancellationWasRespected { get; init; }

    /// <summary>
    /// Indica si un lote Approved de varias solicitudes procesó
    /// todas las solicitudes esperadas.
    /// </summary>
    public bool MultiApprovedBatchWasExecuted { get; init; }

    /// <summary>
    /// Indica si la decisión Approved fue reenviada a todas las
    /// solicitudes individuales del lote.
    /// </summary>
    public bool MultiApprovedDecisionWasForwarded { get; init; }

    /// <summary>
    /// Indica si un fallo durante una finalización Approved detuvo
    /// correctamente las solicitudes posteriores.
    /// </summary>
    public bool ApprovedFailureStoppedBatch { get; init; }

    /// <summary>
    /// Indica si un fallo posterior conservó el resultado Approved
    /// que ya había terminado correctamente.
    /// </summary>
    public bool ApprovedFailurePreservedPreviousResult { get; init; }

    public bool MessagesWereRecorded { get; init; }

    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    public bool WasSuccessful =>
        NullCoordinatorWasRejected &&
        NullBatchWasRejected &&
        PreCancellationWasRespected &&
        ControlledResultWasCreated &&
        BatchIdentityWasCreated &&
        TimesWereRecorded &&
        EmptyResultWasNotSuccessful &&
        BatchIdentityWasPreserved &&
        InvalidBatchWasRejected &&
        ValidRequestsWereInspected &&
        ProductiveResultsWereCreated &&
        IndividualPrepareWasCalledOnce &&
        IndividualCompleteWasCalledOnce &&
        DeclinedDecisionWasForwarded &&
        ApprovedResultsWereCreated &&
        ApprovedCompleteWasCalledOnce &&
        ApprovedDecisionWasForwarded &&
        UnsupportedDecisionWasRejected &&
        FailFastStoppedAfterSecondPrepare &&
        PartialFailureResultWasReturned &&
        PartialFailureWasPreserved &&
        FailFastStoppedAfterSecondComplete &&
        CompleteExceptionWasPreserved &&
        ReturnedPrepareFailureStoppedBatch &&
        ReturnedCompleteFailureStoppedBatch &&
        RemainingRequestsWereReported &&
        MidBatchCancellationWasRespected &&
        MultiApprovedBatchWasExecuted &&
        MultiApprovedDecisionWasForwarded &&
        ApprovedFailureStoppedBatch &&
        ApprovedFailurePreservedPreviousResult &&
        MessagesWereRecorded;

    public string Summary =>
        WasSuccessful
            ? "El coordinador productivo por lote superó todas " +
              "las comprobaciones estructurales iniciales."
            : "El coordinador productivo por lote no superó todas " +
              "las comprobaciones estructurales iniciales.";
}