using AudioMetadataManager.UI.Services.MetadataSources.Matching;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison.Comparers;

/// <summary>
/// Compara valores de artista utilizando el calculador
/// especializado de similitud artística.
///
/// Este comparador conserva la responsabilidad específica
/// del campo Artist y no modifica ninguno de los valores.
/// </summary>
public sealed class ArtistComparer :
    IMetadataFieldComparer
{
    private const double NormalizedMatchThreshold =
        0.98;

    private const double ProbableMatchThreshold =
        0.80;

    private readonly ArtistSimilarityCalculator
        _similarityCalculator;

    /// <summary>
    /// Crea el comparador con sus dependencias
    /// predeterminadas.
    /// </summary>
    public ArtistComparer()
        : this(
            new ArtistSimilarityCalculator())
    {
    }

    /// <summary>
    /// Crea el comparador con un calculador personalizado.
    ///
    /// Este constructor facilitará pruebas aisladas.
    /// </summary>
    public ArtistComparer(
        ArtistSimilarityCalculator similarityCalculator)
    {
        _similarityCalculator =
            similarityCalculator ??
            throw new ArgumentNullException(
                nameof(similarityCalculator));
    }

    /// <summary>
    /// Nombre estable del campo comparado.
    /// </summary>
    public string FieldName =>
        "Artist";

    /// <summary>
    /// Orden de ejecución dentro de la comparación completa.
    /// </summary>
    public int Order =>
        100;

    /// <summary>
    /// Compara el artista local con el artista obtenido
    /// desde una fuente de referencia.
    /// </summary>
    public MetadataFieldComparisonResult Compare(
        string? localValue,
        string? referenceValue)
    {
        string normalizedLocalValue =
            NormalizeInput(
                localValue);

        string normalizedReferenceValue =
            NormalizeInput(
                referenceValue);

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
                "Ninguna de las dos fuentes contiene un artista utilizable.");
        }

        if (!hasLocalValue)
        {
            return CreateResult(
                normalizedLocalValue,
                normalizedReferenceValue,
                MetadataFieldComparisonStatus.MissingLocalValue,
                0,
                "La fuente local no contiene un artista utilizable.");
        }

        if (!hasReferenceValue)
        {
            return CreateResult(
                normalizedLocalValue,
                normalizedReferenceValue,
                MetadataFieldComparisonStatus.MissingReferenceValue,
                0,
                "La fuente de referencia no contiene un artista utilizable.");
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
                "Los valores de artista son idénticos.");
        }

        double similarity =
            _similarityCalculator.Calculate(
                normalizedLocalValue,
                normalizedReferenceValue);

        if (similarity >=
            NormalizedMatchThreshold)
        {
            return CreateResult(
                normalizedLocalValue,
                normalizedReferenceValue,
                MetadataFieldComparisonStatus.NormalizedMatch,
                similarity,
                "Los artistas coinciden después de aplicar la comparación normalizada.");
        }

        if (similarity >=
            ProbableMatchThreshold)
        {
            return CreateResult(
                normalizedLocalValue,
                normalizedReferenceValue,
                MetadataFieldComparisonStatus.ProbableMatch,
                similarity,
                "Los artistas presentan una similitud alta, pero requieren confirmación.");
        }

        return CreateResult(
            normalizedLocalValue,
            normalizedReferenceValue,
            MetadataFieldComparisonStatus.Conflict,
            similarity,
            "Los valores de artista presentan una discrepancia relevante.");
    }

    /// <summary>
    /// Construye un resultado uniforme para el campo Artist.
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
    /// Elimina únicamente espacios exteriores.
    ///
    /// Los conectores internos como &, feat., vs y x
    /// se conservan para que el calculador especializado
    /// pueda procesarlos.
    /// </summary>
    private static string NormalizeInput(
        string? value)
    {
        return value?.Trim() ??
            string.Empty;
    }
}