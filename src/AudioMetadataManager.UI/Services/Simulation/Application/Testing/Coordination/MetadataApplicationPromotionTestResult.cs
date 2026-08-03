namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Contiene el resultado de la prueba controlada del servicio
/// de promoción de copias verificadas.
/// </summary>
public sealed class MetadataApplicationPromotionTestResult
{
    /// <summary>
    /// Indica si el entorno temporal de prueba fue preparado.
    /// </summary>
    public bool TestEnvironmentWasPrepared { get; init; }

    /// <summary>
    /// Indica si las entradas fueron validadas.
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
    /// Indica si la sustitución fue ejecutada.
    /// </summary>
    public bool ReplacementWasExecuted { get; init; }

    /// <summary>
    /// Indica si el destino final coincide con la copia
    /// verificada.
    /// </summary>
    public bool PromotedFileWasVerified { get; init; }

    /// <summary>
    /// Indica si el archivo original utilizado como fuente de
    /// prueba permaneció intacto.
    /// </summary>
    public bool ReferenceOriginalRemainedUnchanged { get; init; }

    /// <summary>
    /// Indica si la copia verificada permaneció disponible
    /// después de la promoción.
    /// </summary>
    public bool VerifiedCopyWasPreserved { get; init; }

    /// <summary>
    /// Indica si no fue necesario ejecutar una reversión.
    /// </summary>
    public bool RollbackWasNotRequired { get; init; }

    /// <summary>
    /// Indica si el entorno temporal fue eliminado.
    /// </summary>
    public bool TestEnvironmentWasRemoved { get; init; }

    /// <summary>
    /// Indica si el respaldo productivo temporal fue eliminado
    /// al limpiar el entorno de prueba.
    /// </summary>
    public bool TemporaryBackupWasRemoved { get; init; }

    /// <summary>
    /// Mensaje de error capturado durante la prueba.
    /// </summary>
    public string ErrorMessage { get; init; } =
        string.Empty;

    /// <summary>
    /// Mensajes auditables generados por la prueba.
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
        InputsWereValidated &&
        ProductiveBackupWasCreated &&
        ProductiveBackupWasVerified &&
        ReplacementWasExecuted &&
        PromotedFileWasVerified &&
        ReferenceOriginalRemainedUnchanged &&
        VerifiedCopyWasPreserved &&
        RollbackWasNotRequired &&
        TestEnvironmentWasRemoved &&
        TemporaryBackupWasRemoved;

    /// <summary>
    /// Resumen compacto de la prueba.
    /// </summary>
    public string Summary =>
        WasSuccessful
            ? "La promoción controlada sobre archivos " +
              "temporales terminó correctamente."
            : "La prueba de promoción controlada no superó " +
              "todas las comprobaciones.";
}