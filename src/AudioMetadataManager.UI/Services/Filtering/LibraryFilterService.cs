using System.Globalization;
using System.Text;
using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.Filtering.Models;

namespace AudioMetadataManager.UI.Services.Filtering;

/// <summary>
/// Servicio de búsqueda y filtrado en tiempo real para la biblioteca de archivos de audio.
/// Soporta búsqueda multi-término insensible a mayúsculas y acentos, y filtros por formato, estado y calidad.
/// </summary>
public sealed class LibraryFilterService : ILibraryFilterService
{
    public bool Matches(AudioFile file, LibraryFilterCriteria criteria)
    {
        if (file is null)
        {
            return false;
        }

        if (criteria is null || !criteria.HasActiveFilters)
        {
            return true;
        }

        // 1. Filtrado por Formato
        if (!MatchesFormat(file, criteria.FormatFilter))
        {
            return false;
        }

        // 2. Filtrado por Estado de Análisis / Simulación
        if (!MatchesStatus(file, criteria.StatusFilter))
        {
            return false;
        }

        // 3. Filtrado por Calidad de Audio
        if (!MatchesQuality(file, criteria.QualityFilter))
        {
            return false;
        }

        // 4. Búsqueda de Texto Libre
        if (!MatchesSearchText(file, criteria.SearchText))
        {
            return false;
        }

        return true;
    }

    public IReadOnlyList<AudioFile> Filter(IEnumerable<AudioFile> files, LibraryFilterCriteria criteria)
    {
        if (files is null)
        {
            return Array.Empty<AudioFile>();
        }

        if (criteria is null || !criteria.HasActiveFilters)
        {
            return files.ToList();
        }

        return files.Where(f => Matches(f, criteria)).ToList();
    }

    private static bool MatchesFormat(AudioFile file, string? formatFilter)
    {
        if (string.IsNullOrWhiteSpace(formatFilter) ||
            string.Equals(formatFilter, "Todos", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        string ext = (file.Extension ?? string.Empty).ToLowerInvariant().Trim();

        return formatFilter.ToUpperInvariant() switch
        {
            "MP3" => ext == ".mp3",
            "FLAC" => ext == ".flac",
            "WAV" => ext == ".wav",
            "AIFF" => ext is ".aiff" or ".aif",
            "M4A / AAC" or "M4A" or "AAC" => ext is ".m4a" or ".aac" or ".mp4",
            _ => ext.Contains(formatFilter.ToLowerInvariant())
        };
    }

    private static bool MatchesStatus(AudioFile file, string? statusFilter)
    {
        if (string.IsNullOrWhiteSpace(statusFilter) ||
            string.Equals(statusFilter, "Todos", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return statusFilter switch
        {
            "Con propuesta de cambio" =>
                file.Simulation?.HasChanges == true ||
                file.CanRenameSafely ||
                string.Equals(file.Status, "Con cambios", StringComparison.OrdinalIgnoreCase),

            "Sin cambios" =>
                file.Simulation is not null && !file.Simulation.HasChanges,

            "Sin analizar" =>
                file.Analysis is null && file.Simulation is null,

            "Requiere revisión" =>
                file.Analysis?.RequiresManualReview == true ||
                file.Simulation?.RequiresManualReview == true,

            _ => true
        };
    }

    private static bool MatchesQuality(AudioFile file, string? qualityFilter)
    {
        if (string.IsNullOrWhiteSpace(qualityFilter) ||
            string.Equals(qualityFilter, "Todos", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        string ext = (file.Extension ?? string.Empty).ToLowerInvariant().Trim();
        bool isLossless = file.QualityAnalysis?.IsLossless == true ||
                          ext is ".flac" or ".wav" or ".aiff" or ".aif" or ".alac" or ".ape";

        return qualityFilter switch
        {
            "Lossless" => isLossless,
            "≥ 320 kbps" => file.Bitrate >= 320,
            "< 320 kbps" => file.Bitrate > 0 && file.Bitrate < 320,
            _ => true
        };
    }

    private static bool MatchesSearchText(AudioFile file, string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return true;
        }

        string normalizedQuery = NormalizeString(searchText);
        string[] searchTerms = normalizedQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (searchTerms.Length == 0)
        {
            return true;
        }

        // Construir contenido consolidado del archivo para búsqueda
        var sb = new StringBuilder();
        sb.Append(' ').Append(file.FileName);
        sb.Append(' ').Append(file.FullPath);
        sb.Append(' ').Append(file.Artist);
        sb.Append(' ').Append(file.ParsedName?.Artist);
        sb.Append(' ').Append(file.Title);
        sb.Append(' ').Append(file.ParsedName?.Title);
        sb.Append(' ').Append(file.Version);
        sb.Append(' ').Append(file.ParsedName?.Version);
        sb.Append(' ').Append(file.Album);
        sb.Append(' ').Append(file.Genre);
        sb.Append(' ').Append(file.Label);
        sb.Append(' ').Append(file.Simulation?.ProposedFileName);

        string normalizedTarget = NormalizeString(sb.ToString());

        // Todos los términos de búsqueda deben estar presentes (AND lógico)
        foreach (string term in searchTerms)
        {
            if (!normalizedTarget.Contains(term, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Elimina tildes/acentos y convierte la cadena a minúsculas para comparaciones normalizadas.
    /// </summary>
    public static string NormalizeString(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        string normalizedFormD = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalizedFormD.Length);

        foreach (char c in normalizedFormD)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }
}
