namespace AudioMetadataManager.UI.Services.Parsing.VersionRules;

public interface IVersionRule
{
    bool TryParse(
        string input,
        out string title,
        out string version);
}