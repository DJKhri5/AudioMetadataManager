using System.Text.RegularExpressions;

namespace AudioMetadataManager.UI.Services.Parsing.VersionRules;

public class SuffixVersionRule : IVersionRule
{
    private static readonly string[] VersionPatterns =
    {
        "extended mix",
        "original mix",
        "radio edit",
        "club mix",
        "dub mix",
        "vip mix",
        "instrumental",
        "acapella",
        "remaster",
        "rework",
        "bootleg",
        "mashup",
        "remix",
        "live",
        "edit"
    };

    public bool TryParse(
        string input,
        out string title,
        out string version)
    {
        title = input.Trim();
        version = string.Empty;

        foreach (string pattern in VersionPatterns)
        {
            Match match = Regex.Match(
                input,
                $@"^(?<title>.+?)\s+(?<version>(?:.+?\s+)?{Regex.Escape(pattern)})\s*$",
                RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                continue;
            }

            string possibleTitle =
                match.Groups["title"].Value.Trim();

            string possibleVersion =
                match.Groups["version"].Value.Trim();

            if (string.IsNullOrWhiteSpace(possibleTitle) ||
                string.IsNullOrWhiteSpace(possibleVersion))
            {
                continue;
            }

            title = possibleTitle;
            version = possibleVersion;

            return true;
        }

        return false;
    }
}