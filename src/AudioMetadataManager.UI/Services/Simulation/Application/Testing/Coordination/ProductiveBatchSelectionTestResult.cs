namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Consolida las comprobaciones estructurales de la
/// selección productiva multiarchivo.
/// </summary>
public sealed class ProductiveBatchSelectionTestResult
{
    public bool EmptySelectionWasCreated { get; init; }

    public bool ApprovedPlanWasAdded { get; init; }

    public bool DuplicatePathWasReplaced { get; init; }

    public bool SecondPlanWasAdded { get; init; }

    public bool CountsWereUpdated { get; init; }

    public bool ItemWasRemoved { get; init; }

    public bool PlanWithoutApprovalRemovedExistingItem
    {
        get;
        init;
    }

    public bool BatchRequestWasCreated { get; init; }

    public bool BatchRequestWasStructurallyValid { get; init; }

    public bool BatchCountsWerePreserved { get; init; }

    public bool SelectionWasCleared { get; init; }

    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    public bool WasSuccessful =>
        EmptySelectionWasCreated &&
        ApprovedPlanWasAdded &&
        DuplicatePathWasReplaced &&
        SecondPlanWasAdded &&
        CountsWereUpdated &&
        ItemWasRemoved &&
        PlanWithoutApprovalRemovedExistingItem &&
        BatchRequestWasCreated &&
        BatchRequestWasStructurallyValid &&
        BatchCountsWerePreserved &&
        SelectionWasCleared;

    public string Summary =>
        WasSuccessful
            ? "La selección productiva por lote superó " +
              "todas las comprobaciones estructurales."
            : "La selección productiva por lote presentó " +
              "una o más comprobaciones fallidas.";
}