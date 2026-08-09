namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

public sealed class
    MetadataProductiveTwoPhaseBatchPreparationTestResult
{
    public bool NullCoordinatorWasRejected { get; init; }

    public bool NullBatchWasRejected { get; init; }

    public bool InvalidBatchWasRejected { get; init; }

    public bool AllRequestsWerePrepared { get; init; }

    public bool PreparationsWerePending { get; init; }

    public bool BatchWasReadyForDecision { get; init; }

    public bool PreparationFailureStoppedBatch { get; init; }

    public bool PendingPreparationsWereCleanedUp { get; init; }

    public bool FailedBatchWasNotReadyForDecision { get; init; }

    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    public bool WasSuccessful =>
        NullCoordinatorWasRejected &&
        NullBatchWasRejected &&
        InvalidBatchWasRejected &&
        AllRequestsWerePrepared &&
        PreparationsWerePending &&
        BatchWasReadyForDecision &&
        PreparationFailureStoppedBatch &&
        PendingPreparationsWereCleanedUp &&
        FailedBatchWasNotReadyForDecision;

    public string Summary =>
        WasSuccessful
            ? "La preparación productiva batch en dos fases " +
              "superó todas las comprobaciones."
            : "La preparación productiva batch en dos fases " +
              "presentó una o más comprobaciones fallidas.";
}