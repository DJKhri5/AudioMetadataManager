namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Context;

/// <summary>
/// Contiene el resultado de la prueba estructural del ciclo de
/// vida de MetadataApplicationContext.
/// </summary>
public sealed class MetadataApplicationContextTestResult
{
    public bool ContextStartedActive { get; init; }

    public bool StageWasRegistered { get; init; }

    public bool DuplicateStageWasRejected { get; init; }

    public bool PrematureBuildWasRejected { get; init; }

    public bool ContextWasFinalized { get; init; }

    public bool PipelineResultWasBuilt { get; init; }

    public bool PostCompletionMutationWasRejected { get; init; }

    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    public bool WasSuccessful =>
        ContextStartedActive &&
        StageWasRegistered &&
        DuplicateStageWasRejected &&
        PrematureBuildWasRejected &&
        ContextWasFinalized &&
        PipelineResultWasBuilt &&
        PostCompletionMutationWasRejected;

    public string Summary =>
        WasSuccessful
            ? "MetadataApplicationContext superó todas las " +
              "comprobaciones estructurales."
            : "MetadataApplicationContext no superó todas las " +
              "comprobaciones estructurales.";
}