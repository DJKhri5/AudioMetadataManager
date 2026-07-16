using AudioMetadataManager.UI.Models;

namespace AudioMetadataManager.UI.Services.Analysis.ConfidenceRules;

public class VersionConfidenceRule : IConfidenceRule
{
    public int Priority => 400;

    public ConfidenceRuleResult Evaluate(AudioFile audioFile)
    {
        bool detected =
            !string.IsNullOrWhiteSpace(
                audioFile.ParsedName?.Version);

        return new ConfidenceRuleResult
        {
            RuleName = nameof(VersionConfidenceRule),
            Points = detected ? 10 : 0,
            MaximumPoints = 10,
            Passed = detected,
            Message = detected
                ? "Se detectó una versión musical."
                : "No se detectó una versión específica."
        };
    }
}