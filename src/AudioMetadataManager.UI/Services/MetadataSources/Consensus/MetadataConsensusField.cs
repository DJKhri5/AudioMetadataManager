namespace AudioMetadataManager.UI.Services.MetadataSources.Consensus;

/// <summary>
/// Representa la decisión de consenso para un único campo
/// de metadatos, como Artista, Título, Versión o Año.
/// </summary>
public class MetadataConsensusField
{
    /// <summary>
    /// Nombre legible del campo evaluado.
    ///
    /// Ejemplos:
    /// Artista
    /// Título
    /// Versión
    /// Año
    /// Sello
    /// ISRC
    /// </summary>
    public string FieldName { get; set; } =
        string.Empty;

    /// <summary>
    /// Valor finalmente elegido por el motor de consenso.
    ///
    /// Este valor todavía es una propuesta y no modifica
    /// automáticamente el archivo.
    /// </summary>
    public string SelectedValue { get; set; } =
        string.Empty;

    /// <summary>
    /// Puntuación de confianza del valor elegido.
    /// El rango previsto es de 0 a 100.
    /// </summary>
    public int ConfidenceScore { get; set; }

    /// <summary>
    /// Fuentes externas que respaldan el valor elegido.
    ///
    /// Ejemplo:
    /// Beatport, Discogs y Spotify.
    /// </summary>
    public List<string> SupportingSources { get; set; } =
        new();

    /// <summary>
    /// Valores alternativos encontrados durante el consenso.
    ///
    /// La clave representa el nombre de la fuente.
    /// El valor representa el dato entregado por esa fuente.
    /// </summary>
    public Dictionary<string, string> AlternativeValues { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Indica si las fuentes externas entregaron valores
    /// suficientemente distintos como para considerarlos
    /// un conflicto.
    /// </summary>
    public bool HasConflict { get; set; }

    /// <summary>
    /// Indica si el usuario debe revisar este campo antes
    /// de aplicarlo.
    /// </summary>
    public bool RequiresManualReview { get; set; } = true;

    /// <summary>
    /// Indica si alguna fuente que respalda el valor exige
    /// aprobación manual obligatoria, como SoundCloud.
    /// </summary>
    public bool RequiresSourceApproval { get; set; }

    /// <summary>
    /// Explicación legible de cómo se obtuvo el consenso.
    /// </summary>
    public string Reason { get; set; } =
        string.Empty;

    /// <summary>
    /// Indica si existe un valor seleccionado utilizable.
    /// </summary>
    public bool HasSelectedValue =>
        !string.IsNullOrWhiteSpace(
            SelectedValue);

    /// <summary>
    /// Cantidad de fuentes que respaldan el valor elegido.
    /// </summary>
    public int SupportingSourceCount =>
        SupportingSources.Count;

    /// <summary>
    /// Nivel descriptivo de confianza.
    /// </summary>
    public string ConfidenceLevel =>
        ConfidenceScore switch
        {
            >= 95 => "Muy alta",
            >= 85 => "Alta",
            >= 70 => "Media",
            >= 50 => "Baja",
            _ => "Muy baja"
        };

    /// <summary>
    /// Lista legible de las fuentes que respaldan
    /// el valor seleccionado.
    /// </summary>
    public string SupportingSourcesDisplay =>
        SupportingSources.Count == 0
            ? "Sin fuentes de respaldo"
            : string.Join(
                ", ",
                SupportingSources);

    /// <summary>
    /// Texto legible para mostrar si existe conflicto.
    /// </summary>
    public string ConflictDisplay =>
        HasConflict
            ? "Sí"
            : "No";

    /// <summary>
    /// Texto legible para mostrar si requiere revisión.
    /// </summary>
    public string ManualReviewDisplay =>
        RequiresManualReview
            ? "Sí"
            : "No";

    /// <summary>
    /// Resumen compacto para la interfaz.
    /// </summary>
    public string Summary
    {
        get
        {
            if (!HasSelectedValue)
            {
                return
                    $"{FieldName}: sin una propuesta utilizable.";
            }

            string conflictText =
                HasConflict
                    ? "Conflicto detectado."
                    : "Sin conflicto.";

            string reviewText =
                RequiresManualReview
                    ? "Requiere revisión manual."
                    : "No requiere revisión manual.";

            return
                $"{FieldName}: {SelectedValue} · " +
                $"Confianza {ConfidenceScore}% · " +
                $"{conflictText} · " +
                $"{reviewText}";
        }
    }
}