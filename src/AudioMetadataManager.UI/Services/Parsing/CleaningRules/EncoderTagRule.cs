using System.Text.RegularExpressions;

namespace AudioMetadataManager.UI.Services.Parsing.CleaningRules;

public class EncoderTagRule : ICleaningRule
{
    public int Priority => 300;

    public string Apply(string input)
    {
        string result = input;

        result = Regex.Replace(
            result,
            @"\s+(?:encoded|uploaded|shared|ripped|rip)\s+by\s+[^()\[\]]+$",
            string.Empty,
            RegexOptions.IgnoreCase);

        result = Regex.Replace(
            result,
            @"\s+by\s+P\d+\s*$",
            string.Empty,
            RegexOptions.IgnoreCase);

        return result.Trim();
    }
}