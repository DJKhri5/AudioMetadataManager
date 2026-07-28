namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.PipelineExecution;

/// <summary>
/// Resultado de las pruebas estructurales realizadas sobre
/// MetadataApplicationPipelineExecutor.
/// </summary>
public sealed class MetadataApplicationPipelineExecutorTestResult
{
    public bool StagesWereOrderedCorrectly { get; init; }

    public bool CompleteExecutionSucceeded { get; init; }

    public bool BlockingFailureStoppedExecution { get; init; }

    public bool DuplicateIdentityWasRejected { get; init; }

    public bool DuplicateOrderWasRejectedWhenConfigured
    { get; init; }

    public bool AutomaticCompletionWorked { get; init; }

    public bool ContextWasPreserved { get; init; }

    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    public bool WasSuccessful =>
        StagesWereOrderedCorrectly &&
        CompleteExecutionSucceeded &&
        BlockingFailureStoppedExecution &&
        DuplicateIdentityWasRejected &&
        DuplicateOrderWasRejectedWhenConfigured &&
        AutomaticCompletionWorked &&
        ContextWasPreserved;

    public string Summary =>
        WasSuccessful
            ? "MetadataApplicationPipelineExecutor superó todas " +
              "las comprobaciones estructurales."
            : "MetadataApplicationPipelineExecutor no superó " +
              "todas las comprobaciones estructurales.";
}