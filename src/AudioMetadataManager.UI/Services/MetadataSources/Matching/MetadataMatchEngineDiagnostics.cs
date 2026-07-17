using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources.Matching;

/// <summary>
/// Ejecuta pruebas controladas del MetadataMatchEngine.
///
/// Esta clase se utiliza durante el desarrollo para comprobar
/// que el motor asigna puntuaciones coherentes antes de
/// conectarlo con plataformas externas reales.
/// </summary>
public static class MetadataMatchEngineDiagnostics
{
    /// <summary>
    /// Ejecuta una comparación controlada con:
    /// - un candidato correcto;
    /// - un candidato incorrecto;
    /// - un candidato procedente de SoundCloud.
    /// </summary>
    public static IReadOnlyList<MetadataMatchResult> Run()
    {
        MetadataSearchRequest request =
            CreateLocalRequest();

        List<MetadataCandidate> candidates =
            CreateCandidates();

        MetadataMatchEngine engine = new();

        List<MetadataMatchResult> results = new();

        foreach (MetadataCandidate candidate in candidates)
        {
            bool requiresSourceApproval =
                string.Equals(
                    candidate.SourceName,
                    "SoundCloud",
                    StringComparison.OrdinalIgnoreCase);

            MetadataMatchResult result =
                engine.Evaluate(
                    request,
                    candidate,
                    requiresSourceApproval);

            results.Add(result);
        }

        return results
            .OrderByDescending(
                result => result.FinalScore)
            .ToList();
    }

    /// <summary>
    /// Genera un informe de texto para mostrar posteriormente
    /// en el registro de actividad.
    /// </summary>
    public static string BuildReport()
    {
        IReadOnlyList<MetadataMatchResult> results =
            Run();

        List<string> lines = new()
        {
            "=== Diagnóstico del MetadataMatchEngine ==="
        };

        foreach (MetadataMatchResult result in results)
        {
            lines.Add(string.Empty);

            lines.Add(
                $"Fuente: {result.Candidate.SourceName}");

            lines.Add(
                $"Candidato: {result.Candidate.DisplayName}");

            lines.Add(
                $"Artista: {result.ArtistScore}%");

            lines.Add(
                $"Título: {result.TitleScore}%");

            lines.Add(
                $"Versión: {result.VersionScore}%");

            lines.Add(
                $"Duración: {result.DurationScore}%");

            lines.Add(
                $"Año: {result.YearScore}%");

            lines.Add(
                $"Puntuación final: {result.FinalScore}%");

            lines.Add(
                $"Nivel: {result.ConfidenceLevel}");

            lines.Add(
                $"Revisión manual: " +
                $"{ToSpanish(result.RequiresManualReview)}");

            lines.Add(
                $"Aprobación de fuente: " +
                $"{ToSpanish(result.RequiresSourceApproval)}");

            lines.Add(
                $"Resumen: {result.Summary}");
        }

        lines.Add(string.Empty);
        lines.Add("=== Fin del diagnóstico ===");

        return string.Join(
            Environment.NewLine,
            lines);
    }

    private static MetadataSearchRequest CreateLocalRequest()
    {
        return new MetadataSearchRequest
        {
            FileName =
                "ben-nicky-x-uberjak-d-x-trey-pearce-" +
                "relapse-extended-mix.mp3",

            ParsedArtist =
                "Ben Nicky x Uberjak D x Trey Pearce",

            ParsedTitle =
                "Relapse",

            ParsedVersion =
                "Extended Mix",

            TaggedArtist =
                string.Empty,

            TaggedTitle =
                string.Empty,

            TaggedAlbum =
                string.Empty,

            TaggedYear =
                2024,

            Duration =
                TimeSpan.FromMinutes(4) +
                TimeSpan.FromSeconds(32)
        };
    }

    private static List<MetadataCandidate> CreateCandidates()
    {
        return new List<MetadataCandidate>
        {
            CreateCorrectBeatportCandidate(),
            CreateIncorrectSpotifyCandidate(),
            CreateSoundCloudCandidate()
        };
    }

    private static MetadataCandidate
        CreateCorrectBeatportCandidate()
    {
        return new MetadataCandidate
        {
            SourceName = "Beatport",
            SourceId = "beatport-test-001",

            Artist =
                "Ben Nicky x Uberjak'd x Trey Pearce",

            Title = "Relapse",
            Version = "Extended Mix",

            ReleaseTitle = "Relapse",
            Label = "Test Label",
            Genre = "Trance",

            Year = 2024,

            Duration =
                TimeSpan.FromMinutes(4) +
                TimeSpan.FromSeconds(34),

            SourceRank = 1
        };
    }

    private static MetadataCandidate
        CreateIncorrectSpotifyCandidate()
    {
        return new MetadataCandidate
        {
            SourceName = "Spotify",
            SourceId = "spotify-test-001",

            Artist = "David Elder",
            Title = "Relax My Eyes",
            Version = "Extended Mix",

            ReleaseTitle = "Relax My Eyes",
            Genre = "Electronic",

            Year = 2023,

            Duration =
                TimeSpan.FromMinutes(4) +
                TimeSpan.FromSeconds(31),

            SourceRank = 1
        };
    }

    private static MetadataCandidate
        CreateSoundCloudCandidate()
    {
        return new MetadataCandidate
        {
            SourceName = "SoundCloud",
            SourceId = "soundcloud-test-001",

            Artist =
                "Ben Nicky x Uberjak'd x Trey Pearce",

            Title = "Relapse",
            Version = "Extended Mix",

            Year = 2024,

            Duration =
                TimeSpan.FromMinutes(4) +
                TimeSpan.FromSeconds(33),

            SourceRank = 1
        };
    }

    private static string ToSpanish(bool value)
    {
        return value
            ? "Sí"
            : "No";
    }
}