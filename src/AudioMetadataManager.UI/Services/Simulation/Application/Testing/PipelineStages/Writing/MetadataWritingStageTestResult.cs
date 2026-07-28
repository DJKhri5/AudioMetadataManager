namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.PipelineStages.Writing;

/// <summary>
/// Resultado de las pruebas estructurales realizadas sobre
/// MetadataWritingStage.
/// </summary>
public sealed class MetadataWritingStageTestResult
{
    public bool SuccessfulResultWasCompleted { get; init; }

    public bool NoWritableChangesHadWarnings { get; init; }

    public bool CancelledResultWasCancelled { get; init; }

    public bool FailedResultWasFailed { get; init; }

    public bool MissingBackupWasRejected { get; init; }

    public bool WriterResultsWereStored { get; init; }

    public bool WriteRequestsWereMapped { get; init; }

    public bool CancellationTokenWasForwarded { get; init; }

    public bool StageResultsWereAuditable { get; init; }

    public bool DuplicateExecutionWasRejected { get; init; }

    public bool InjectedEngineWasUsed { get; init; }

    public bool TemporaryFilesWereCleaned { get; init; }

    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    public bool WasSuccessful =>
        SuccessfulResultWasCompleted &&
        NoWritableChangesHadWarnings &&
        CancelledResultWasCancelled &&
        FailedResultWasFailed &&
        MissingBackupWasRejected &&
        WriterResultsWereStored &&
        WriteRequestsWereMapped &&
        CancellationTokenWasForwarded &&
        StageResultsWereAuditable &&
        DuplicateExecutionWasRejected &&
        InjectedEngineWasUsed &&
        TemporaryFilesWereCleaned;

    public string Summary =>
        WasSuccessful
            ? "MetadataWritingStage superó todas las " +
              "comprobaciones estructurales."
            : "MetadataWritingStage no superó todas las " +
              "comprobaciones estructurales.";
}