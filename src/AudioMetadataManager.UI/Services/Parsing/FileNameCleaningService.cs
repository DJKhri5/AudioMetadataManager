using AudioMetadataManager.UI.Services.Parsing.CleaningRules;
using System.Text.RegularExpressions;

namespace AudioMetadataManager.UI.Services.Parsing;

public class FileNameCleaningService
{
    private readonly IReadOnlyList<ICleaningRule> _rules;

    public FileNameCleaningService()
    {
        _rules = CreateRules()
            .OrderBy(rule => rule.Priority)
            .ToList();
    }

    public string Clean(string name)
    {
        string result = name.Trim();

        foreach (ICleaningRule rule in _rules)
        {
            result = rule.Apply(result);
        }

        result = Regex.Replace(
            result,
            @"\s+",
            " ");

        return result.Trim();
    }

    private static IReadOnlyList<ICleaningRule> CreateRules()
    {
        return new List<ICleaningRule>
        {
            new LeadingTrackNumberRule(),
            new SiteTagRule(),
            new EncoderTagRule()
        };
    }
}