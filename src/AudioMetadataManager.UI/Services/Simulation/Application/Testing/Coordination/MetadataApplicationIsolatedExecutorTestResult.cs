namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Contiene el resultado de la prueba integral controlada del
/// ejecutor aislado.
/// </summary>
public sealed class MetadataApplicationIsolatedExecutorTestResult
{
    public bool IsolationWasPrepared { get; init; }

    public bool PipelineWasSuccessful { get; init; }

    public bool OriginalFileRemainedUnchanged { get; init; }

    public bool WorkingCopyWasModified { get; init; }

    public bool InitialBackupWasPreserved { get; init; }

    public bool CleanupWasSuccessful { get; init; }

    public string RequestedGenre { get; init; } =
        string.Empty;

    public string PersistedGenre { get; init; } =
        string.Empty;

    public string ErrorMessage { get; init; } =
        string.Empty;

    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    public bool GenreWasPersisted =>
        !string.IsNullOrWhiteSpace(
            RequestedGenre) &&
        string.Equals(
            RequestedGenre,
            PersistedGenre,
            StringComparison.OrdinalIgnoreCase);

    public bool WasSuccessful =>
        string.IsNullOrWhiteSpace(
            ErrorMessage) &&
        IsolationWasPrepared &&
        PipelineWasSuccessful &&
        OriginalFileRemainedUnchanged &&
        WorkingCopyWasModified &&
        InitialBackupWasPreserved &&
        CleanupWasSuccessful &&
        GenreWasPersisted;

    public string Summary =>
        WasSuccessful
            ? "El ejecutor aislado superó todas las " +
              "comprobaciones funcionales y de seguridad."
            : "El ejecutor aislado no superó todas las " +
              "comprobaciones funcionales y de seguridad.";
}