namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Contiene el resultado de la prueba controlada de conservación
/// de una ejecución aislada satisfactoria.
/// </summary>
public sealed class MetadataApplicationPreservedExecutionTestResult
{
    public bool ExecutionWasSuccessful { get; init; }

    public bool EnvironmentWasPreserved { get; init; }

    public bool CleanupWasDeferred { get; init; }

    public bool WorkingCopyStillExisted { get; init; }

    public bool InitialBackupStillExisted { get; init; }

    public bool OriginalFileRemainedUnchanged { get; init; }

    public bool WorkingCopyWasModified { get; init; }

    public bool ManualCleanupWasSuccessful { get; init; }

    public bool TemporaryDirectoryWasRemoved { get; init; }

    public string ErrorMessage { get; init; } =
        string.Empty;

    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    public bool WasSuccessful =>
        string.IsNullOrWhiteSpace(
            ErrorMessage) &&
        ExecutionWasSuccessful &&
        EnvironmentWasPreserved &&
        CleanupWasDeferred &&
        WorkingCopyStillExisted &&
        InitialBackupStillExisted &&
        OriginalFileRemainedUnchanged &&
        WorkingCopyWasModified &&
        ManualCleanupWasSuccessful &&
        TemporaryDirectoryWasRemoved;

    public string Summary =>
        WasSuccessful
            ? "La copia verificada fue conservada y eliminada " +
              "posteriormente de forma controlada."
            : "La prueba de conservación controlada no superó " +
              "todas las comprobaciones.";
}