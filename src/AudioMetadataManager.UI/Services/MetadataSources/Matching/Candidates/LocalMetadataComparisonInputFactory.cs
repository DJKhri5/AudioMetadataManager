using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Comparison;
using AudioMetadataManager.UI.Services.Parsing;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Matching.Candidates;

/// <summary>
/// Construye una identidad local combinada para evaluar
/// candidatos externos.
///
/// Prioriza la información interpretada desde el nombre cuando
/// es utilizable y completa los campos restantes con las
/// etiquetas actuales del archivo.
/// </summary>
public sealed class LocalMetadataComparisonInputFactory
{
    private readonly VersionParser
        _versionParser;

    /// <summary>
    /// Crea la fábrica con el parser de versiones
    /// predeterminado.
    /// </summary>
    public LocalMetadataComparisonInputFactory()
        : this(
            new VersionParser())
    {
    }

    /// <summary>
    /// Crea la fábrica con un parser de versiones
    /// personalizado.
    /// </summary>
    public LocalMetadataComparisonInputFactory(
        VersionParser versionParser)
    {
        _versionParser =
            versionParser ??
            throw new ArgumentNullException(
                nameof(versionParser));
    }

    /// <summary>
    /// Construye la identidad local utilizada por el motor de
    /// evaluación de candidatos.
    /// </summary>
    public MetadataComparisonInput Create(
        AudioFile audioFile,
        ParsedFileName parsedFileName)
    {
        ArgumentNullException.ThrowIfNull(
            audioFile);

        ArgumentNullException.ThrowIfNull(
            parsedFileName);

        string taggedTitle =
            Normalize(
                audioFile.Title);

        string taggedVersion =
            ExtractTaggedVersion(
                taggedTitle);

        string taggedTitleWithoutVersion =
            RemoveTaggedVersion(
                taggedTitle);

        bool useParsedIdentity =
            parsedFileName.WasParsedSuccessfully &&
            !string.IsNullOrWhiteSpace(
                parsedFileName.Artist) &&
            !string.IsNullOrWhiteSpace(
                parsedFileName.Title);

        return new MetadataComparisonInput
        {
            SourceName =
                "Identidad local combinada",

            Artist =
                useParsedIdentity
                    ? NormalizeNullable(
                        parsedFileName.Artist)
                    : NormalizeNullable(
                        audioFile.Artist),

            Title =
                useParsedIdentity
                    ? NormalizeNullable(
                        parsedFileName.Title)
                    : NormalizeNullable(
                        taggedTitleWithoutVersion),

            Version =
                FirstAvailable(
                    parsedFileName.Version,
                    taggedVersion),

            Album =
                NormalizeNullable(
                    audioFile.Album),

            Genre =
                NormalizeNullable(
                    audioFile.Genre),

            Label =
                NormalizeNullable(
                    ReadLabel(
                        audioFile))
        };
    }

    /// <summary>
    /// Extrae una versión incluida dentro del título
    /// almacenado en las etiquetas.
    /// </summary>
    private string ExtractTaggedVersion(
        string taggedTitle)
    {
        if (string.IsNullOrWhiteSpace(
                taggedTitle))
        {
            return string.Empty;
        }

        (string Title, string Version) parsedTitle =
            _versionParser.Parse(
                taggedTitle);

        return Normalize(
            parsedTitle.Version);
    }

    /// <summary>
    /// Elimina la versión del título etiquetado cuando pudo
    /// identificarse de forma segura.
    /// </summary>
    private string RemoveTaggedVersion(
        string taggedTitle)
    {
        if (string.IsNullOrWhiteSpace(
                taggedTitle))
        {
            return string.Empty;
        }

        (string Title, string Version) parsedTitle =
            _versionParser.Parse(
                taggedTitle);

        return string.IsNullOrWhiteSpace(
                    parsedTitle.Title)
                        ? taggedTitle
                        : Normalize(
                            parsedTitle.Title);
    }

    /// <summary>
    /// Lee el sello almacenado en las etiquetas locales.
    /// </summary>
    private static string ReadLabel(
        AudioFile audioFile)
    {
        return Normalize(
            audioFile.Label);
    }

    private static string? FirstAvailable(
        string? preferredValue,
        string? fallbackValue)
    {
        string preferred =
            Normalize(
                preferredValue);

        if (!string.IsNullOrWhiteSpace(
                preferred))
        {
            return preferred;
        }

        string fallback =
            Normalize(
                fallbackValue);

        return string.IsNullOrWhiteSpace(
                fallback)
                    ? null
                    : fallback;
    }

    private static string? NormalizeNullable(
        string? value)
    {
        string normalized =
            Normalize(
                value);

        return string.IsNullOrWhiteSpace(
                normalized)
                    ? null
                    : normalized;
    }

    private static string Normalize(
        string? value)
    {
        return string.IsNullOrWhiteSpace(
                value)
                    ? string.Empty
                    : value.Trim();
    }
}
