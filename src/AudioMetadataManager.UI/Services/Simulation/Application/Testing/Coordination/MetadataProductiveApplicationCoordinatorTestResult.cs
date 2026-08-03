namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Contiene el resultado de las pruebas controladas del
/// coordinador productivo individual.
/// </summary>
public sealed class
    MetadataProductiveApplicationCoordinatorTestResult
{
    /// <summary>
    /// Indica si una solicitud nula fue rechazada durante la
    /// preparación.
    /// </summary>
    public bool NullRequestWasRejected { get; init; }

    /// <summary>
    /// Indica si la preparación produjo una copia verificada y
    /// conservada.
    /// </summary>
    public bool VerifiedCopyWasPrepared { get; init; }

    /// <summary>
    /// Indica si la preparación quedó pendiente de una decisión
    /// de promoción.
    /// </summary>
    public bool PromotionDecisionWasPending { get; init; }

    /// <summary>
    /// Indica si el archivo original permaneció intacto durante
    /// la preparación.
    /// </summary>
    public bool OriginalRemainedUnchangedDuringPreparation
    { get; init; }

    /// <summary>
    /// Indica si una finalización con decisión Declined fue
    /// procesada correctamente.
    /// </summary>
    public bool DeclinedDecisionWasHandled { get; init; }

    /// <summary>
    /// Indica si el rechazo evitó entregar el archivo original al
    /// servicio de promoción.
    /// </summary>
    public bool DeclinedDecisionSkippedPromotion { get; init; }

    /// <summary>
    /// Indica si el rechazo terminó con el archivo original en un
    /// estado seguro.
    /// </summary>
    public bool DeclinedOriginalEndedInSafeState { get; init; }

    /// <summary>
    /// Indica si el entorno aislado fue eliminado después del
    /// rechazo.
    /// </summary>
    public bool DeclinedEnvironmentWasCleaned { get; init; }

    /// <summary>
    /// Indica si el resultado del rechazo fue clasificado como
    /// una finalización segura y voluntaria.
    /// </summary>
    public bool DeclinedResultWasSuccessful { get; init; }

    /// <summary>
    /// Indica si una decisión no permitida fue rechazada.
    /// </summary>
    public bool InvalidDecisionWasRejected { get; init; }

    /// <summary>
    /// Indica si una preparación ya finalizada no pudo reutilizarse
    /// para una segunda finalización.
    /// </summary>
    public bool ReusedPreparationWasRejected { get; init; }

    /// <summary>
    /// Indica si los archivos y carpetas temporales utilizados por
    /// la prueba fueron eliminados.
    /// </summary>
    public bool TemporaryEnvironmentWasRemoved { get; init; }

    /// <summary>
    /// Mensaje de error inesperado producido durante la prueba.
    /// </summary>
    public string ErrorMessage { get; init; } =
        string.Empty;

    /// <summary>
    /// Mensajes auditables producidos durante las comprobaciones.
    /// </summary>
    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Indica si todas las comprobaciones fueron superadas.
    /// </summary>
    public bool WasSuccessful =>
        string.IsNullOrWhiteSpace(
            ErrorMessage) &&
        NullRequestWasRejected &&
        VerifiedCopyWasPrepared &&
        PromotionDecisionWasPending &&
        OriginalRemainedUnchangedDuringPreparation &&
        DeclinedDecisionWasHandled &&
        DeclinedDecisionSkippedPromotion &&
        DeclinedOriginalEndedInSafeState &&
        DeclinedEnvironmentWasCleaned &&
        DeclinedResultWasSuccessful &&
        InvalidDecisionWasRejected &&
        ReusedPreparationWasRejected &&
        TemporaryEnvironmentWasRemoved;

    /// <summary>
    /// Resumen compacto de la prueba.
    /// </summary>
    public string Summary =>
        WasSuccessful
            ? "El coordinador productivo individual superó " +
              "todas las comprobaciones controladas."
            : "El coordinador productivo individual no superó " +
              "todas las comprobaciones controladas.";
}