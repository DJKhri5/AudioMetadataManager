using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AudioMetadataManager.UI.Services.MetadataSources.Matching;

/// <summary>
/// Calcula el grado de similitud entre dos textos musicales.
/// La puntuación resultante se encuentra entre 0 y 100.
/// </summary>
public class TextSimilarityCalculator
{
    /// <summary>
    /// Compara dos textos utilizando una combinación de:
    /// coincidencia exacta normalizada, palabras compartidas
    /// y distancia de edición.
    /// </summary>
    public int Calculate(
        string? firstText,
        string? secondText)
    {
        string first = Normalize(firstText);
        string second = Normalize(secondText);

        if (string.IsNullOrWhiteSpace(first) &&
            string.IsNullOrWhiteSpace(second))
        {
            return 100;
        }

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

        int tokenScore =
            CalculateTokenSimilarity(
                first,
                second);

        int editScore =
            CalculateEditSimilarity(
                first,
                second);

        /*
         * Las palabras compartidas tienen mayor peso porque
         * en metadatos musicales el orden de artistas y conectores
         * puede variar entre plataformas.
         */
        double weightedScore =
            tokenScore * 0.65 +
            editScore * 0.35;

        return Math.Clamp(
            (int)Math.Round(weightedScore),
            0,
            100);
    }

    /// <summary>
    /// Normaliza el texto únicamente para comparación.
    /// El valor original nunca se modifica.
    /// </summary>
    public string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string normalized =
            RemoveDiacritics(value)
                .ToLowerInvariant();

        /*
         * Unifica conectores frecuentes para facilitar
         * comparaciones entre plataformas.
         *
         * Ejemplos:
         * feat, ft, featuring -> feat
         * and, +              -> &
         * vs.                 -> vs
         */
        normalized = Regex.Replace(
            normalized,
            @"\b(featuring|feat\.?|ft\.?)\b",
            " feat ",
            RegexOptions.IgnoreCase |
            RegexOptions.CultureInvariant);

        normalized = Regex.Replace(
            normalized,
            @"\b(and)\b|\+",
            " & ",
            RegexOptions.IgnoreCase |
            RegexOptions.CultureInvariant);

        normalized = Regex.Replace(
            normalized,
            @"\bvs\.\b",
            " vs ",
            RegexOptions.IgnoreCase |
            RegexOptions.CultureInvariant);

        /*
         * Los apóstrofes y signos se eliminan solo para comparar.
         * Esto permite reconocer Uberjak D y Uberjak'd como
         * textos cercanos sin cambiar su grafía real.
         */
        normalized = Regex.Replace(
            normalized,
            @"['’`]",
            string.Empty,
            RegexOptions.CultureInvariant);

        normalized = Regex.Replace(
            normalized,
            @"[^\p{L}\p{N}&]+",
            " ",
            RegexOptions.CultureInvariant);

        normalized = Regex.Replace(
            normalized,
            @"\s+",
            " ",
            RegexOptions.CultureInvariant);

        return normalized.Trim();
    }

    private static int CalculateTokenSimilarity(
        string first,
        string second)
    {
        HashSet<string> firstTokens =
            Tokenize(first);

        HashSet<string> secondTokens =
            Tokenize(second);

        if (firstTokens.Count == 0 &&
            secondTokens.Count == 0)
        {
            return 100;
        }

        if (firstTokens.Count == 0 ||
            secondTokens.Count == 0)
        {
            return 0;
        }

        int intersectionCount =
            firstTokens.Intersect(secondTokens).Count();

        int unionCount =
            firstTokens.Union(secondTokens).Count();

        if (unionCount == 0)
        {
            return 0;
        }

        double score =
            intersectionCount * 100.0 /
            unionCount;

        return Math.Clamp(
            (int)Math.Round(score),
            0,
            100);
    }

    private static HashSet<string> Tokenize(
        string value)
    {
        return value
            .Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .ToHashSet(
                StringComparer.Ordinal);
    }

    private static int CalculateEditSimilarity(
        string first,
        string second)
    {
        int maximumLength =
            Math.Max(
                first.Length,
                second.Length);

        if (maximumLength == 0)
        {
            return 100;
        }

        int distance =
            CalculateLevenshteinDistance(
                first,
                second);

        double similarity =
            1.0 -
            distance / (double)maximumLength;

        return Math.Clamp(
            (int)Math.Round(similarity * 100),
            0,
            100);
    }

    private static int CalculateLevenshteinDistance(
        string first,
        string second)
    {
        int[,] matrix =
            new int[
                first.Length + 1,
                second.Length + 1];

        for (int firstIndex = 0;
             firstIndex <= first.Length;
             firstIndex++)
        {
            matrix[firstIndex, 0] =
                firstIndex;
        }

        for (int secondIndex = 0;
             secondIndex <= second.Length;
             secondIndex++)
        {
            matrix[0, secondIndex] =
                secondIndex;
        }

        for (int firstIndex = 1;
             firstIndex <= first.Length;
             firstIndex++)
        {
            for (int secondIndex = 1;
                 secondIndex <= second.Length;
                 secondIndex++)
            {
                int substitutionCost =
                    first[firstIndex - 1] ==
                    second[secondIndex - 1]
                        ? 0
                        : 1;

                matrix[firstIndex, secondIndex] =
                    Math.Min(
                        Math.Min(
                            matrix[firstIndex - 1, secondIndex] + 1,
                            matrix[firstIndex, secondIndex - 1] + 1),
                        matrix[firstIndex - 1, secondIndex - 1] +
                        substitutionCost);
            }
        }

        return matrix[
            first.Length,
            second.Length];
    }

    private static string RemoveDiacritics(
        string value)
    {
        string decomposed =
            value.Normalize(
                NormalizationForm.FormD);

        StringBuilder builder = new();

        foreach (char character in decomposed)
        {
            UnicodeCategory category =
                CharUnicodeInfo.GetUnicodeCategory(
                    character);

            if (category !=
                UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder
            .ToString()
            .Normalize(
                NormalizationForm.FormC);
    }
}