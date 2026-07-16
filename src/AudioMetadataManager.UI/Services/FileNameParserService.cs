using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.Parsing;
using System.IO;

namespace AudioMetadataManager.UI.Services;

public class FileNameParserService
{
    private readonly FileNameCleaningService _cleaner = new();
    private readonly ArtistParser _artistParser = new();
    private readonly TitleParser _titleParser = new();
    private readonly VersionParser _versionParser = new();
    public ParsedFileName Parse(AudioFile audioFile)
    {
        ParsedFileName result = new();

        result.OriginalName =
            Path.GetFileNameWithoutExtension(audioFile.FileName);

        result.CleanName =
            _cleaner.Clean(result.OriginalName);

        string separator = " - ";
        int separatorIndex = result.CleanName.IndexOf(separator);

        if (separatorIndex < 0)
        {
            result.Notes = "No se encontró el separador ' - '.";
            result.WasParsedSuccessfully = false;

            return result;
        }

        result.Artist =
            _artistParser.Parse(result.CleanName);

        result.Title =
            _titleParser.Parse(result.CleanName);

        (string parsedTitle, string parsedVersion) =
            _versionParser.Parse(result.Title);

        result.Title = parsedTitle;
        result.Version = parsedVersion;

        result.WasParsedSuccessfully =
            !string.IsNullOrWhiteSpace(result.Artist) &&
            !string.IsNullOrWhiteSpace(result.Title);

        result.WasCleaned =
            !string.Equals(
                result.OriginalName,
                result.CleanName,
                StringComparison.Ordinal);

        return result;
    }
}