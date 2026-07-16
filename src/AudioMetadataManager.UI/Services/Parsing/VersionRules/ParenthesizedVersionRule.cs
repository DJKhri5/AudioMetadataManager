using System.Text.RegularExpressions;

namespace AudioMetadataManager.UI.Services.Parsing.VersionRules;

public class ParenthesizedVersionRule : IVersionRule
{
    private static readonly string[] VersionKeywords =
    {
        "mix",
        "remix",
        "edit",
        "rework",
        "bootleg",
        "mashup",
        "dub",
        "version",
        "remaster",
        "live",
        "vip",
        "instrumental",
        "acapella"
    };

    public bool TryParse(
        string input,
        out string title,
        out string version)
    {
        title = input.Trim();
        version = string.Empty;

        Match match = Regex.Match(
            input,
            @"^(?<title>.+?)\s*\((?<version>[^()]*)\)\s*$");

        if (!match.Success)
        {
            return false;
        }

        string possibleVersion =
            match.Groups["version"].Value.Trim();

        if (!LooksLikeVersion(possibleVersion))
        {
            return false;
        }

        title =
            match.Groups["title"].Value.Trim();

        version = possibleVersion;

        return true;
    }

    private static bool LooksLikeVersion(string value)
    {
        return VersionKeywords.Any(keyword =>
            value.Contains(
                keyword,
                StringComparison.OrdinalIgnoreCase));
    }
}