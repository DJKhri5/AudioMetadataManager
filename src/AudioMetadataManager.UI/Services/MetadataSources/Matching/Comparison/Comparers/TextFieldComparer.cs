namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison.Comparers;

/// <summary>
/// Compara un campo textual genérico entre una fuente local
/// y una fuente de referencia.
///
/// Esta primera versión distingue valores ausentes,
/// coincidencias exactas y conflictos literales.
/// Posteriormente incorporará normalización segura y
/// similitud textual sin cambiar su contrato público.
/// </summary>
public sealed class TextFieldComparer :
    IMetadataFieldComparer
{
    /// <summary>
    /// Crea un comparador para el campo indicado.
    /// </summary>
    public TextFieldComparer(
        string fieldName,
        int order)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            throw new ArgumentException(
                "El nombre del campo no puede estar vacío.",
                nameof(fieldName));
        }

        if (order < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(order),
                order,
                "El orden no puede ser negativo.");
        }

        FieldName =
            fieldName.Trim();

        Order =
            order;
    }

    /// <summary>
    /// Nombre estable del campo comparado.
    /// </summary>
    public string FieldName { get; }

    /// <summary>
    /// Orden de ejecución dentro de una comparación completa.
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// Compara los dos valores textuales.
    /// </summary>
    public MetadataFieldComparisonResult Compare(
        string? localValue,
        string? referenceValue)
    {
        string normalizedLocalValue =
            NormalizeInput(localValue);

        string normalizedReferenceValue =
            NormalizeInput(referenceValue);

        bool hasLocalValue =
            !string.IsNullOrWhiteSpace(
                normalizedLocalValue);

        bool hasReferenceValue =
            !string.IsNullOrWhiteSpace(
                normalizedReferenceValue);

        if (!hasLocalValue &&
            !hasReferenceValue)
        {
            return CreateResult(
                normalizedLocalValue,
                normalizedReferenceValue,
                MetadataFieldComparisonStatus.MissingBothValues,
                0,
                "Ninguna de las dos fuentes contiene un valor utilizable.");
        }

        if (!hasLocalValue)
        {
            return CreateResult(
                normalizedLocalValue,
                normalizedReferenceValue,
                MetadataFieldComparisonStatus.MissingLocalValue,
                0,
                "La fuente local no contiene un valor utilizable.");
        }

        if (!hasReferenceValue)
        {
            return CreateResult(
                normalizedLocalValue,
                normalizedReferenceValue,
                MetadataFieldComparisonStatus.MissingReferenceValue,
                0,
                "La fuente de referencia no contiene un valor utilizable.");
        }

        bool exactMatch =
            string.Equals(
                normalizedLocalValue,
                normalizedReferenceValue,
                StringComparison.Ordinal);

        if (exactMatch)
        {
            return CreateResult(
                normalizedLocalValue,
                normalizedReferenceValue,
                MetadataFieldComparisonStatus.ExactMatch,
                1,
                "Los valores son idénticos.");
        }

        return CreateResult(
            normalizedLocalValue,
            normalizedReferenceValue,
            MetadataFieldComparisonStatus.Conflict,
            0,
            "Los valores son diferentes.");
    }

    /// <summary>
    /// Construye un resultado uniforme para este campo.
    /// </summary>
    private MetadataFieldComparisonResult CreateResult(
        string localValue,
        string referenceValue,
        MetadataFieldComparisonStatus status,
        double similarity,
        string explanation)
    {
        return new MetadataFieldComparisonResult
        {
            FieldName =
                FieldName,

            LocalValue =
                localValue,

            ReferenceValue =
                referenceValue,

            Status =
                status,

            Similarity =
                Math.Clamp(
                    similarity,
                    0,
                    1),

            Explanation =
                explanation
        };
    }

    /// <summary>
    /// Normaliza únicamente espacios exteriores.
    ///
    /// No altera mayúsculas, acentos, signos ni conectores.
    /// La normalización semántica se incorporará después.
    /// </summary>
    private static string NormalizeInput(
        string? value)
    {
        return value?.Trim() ??
            string.Empty;
    }
}