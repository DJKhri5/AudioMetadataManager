namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Consolida el resultado de una promoción controlada de una
/// copia verificada hacia un archivo de destino.
///
/// Este modelo no ejecuta por sí mismo ninguna operación sobre
/// archivos.
/// </summary>
public sealed class MetadataApplicationPromotionResult
{
    /// <summary>
    /// Ruta de la copia verificada utilizada como origen.
    /// </summary>
    public string VerifiedWorkingCopyPath { get; init; } =
        string.Empty;

    /// <summary>
    /// Ruta del archivo que se pretendía actualizar.
    /// </summary>
    public string DestinationFilePath { get; init; } =
        string.Empty;

    /// <summary>
    /// Ruta del respaldo productivo creado antes de cualquier
    /// sustitución.
    /// </summary>
    public string ProductiveBackupPath { get; init; } =
        string.Empty;

    /// <summary>
    /// Hash del archivo de destino antes de la promoción.
    /// </summary>
    public string DestinationHashBefore { get; init; } =
        string.Empty;

    /// <summary>
    /// Hash de la copia verificada antes de la promoción.
    /// </summary>
    public string VerifiedCopyHash { get; init; } =
        string.Empty;

    /// <summary>
    /// Hash del archivo de destino después de la promoción.
    /// </summary>
    public string DestinationHashAfter { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si las rutas y archivos de entrada superaron las
    /// validaciones previas.
    /// </summary>
    public bool InputsWereValidated { get; init; }

    /// <summary>
    /// Indica si se creó y verificó un respaldo productivo del
    /// archivo de destino antes de sustituirlo.
    /// </summary>
    public bool ProductiveBackupWasCreated { get; init; }

    /// <summary>
    /// Indica si el respaldo productivo coincide con el estado
    /// original del archivo de destino.
    /// </summary>
    public bool ProductiveBackupWasVerified { get; init; }

    /// <summary>
    /// Indica si la operación de sustitución fue ejecutada.
    /// </summary>
    public bool ReplacementWasExecuted { get; init; }

    /// <summary>
    /// Indica si el archivo promovido coincide con la copia
    /// verificada.
    /// </summary>
    public bool PromotedFileWasVerified { get; init; }

    /// <summary>
    /// Indica si fue necesario intentar una reversión.
    /// </summary>
    public bool RollbackWasAttempted { get; init; }

    /// <summary>
    /// Indica si la reversión restauró correctamente el archivo
    /// de destino.
    /// </summary>
    public bool RollbackWasSuccessful { get; init; }

    /// <summary>
    /// Indica si la copia verificada fue conservada después de la
    /// operación para diagnóstico o reintento.
    /// </summary>
    public bool VerifiedCopyWasPreserved { get; init; }

    /// <summary>
    /// Mensaje de error capturado durante la validación,
    /// respaldo, sustitución, verificación o reversión.
    /// </summary>
    public string ErrorMessage { get; init; } =
        string.Empty;

    /// <summary>
    /// Mensajes auditables producidos durante la operación.
    /// </summary>
    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Indica si la promoción terminó correctamente.
    /// </summary>
    public bool WasSuccessful =>
        string.IsNullOrWhiteSpace(
            ErrorMessage) &&
        InputsWereValidated &&
        ProductiveBackupWasCreated &&
        ProductiveBackupWasVerified &&
        ReplacementWasExecuted &&
        PromotedFileWasVerified &&
        !RollbackWasAttempted;

    /// <summary>
    /// Indica si un fallo fue recuperado correctamente mediante
    /// el respaldo productivo.
    /// </summary>
    public bool WasSafelyRolledBack =>
        !WasSuccessful &&
        RollbackWasAttempted &&
        RollbackWasSuccessful;

    /// <summary>
    /// Indica si la operación terminó en un estado de archivo
    /// controlado, ya sea por promoción correcta o reversión.
    /// </summary>
    public bool DestinationEndedInSafeState =>
        WasSuccessful ||
        WasSafelyRolledBack;

    /// <summary>
    /// Resumen compacto del resultado.
    /// </summary>
    public string Summary
    {
        get
        {
            if (WasSuccessful)
            {
                return
                    "La copia verificada fue promovida " +
                    "correctamente y el respaldo productivo " +
                    "quedó disponible.";
            }

            if (WasSafelyRolledBack)
            {
                return
                    "La promoción no pudo completarse, pero el " +
                    "archivo de destino fue restaurado desde el " +
                    "respaldo productivo.";
            }

            if (!string.IsNullOrWhiteSpace(
                    ErrorMessage))
            {
                return
                    "La promoción controlada terminó con un " +
                    $"error: {ErrorMessage}";
            }

            return
                "La promoción controlada no terminó en un " +
                "estado seguro verificado.";
        }
    }
}