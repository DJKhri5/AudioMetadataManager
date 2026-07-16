using AudioMetadataManager.UI.Models;

namespace AudioMetadataManager.UI.Services.Analysis.ConfidenceRules;

public class TitleConfidenceRule : IConfidenceRule
{
    public int Priority => 300;

    public ConfidenceRuleResult Evaluate(AudioFile audioFile)
    {
        bool passed =
            audioFile.Comparison?.TitleMatches == true;

        return new ConfidenceRuleResult
        {
            RuleName = nameof(TitleConfidenceRule),
            Points = passed ? 35 : 0,
            MaximumPoints = 35,
            Passed = passed,
            Message = passed
                ? "El título coincide con los metadatos."
                : "El título requiere revisión."
        };
    }
}