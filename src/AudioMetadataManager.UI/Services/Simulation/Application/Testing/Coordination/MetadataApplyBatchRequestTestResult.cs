namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Conserva el resultado de las pruebas estructurales de una
/// solicitud productiva por lote.
/// </summary>
public sealed class MetadataApplyBatchRequestTestResult
{
    public bool EmptyBatchWasRejected { get; init; }

    public bool ValidBatchWasAccepted { get; init; }

    public bool ValidRequestsWereCounted { get; init; }

    public bool ValidChangesWereCounted { get; init; }

    public bool InvalidRequestsWereIgnored { get; init; }

    public bool DuplicatePathsWereDetected { get; init; }

    public bool DuplicateBatchWasRejected { get; init; }

    public bool BatchIdentityWasCreated { get; init; }

    public bool CreationTimeWasRecorded { get; init; }

    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    public bool WasSuccessful =>
        EmptyBatchWasRejected &&
        ValidBatchWasAccepted &&
        ValidRequestsWereCounted &&
        ValidChangesWereCounted &&
        InvalidRequestsWereIgnored &&
        DuplicatePathsWereDetected &&
        DuplicateBatchWasRejected &&
        BatchIdentityWasCreated &&
        CreationTimeWasRecorded;

    public string Summary =>
        WasSuccessful
            ? "La solicitud productiva por lote superó todas las " +
              "comprobaciones estructurales."
            : "La solicitud productiva por lote no superó todas " +
              "las comprobaciones estructurales.";
}