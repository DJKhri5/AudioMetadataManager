namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Contiene el resultado de las pruebas controladas del
/// coordinador productivo de aplicación de metadatos.
/// </summary>
public sealed class MetadataApplicationCoordinatorTestResult
{
    /// <summary>
    /// Indica si una solicitud nula fue rechazada.
    /// </summary>
    public bool NullRequestWasRejected { get; init; }

    /// <summary>
    /// Indica si una fábrica nula fue rechazada.
    /// </summary>
    public bool NullExecutorFactoryWasRejected { get; init; }

    /// <summary>
    /// Indica si una cancelación previa fue devuelta como
    /// resultado auditable.
    /// </summary>
    public bool PreCancelledExecutionWasHandled { get; init; }

    /// <summary>
    /// Indica si la cancelación produjo la razón de detención
    /// esperada.
    /// </summary>
    public bool CancellationStopReasonWasCorrect { get; init; }

    /// <summary>
    /// Indica si una fábrica que devuelve un ejecutor nulo fue
    /// controlada correctamente.
    /// </summary>
    public bool NullExecutorWasHandled { get; init; }

    /// <summary>
    /// Indica si el ejecutor nulo produjo un error inesperado
    /// auditable.
    /// </summary>
    public bool NullExecutorStopReasonWasCorrect { get; init; }

    /// <summary>
    /// Indica si una excepción producida por la fábrica fue
    /// controlada correctamente.
    /// </summary>
    public bool FactoryExceptionWasHandled { get; init; }

    /// <summary>
    /// Indica si la excepción de fábrica produjo la razón de
    /// detención esperada.
    /// </summary>
    public bool FactoryExceptionStopReasonWasCorrect { get; init; }

    /// <summary>
    /// Indica si todos los resultados controlados quedaron
    /// correctamente finalizados.
    /// </summary>
    public bool ResultsWereFinalized { get; init; }

    /// <summary>
    /// Mensajes producidos durante las comprobaciones.
    /// </summary>
    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Indica si todas las comprobaciones fueron superadas.
    /// </summary>
    public bool WasSuccessful =>
        NullRequestWasRejected &&
        NullExecutorFactoryWasRejected &&
        PreCancelledExecutionWasHandled &&
        CancellationStopReasonWasCorrect &&
        NullExecutorWasHandled &&
        NullExecutorStopReasonWasCorrect &&
        FactoryExceptionWasHandled &&
        FactoryExceptionStopReasonWasCorrect &&
        ResultsWereFinalized;

    /// <summary>
    /// Resumen compacto de la prueba.
    /// </summary>
    public string Summary =>
        WasSuccessful
            ? "El coordinador productivo superó todas las " +
              "comprobaciones controladas."
            : "El coordinador productivo no superó todas las " +
              "comprobaciones controladas.";
}