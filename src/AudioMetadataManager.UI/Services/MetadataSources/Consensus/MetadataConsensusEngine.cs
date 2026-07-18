using AudioMetadataManager.UI.Services.MetadataSources.Matching;
using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources.Consensus;

/// <summary>
/// Coordina el proceso de consenso entre varios resultados
/// previamente evaluados por MetadataMatchEngine.
///
/// Este motor no consulta Internet ni modifica archivos.
/// Su responsabilidad es convertir varios candidatos externos
/// en una única propuesta estructurada de metadatos.
/// </summary>
public class MetadataConsensusEngine
{
    private readonly MetadataConsensusCalculator
        _consensusCalculator = new();

    /// <summary>
    /// Construye un resultado completo de consenso a partir
    /// de coincidencias previamente evaluadas.
    /// </summary>
    /// <param name="matchResults">
    /// Resultados producidos por MetadataMatchEngine.
    /// </param>
    public MetadataConsensusResult Build(
        IEnumerable<MetadataMatchResult> matchResults)
    {
        ArgumentNullException.ThrowIfNull(matchResults);

        /*
         * Conservamos solamente coincidencias utilizables.
         *
         * Además, si una plataforma entregó varios candidatos,
         * usamos solamente el candidato mejor puntuado de esa
         * plataforma para evitar que una sola fuente obtenga
         * varios votos dentro del consenso.
         */
        IReadOnlyList<MetadataMatchResult> usableMatches =
            SelectBestMatchPerSource(matchResults);

        MetadataConsensusResult result = new();

        if (usableMatches.Count == 0)
        {
            result.Summary =
                "No existen coincidencias externas utilizables " +
                "para construir un consenso.";

            return result;
        }

        result.Artist =
            _consensusCalculator.Calculate(
                "Artista",
                BuildTextValues(
                    usableMatches,
                    candidate => candidate.Artist));

        result.Title =
            _consensusCalculator.Calculate(
                "Título",
                BuildTextValues(
                    usableMatches,
                    candidate => candidate.Title));

        result.Version =
            _consensusCalculator.Calculate(
                "Versión",
                BuildTextValues(
                    usableMatches,
                    candidate => candidate.Version));

        result.Album =
            _consensusCalculator.Calculate(
                "Álbum",
                BuildTextValues(
                    usableMatches,
                    candidate => candidate.ReleaseTitle));

        result.Genre =
            _consensusCalculator.Calculate(
                "Género",
                BuildTextValues(
                    usableMatches,
                    candidate => candidate.Genre));

        result.Year =
            _consensusCalculator.Calculate(
                "Año",
                BuildYearValues(usableMatches));

        result.Label =
            _consensusCalculator.Calculate(
                "Sello",
                BuildTextValues(
                    usableMatches,
                    candidate => candidate.Label));

        result.Duration =
            _consensusCalculator.Calculate(
                "Duración",
                BuildDurationValues(usableMatches));

        result.CoverArt =
            _consensusCalculator.Calculate(
                "Portada",
                BuildTextValues(
                    usableMatches,
                    candidate => candidate.ArtworkUrl));

        /*
         * Estos campos todavía no existen en MetadataCandidate.
         * Se conservan vacíos hasta que los proveedores reales
         * puedan entregarlos.
         */
        result.CatalogNumber =
            CreateUnavailableField(
                "Número de catálogo");

        result.Isrc =
            CreateUnavailableField(
                "ISRC");

        result.Bpm =
            CreateUnavailableField(
                "BPM");

        result.MusicalKey =
            CreateUnavailableField(
                "Tonalidad");

        result.GeneratedAt =
            DateTime.Now;

        result.Summary =
            BuildSummary(
                result,
                usableMatches);

        return result;
    }

    /// <summary>
    /// Selecciona como máximo un resultado por plataforma.
    ///
    /// Si una plataforma entregó más de un candidato,
    /// se conserva el de mayor FinalScore.
    /// </summary>
    private static IReadOnlyList<MetadataMatchResult>
        SelectBestMatchPerSource(
            IEnumerable<MetadataMatchResult> matchResults)
    {
        return matchResults
            .Where(
                result =>
                    result != null &&
                    result.IsUsableMatch &&
                    !string.IsNullOrWhiteSpace(
                        result.Candidate.SourceName))
            .GroupBy(
                result =>
                    result.Candidate.SourceName,
                StringComparer.OrdinalIgnoreCase)
            .Select(
                group =>
                    group
                        .OrderByDescending(
                            result => result.FinalScore)
                        .ThenBy(
                            result =>
                                result.Candidate.SourceRank)
                        .First())
            .OrderByDescending(
                result => result.FinalScore)
            .ToList();
    }

    /// <summary>
    /// Construye un diccionario Fuente → Valor para un campo
    /// textual de MetadataCandidate.
    /// </summary>
    private static IReadOnlyDictionary<string, string?>
        BuildTextValues(
            IEnumerable<MetadataMatchResult> matches,
            Func<MetadataCandidate, string> valueSelector)
    {
        Dictionary<string, string?> values =
            new(StringComparer.OrdinalIgnoreCase);

        foreach (MetadataMatchResult match in matches)
        {
            string sourceName =
                match.Candidate.SourceName.Trim();

            string value =
                valueSelector(match.Candidate)?.Trim() ??
                string.Empty;

            values[sourceName] =
                value;
        }

        return values;
    }

    /// <summary>
    /// Convierte los años válidos en texto para que puedan
    /// utilizar el mismo MetadataConsensusCalculator.
    ///
    /// El año cero representa información no disponible.
    /// </summary>
    private static IReadOnlyDictionary<string, string?>
        BuildYearValues(
            IEnumerable<MetadataMatchResult> matches)
    {
        Dictionary<string, string?> values =
            new(StringComparer.OrdinalIgnoreCase);

        foreach (MetadataMatchResult match in matches)
        {
            string sourceName =
                match.Candidate.SourceName.Trim();

            uint year =
                match.Candidate.Year;

            values[sourceName] =
                year == 0
                    ? string.Empty
                    : year.ToString();
        }

        return values;
    }

    /// <summary>
    /// Convierte las duraciones válidas al formato mm:ss.
    ///
    /// Esta primera implementación trabaja con valores
    /// textuales exactos. Más adelante agregaremos tolerancia
    /// específica para pequeñas diferencias de segundos.
    /// </summary>
    private static IReadOnlyDictionary<string, string?>
        BuildDurationValues(
            IEnumerable<MetadataMatchResult> matches)
    {
        Dictionary<string, string?> values =
            new(StringComparer.OrdinalIgnoreCase);

        foreach (MetadataMatchResult match in matches)
        {
            string sourceName =
                match.Candidate.SourceName.Trim();

            TimeSpan duration =
                match.Candidate.Duration;

            values[sourceName] =
                duration <= TimeSpan.Zero
                    ? string.Empty
                    : FormatDuration(duration);
        }

        return values;
    }

    /// <summary>
    /// Formatea una duración para presentación y consenso.
    /// </summary>
    private static string FormatDuration(
        TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
        {
            return duration.ToString(
                @"h\:mm\:ss");
        }

        return duration.ToString(
            @"m\:ss");
    }

    /// <summary>
    /// Crea un campo para el que todavía no existe información
    /// en el modelo MetadataCandidate.
    /// </summary>
    private static MetadataConsensusField
        CreateUnavailableField(
            string fieldName)
    {
        return new MetadataConsensusField
        {
            FieldName = fieldName,

            SelectedValue =
                string.Empty,

            ConfidenceScore =
                0,

            HasConflict =
                false,

            RequiresSourceApproval =
                false,

            RequiresManualReview =
                true,

            Reason =
                "El modelo actual todavía no recibe este dato " +
                "desde las plataformas externas."
        };
    }

    /// <summary>
    /// Genera una explicación general del consenso.
    /// </summary>
    private static string BuildSummary(
        MetadataConsensusResult result,
        IReadOnlyList<MetadataMatchResult> usableMatches)
    {
        List<string> sourceNames =
            usableMatches
                .Select(
                    match =>
                        match.Candidate.SourceName)
                .Where(
                    source =>
                        !string.IsNullOrWhiteSpace(source))
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    source => source)
                .ToList();

        string sourcesDisplay =
            sourceNames.Count == 0
                ? "Sin fuentes"
                : string.Join(
                    ", ",
                    sourceNames);

        return
            $"Consenso construido con " +
            $"{usableMatches.Count} coincidencia(s) " +
            $"procedente(s) de: {sourcesDisplay}. " +
            $"{result.ProposedFieldCount} campo(s) " +
            $"contienen una propuesta. " +
            $"{result.ConflictCount} conflicto(s) detectado(s). " +
            $"{result.ManualReviewCount} campo(s) requieren " +
            $"revisión manual. " +
            $"Confianza promedio: " +
            $"{result.AverageConfidenceScore}%.";
    }
}