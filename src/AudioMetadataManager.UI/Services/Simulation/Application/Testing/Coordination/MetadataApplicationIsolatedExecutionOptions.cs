namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Configura el ciclo de vida del entorno utilizado durante una
/// ejecución coordinada sobre una copia temporal aislada.
/// </summary>
public sealed class MetadataApplicationIsolatedExecutionOptions
{
    /// <summary>
    /// Indica si el entorno temporal debe eliminarse
    /// automáticamente después de completar la ejecución y sus
    /// verificaciones.
    ///
    /// El valor predeterminado mantiene el comportamiento seguro
    /// utilizado por los diagnósticos actuales.
    /// </summary>
    public bool CleanupAfterExecution { get; init; } =
        true;

    /// <summary>
    /// Indica si una ejecución satisfactoria puede conservar la
    /// copia temporal para una operación posterior de promoción
    /// controlada.
    ///
    /// Esta opción no aplica cambios al archivo original.
    /// </summary>
    public bool PreserveVerifiedWorkingCopy { get; init; }

    /// <summary>
    /// Indica si el entorno debe limpiarse cuando la ejecución
    /// falla o es cancelada, incluso cuando se solicitó conservar
    /// una copia satisfactoria.
    /// </summary>
    public bool CleanupAfterFailure { get; init; } =
        true;

    /// <summary>
    /// Configuración predeterminada para pruebas y diagnósticos.
    ///
    /// Siempre elimina el entorno temporal al finalizar.
    /// </summary>
    public static MetadataApplicationIsolatedExecutionOptions
        SafeCleanupDefault =>
            new()
            {
                CleanupAfterExecution =
                    true,

                PreserveVerifiedWorkingCopy =
                    false,

                CleanupAfterFailure =
                    true
            };

    /// <summary>
    /// Configuración destinada a preparar una copia verificada
    /// para una segunda confirmación.
    ///
    /// La copia se conserva únicamente cuando la ejecución
    /// aislada termina correctamente.
    /// </summary>
    public static MetadataApplicationIsolatedExecutionOptions
        PreserveSuccessfulExecution =>
            new()
            {
                CleanupAfterExecution =
                    false,

                PreserveVerifiedWorkingCopy =
                    true,

                CleanupAfterFailure =
                    true
            };
}