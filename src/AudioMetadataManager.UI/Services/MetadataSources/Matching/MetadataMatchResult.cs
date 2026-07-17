using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources.Matching;

/// <summary>
/// Representa el resultado de comparar un candidato externo
/// con la información disponible en el archivo local.
/// </summary>
public class MetadataMatchResult
{
    /// <summary>
    /// Candidato externo que fue evaluado.
    /// </summary>
    public MetadataCandidate Candidate { get; set; } = new();

    /// <summary>
    /// Puntuación obtenida por la coincidencia del artista.
    /// Rango previsto: 0 a 100.
    /// </summary>
    public int ArtistScore { get; set; }

    /// <summary>
    /// Puntuación obtenida por la coincidencia del título.
    /// Rango previsto: 0 a 100.
    /// </summary>
    public int TitleScore { get; set; }

    /// <summary>
    /// Puntuación obtenida por la coincidencia de la versión.
    /// Ejemplos: Extended Mix, Original Mix o Radio Edit.
    /// </summary>
    public int VersionScore { get; set; }

    /// <summary>
    /// Puntuación obtenida al comparar las duraciones.
    /// </summary>
    public int DurationScore { get; set; }

    /// <summary>
    /// Puntuación obtenida al comparar el año.
    /// </summary>
    public int YearScore { get; set; }

    /// <summary>
    /// Puntuación final ponderada.
    /// Este será el valor utilizado para ordenar candidatos.
    /// </summary>
    public int FinalScore { get; set; }

    /// <summary>
    /// Indica si el candidato necesita revisión manual.
    /// </summary>
    public bool RequiresManualReview { get; set; } = true;

    /// <summary>
    /// Indica si los datos provienen de una fuente que exige
    /// aprobación manual obligatoria, como SoundCloud.
    /// </summary>
    public bool RequiresSourceApproval { get; set; }

    /// <summary>
    /// Explicaciones generadas durante la comparación.
    /// Ejemplos:
    /// "El título coincide"
    /// "La duración difiere en 3 segundos"
    /// </summary>
    public List<string> Reasons { get; set; } = new();

    /// <summary>
    /// Nivel descriptivo asociado a la puntuación final.
    /// </summary>
    public string ConfidenceLevel =>
        FinalScore switch
        {
            >= 95 => "Muy alta",
            >= 85 => "Alta",
            >= 70 => "Media",
            >= 50 => "Baja",
            _ => "Muy baja"
        };

    /// <summary>
    /// Indica si el candidato alcanza un nivel suficiente
    /// para considerarse una coincidencia útil.
    /// No implica aplicación automática.
    /// </summary>
    public bool IsUsableMatch =>
        Candidate.HasIdentity &&
        FinalScore >= 70;

    /// <summary>
    /// Resumen legible de las razones de la puntuación.
    /// </summary>
    public string Summary =>
        Reasons.Count == 0
            ? "No existen detalles de comparación."
            : string.Join(" · ", Reasons);

    /// <summary>
    /// Texto compacto para mostrar el resultado en la interfaz.
    /// </summary>
    public string DisplayName =>
        $"{Candidate.DisplayName} · " +
        $"{Candidate.SourceName} · " +
        $"{FinalScore}%";
}