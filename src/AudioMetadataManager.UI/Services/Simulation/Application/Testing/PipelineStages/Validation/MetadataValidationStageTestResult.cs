namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.PipelineStages.Validation;

/// <summary>
/// Resultado de las pruebas estructurales realizadas sobre
/// MetadataValidationStage.
/// </summary>
public sealed class MetadataValidationStageTestResult
{
    public bool ValidResultWasCompleted { get; init; }

    public bool WarningResultWasCompletedWithWarnings
    { get; init; }

    public bool InvalidResultWasFailed { get; init; }

    public bool ValidationResultsWereStored { get; init; }

    public bool StageResultsWereAuditable { get; init; }

    public bool DuplicateExecutionWasRejected { get; init; }

    public bool InjectedValidatorWasUsed { get; init; }

    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    public bool WasSuccessful =>
        ValidResultWasCompleted &&
        WarningResultWasCompletedWithWarnings &&
        InvalidResultWasFailed &&
        ValidationResultsWereStored &&
        StageResultsWereAuditable &&
        DuplicateExecutionWasRejected &&
        InjectedValidatorWasUsed;

    public string Summary =>
        WasSuccessful
            ? "MetadataValidationStage superó todas las " +
              "comprobaciones estructurales."
            : "MetadataValidationStage no superó todas las " +
              "comprobaciones estructurales.";
}