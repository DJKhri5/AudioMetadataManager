namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Infrastructure;

/// <summary>
/// Contiene las comprobaciones realizadas después de ejecutar
/// una operación sobre una copia aislada.
/// </summary>
public sealed class FileIsolationVerificationResult
{
    /// <summary>
    /// Contexto utilizado durante la prueba.
    /// </summary>
    public FileIsolationContext Context { get; init; } =
        new();

    /// <summary>
    /// Hash SHA-256 del archivo original después de la prueba.
    /// </summary>
    public string OriginalHashAfter { get; init; } =
        string.Empty;

    /// <summary>
    /// Hash SHA-256 de la copia de trabajo después de la prueba.
    /// </summary>
    public string WorkingCopyHashAfter { get; init; } =
        string.Empty;

    /// <summary>
    /// Mensajes generados durante la verificación.
    /// </summary>
    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Indica si el archivo original permaneció intacto.
    /// </summary>
    public bool OriginalFileRemainedUnchanged =>
        !string.IsNullOrWhiteSpace(
            Context.OriginalHashBefore) &&
        string.Equals(
            Context.OriginalHashBefore,
            OriginalHashAfter,
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Indica si la copia de trabajo fue modificada.
    /// </summary>
    public bool WorkingCopyWasModified =>
        !string.IsNullOrWhiteSpace(
            Context.WorkingCopyHashBefore) &&
        !string.IsNullOrWhiteSpace(
            WorkingCopyHashAfter) &&
        !string.Equals(
            Context.WorkingCopyHashBefore,
            WorkingCopyHashAfter,
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Indica si el respaldo conserva el estado previo de la
    /// copia de trabajo.
    /// </summary>
    public bool BackupMatchesInitialWorkingCopy =>
        Context.BackupMatchesInitialWorkingCopy;

    /// <summary>
    /// Indica si se superaron todas las comprobaciones básicas
    /// de aislamiento.
    /// </summary>
    public bool WasSuccessful =>
        OriginalFileRemainedUnchanged &&
        WorkingCopyWasModified &&
        BackupMatchesInitialWorkingCopy;

    /// <summary>
    /// Resumen compacto de la verificación.
    /// </summary>
    public string Summary
    {
        get
        {
            if (WasSuccessful)
            {
                return
                    "La copia de trabajo fue modificada, el " +
                    "respaldo coincide con el estado inicial y " +
                    "el archivo original permaneció intacto.";
            }

            return
                "El entorno aislado no superó todas las " +
                "comprobaciones de seguridad.";
        }
    }
}