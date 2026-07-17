using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources.Matching;

/// <summary>
/// Compara candidatos obtenidos desde fuentes externas
/// con la información disponible en el archivo local.
/// </summary>
public class MetadataMatchEngine
{
    private readonly TextSimilarityCalculator _textSimilarity =
        new();

    private readonly ArtistSimilarityCalculator _artistSimilarity =
    new();

    /*
     * Pesos utilizados para obtener la puntuación final.
     *
     * Artista y título son los campos más importantes.
     * La versión también tiene un peso alto porque permite
     * distinguir Extended Mix, Original Mix, Remix, etc.
     *
     * Duración y año actúan como confirmaciones adicionales.
     */
    private const double ArtistWeight = 0.30;
    private const double TitleWeight = 0.35;
    private const double VersionWeight = 0.20;
    private const double DurationWeight = 0.10;
    private const double YearWeight = 0.05;

    /// <summary>
    /// Evalúa un candidato externo frente a la información
    /// local recopilada en MetadataSearchRequest.
    /// </summary>
    /// <param name="request">
    /// Información extraída desde el nombre del archivo,
    /// las etiquetas y los datos técnicos locales.
    /// </param>
    /// <param name="candidate">
    /// Resultado obtenido desde Discogs, Beatport,
    /// Spotify, SoundCloud u otra plataforma.
    /// </param>
    /// <param name="requiresSourceApproval">
    /// Indica si la plataforma exige aprobación manual
    /// obligatoria. SoundCloud utilizará true.
    /// </param>
    public MetadataMatchResult Evaluate(
        MetadataSearchRequest request,
        MetadataCandidate candidate,
        bool requiresSourceApproval = false)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(candidate);

        MetadataMatchResult result = new()
        {
            Candidate = candidate,
            RequiresSourceApproval = requiresSourceApproval
        };

        string localArtist =
            SelectLocalArtist(request);

        string localTitle =
            SelectLocalTitle(request);

        string localVersion =
            request.ParsedVersion;

        result.ArtistScore =
            _artistSimilarity.Calculate(
                localArtist,
                candidate.Artist);

        result.TitleScore =
            _textSimilarity.Calculate(
                localTitle,
                candidate.Title);

        result.VersionScore =
            CalculateVersionScore(
                localVersion,
                candidate.Version);

        result.DurationScore =
            CalculateDurationScore(
                request.Duration,
                candidate.Duration);

        result.YearScore =
            CalculateYearScore(
                request.TaggedYear,
                candidate.Year);

        result.FinalScore =
            CalculateFinalScore(
                request,
                candidate,
                result);

        result.RequiresManualReview =
            DetermineManualReview(result);

        AddArtistReason(
            result,
            localArtist,
            candidate.Artist);

        AddTitleReason(
            result,
            localTitle,
            candidate.Title);

        AddVersionReason(
            result,
            localVersion,
            candidate.Version);

        AddDurationReason(
            result,
            request.Duration,
            candidate.Duration);

        AddYearReason(
            result,
            request.TaggedYear,
            candidate.Year);

        AddFinalDecisionReason(result);

        return result;
    }

    /// <summary>
    /// Evalúa varios candidatos y los devuelve ordenados
    /// desde la puntuación más alta a la más baja.
    /// </summary>
    public IReadOnlyList<MetadataMatchResult> EvaluateAll(
        MetadataSearchRequest request,
        IEnumerable<MetadataCandidate> candidates,
        bool requiresSourceApproval = false)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(candidates);

        return candidates
            .Select(
                candidate =>
                    Evaluate(
                        request,
                        candidate,
                        requiresSourceApproval))
            .OrderByDescending(
                result => result.FinalScore)
            .ThenBy(
                result => result.Candidate.SourceRank)
            .ToList();
    }

    private static string SelectLocalArtist(
        MetadataSearchRequest request)
    {
        if (!string.IsNullOrWhiteSpace(
                request.ParsedArtist))
        {
            return request.ParsedArtist;
        }

        return request.TaggedArtist;
    }

    private static string SelectLocalTitle(
        MetadataSearchRequest request)
    {
        if (!string.IsNullOrWhiteSpace(
                request.ParsedTitle))
        {
            return request.ParsedTitle;
        }

        return request.TaggedTitle;
    }

    private int CalculateVersionScore(
        string? localVersion,
        string? candidateVersion)
    {
        bool localMissing =
            string.IsNullOrWhiteSpace(localVersion);

        bool candidateMissing =
            string.IsNullOrWhiteSpace(candidateVersion);

        /*
         * Si ninguna parte conoce la versión, no existe
         * información suficiente para compararla.
         */
        if (localMissing && candidateMissing)
        {
            return 0;
        }

        /*
         * Si solo una parte contiene versión, la coincidencia
         * no puede considerarse confirmada.
         */
        if (localMissing || candidateMissing)
        {
            return 0;
        }

        return _textSimilarity.Calculate(
            localVersion,
            candidateVersion);
    }

    private static int CalculateDurationScore(
        TimeSpan localDuration,
        TimeSpan candidateDuration)
    {
        if (localDuration <= TimeSpan.Zero ||
            candidateDuration <= TimeSpan.Zero)
        {
            return 0;
        }

        double differenceSeconds =
            Math.Abs(
                (localDuration - candidateDuration)
                .TotalSeconds);

        return differenceSeconds switch
        {
            <= 2 => 100,
            <= 5 => 95,
            <= 10 => 85,
            <= 20 => 70,
            <= 30 => 55,
            <= 60 => 30,
            _ => 0
        };
    }

    private static int CalculateYearScore(
        uint localYear,
        uint candidateYear)
    {
        if (localYear == 0 ||
            candidateYear == 0)
        {
            return 0;
        }

        int difference =
            Math.Abs(
                (int)localYear -
                (int)candidateYear);

        return difference switch
        {
            0 => 100,
            1 => 70,
            2 => 40,
            _ => 0
        };
    }

    private static int CalculateFinalScore(
        MetadataSearchRequest request,
        MetadataCandidate candidate,
        MetadataMatchResult result)
    {
        double weightedPoints = 0;
        double availableWeight = 0;

        /*
         * Artista
         */
        if (HasComparableText(
                SelectLocalArtist(request),
                candidate.Artist))
        {
            weightedPoints +=
                result.ArtistScore *
                ArtistWeight;

            availableWeight +=
                ArtistWeight;
        }

        /*
         * Título
         */
        if (HasComparableText(
                SelectLocalTitle(request),
                candidate.Title))
        {
            weightedPoints +=
                result.TitleScore *
                TitleWeight;

            availableWeight +=
                TitleWeight;
        }

        /*
         * Versión
         */
        if (HasComparableText(
                request.ParsedVersion,
                candidate.Version))
        {
            weightedPoints +=
                result.VersionScore *
                VersionWeight;

            availableWeight +=
                VersionWeight;
        }

        /*
         * Duración
         */
        if (request.Duration > TimeSpan.Zero &&
            candidate.Duration > TimeSpan.Zero)
        {
            weightedPoints +=
                result.DurationScore *
                DurationWeight;

            availableWeight +=
                DurationWeight;
        }

        /*
         * Año
         */
        if (request.TaggedYear > 0 &&
            candidate.Year > 0)
        {
            weightedPoints +=
                result.YearScore *
                YearWeight;

            availableWeight +=
                YearWeight;
        }

        /*
         * No castigamos automáticamente los campos ausentes.
         * La puntuación se calcula sobre la información que
         * realmente puede compararse.
         */
        if (availableWeight <= 0)
        {
            return 0;
        }

        double finalScore =
            weightedPoints /
            availableWeight;

        return Math.Clamp(
            (int)Math.Round(finalScore),
            0,
            100);
    }

    private static bool HasComparableText(
        string? first,
        string? second)
    {
        return
            !string.IsNullOrWhiteSpace(first) &&
            !string.IsNullOrWhiteSpace(second);
    }

    private static bool DetermineManualReview(
        MetadataMatchResult result)
    {
        /*
         * Una fuente como SoundCloud exige revisión manual
         * independientemente de la puntuación obtenida.
         */
        if (result.RequiresSourceApproval)
        {
            return true;
        }

        /*
         * Nunca se permite aplicación automática cuando
         * artista o título presentan coincidencias débiles.
         */
        if (result.ArtistScore < 80 ||
            result.TitleScore < 85)
        {
            return true;
        }

        /*
         * Solo las coincidencias con puntuación final alta
         * pueden dejar de requerir revisión manual.
         */
        return result.FinalScore < 90;
    }

    private static void AddArtistReason(
        MetadataMatchResult result,
        string localArtist,
        string candidateArtist)
    {
        if (!HasComparableText(
                localArtist,
                candidateArtist))
        {
            result.Reasons.Add(
                "No existe información suficiente para comparar el artista.");

            return;
        }

        string message =
            result.ArtistScore switch
            {
                >= 95 =>
                    "El artista coincide casi exactamente.",

                >= 80 =>
                    "El artista presenta una coincidencia alta.",

                >= 60 =>
                    "El artista presenta una coincidencia parcial.",

                _ =>
                    "El artista difiere considerablemente."
            };

        result.Reasons.Add(message);
    }

    private static void AddTitleReason(
        MetadataMatchResult result,
        string localTitle,
        string candidateTitle)
    {
        if (!HasComparableText(
                localTitle,
                candidateTitle))
        {
            result.Reasons.Add(
                "No existe información suficiente para comparar el título.");

            return;
        }

        string message =
            result.TitleScore switch
            {
                >= 95 =>
                    "El título coincide casi exactamente.",

                >= 80 =>
                    "El título presenta una coincidencia alta.",

                >= 60 =>
                    "El título presenta una coincidencia parcial.",

                _ =>
                    "El título difiere considerablemente."
            };

        result.Reasons.Add(message);
    }

    private static void AddVersionReason(
        MetadataMatchResult result,
        string localVersion,
        string candidateVersion)
    {
        if (!HasComparableText(
                localVersion,
                candidateVersion))
        {
            result.Reasons.Add(
                "La versión no está disponible en ambas fuentes.");

            return;
        }

        string message =
            result.VersionScore switch
            {
                >= 95 =>
                    "La versión coincide casi exactamente.",

                >= 80 =>
                    "La versión presenta una coincidencia alta.",

                >= 60 =>
                    "La versión presenta una coincidencia parcial.",

                _ =>
                    "La versión no coincide."
            };

        result.Reasons.Add(message);
    }

    private static void AddDurationReason(
        MetadataMatchResult result,
        TimeSpan localDuration,
        TimeSpan candidateDuration)
    {
        if (localDuration <= TimeSpan.Zero ||
            candidateDuration <= TimeSpan.Zero)
        {
            result.Reasons.Add(
                "No existe duración suficiente para comparar.");

            return;
        }

        int differenceSeconds =
            (int)Math.Round(
                Math.Abs(
                    (localDuration - candidateDuration)
                    .TotalSeconds));

        if (differenceSeconds == 0)
        {
            result.Reasons.Add(
                "La duración coincide exactamente.");

            return;
        }

        result.Reasons.Add(
            $"La duración difiere en {differenceSeconds} segundo(s).");
    }

    private static void AddYearReason(
        MetadataMatchResult result,
        uint localYear,
        uint candidateYear)
    {
        if (localYear == 0 ||
            candidateYear == 0)
        {
            result.Reasons.Add(
                "No existe año suficiente para comparar.");

            return;
        }

        if (localYear == candidateYear)
        {
            result.Reasons.Add(
                "El año coincide.");

            return;
        }

        result.Reasons.Add(
            $"El año local ({localYear}) difiere del candidato " +
            $"({candidateYear}).");
    }

    private static void AddFinalDecisionReason(
        MetadataMatchResult result)
    {
        if (result.RequiresSourceApproval)
        {
            result.Reasons.Add(
                "La fuente exige aprobación manual obligatoria.");

            return;
        }

        if (result.RequiresManualReview)
        {
            result.Reasons.Add(
                "La coincidencia requiere revisión manual.");

            return;
        }

        result.Reasons.Add(
            "La coincidencia alcanza los criterios técnicos de alta confianza.");
    }
}