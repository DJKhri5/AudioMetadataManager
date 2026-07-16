namespace AudioMetadataManager.UI.Services.Analysis.ConfidenceRules;

public class ConfidenceRuleResult
{
    public string RuleName { get; init; } = string.Empty;

    public int Points { get; init; }

    public int MaximumPoints { get; init; }

    public bool Passed { get; init; }

    public string Message { get; init; } = string.Empty;
}