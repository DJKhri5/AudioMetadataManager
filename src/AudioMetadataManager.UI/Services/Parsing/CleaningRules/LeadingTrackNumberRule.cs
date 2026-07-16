using System.Text.RegularExpressions;

namespace AudioMetadataManager.UI.Services.Parsing.CleaningRules;

public class LeadingTrackNumberRule : ICleaningRule
{
    public int Priority => 100;

    public string Apply(string input)
    {
        return Regex.Replace(
            input,
            @"^\s*\d+\s*[-._]\s*",
            string.Empty);
    }
}