using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.Parsing;
using System.IO;
using System.Text.RegularExpressions;

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
        int separatorIndex =
            result.CleanName.IndexOf(
                separator,
                StringComparison.Ordinal);

        /*
         * Flujo normal:
         * Artista - Título (Versión)
         */
        if (separatorIndex >= 0)
        {
            ParseStandardName(result);

            return result;
        }

        /*
         * Flujo alternativo para nombres tipo slug:
         * artista-x-artista-titulo-extended-mix
         */
        if (TryParseSlugName(result.CleanName, result))
        {
            result.WasParsedSuccessfully = true;

            result.WasCleaned =
                !string.Equals(
                    result.OriginalName,
                    result.CleanName,
                    StringComparison.Ordinal);

            result.Notes =
                "Nombre interpretado mediante la regla de formato slug. " +
                "Requiere revisión manual.";

            return result;
        }

        result.Notes =
            "No se encontró el separador ' - ' y el nombre no pudo " +
            "interpretarse mediante la regla de formato slug.";

        result.WasParsedSuccessfully = false;

        result.WasCleaned =
            !string.Equals(
                result.OriginalName,
                result.CleanName,
                StringComparison.Ordinal);

        return result;
    }

    private void ParseStandardName(
        ParsedFileName result)
    {
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
    }

    private static bool TryParseSlugName(
        string cleanName,
        ParsedFileName result)
    {
        if (string.IsNullOrWhiteSpace(cleanName))
        {
            return false;
        }

        string slug =
            RemoveTrailingIdentifier(cleanName);

        if (string.IsNullOrWhiteSpace(slug))
        {
            return false;
        }

        string version = string.Empty;

        if (slug.EndsWith(
                "-extended-mix",
                StringComparison.OrdinalIgnoreCase))
        {
            version = "Extended Mix";

            slug = slug[
                ..^"-extended-mix".Length];
        }
        else if (slug.EndsWith(
                     "-original-mix",
                     StringComparison.OrdinalIgnoreCase))
        {
            version = "Original Mix";

            slug = slug[
                ..^"-original-mix".Length];
        }
        else if (slug.EndsWith(
                     "-radio-edit",
                     StringComparison.OrdinalIgnoreCase))
        {
            version = "Radio Edit";

            slug = slug[
                ..^"-radio-edit".Length];
        }

        string[] tokens =
            slug.Split(
                '-',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries);

        /*
         * Esta primera regla es deliberadamente conservadora.
         * Necesitamos al menos artistas y título.
         */
        if (tokens.Length < 3)
        {
            return false;
        }

        int titleStartIndex =
            FindLikelyTitleStart(tokens);

        if (titleStartIndex <= 0 ||
            titleStartIndex >= tokens.Length)
        {
            return false;
        }

        string[] artistTokens =
            tokens[..titleStartIndex];

        string[] titleTokens =
            tokens[titleStartIndex..];

        string artist =
            BuildArtistText(artistTokens);

        string title =
            BuildNormalText(titleTokens);

        if (string.IsNullOrWhiteSpace(artist) ||
            string.IsNullOrWhiteSpace(title))
        {
            return false;
        }

        result.Artist = artist;
        result.Title = title;
        result.Version = version;

        return true;
    }

    private static string RemoveTrailingIdentifier(
        string value)
    {
        /*
         * Elimina sufijos como:
         * --456244620_86919875
         */
        return Regex.Replace(
            value,
            @"--\d+(?:_\d+)*$",
            string.Empty,
            RegexOptions.CultureInvariant);
    }

    private static int FindLikelyTitleStart(
        string[] tokens)
    {
        /*
         * En nombres tipo:
         *
         * ben-nicky-x-uberjakd-d-x-trey-pearce-relapse
         *
         * la última palabra se considera inicialmente el título.
         *
         * Es una heurística provisional y por eso el resultado
         * siempre debe pasar por revisión manual.
         */
        return tokens.Length - 1;
    }

    private static string BuildArtistText(
        string[] tokens)
    {
        List<string> parts = new();
        List<string> currentArtist = new();

        foreach (string token in tokens)
        {
            if (IsArtistConnector(token))
            {
                AppendArtistPart(
                    parts,
                    currentArtist);

                parts.Add(
                    NormalizeConnector(token));

                continue;
            }

            currentArtist.Add(
                FormatToken(token));
        }

        AppendArtistPart(
            parts,
            currentArtist);

        return string.Join(
            " ",
            parts);
    }

    private static void AppendArtistPart(
        List<string> parts,
        List<string> currentArtist)
    {
        if (currentArtist.Count == 0)
        {
            return;
        }

        parts.Add(
            string.Join(
                " ",
                currentArtist));

        currentArtist.Clear();
    }

    private static bool IsArtistConnector(
        string token)
    {
        return token.Equals(
                   "x",
                   StringComparison.OrdinalIgnoreCase) ||
               token.Equals(
                   "vs",
                   StringComparison.OrdinalIgnoreCase) ||
               token.Equals(
                   "feat",
                   StringComparison.OrdinalIgnoreCase) ||
               token.Equals(
                   "ft",
                   StringComparison.OrdinalIgnoreCase) ||
               token.Equals(
                   "and",
                   StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeConnector(
        string token)
    {
        if (token.Equals(
                "feat",
                StringComparison.OrdinalIgnoreCase) ||
            token.Equals(
                "ft",
                StringComparison.OrdinalIgnoreCase))
        {
            return "feat.";
        }

        if (token.Equals(
                "and",
                StringComparison.OrdinalIgnoreCase))
        {
            return "&";
        }

        return token.ToLowerInvariant();
    }

    private static string BuildNormalText(
        string[] tokens)
    {
        return string.Join(
            " ",
            tokens.Select(FormatToken));
    }

    private static string FormatToken(
        string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return string.Empty;
        }

        if (token.Length == 1)
        {
            return token.ToUpperInvariant();
        }

        return char.ToUpperInvariant(token[0]) +
               token[1..].ToLowerInvariant();
    }
}