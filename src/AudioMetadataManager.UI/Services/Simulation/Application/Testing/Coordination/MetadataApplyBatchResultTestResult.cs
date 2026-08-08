namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Conserva el resultado de las comprobaciones estructurales
/// realizadas sobre MetadataApplyBatchResult.
/// </summary>
public sealed class MetadataApplyBatchResultTestResult
{
    public bool EmptyResultWasRejected { get; init; }

    public bool SuccessfulResultsWereCounted { get; init; }

    public bool FailedResultsWereCounted { get; init; }

    public bool SuccessfulBatchWasDetected { get; init; }

    public bool PartialFailureWasDetected { get; init; }

    public bool BatchIdentityWasPreserved { get; init; }

    public bool TimesWerePreserved { get; init; }

    public bool DurationWasCalculated { get; init; }

    public bool MessagesWerePreserved { get; init; }

    public bool SummaryWasGenerated { get; init; }

    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Indica si todas las comprobaciones estructurales del
    /// resultado por lote terminaron correctamente.
    /// </summary>
    public bool WasSuccessful =>
        EmptyResultWasRejected &&
        SuccessfulResultsWereCounted &&
        FailedResultsWereCounted &&
        SuccessfulBatchWasDetected &&
        PartialFailureWasDetected &&
        BatchIdentityWasPreserved &&
        TimesWerePreserved &&
        DurationWasCalculated &&
        MessagesWerePreserved &&
        SummaryWasGenerated;

    /// <summary>
    /// Resumen compacto de la prueba.
    /// </summary>
    public string Summary =>
        WasSuccessful
            ? "El resultado productivo por lote superó todas las " +
              "comprobaciones estructurales."
            : "El resultado productivo por lote no superó todas " +
              "las comprobaciones estructurales.";
}