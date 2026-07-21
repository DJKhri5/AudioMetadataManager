namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Confidence.Models;

/// <summary>
/// Representa la decisión global obtenida después de evaluar
/// una comparación completa de metadatos.
///
/// La decisión no aplica cambios automáticamente. Solamente
/// describe el nivel de confianza alcanzado y la acción
/// recomendada para el flujo de revisión.
/// </summary>
public enum MetadataComparisonDecision
{
    /// <summary>
    /// La evaluación todavía no se ha ejecutado o no existen
    /// datos suficientes para emitir una conclusión.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// La coincidencia es fuerte y no presenta conflictos
    /// relevantes en los campos críticos.
    /// </summary>
    Accepted = 1,

    /// <summary>
    /// La coincidencia parece válida, pero existe información
    /// incompleta o alguna diferencia menor que conviene revisar.
    /// </summary>
    AcceptedWithReview = 2,

    /// <summary>
    /// La comparación presenta dudas, baja cobertura o
    /// discrepancias que requieren una decisión manual.
    /// </summary>
    ManualReviewRequired = 3,

    /// <summary>
    /// Existen conflictos importantes, especialmente en
    /// campos críticos como Artist, Title o Version.
    /// </summary>
    Rejected = 4,

    /// <summary>
    /// La cantidad o calidad de la información disponible
    /// no permite evaluar la coincidencia de forma fiable.
    /// </summary>
    InsufficientData = 5
}