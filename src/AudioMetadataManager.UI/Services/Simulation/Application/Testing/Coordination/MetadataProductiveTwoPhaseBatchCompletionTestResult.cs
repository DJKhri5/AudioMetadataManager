namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

public sealed class
    MetadataProductiveTwoPhaseBatchCompletionTestResult
{
    public bool NullPreparationWasRejected { get; init; }

    public bool UnsupportedDecisionWasRejected { get; init; }

    public bool InvalidPreparationWasRejected { get; init; }

    public bool DeclinedCompletedAllPreparations { get; init; }

    public bool DeclinedWasForwardedToAll { get; init; }

    public bool DeclinedBatchWasSuccessful { get; init; }

    public bool ApprovedCompletedAllPreparations { get; init; }

    public bool ApprovedWasForwardedToAll { get; init; }

    public bool ApprovedBatchWasSuccessful { get; init; }

    public bool ApprovedFailureStoppedFurtherPromotion { get; init; }

    public bool RemainingPreparationsWereDeclined { get; init; }

    public bool FailedApprovedBatchWasNotSuccessful { get; init; }

    public bool CancellationCleanedPendingPreparations { get; init; }

    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    public bool WasSuccessful =>
        NullPreparationWasRejected &&
        UnsupportedDecisionWasRejected &&
        InvalidPreparationWasRejected &&
        DeclinedCompletedAllPreparations &&
        DeclinedWasForwardedToAll &&
        DeclinedBatchWasSuccessful &&
        ApprovedCompletedAllPreparations &&
        ApprovedWasForwardedToAll &&
        ApprovedBatchWasSuccessful &&
        ApprovedFailureStoppedFurtherPromotion &&
        RemainingPreparationsWereDeclined &&
        FailedApprovedBatchWasNotSuccessful &&
        CancellationCleanedPendingPreparations;

    public string Summary =>
        WasSuccessful
            ? "La finalización productiva batch en dos fases " +
              "superó todas las comprobaciones."
            : "La finalización productiva batch en dos fases " +
              "presentó una o más comprobaciones fallidas.";
}