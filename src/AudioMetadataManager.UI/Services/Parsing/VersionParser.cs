using AudioMetadataManager.UI.Services.Parsing.VersionRules;

namespace AudioMetadataManager.UI.Services.Parsing;

public class VersionParser
{
    private readonly IReadOnlyList<IVersionRule> _rules;

    public VersionParser()
    {
        _rules = CreateRules();
    }

    public (string Title, string Version) Parse(string title)
    {
        string originalTitle = title.Trim();

        foreach (IVersionRule rule in _rules)
        {
            if (rule.TryParse(
                originalTitle,
                out string parsedTitle,
                out string parsedVersion))
            {
                return (parsedTitle, parsedVersion);
            }
        }

        return (originalTitle, string.Empty);
    }

    private static IReadOnlyList<IVersionRule> CreateRules()
    {
        return new List<IVersionRule>
        {
            new ParenthesizedVersionRule(),
            new BracketVersionRule(),
            new SuffixVersionRule()
        };
    }
}