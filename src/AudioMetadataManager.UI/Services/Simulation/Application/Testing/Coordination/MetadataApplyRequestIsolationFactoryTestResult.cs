namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Contiene el resultado de las pruebas controladas de la
/// fábrica de solicitudes aisladas.
/// </summary>
public sealed class MetadataApplyRequestIsolationFactoryTestResult
{
    public bool NullRequestWasRejected { get; init; }

    public bool EmptyPathWasRejected { get; init; }

    public bool IdentifiersWerePreserved { get; init; }

    public bool CreationTimeWasPreserved { get; init; }

    public bool ChangesWerePreserved { get; init; }

    public bool RequirementsWerePreserved { get; init; }

    public bool WorkingCopyPathWasApplied { get; init; }

    public bool WorkingCopyFileNameWasApplied { get; init; }

    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    public bool WasSuccessful =>
        NullRequestWasRejected &&
        EmptyPathWasRejected &&
        IdentifiersWerePreserved &&
        CreationTimeWasPreserved &&
        ChangesWerePreserved &&
        RequirementsWerePreserved &&
        WorkingCopyPathWasApplied &&
        WorkingCopyFileNameWasApplied;

    public string Summary =>
        WasSuccessful
            ? "La fábrica de solicitudes aisladas superó todas " +
              "las comprobaciones controladas."
            : "La fábrica de solicitudes aisladas no superó " +
              "todas las comprobaciones controladas.";
}