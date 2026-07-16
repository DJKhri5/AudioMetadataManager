namespace AudioMetadataManager.UI.Services.Parsing;

public class TitleParser
{
    public string Parse(string cleanName)
    {
        int separator = cleanName.IndexOf(" - ");

        if (separator < 0)
            return "";

        return cleanName[(separator + 3)..].Trim();
    }
}