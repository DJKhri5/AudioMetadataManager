namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Execution;

/// <summary>
/// Define el comportamiento general del ejecutor del pipeline.
/// </summary>
public sealed class MetadataApplicationPipelineOptions
{
    /// <summary>
    /// Indica si el ejecutor debe detenerse cuando una etapa
    /// termina con un fallo bloqueante.
    /// </summary>
    public bool StopOnBlockingFailure { get; init; } =
        true;

    /// <summary>
    /// Indica si el ejecutor debe detenerse cuando una etapa
    /// termina cancelada.
    /// </summary>
    public bool StopOnCancellation { get; init; } =
        true;

    /// <summary>
    /// Indica si una etapa omitida debe considerarse una razón
    /// para detener el flujo.
    ///
    /// El valor predeterminado es falso porque una omisión puede
    /// formar parte de un flujo diagnóstico válido.
    /// </summary>
    public bool StopOnSkippedStage { get; init; }

    /// <summary>
    /// Indica si deben rechazarse etapas que compartan el mismo
    /// orden de ejecución.
    ///
    /// El valor predeterminado permite compartir el orden y
    /// utiliza la identidad funcional de la etapa como segundo
    /// criterio estable.
    /// </summary>
    public bool RejectDuplicateExecutionOrder { get; init; }

    /// <summary>
    /// Indica si el contexto debe finalizarse automáticamente
    /// cuando todas las etapas terminan sin fallos bloqueantes.
    ///
    /// El valor predeterminado de esta clase es falso. La
    /// composición predeterminada del pipeline
    /// (MetadataApplicationPipelineFactory.CreateDefault) lo
    /// activa explícitamente, ya que incluye MetadataFinalizationStage,
    /// la etapa real que construye MetadataApplyResult.
    /// </summary>
    public bool CompleteContextAutomatically { get; init; }

    /// <summary>
    /// Configuración predeterminada segura.
    /// </summary>
    public static MetadataApplicationPipelineOptions Default =>
        new();
}