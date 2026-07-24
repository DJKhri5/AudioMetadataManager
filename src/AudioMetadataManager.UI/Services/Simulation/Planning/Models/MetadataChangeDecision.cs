namespace AudioMetadataManager.UI.Services.Simulation
    .Planning.Models;

/// <summary>
/// Describe la decisión tomada para una propuesta individual
/// de modificación de metadatos.
/// </summary>
public enum MetadataChangeDecision
{
    /// <summary>
    /// La propuesta todavía no ha sido evaluada.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// El valor actual ya coincide con el valor propuesto.
    /// No se necesita ninguna modificación.
    /// </summary>
    NoChangeRequired = 1,

    /// <summary>
    /// La propuesta cumple las condiciones técnicas para ser
    /// aplicada sin intervención adicional.
    ///
    /// En esta etapa sólo se marca como elegible; todavía no
    /// se modifica el archivo.
    /// </summary>
    EligibleForAutomaticApply = 2,

    /// <summary>
    /// La propuesta es razonable, pero debe ser revisada y
    /// aprobada por el usuario.
    /// </summary>
    ManualReviewRequired = 3,

    /// <summary>
    /// No existe evidencia suficiente para reemplazar el valor
    /// actual.
    /// </summary>
    InsufficientEvidence = 4,

    /// <summary>
    /// Existen propuestas incompatibles o un conflicto sin
    /// resolver.
    /// </summary>
    Conflict = 5,

    /// <summary>
    /// La propuesta fue descartada por una regla de seguridad.
    /// </summary>
    Rejected = 6
}