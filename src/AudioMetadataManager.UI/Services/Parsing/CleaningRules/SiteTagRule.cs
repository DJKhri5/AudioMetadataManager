using System.Text.RegularExpressions;

namespace AudioMetadataManager.UI.Services.Parsing.CleaningRules;

public class SiteTagRule : ICleaningRule
{
    public int Priority => 200;

    public string Apply(string input)
    {
        string result = input;

        result = Regex.Replace(
            result,
            @"[\[(]\s*(?:www\.)?4clubbers\.pl\s*[\])]",
            string.Empty,
            RegexOptions.IgnoreCase);

        result = Regex.Replace(
            result,
            @"(?:www\.)?4clubbers\.pl",
            string.Empty,
            RegexOptions.IgnoreCase);

        return result;
    }
}