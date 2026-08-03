namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Contiene el resultado de la prueba controlada del camino
/// Approved del coordinador productivo individual.
/// </summary>
public sealed class
    MetadataProductiveApplicationApprovedTestResult
{
    /// <summary>
    /// Indica si el entorno temporal general fue preparado.
    /// </summary>
    public bool TestEnvironmentWasPrepared { get; init; }

    /// <summary>
    /// Indica si la preparación produjo una copia verificada y
    /// conservada.
    /// </summary>
    public bool VerifiedCopyWasPrepared { get; init; }

    /// <summary>
    /// Indica si la preparación quedó pendiente de una decisión.
    /// </summary>
    public bool PromotionDecisionWasPending { get; init; }

    /// <summary>
    /// Indica si el destino temporal permaneció intacto durante
    /// la preparación aislada.
    /// </summary>
    public bool DestinationRemainedUnchangedDuringPreparation
    { get; init; }

    /// <summary>
    /// Indica si la decisión Approved fue registrada.
    /// </summary>
    public bool ApprovedDecisionWasHandled { get; init; }

    /// <summary>
    /// Indica si la promoción fue ejecutada correctamente.
    /// </summary>
    public bool PromotionWasSuccessful { get; init; }

    /// <summary>
    /// Indica si el respaldo productivo fue creado.
    /// </summary>
    public bool ProductiveBackupWasCreated { get; init; }

    /// <summary>
    /// Indica si el respaldo productivo fue verificado.
    /// </summary>
    public bool ProductiveBackupWasVerified { get; init; }

    /// <summary>
    /// Indica si el reemplazo controlado fue ejecutado.
    /// </summary>
    public bool ReplacementWasExecuted { get; init; }

    /// <summary>
    /// Indica si el destino promovido coincide con la copia
    /// verificada preparada por el pipeline.
    /// </summary>
    public bool PromotedDestinationWasVerified { get; init; }

    /// <summary>
    /// Indica si el destino temporal contiene el género solicitado
    /// después de la promoción.
    /// </summary>
    public bool RequestedGenreWasPersisted { get; init; }

    /// <summary>
    /// Indica si no fue necesario ejecutar una reversión.
    /// </summary>
    public bool RollbackWasNotRequired { get; init; }

    /// <summary>
    /// Indica si el archivo de referencia usado como fuente
    /// permaneció intacto.
    /// </summary>
    public bool ReferenceOriginalRemainedUnchanged { get; init; }

    /// <summary>
    /// Indica si el destino terminó en un estado seguro.
    /// </summary>
    public bool DestinationEndedInSafeState { get; init; }

    /// <summary>
    /// Indica si la limpieza final del entorno aislado fue
    /// ejecutada.
    /// </summary>
    public bool FinalCleanupWasAttempted { get; init; }

    /// <summary>
    /// Indica si el entorno aislado fue eliminado correctamente.
    /// </summary>
    public bool FinalCleanupWasSuccessful { get; init; }

    /// <summary>
    /// Indica si el resultado productivo fue clasificado como una
    /// promoción satisfactoria.
    /// </summary>
    public bool ProductiveResultWasSuccessful { get; init; }

    /// <summary>
    /// Indica si el entorno temporal general fue eliminado al
    /// finalizar la prueba.
    /// </summary>
    public bool TemporaryEnvironmentWasRemoved { get; init; }

    /// <summary>
    /// Indica si el respaldo productivo temporal fue eliminado
    /// junto con el entorno controlado.
    /// </summary>
    public bool TemporaryProductiveBackupWasRemoved { get; init; }

    /// <summary>
    /// Género solicitado durante la prueba.
    /// </summary>
    public string RequestedGenre { get; init; } =
        string.Empty;

    /// <summary>
    /// Género leído desde el destino después de la promoción.
    /// </summary>
    public string PersistedGenre { get; init; } =
        string.Empty;

    /// <summary>
    /// Mensaje de error inesperado.
    /// </summary>
    public string ErrorMessage { get; init; } =
        string.Empty;

    /// <summary>
    /// Mensajes auditables producidos por la prueba.
    /// </summary>
    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Indica si todas las comprobaciones fueron superadas.
    /// </summary>
    public bool WasSuccessful =>
        string.IsNullOrWhiteSpace(
            ErrorMessage) &&
        TestEnvironmentWasPrepared &&
        VerifiedCopyWasPrepared &&
        PromotionDecisionWasPending &&
        DestinationRemainedUnchangedDuringPreparation &&
        ApprovedDecisionWasHandled &&
        PromotionWasSuccessful &&
        ProductiveBackupWasCreated &&
        ProductiveBackupWasVerified &&
        ReplacementWasExecuted &&
        PromotedDestinationWasVerified &&
        RequestedGenreWasPersisted &&
        RollbackWasNotRequired &&
        ReferenceOriginalRemainedUnchanged &&
        DestinationEndedInSafeState &&
        FinalCleanupWasAttempted &&
        FinalCleanupWasSuccessful &&
        ProductiveResultWasSuccessful &&
        TemporaryEnvironmentWasRemoved &&
        TemporaryProductiveBackupWasRemoved;

    /// <summary>
    /// Resumen compacto de la prueba.
    /// </summary>
    public string Summary =>
        WasSuccessful
            ? "El camino Approved del coordinador productivo " +
              "terminó correctamente sobre un destino temporal."
            : "El camino Approved del coordinador productivo no " +
              "superó todas las comprobaciones controladas.";
}