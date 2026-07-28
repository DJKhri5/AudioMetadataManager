namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.PipelineStages.Backup;

/// <summary>
/// Resultado de las pruebas estructurales realizadas sobre
/// MetadataBackupStage.
/// </summary>
public sealed class MetadataBackupStageTestResult
{
    public bool SuccessfulResultWasCompleted { get; init; }

    public bool FailedResultWasFailed { get; init; }

    public bool CancelledResultWasCancelled { get; init; }

    public bool BackupResultsWereStored { get; init; }

    public bool BackupRequestsWereMapped { get; init; }

    public bool CancellationTokenWasForwarded { get; init; }

    public bool StageResultsWereAuditable { get; init; }

    public bool DuplicateExecutionWasRejected { get; init; }

    public bool InjectedEngineWasUsed { get; init; }

    public bool TemporaryFilesWereCleaned { get; init; }

    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    public bool WasSuccessful =>
        SuccessfulResultWasCompleted &&
        FailedResultWasFailed &&
        CancelledResultWasCancelled &&
        BackupResultsWereStored &&
        BackupRequestsWereMapped &&
        CancellationTokenWasForwarded &&
        StageResultsWereAuditable &&
        DuplicateExecutionWasRejected &&
        InjectedEngineWasUsed &&
        TemporaryFilesWereCleaned;

    public string Summary =>
        WasSuccessful
            ? "MetadataBackupStage superó todas las " +
              "comprobaciones estructurales."
            : "MetadataBackupStage no superó todas las " +
              "comprobaciones estructurales.";
}