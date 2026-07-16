using AudioMetadataManager.UI.Models;

namespace AudioMetadataManager.UI.Services.Analysis.ConfidenceRules;

public class ArtistConfidenceRule : IConfidenceRule
{
    public int Priority => 200;

    public ConfidenceRuleResult Evaluate(AudioFile audioFile)
    {
        bool passed =
            audioFile.Comparison?.ArtistMatches == true;

        return new ConfidenceRuleResult
        {
            RuleName = nameof(ArtistConfidenceRule),
            Points = passed ? 35 : 0,
            MaximumPoints = 35,
            Passed = passed,
            Message = passed
                ? "El artista coincide con los metadatos."
                : "El artista requiere revisión."
        };
    }
}