using System.Text.RegularExpressions;

namespace AudioMetadataManager.UI.Services.MetadataSources.Matching;

/// <summary>
/// Compara cadenas que contienen uno o varios artistas.
///
/// Separa las colaboraciones y compara cada artista
/// individualmente, evitando que diferencias en conectores
/// reduzcan excesivamente la puntuación.
/// </summary>
public class ArtistSimilarityCalculator
{
    private readonly TextSimilarityCalculator _textSimilarity =
        new();

    /// <summary>
    /// Calcula la similitud entre dos conjuntos de artistas.
    /// El resultado se encuentra entre 0 y 100.
    /// </summary>
    public int Calculate(
        string? localArtists,
        string? candidateArtists)
    {
        List<string> localArtistList =
            SplitArtists(localArtists);

        List<string> candidateArtistList =
            SplitArtists(candidateArtists);

        if (localArtistList.Count == 0 ||
            candidateArtistList.Count == 0)
        {
            return 0;
        }

        /*
         * Calculamos la mejor coincidencia para cada artista
         * del archivo local dentro de la lista del candidato.
         */
        List<int> localToCandidateScores =
            CalculateBestScores(
                localArtistList,
                candidateArtistList);

        /*
         * También hacemos la comparación inversa.
         * Esto impide otorgar 100 % cuando una lista contiene
         * artistas adicionales que no aparecen en la otra.
         */
        List<int> candidateToLocalScores =
            CalculateBestScores(
                candidateArtistList,
                localArtistList);

        double localAverage =
            localToCandidateScores.Average();

        double candidateAverage =
            candidateToLocalScores.Average();

        /*
         * Ambos sentidos tienen el mismo peso.
         */
        double finalScore =
            localAverage * 0.50 +
            candidateAverage * 0.50;

        return Math.Clamp(
            (int)Math.Round(finalScore),
            0,
            100);
    }

    /// <summary>
    /// Separa una cadena de colaboración en artistas
    /// individuales.
    ///
    /// Se reconocen conectores como:
    /// &, x, feat., ft., featuring, vs. y versus.
    /// </summary>
    public List<string> SplitArtists(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new List<string>();
        }

        string normalized =
            value.Trim();

        /*
         * Los conectores se sustituyen temporalmente
         * por un separador único.
         *
         * La letra x solo se interpreta como conector
         * cuando está rodeada por espacios.
         */
        normalized = Regex.Replace(
            normalized,
            @"\s+(feat\.?|ft\.?|featuring)\s+",
            " | ",
            RegexOptions.IgnoreCase |
            RegexOptions.CultureInvariant);

        normalized = Regex.Replace(
            normalized,
            @"\s+(vs\.?|versus)\s+",
            " | ",
            RegexOptions.IgnoreCase |
            RegexOptions.CultureInvariant);

        normalized = Regex.Replace(
            normalized,
            @"\s+[x×]\s+",
            " | ",
            RegexOptions.IgnoreCase |
            RegexOptions.CultureInvariant);

        normalized = Regex.Replace(
            normalized,
            @"\s*&\s*",
            " | ",
            RegexOptions.CultureInvariant);

        normalized = Regex.Replace(
            normalized,
            @"\s*\+\s*",
            " | ",
            RegexOptions.CultureInvariant);

        return normalized
            .Split(
                '|',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .Where(
                artist =>
                    !string.IsNullOrWhiteSpace(artist))
            .Distinct(
                StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private List<int> CalculateBestScores(
        IReadOnlyList<string> sourceArtists,
        IReadOnlyList<string> targetArtists)
    {
        List<int> scores = new();

        foreach (string sourceArtist in sourceArtists)
        {
            int bestScore = 0;

            foreach (string targetArtist in targetArtists)
            {
                int score =
                    CalculateSingleArtistScore(
                        sourceArtist,
                        targetArtist);

                if (score > bestScore)
                {
                    bestScore = score;
                }
            }

            scores.Add(bestScore);
        }

        return scores;
    }

    private int CalculateSingleArtistScore(
        string firstArtist,
        string secondArtist)
    {
        string first =
            _textSimilarity.Normalize(firstArtist);

        string second =
            _textSimilarity.Normalize(secondArtist);

        if (string.IsNullOrWhiteSpace(first) ||
            string.IsNullOrWhiteSpace(second))
        {
            return 0;
        }

        if (string.Equals(
                first,
                second,
                StringComparison.Ordinal))
        {
            return 100;
        }

        int normalScore =
            _textSimilarity.Calculate(
                first,
                second);

        /*
         * Caso especial para grafías como:
         *
         * Uberjak D
         * Uberjak'd
         *
         * Después de eliminar espacios y apóstrofes,
         * ambas quedan como "uberjakd".
         */
        string compactFirst =
            RemoveComparisonSeparators(first);

        string compactSecond =
            RemoveComparisonSeparators(second);

        if (string.Equals(
                compactFirst,
                compactSecond,
                StringComparison.Ordinal))
        {
            return 100;
        }

        int compactScore =
            _textSimilarity.Calculate(
                compactFirst,
                compactSecond);

        return Math.Max(
            normalScore,
            compactScore);
    }

    private static string RemoveComparisonSeparators(
        string value)
    {
        return Regex.Replace(
            value,
            @"[\s\-_]+",
            string.Empty,
            RegexOptions.CultureInvariant);
    }
}