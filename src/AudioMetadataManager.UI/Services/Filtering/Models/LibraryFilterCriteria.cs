namespace AudioMetadataManager.UI.Services.Filtering.Models;

/// <summary>
/// Criterios de filtrado y búsqueda en tiempo real para la biblioteca de pistas.
/// </summary>
public sealed class LibraryFilterCriteria
{
    /// <summary>
    /// Texto libre de búsqueda (coincide con Artista, Título, Álbum, Sello, Nombre de archivo, etc.).
    /// </summary>
    public string SearchText { get; set; } = string.Empty;

    /// <summary>
    /// Filtro por formato de archivo (ej. "Todos", "MP3", "FLAC", "WAV", "AIFF", "M4A / AAC").
    /// </summary>
    public string FormatFilter { get; set; } = "Todos";

    /// <summary>
    /// Filtro por estado de análisis/simulación (ej. "Todos", "Con propuesta de cambio", "Sin cambios", "Sin analizar", "Requiere revisión").
    /// </summary>
    public string StatusFilter { get; set; } = "Todos";

    /// <summary>
    /// Filtro por calidad de audio (ej. "Todos", "Lossless", "≥ 320 kbps", "< 320 kbps").
    /// </summary>
    public string QualityFilter { get; set; } = "Todos";

    /// <summary>
    /// Indica si hay algún criterio de filtrado activo distinto a los valores por defecto.
    /// </summary>
    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(SearchText) ||
        !string.Equals(FormatFilter, "Todos", StringComparison.OrdinalIgnoreCase) ||
        !string.Equals(StatusFilter, "Todos", StringComparison.OrdinalIgnoreCase) ||
        !string.Equals(QualityFilter, "Todos", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Restablece todos los criterios a sus valores iniciales.
    /// </summary>
    public void Reset()
    {
        SearchText = string.Empty;
        FormatFilter = "Todos";
        StatusFilter = "Todos";
        QualityFilter = "Todos";
    }
}
