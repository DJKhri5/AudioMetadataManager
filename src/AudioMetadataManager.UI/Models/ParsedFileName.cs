namespace AudioMetadataManager.UI.Models;

public class ParsedFileName
{
    public string OriginalName { get; set; } = string.Empty;

    public string CleanName { get; set; } = string.Empty;

    public string Artist { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public bool WasParsedSuccessfully { get; set; }

    public bool WasCleaned { get; set; }

    public string Notes { get; set; } = string.Empty;
}