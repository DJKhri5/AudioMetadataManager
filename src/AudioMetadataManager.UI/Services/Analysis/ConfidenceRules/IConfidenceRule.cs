using AudioMetadataManager.UI.Models;

namespace AudioMetadataManager.UI.Services.Analysis.ConfidenceRules;

public interface IConfidenceRule
{
    int Priority { get; }

    ConfidenceRuleResult Evaluate(AudioFile audioFile);
}