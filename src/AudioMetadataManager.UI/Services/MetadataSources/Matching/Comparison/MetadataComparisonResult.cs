using System.Collections.Generic;
using System.Linq;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison;

/// <summary>
/// Contiene el resultado completo de la comparación
/// entre los metadatos locales y una fuente externa.
/// </summary>
public sealed class MetadataComparisonResult
{
    /// <summary>
    /// Nombre de la fuente considerada local o principal.
    ///
    /// Ejemplos:
    /// Archivo local
    /// Nombre del archivo
    /// Etiquetas internas
    /// </summary>
    public string LocalSourceName { get; init; } =
        string.Empty;

    /// <summary>
    /// Nombre de la fuente utilizada como referencia.
    ///
    /// Ejemplos:
    /// Discogs
    /// Beatport
    /// Spotify
    /// SoundCloud
    /// </summary>
    public string ReferenceSourceName { get; init; } =
        string.Empty;

    /// <summary>
    /// Nombre local preparado para mostrarse en informes
    /// y en la interfaz de usuario.
    /// </summary>
    public string LocalSourceDisplayName =>
        string.IsNullOrWhiteSpace(
            LocalSourceName)
            ? "Fuente local sin identificar"
            : LocalSourceName.Trim();

    /// <summary>
    /// Nombre de referencia preparado para mostrarse
    /// en informes y en la interfaz de usuario.
    /// </summary>
    public string ReferenceSourceDisplayName =>
        string.IsNullOrWhiteSpace(
            ReferenceSourceName)
            ? "Fuente de referencia sin identificar"
            : ReferenceSourceName.Trim();

    /// <summary>
    /// Comparaciones individuales realizadas.
    /// </summary>
    public List<MetadataFieldComparisonResult> Fields { get; } = new();

    /// <summary>
    /// Cantidad total de campos comparados.
    /// </summary>
    public int TotalFields =>
        Fields.Count;

    /// <summary>
    /// Coincidencias exactas.
    /// </summary>
    public int ExactMatches =>
        Fields.Count(f =>
            f.Status == MetadataFieldComparisonStatus.ExactMatch);

    /// <summary>
    /// Coincidencias normalizadas.
    /// </summary>
    public int NormalizedMatches =>
        Fields.Count(f =>
            f.Status == MetadataFieldComparisonStatus.NormalizedMatch);

    /// <summary>
    /// Coincidencias probables.
    /// </summary>
    public int ProbableMatches =>
        Fields.Count(f =>
            f.Status == MetadataFieldComparisonStatus.ProbableMatch);

    /// <summary>
    /// Conflictos detectados.
    /// </summary>
    public int Conflicts =>
        Fields.Count(f =>
            f.Status == MetadataFieldComparisonStatus.Conflict);

    /// <summary>
    /// Campos sin información local.
    /// </summary>
    public int MissingLocalValues =>
        Fields.Count(f =>
            f.Status == MetadataFieldComparisonStatus.MissingLocalValue);

    /// <summary>
    /// Campos sin información de referencia.
    /// </summary>
    public int MissingReferenceValues =>
        Fields.Count(f =>
            f.Status == MetadataFieldComparisonStatus.MissingReferenceValue);

    /// <summary>
    /// Campos para los que ninguna de las dos fuentes contiene
    /// un valor utilizable.
    /// </summary>
    public int MissingBothValues =>
        Fields.Count(f =>
            f.Status ==
            MetadataFieldComparisonStatus.MissingBothValues);

    /// <summary>
    /// Similitud promedio.
    /// </summary>
    public double AverageSimilarity =>
        Fields.Count == 0
            ? 0
            : Fields.Average(f => f.Similarity);

    /// <summary>
    /// Cantidad de campos que contienen información utilizable
    /// en al menos una de las dos fuentes.
    ///
    /// Se excluyen los campos ausentes en ambos lados y los
    /// campos que no resultan aplicables.
    /// </summary>
    public int FieldsWithAnyValue =>
        Fields.Count(f =>
            f.Status !=
                MetadataFieldComparisonStatus.MissingBothValues &&
            f.Status !=
                MetadataFieldComparisonStatus.NotApplicable);

    /// <summary>
    /// Cantidad de campos en los que ambas fuentes contienen
    /// valores utilizables y, por lo tanto, pueden compararse.
    ///
    /// Incluye coincidencias exactas, normalizadas, probables
    /// y conflictos.
    /// </summary>
    public int ComparableFields =>
        Fields.Count(f =>
            f.Status ==
                MetadataFieldComparisonStatus.ExactMatch ||
            f.Status ==
                MetadataFieldComparisonStatus.NormalizedMatch ||
            f.Status ==
                MetadataFieldComparisonStatus.ProbableMatch ||
            f.Status ==
                MetadataFieldComparisonStatus.Conflict);

    /// <summary>
    /// Similitud promedio calculada únicamente sobre campos
    /// que contienen valores utilizables en ambas fuentes.
    ///
    /// Los campos ausentes o no aplicables no reducen
    /// artificialmente esta métrica.
    /// </summary>
    public double EffectiveSimilarity
    {
        get
        {
            List<MetadataFieldComparisonResult>
                comparableFields =
                    Fields
                        .Where(f =>
                            f.Status ==
                                MetadataFieldComparisonStatus.ExactMatch ||
                            f.Status ==
                                MetadataFieldComparisonStatus.NormalizedMatch ||
                            f.Status ==
                                MetadataFieldComparisonStatus.ProbableMatch ||
                            f.Status ==
                                MetadataFieldComparisonStatus.Conflict)
                        .ToList();

            return comparableFields.Count == 0
                ? 0
                : comparableFields.Average(
                    f => f.Similarity);
        }
    }

    /// <summary>
    /// Proporción de campos que contienen información utilizable
    /// en al menos una de las dos fuentes.
    ///
    /// Un resultado de 1 representa una cobertura del 100 %.
    /// </summary>
    public double InformationCoverage
    {
        get
        {
            int applicableFields =
                Fields.Count(f =>
                    f.Status !=
                        MetadataFieldComparisonStatus.NotApplicable);

            return applicableFields == 0
                ? 0
                : (double)FieldsWithAnyValue /
                  applicableFields;
        }
    }

    /// <summary>
    /// Indica si existe al menos un conflicto objetivo entre
    /// los valores comparados.
    ///
    /// Esta propiedad no determina si la coincidencia debe
    /// aceptarse o rechazarse.
    /// </summary>
    public bool HasConflicts =>
        Conflicts > 0;

    /// <summary>
    /// Indica si existe al menos un campo con información
    /// disponible solamente en una de las dos fuentes.
    ///
    /// Esta propiedad describe la integridad de la comparación,
    /// pero no determina su nivel de confianza.
    /// </summary>
    public bool HasSingleSourceValues =>
        MissingLocalValues > 0 ||
        MissingReferenceValues > 0;

    /// <summary>
    /// Indica si existen campos aplicables sin información
    /// en ninguna de las dos fuentes.
    /// </summary>
    public bool HasFieldsMissingFromBothSources =>
        MissingBothValues > 0;

    /// <summary>
    /// Indica si existe al menos un campo realmente comparable,
    /// es decir, con valores disponibles en ambas fuentes.
    /// </summary>
    public bool HasComparableFields =>
        ComparableFields > 0;
}