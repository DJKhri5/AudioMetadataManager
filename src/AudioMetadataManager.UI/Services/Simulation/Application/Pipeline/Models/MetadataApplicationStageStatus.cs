namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;

/// <summary>
/// Describe el estado de una etapa individual del pipeline.
/// </summary>
public enum MetadataApplicationStageStatus
{
    /// <summary>
    /// La etapa todavía no ha comenzado.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// La etapa se está ejecutando.
    /// </summary>
    Running = 1,

    /// <summary>
    /// La etapa terminó correctamente.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// La etapa terminó con advertencias no bloqueantes.
    /// </summary>
    CompletedWithWarnings = 3,

    /// <summary>
    /// La etapa no pudo completarse.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// La etapa no se ejecutó porque una etapa anterior detuvo
    /// el pipeline.
    /// </summary>
    Skipped = 5,

    /// <summary>
    /// La etapa fue cancelada durante su ejecución.
    /// </summary>
    Cancelled = 6
}