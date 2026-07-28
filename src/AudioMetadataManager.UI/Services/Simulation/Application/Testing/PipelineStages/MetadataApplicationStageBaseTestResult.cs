namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.PipelineStages;

/// <summary>
/// Resultado de las pruebas estructurales aplicadas a
/// MetadataApplicationStageBase.
/// </summary>
public sealed class MetadataApplicationStageBaseTestResult
{
    public bool SuccessfulStageWasRegistered { get; init; }

    public bool FailedStageWasRegistered { get; init; }

    public bool DuplicateExecutionWasRejected { get; init; }

    public bool CancelledStageWasRegistered { get; init; }

    public bool ExecutionMetadataWasPreserved { get; init; }

    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    public bool WasSuccessful =>
        SuccessfulStageWasRegistered &&
        FailedStageWasRegistered &&
        DuplicateExecutionWasRejected &&
        CancelledStageWasRegistered &&
        ExecutionMetadataWasPreserved;

    public string Summary =>
        WasSuccessful
            ? "MetadataApplicationStageBase superó todas las " +
              "comprobaciones estructurales."
            : "MetadataApplicationStageBase no superó todas las " +
              "comprobaciones estructurales.";
}