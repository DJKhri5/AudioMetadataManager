namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.PipelineStages.Verification;

/// <summary>
/// Resultado de las pruebas estructurales realizadas sobre
/// MetadataVerificationStage.
/// </summary>
public sealed class MetadataVerificationStageTestResult
{
    public bool SuccessfulVerificationWasCompleted
    { get; init; }

    public bool FailedVerificationWasFailed
    { get; init; }

    public bool MissingWriteResultWasRejected
    { get; init; }

    public bool NoWritableChangesWasSkipped
    { get; init; }

    public bool CancelledWriteWasCancelled
    { get; init; }

    public bool FailedWriteWasRejected
    { get; init; }

    public bool VerificationResultWasStored
    { get; init; }

    public bool VerificationInputsWereMapped
    { get; init; }

    public bool PictureCountBeforeWasForwarded
    { get; init; }

    public bool CancellationWasHonored
    { get; init; }

    public bool StageResultsWereAuditable
    { get; init; }

    public bool DuplicateExecutionWasRejected
    { get; init; }

    public bool InjectedEngineWasUsed
    { get; init; }

    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    public bool WasSuccessful =>
        SuccessfulVerificationWasCompleted &&
        FailedVerificationWasFailed &&
        MissingWriteResultWasRejected &&
        NoWritableChangesWasSkipped &&
        CancelledWriteWasCancelled &&
        FailedWriteWasRejected &&
        VerificationResultWasStored &&
        VerificationInputsWereMapped &&
        PictureCountBeforeWasForwarded &&
        CancellationWasHonored &&
        StageResultsWereAuditable &&
        DuplicateExecutionWasRejected &&
        InjectedEngineWasUsed;

    public string Summary =>
        WasSuccessful
            ? "MetadataVerificationStage superó todas las " +
              "comprobaciones estructurales."
            : "MetadataVerificationStage no superó todas las " +
              "comprobaciones estructurales.";
}