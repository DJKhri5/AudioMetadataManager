using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AudioMetadataManager.UI.Services;

public class TextNormalizationService
{
    public string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string result = value.Trim().ToLowerInvariant();

        result = RemoveDiacritics(result);

        result = Regex.Replace(
            result,
            @"\b(featuring|ft)\.?\b",
            "feat");

        result = Regex.Replace(
            result,
            @"\b(and)\b",
            "&");

        result = Regex.Replace(
            result,
            @"\((ofc|official)\)",
            string.Empty);

        result = Regex.Replace(
            result,
            @"\b(feat|vs|x)\.?\b",
            " ");

        result = result.Replace("&", " ");

        result = Regex.Replace(
            result,
            @"[^a-z0-9]+",
            " ");

        result = Regex.Replace(
            result,
            @"\s+",
            " ");

        return result.Trim();
    }

    private static string RemoveDiacritics(string value)
    {
        string normalized =
            value.Normalize(NormalizationForm.FormD);

        StringBuilder builder = new();

        foreach (char character in normalized)
        {
            UnicodeCategory category =
                CharUnicodeInfo.GetUnicodeCategory(character);

            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder
            .ToString()
            .Normalize(NormalizationForm.FormC);
    }
}