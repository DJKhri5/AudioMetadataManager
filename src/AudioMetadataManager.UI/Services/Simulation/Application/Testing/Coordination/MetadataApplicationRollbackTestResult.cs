namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Contiene el resultado de la prueba controlada de reversión
/// automática después de una verificación fallida simulada.
/// </summary>
public sealed class MetadataApplicationRollbackTestResult
{
    /// <summary>
    /// Indica si el entorno temporal de prueba fue preparado.
    /// </summary>
    public bool TestEnvironmentWasPrepared { get; init; }

    /// <summary>
    /// Indica si las entradas fueron validadas antes de promover.
    /// </summary>
    public bool InputsWereValidated { get; init; }

    /// <summary>
    /// Indica si el respaldo productivo fue creado.
    /// </summary>
    public bool ProductiveBackupWasCreated { get; init; }

    /// <summary>
    /// Indica si el respaldo productivo fue verificado.
    /// </summary>
    public bool ProductiveBackupWasVerified { get; init; }

    /// <summary>
    /// Indica si la sustitución temporal fue ejecutada antes del
    /// fallo simulado.
    /// </summary>
    public bool ReplacementWasExecuted { get; init; }

    /// <summary>
    /// Indica si la verificación posterior fue rechazada de forma
    /// deliberada para activar la reversión.
    /// </summary>
    public bool VerificationFailureWasSimulated { get; init; }

    /// <summary>
    /// Indica si el servicio intentó revertir el destino.
    /// </summary>
    public bool RollbackWasAttempted { get; init; }

    /// <summary>
    /// Indica si la reversión terminó correctamente.
    /// </summary>
    public bool RollbackWasSuccessful { get; init; }

    /// <summary>
    /// Indica si el destino restaurado coincide con su estado
    /// anterior a la sustitución.
    /// </summary>
    public bool DestinationWasRestored { get; init; }

    /// <summary>
    /// Indica si el original de referencia permaneció intacto.
    /// </summary>
    public bool ReferenceOriginalRemainedUnchanged { get; init; }

    /// <summary>
    /// Indica si la copia verificada permaneció disponible
    /// después de la reversión.
    /// </summary>
    public bool VerifiedCopyWasPreserved { get; init; }

    /// <summary>
    /// Indica si el destino terminó en un estado seguro
    /// verificado.
    /// </summary>
    public bool DestinationEndedInSafeState { get; init; }

    /// <summary>
    /// Indica si el entorno temporal fue eliminado.
    /// </summary>
    public bool TestEnvironmentWasRemoved { get; init; }

    /// <summary>
    /// Indica si el respaldo productivo temporal fue eliminado
    /// junto con el entorno de prueba.
    /// </summary>
    public bool TemporaryBackupWasRemoved { get; init; }

    /// <summary>
    /// Mensaje de error esperado durante la simulación.
    /// </summary>
    public string ExpectedErrorMessage { get; init; } =
        string.Empty;

    /// <summary>
    /// Mensajes auditables generados durante la prueba.
    /// </summary>
    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Indica si todas las comprobaciones fueron superadas.
    ///
    /// La existencia de un error es esperada en esta prueba,
    /// porque activa deliberadamente la reversión automática.
    /// </summary>
    public bool WasSuccessful =>
        TestEnvironmentWasPrepared &&
        InputsWereValidated &&
        ProductiveBackupWasCreated &&
        ProductiveBackupWasVerified &&
        ReplacementWasExecuted &&
        VerificationFailureWasSimulated &&
        RollbackWasAttempted &&
        RollbackWasSuccessful &&
        DestinationWasRestored &&
        ReferenceOriginalRemainedUnchanged &&
        VerifiedCopyWasPreserved &&
        DestinationEndedInSafeState &&
        TestEnvironmentWasRemoved &&
        TemporaryBackupWasRemoved &&
        !string.IsNullOrWhiteSpace(
            ExpectedErrorMessage);

    /// <summary>
    /// Resumen compacto de la prueba.
    /// </summary>
    public string Summary =>
        WasSuccessful
            ? "La verificación fallida simulada activó una " +
              "reversión automática correcta y el destino fue " +
              "restaurado."
            : "La prueba de reversión automática no superó " +
              "todas las comprobaciones.";
}