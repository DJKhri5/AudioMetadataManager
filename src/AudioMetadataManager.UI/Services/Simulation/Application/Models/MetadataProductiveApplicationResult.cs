using AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

/// <summary>
/// Consolida el resultado completo de una aplicación productiva
/// individual de metadatos.
///
/// El flujo comprende la ejecución aislada, la segunda
/// confirmación, la eventual promoción hacia el archivo original
/// y la limpieza final del entorno temporal.
/// </summary>
public sealed class MetadataProductiveApplicationResult
{
    /// <summary>
    /// Resultado de la ejecución inicial realizada sobre una
    /// copia temporal aislada.
    /// </summary>
    public MetadataApplicationIsolatedExecutionResult?
        IsolatedExecutionResult
    { get; init; }

    /// <summary>
    /// Decisión registrada durante la segunda confirmación.
    /// </summary>
    public MetadataPromotionDecision PromotionDecision
    { get; init; } =
        MetadataPromotionDecision.NotRequested;

    /// <summary>
    /// Resultado de la promoción de la copia verificada hacia el
    /// archivo original.
    ///
    /// Será nulo cuando la promoción no haya sido solicitada,
    /// esté pendiente, haya sido rechazada o no esté disponible.
    /// </summary>
    public MetadataApplicationPromotionResult?
        PromotionResult
    { get; init; }

    /// <summary>
    /// Indica si se intentó limpiar el entorno aislado después de
    /// finalizar la decisión de promoción.
    /// </summary>
    public bool FinalCleanupWasAttempted { get; init; }

    /// <summary>
    /// Indica si el entorno aislado fue eliminado correctamente
    /// al finalizar el flujo.
    /// </summary>
    public bool FinalCleanupWasSuccessful { get; init; }

    /// <summary>
    /// Mensaje de error capturado fuera de los resultados
    /// específicos de aislamiento o promoción.
    /// </summary>
    public string ErrorMessage { get; init; } =
        string.Empty;

    /// <summary>
    /// Mensajes auditables generados durante el flujo completo.
    /// </summary>
    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Indica si la ejecución aislada produjo una copia segura y
    /// promovible.
    /// </summary>
    public bool VerifiedCopyWasPrepared =>
        IsolatedExecutionResult?.WasSuccessful == true &&
        IsolatedExecutionResult.EnvironmentWasPreserved &&
        IsolatedExecutionResult.IsolationContext is not null;

    /// <summary>
    /// Indica si el usuario rechazó voluntariamente la promoción.
    ///
    /// Este estado no representa un error técnico.
    /// </summary>
    public bool PromotionWasDeclined =>
        PromotionDecision ==
        MetadataPromotionDecision.Declined;

    /// <summary>
    /// Indica si el usuario aprobó la promoción.
    /// </summary>
    public bool PromotionWasApproved =>
        PromotionDecision ==
        MetadataPromotionDecision.Approved;

    /// <summary>
    /// Indica si la promoción aprobada terminó correctamente.
    /// </summary>
    public bool PromotionWasSuccessful =>
        PromotionWasApproved &&
        PromotionResult?.WasSuccessful == true;

    /// <summary>
    /// Indica si una promoción fallida fue revertida
    /// correctamente.
    /// </summary>
    public bool PromotionWasSafelyRolledBack =>
        PromotionWasApproved &&
        PromotionResult?.WasSafelyRolledBack == true;

    /// <summary>
    /// Indica si el archivo original terminó en un estado seguro.
    ///
    /// Cuando la promoción fue rechazada, el original continúa
    /// intacto porque nunca se entregó al servicio de promoción.
    /// </summary>
    public bool OriginalEndedInSafeState =>
        PromotionWasDeclined ||
        PromotionWasSuccessful ||
        PromotionWasSafelyRolledBack;

    /// <summary>
    /// Indica si el flujo terminó correctamente después de una
    /// promoción aprobada.
    /// </summary>
    public bool WasSuccessfullyPromoted =>
        string.IsNullOrWhiteSpace(
            ErrorMessage) &&
        VerifiedCopyWasPrepared &&
        PromotionWasSuccessful &&
        FinalCleanupWasAttempted &&
        FinalCleanupWasSuccessful;

    /// <summary>
    /// Indica si el flujo terminó correctamente después de que el
    /// usuario rechazara la promoción.
    /// </summary>
    public bool WasSafelyDeclined =>
        string.IsNullOrWhiteSpace(
            ErrorMessage) &&
        VerifiedCopyWasPrepared &&
        PromotionWasDeclined &&
        FinalCleanupWasAttempted &&
        FinalCleanupWasSuccessful;

    /// <summary>
    /// Indica si el flujo terminó de forma controlada y segura.
    ///
    /// Una promoción revertida correctamente deja el original en
    /// estado seguro, aunque la aplicación solicitada no se
    /// considere exitosa.
    /// </summary>
    public bool EndedInControlledState =>
        WasSuccessfullyPromoted ||
        WasSafelyDeclined ||
        (
            VerifiedCopyWasPrepared &&
            PromotionWasSafelyRolledBack &&
            FinalCleanupWasAttempted &&
            FinalCleanupWasSuccessful
        );

    /// <summary>
    /// Resumen compacto del resultado productivo.
    /// </summary>
    public string Summary
    {
        get
        {
            if (WasSuccessfullyPromoted)
            {
                return
                    "La copia verificada fue promovida " +
                    "correctamente al archivo original y el " +
                    "entorno temporal fue eliminado.";
            }

            if (WasSafelyDeclined)
            {
                return
                    "El usuario rechazó la promoción, el archivo " +
                    "original permaneció intacto y el entorno " +
                    "temporal fue eliminado.";
            }

            if (PromotionWasSafelyRolledBack &&
                FinalCleanupWasSuccessful)
            {
                return
                    "La promoción no pudo completarse, pero el " +
                    "archivo original fue restaurado y el entorno " +
                    "temporal fue eliminado.";
            }

            if (!VerifiedCopyWasPrepared)
            {
                return
                    "La ejecución aislada no produjo una copia " +
                    "verificada disponible para promoción.";
            }

            if (!string.IsNullOrWhiteSpace(
                    ErrorMessage))
            {
                return
                    "La aplicación productiva terminó con un " +
                    $"error: {ErrorMessage}";
            }

            return
                "La aplicación productiva no terminó en un " +
                "estado completamente verificado.";
        }
    }
}