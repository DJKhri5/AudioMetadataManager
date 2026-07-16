using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.Analysis.ConfidenceRules;

namespace AudioMetadataManager.UI.Services.Analysis;

public class ConfidenceEngine
{
    private readonly IReadOnlyList<IConfidenceRule> _rules;

    public ConfidenceEngine()
    {
        _rules = CreateRules()
            .OrderBy(r => r.Priority)
            .ToList();
    }

    public AnalysisResult Evaluate(AudioFile audioFile)
    {
        int totalPoints = 0;
        int maximumPoints = 0;

        List<string> messages = new();

        foreach (IConfidenceRule rule in _rules)
        {
            ConfidenceRuleResult result = rule.Evaluate(audioFile);

            totalPoints += result.Points;
            maximumPoints += result.MaximumPoints;

            messages.Add(result.Message);
        }

        int percentage =
            maximumPoints == 0
                ? 0
                : (int)Math.Round(totalPoints * 100.0 / maximumPoints);

        return new AnalysisResult
        {
            ConfidenceScore = percentage,
            ConfidenceLevel = GetLevel(percentage),
            RequiresManualReview = percentage < 80,
            Summary = string.Join(" | ", messages)
        };
    }

    private static IReadOnlyList<IConfidenceRule> CreateRules()
    {
        return new List<IConfidenceRule>
        {
            new ParsedNameConfidenceRule(),
            new ArtistConfidenceRule(),
            new TitleConfidenceRule(),
            new VersionConfidenceRule()
        };
    }

    private static string GetLevel(int confidence)
    {
        if (confidence >= 95)
            return "Muy alta";

        if (confidence >= 80)
            return "Alta";

        if (confidence >= 60)
            return "Media";

        if (confidence >= 40)
            return "Baja";

        return "Muy baja";
    }
}