namespace AudioMetadataManager.UI.Services.Parsing.CleaningRules;

public interface ICleaningRule
{
    int Priority { get; }

    string Apply(string input);
}