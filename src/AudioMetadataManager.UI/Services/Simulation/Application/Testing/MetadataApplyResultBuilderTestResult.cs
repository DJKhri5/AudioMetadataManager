using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing;

/// <summary>
/// Consolida la evidencia obtenida al probar directamente
/// MetadataApplyResultBuilder.
/// </summary>
public sealed class MetadataApplyResultBuilderTestResult
{
    /// <summary>
    /// Resultado final construido durante la prueba.
    /// </summary>
    public MetadataApplyResult? ApplyResult { get; init; }

    /// <summary>
    /// Indica si los identificadores fueron conservados.
    /// </summary>
    public bool IdentifiersPreserved { get; init; }

    /// <summary>
    /// Indica si la información del archivo fue conservada.
    /// </summary>
    public bool FileInformationPreserved { get; init; }

    /// <summary>
    /// Indica si la ruta del respaldo fue conservada.
    /// </summary>
    public bool BackupPathPreserved { get; init; }

    /// <summary>
    /// Indica si se construyó un resultado por cada cambio
    /// válido solicitado.
    /// </summary>
    public bool FieldCountPreserved { get; init; }

    /// <summary>
    /// Indica si los valores original, solicitado y verificado
    /// fueron mapeados correctamente.
    /// </summary>
    public bool FieldValuesPreserved { get; init; }

    /// <summary>
    /// Indica si el estado de escritura fue consolidado.
    /// </summary>
    public bool WriteStatusPreserved { get; init; }

    /// <summary>
    /// Indica si el estado de verificación fue consolidado.
    /// </summary>
    public bool VerificationStatusPreserved { get; init; }

    /// <summary>
    /// Indica si el estado general fue determinado
    /// correctamente.
    /// </summary>
    public bool FinalStatusCorrect { get; init; }

    /// <summary>
    /// Indica si los mensajes de las distintas etapas fueron
    /// incorporados.
    /// </summary>
    public bool MessagesConsolidated { get; init; }

    /// <summary>
    /// Indica si los mensajes duplicados fueron eliminados.
    /// </summary>
    public bool DuplicateMessagesRemoved { get; init; }

    /// <summary>
    /// Indica si los tiempos del resultado son coherentes.
    /// </summary>
    public bool TimingIsValid { get; init; }

    /// <summary>
    /// Momento UTC en que comenzó la prueba.
    /// </summary>
    public DateTimeOffset StartedAtUtc { get; init; }

    /// <summary>
    /// Momento UTC en que finalizó la prueba.
    /// </summary>
    public DateTimeOffset CompletedAtUtc { get; init; }

    /// <summary>
    /// Duración total de la prueba.
    /// </summary>
    public TimeSpan ElapsedTime { get; init; }

    /// <summary>
    /// Mensajes auditables producidos durante la prueba.
    /// </summary>
    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Mensaje correspondiente a una excepción inesperada.
    /// </summary>
    public string ErrorMessage { get; init; } =
        string.Empty;

    /// <summary>
    /// Tipo de excepción capturada.
    /// </summary>
    public string ExceptionType { get; init; } =
        string.Empty;

    /// <summary>
    /// Indica si la prueba completa fue satisfactoria.
    /// </summary>
    public bool WasSuccessful =>
        ApplyResult?.WasSuccessful == true &&
        IdentifiersPreserved &&
        FileInformationPreserved &&
        BackupPathPreserved &&
        FieldCountPreserved &&
        FieldValuesPreserved &&
        WriteStatusPreserved &&
        VerificationStatusPreserved &&
        FinalStatusCorrect &&
        MessagesConsolidated &&
        DuplicateMessagesRemoved &&
        TimingIsValid &&
        string.IsNullOrWhiteSpace(
            ErrorMessage);

    /// <summary>
    /// Resumen legible de la prueba.
    /// </summary>
    public string Summary =>
        WasSuccessful
            ? "El constructor consolidó correctamente el " +
              "resultado final de aplicación."
            : "La prueba del constructor detectó una o más " +
              "diferencias en el resultado consolidado.";
}