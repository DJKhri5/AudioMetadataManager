using AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Infrastructure;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Consolida el resultado de una aplicación de metadatos
/// ejecutada exclusivamente sobre una copia temporal aislada.
/// </summary>
public sealed class MetadataApplicationIsolatedExecutionResult
{
    /// <summary>
    /// Entorno aislado preparado antes de ejecutar el pipeline.
    /// </summary>
    public FileIsolationContext? IsolationContext
    { get; init; }

    /// <summary>
    /// Resultado completo devuelto por el coordinador.
    /// </summary>
    public MetadataApplicationPipelineResult? PipelineResult
    { get; init; }

    /// <summary>
    /// Verificación de seguridad realizada después de ejecutar
    /// el pipeline sobre la copia de trabajo.
    /// </summary>
    public FileIsolationVerificationResult?
        IsolationVerification
    { get; init; }

    /// <summary>
    /// Indica si el entorno temporal fue eliminado después de
    /// completar todas las verificaciones.
    /// </summary>
    public bool CleanupWasSuccessful { get; init; }

    /// <summary>
    /// Indica si el entorno temporal se conservó intencionalmente
    /// después de una ejecución satisfactoria.
    /// </summary>
    public bool EnvironmentWasPreserved { get; init; }

    /// <summary>
    /// Mensaje de error capturado durante la preparación,
    /// ejecución, verificación o limpieza.
    /// </summary>
    public string ErrorMessage { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si el ciclo de vida del entorno terminó de forma
    /// controlada, ya sea mediante limpieza o conservación
    /// intencional.
    /// </summary>
    public bool EnvironmentLifecycleWasHandled =>
        CleanupWasSuccessful ||
        EnvironmentWasPreserved;

    /// <summary>
    /// Indica si el entorno aislado fue creado correctamente.
    /// </summary>
    public bool IsolationWasPrepared =>
        IsolationContext?.IsCreated == true;

    /// <summary>
    /// Indica si el pipeline terminó correctamente sobre la
    /// copia temporal.
    /// </summary>
    public bool PipelineWasSuccessful =>
        PipelineResult?.WasSuccessful == true;

    /// <summary>
    /// Indica si el archivo original permaneció intacto.
    /// </summary>
    public bool OriginalFileRemainedUnchanged =>
        IsolationVerification?
            .OriginalFileRemainedUnchanged == true;

    /// <summary>
    /// Indica si la copia temporal recibió cambios reales.
    /// </summary>
    public bool WorkingCopyWasModified =>
        IsolationVerification?
            .WorkingCopyWasModified == true;

    /// <summary>
    /// Indica si el respaldo inicial de la copia fue preservado.
    /// </summary>
    public bool InitialBackupWasPreserved =>
        IsolationVerification?
            .BackupMatchesInitialWorkingCopy == true;

    /// <summary>
    /// Indica si la ejecución aislada superó todas las
    /// comprobaciones funcionales y de seguridad.
    /// </summary>
    public bool WasSuccessful =>
        string.IsNullOrWhiteSpace(
            ErrorMessage) &&
        IsolationWasPrepared &&
        PipelineWasSuccessful &&
        OriginalFileRemainedUnchanged &&
        WorkingCopyWasModified &&
        InitialBackupWasPreserved &&
        EnvironmentLifecycleWasHandled;

    /// <summary>
    /// Resumen compacto del resultado.
    /// </summary>
    public string Summary
    {
        get
        {
            if (WasSuccessful)
            {
                if (EnvironmentWasPreserved)
                {
                    return
                        "La aplicación aislada terminó correctamente, " +
                        "el archivo original permaneció intacto y la " +
                        "copia verificada fue conservada para una " +
                        "operación posterior.";
                }

                return
                    "La aplicación aislada terminó correctamente, " +
                    "la copia temporal fue modificada, el respaldo " +
                    "fue preservado y el archivo original permaneció " +
                    "intacto.";
            }

            if (!string.IsNullOrWhiteSpace(
                    ErrorMessage))
            {
                return
                    "La aplicación aislada terminó con un error: " +
                    ErrorMessage;
            }

            return
                "La aplicación aislada no superó todas las " +
                "comprobaciones funcionales y de seguridad.";
        }
    }
}