namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.PipelineComposition;

/// <summary>
/// Resultado de las pruebas estructurales realizadas sobre
/// MetadataApplicationPipelineFactory.
/// </summary>
public sealed class MetadataApplicationPipelineFactoryTestResult
{
    public bool ExactlyFiveStagesWereRegistered
    { get; init; }

    public bool ConcreteStageTypesWereCorrect
    { get; init; }

    public bool StageIdentitiesWereCorrect
    { get; init; }

    public bool ExecutionOrdersWereCorrect
    { get; init; }

    public bool FinalStageOrderWasCorrect
    { get; init; }

    public bool DefaultOptionsWereSafe
    { get; init; }

    public bool SuccessiveCreationsWereIndependent
    { get; init; }

    public bool NullOptionsWereRejected
    { get; init; }

    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    public bool WasSuccessful =>
        ExactlyFiveStagesWereRegistered &&
        ConcreteStageTypesWereCorrect &&
        StageIdentitiesWereCorrect &&
        ExecutionOrdersWereCorrect &&
        FinalStageOrderWasCorrect &&
        DefaultOptionsWereSafe &&
        SuccessiveCreationsWereIndependent &&
        NullOptionsWereRejected;

    public string Summary =>
        WasSuccessful
            ? "MetadataApplicationPipelineFactory superó todas las " +
              "comprobaciones estructurales."
            : "MetadataApplicationPipelineFactory no superó todas las " +
              "comprobaciones estructurales.";
}