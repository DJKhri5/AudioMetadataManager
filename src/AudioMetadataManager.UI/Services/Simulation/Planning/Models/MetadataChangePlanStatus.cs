namespace AudioMetadataManager.UI.Services.Simulation
    .Planning.Models;

/// <summary>
/// Describe el estado global de un plan de modificaciones.
/// </summary>
public enum MetadataChangePlanStatus
{
    /// <summary>
    /// El plan todavía no ha sido evaluado.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// No se encontraron modificaciones necesarias.
    /// </summary>
    NoChangesRequired = 1,

    /// <summary>
    /// Existen propuestas, pero todas requieren revisión del
    /// usuario.
    /// </summary>
    ManualReviewRequired = 2,

    /// <summary>
    /// Existen propuestas elegibles para aplicación automática,
    /// aunque todavía no han sido aprobadas ni ejecutadas.
    /// </summary>
    ReadyForSimulation = 3,

    /// <summary>
    /// El plan combina propuestas automáticas y propuestas que
    /// requieren revisión manual.
    /// </summary>
    PartiallyReady = 4,

    /// <summary>
    /// El plan contiene conflictos que impiden continuar sin
    /// intervención del usuario.
    /// </summary>
    BlockedByConflicts = 5,

    /// <summary>
    /// No existe evidencia suficiente para generar un plan
    /// utilizable.
    /// </summary>
    InsufficientEvidence = 6
}