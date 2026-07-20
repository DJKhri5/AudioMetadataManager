namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison;

/// <summary>
/// Representa el resultado de comparar un campo de metadatos
/// entre dos fuentes.
///
/// Este estado no decide automáticamente qué valor debe
/// conservarse. Solamente describe la relación observada.
/// </summary>
public enum MetadataFieldComparisonStatus
{
    /// <summary>
    /// La comparación todavía no fue realizada o no existe
    /// información suficiente para determinar un resultado.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Ambos valores están disponibles y son exactamente
    /// iguales.
    /// </summary>
    ExactMatch = 1,

    /// <summary>
    /// Los valores coinciden después de una normalización
    /// segura, por ejemplo diferencias de mayúsculas,
    /// espacios o signos equivalentes.
    /// </summary>
    NormalizedMatch = 2,

    /// <summary>
    /// Los valores son parecidos, pero la coincidencia no es
    /// suficientemente exacta para considerarla confirmada.
    /// </summary>
    ProbableMatch = 3,

    /// <summary>
    /// Ambos valores están disponibles, pero presentan una
    /// discrepancia relevante.
    /// </summary>
    Conflict = 4,

    /// <summary>
    /// El valor no está disponible en la fuente local.
    /// </summary>
    MissingLocalValue = 5,

    /// <summary>
    /// El valor no está disponible en la fuente de referencia.
    /// </summary>
    MissingReferenceValue = 6,

    /// <summary>
    /// Ninguna de las dos fuentes contiene un valor utilizable.
    /// </summary>
    MissingBothValues = 7,

    /// <summary>
    /// El campo no corresponde o no debe compararse en el
    /// contexto actual.
    /// </summary>
    NotApplicable = 8
}