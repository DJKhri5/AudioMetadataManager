namespace AudioMetadataManager.UI.Services.MetadataSources.Consensus;

/// <summary>
/// Representa el resultado completo del proceso de consenso
/// para un archivo de audio.
///
/// Cada propiedad contiene la decisión tomada para un campo
/// específico, como Artista, Título, Versión o Año.
/// </summary>
public class MetadataConsensusResult
{
    /// <summary>
    /// Consenso obtenido para el artista.
    /// </summary>
    public MetadataConsensusField Artist { get; set; } =
        CreateField("Artista");

    /// <summary>
    /// Consenso obtenido para el título.
    /// </summary>
    public MetadataConsensusField Title { get; set; } =
        CreateField("Título");

    /// <summary>
    /// Consenso obtenido para la versión musical.
    ///
    /// Ejemplos:
    /// Extended Mix
    /// Original Mix
    /// Radio Edit
    /// Remix
    /// </summary>
    public MetadataConsensusField Version { get; set; } =
        CreateField("Versión");

    /// <summary>
    /// Consenso obtenido para el álbum o lanzamiento.
    /// </summary>
    public MetadataConsensusField Album { get; set; } =
        CreateField("Álbum");

    /// <summary>
    /// Consenso obtenido para el género musical.
    /// </summary>
    public MetadataConsensusField Genre { get; set; } =
        CreateField("Género");

    /// <summary>
    /// Consenso obtenido para el año.
    /// El valor se conserva como texto para mantener
    /// un modelo común entre todos los campos.
    /// </summary>
    public MetadataConsensusField Year { get; set; } =
        CreateField("Año");

    /// <summary>
    /// Consenso obtenido para el sello discográfico.
    /// </summary>
    public MetadataConsensusField Label { get; set; } =
        CreateField("Sello");

    /// <summary>
    /// Consenso obtenido para el número de catálogo.
    /// </summary>
    public MetadataConsensusField CatalogNumber { get; set; } =
        CreateField("Número de catálogo");

    /// <summary>
    /// Consenso obtenido para el código ISRC.
    /// </summary>
    public MetadataConsensusField Isrc { get; set; } =
        CreateField("ISRC");

    /// <summary>
    /// Consenso obtenido para los BPM.
    /// </summary>
    public MetadataConsensusField Bpm { get; set; } =
        CreateField("BPM");

    /// <summary>
    /// Consenso obtenido para la tonalidad musical.
    /// </summary>
    public MetadataConsensusField MusicalKey { get; set; } =
        CreateField("Tonalidad");

    /// <summary>
    /// Consenso obtenido para la duración.
    /// </summary>
    public MetadataConsensusField Duration { get; set; } =
        CreateField("Duración");

    /// <summary>
    /// Consenso obtenido para la portada.
    ///
    /// En una fase posterior, SelectedValue podrá contener
    /// una dirección o referencia interna de la imagen.
    /// </summary>
    public MetadataConsensusField CoverArt { get; set; } =
        CreateField("Portada");

    /// <summary>
    /// Fecha y hora en que se generó el consenso.
    /// </summary>
    public DateTime GeneratedAt { get; set; } =
        DateTime.Now;

    /// <summary>
    /// Explicación general del resultado.
    /// </summary>
    public string Summary { get; set; } =
        string.Empty;

    /// <summary>
    /// Devuelve todos los campos del consenso en una lista.
    ///
    /// Esto permitirá que el motor, la interfaz y el sistema
    /// de exportación recorran todos los campos sin repetir
    /// código para cada propiedad.
    /// </summary>
    public IReadOnlyList<MetadataConsensusField> Fields =>
        new List<MetadataConsensusField>
        {
            Artist,
            Title,
            Version,
            Album,
            Genre,
            Year,
            Label,
            CatalogNumber,
            Isrc,
            Bpm,
            MusicalKey,
            Duration,
            CoverArt
        };

    /// <summary>
    /// Campos que contienen una propuesta utilizable.
    /// </summary>
    public IReadOnlyList<MetadataConsensusField> ProposedFields =>
        Fields
            .Where(field => field.HasSelectedValue)
            .ToList();

    /// <summary>
    /// Campos en los que las fuentes entregaron
    /// información contradictoria.
    /// </summary>
    public IReadOnlyList<MetadataConsensusField> ConflictingFields =>
        Fields
            .Where(field => field.HasConflict)
            .ToList();

    /// <summary>
    /// Campos que necesitan revisión manual.
    /// </summary>
    public IReadOnlyList<MetadataConsensusField> ManualReviewFields =>
        Fields
            .Where(field => field.RequiresManualReview)
            .ToList();

    /// <summary>
    /// Cantidad de campos con una propuesta utilizable.
    /// </summary>
    public int ProposedFieldCount =>
        ProposedFields.Count;

    /// <summary>
    /// Cantidad de conflictos detectados.
    /// </summary>
    public int ConflictCount =>
        ConflictingFields.Count;

    /// <summary>
    /// Cantidad de campos que requieren revisión manual.
    /// </summary>
    public int ManualReviewCount =>
        ManualReviewFields.Count;

    /// <summary>
    /// Indica si existe al menos una propuesta disponible.
    /// </summary>
    public bool HasProposal =>
        ProposedFieldCount > 0;

    /// <summary>
    /// Indica si el resultado completo contiene conflictos.
    /// </summary>
    public bool HasConflicts =>
        ConflictCount > 0;

    /// <summary>
    /// Indica si al menos un campo necesita revisión manual.
    /// </summary>
    public bool RequiresManualReview =>
        ManualReviewCount > 0;

    /// <summary>
    /// Calcula la confianza promedio de los campos que
    /// contienen una propuesta.
    ///
    /// Los campos vacíos no participan en el promedio.
    /// </summary>
    public int AverageConfidenceScore
    {
        get
        {
            IReadOnlyList<MetadataConsensusField> proposedFields =
                ProposedFields;

            if (proposedFields.Count == 0)
            {
                return 0;
            }

            double average =
                proposedFields.Average(
                    field => field.ConfidenceScore);

            return Math.Clamp(
                (int)Math.Round(average),
                0,
                100);
        }
    }

    /// <summary>
    /// Nivel descriptivo de la confianza promedio.
    /// </summary>
    public string AverageConfidenceLevel =>
        AverageConfidenceScore switch
        {
            >= 95 => "Muy alta",
            >= 85 => "Alta",
            >= 70 => "Media",
            >= 50 => "Baja",
            _ => "Muy baja"
        };

    /// <summary>
    /// Texto legible para mostrar el estado general
    /// del resultado de consenso.
    /// </summary>
    public string StatusDisplay
    {
        get
        {
            if (!HasProposal)
            {
                return "Sin propuesta";
            }

            if (HasConflicts)
            {
                return "Conflictos detectados";
            }

            if (RequiresManualReview)
            {
                return "Revisión manual";
            }

            return "Consenso confiable";
        }
    }

    /// <summary>
    /// Genera un resumen automático del resultado.
    ///
    /// Si Summary contiene un texto personalizado,
    /// ese texto tendrá prioridad.
    /// </summary>
    public string SummaryDisplay
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(Summary))
            {
                return Summary;
            }

            if (!HasProposal)
            {
                return
                    "No se obtuvo una propuesta de metadatos utilizable.";
            }

            return
                $"{ProposedFieldCount} campo(s) propuesto(s) · " +
                $"{ConflictCount} conflicto(s) · " +
                $"{ManualReviewCount} campo(s) con revisión manual · " +
                $"Confianza promedio {AverageConfidenceScore}%.";
        }
    }

    /// <summary>
    /// Crea un campo inicialmente vacío con revisión manual
    /// activada hasta que el motor de consenso lo evalúe.
    /// </summary>
    private static MetadataConsensusField CreateField(
        string fieldName)
    {
        return new MetadataConsensusField
        {
            FieldName = fieldName,
            RequiresManualReview = true
        };
    }
}