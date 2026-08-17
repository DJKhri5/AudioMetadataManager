using System.IO;
using System.Text.RegularExpressions;
using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.Duplicates.Models;

namespace AudioMetadataManager.UI.Services.Duplicates;

public sealed class AudioDuplicateDetector : IAudioDuplicateDetector
{
    public DuplicateDetectionResult DetectDuplicates(IEnumerable<AudioFile> files)
    {
        if (files is null)
        {
            return new DuplicateDetectionResult
            {
                Groups = Array.Empty<DuplicateGroup>()
            };
        }

        var validFiles = files
            .Where(f => f is not null && !string.IsNullOrWhiteSpace(f.FullPath))
            .ToList();

        if (validFiles.Count < 2)
        {
            return new DuplicateDetectionResult
            {
                Groups = Array.Empty<DuplicateGroup>()
            };
        }

        var groups = new List<DuplicateGroup>();
        var groupedFilePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Fase 1: Detección de duplicados exactos (mismo tamaño y duración casi idéntica)
        var exactCandidates = validFiles
            .Where(f => f.FileSizeBytes > 0)
            .GroupBy(f => f.FileSizeBytes)
            .Where(g => g.Count() > 1);

        foreach (var sizeGroup in exactCandidates)
        {
            var subGroupsByDuration = sizeGroup
                .GroupBy(f => Math.Round(f.Duration.TotalSeconds, 0))
                .Where(g => g.Count() > 1);

            foreach (var exactGroup in subGroupsByDuration)
            {
                var fileList = exactGroup.ToList();
                if (fileList.Count < 2) continue;

                var items = BuildGroupItems(fileList);
                string groupTitle = GetGroupDisplayTitle(fileList.First());

                groups.Add(new DuplicateGroup
                {
                    GroupKey = $"EXACT_{fileList.First().FileSizeBytes}_{Math.Round(fileList.First().Duration.TotalSeconds, 0)}",
                    DisplayTitle = $"{groupTitle} [Mismo tamaño binario]",
                    MatchKind = DuplicateMatchKind.ExactBinary,
                    Items = items
                });

                foreach (var file in fileList)
                {
                    groupedFilePaths.Add(file.FullPath);
                }
            }
        }

        // Fase 2: Detección de duplicados probables por metadatos normalizados (Artista + Título + Versión)
        var remainingFiles = validFiles
            .Where(f => !groupedFilePaths.Contains(f.FullPath))
            .ToList();

        var metadataGroups = remainingFiles
            .GroupBy(GetNormalizedMetadataKey)
            .Where(g => !string.IsNullOrEmpty(g.Key) && g.Count() > 1);

        foreach (var metaGroup in metadataGroups)
        {
            var fileList = metaGroup.ToList();
            if (fileList.Count < 2) continue;

            var items = BuildGroupItems(fileList);
            string groupTitle = GetGroupDisplayTitle(fileList.First());

            groups.Add(new DuplicateGroup
            {
                GroupKey = $"META_{metaGroup.Key}",
                DisplayTitle = groupTitle,
                MatchKind = DuplicateMatchKind.ProbableMetadata,
                Items = items
            });
        }

        return new DuplicateDetectionResult
        {
            Groups = groups
                .OrderBy(g => g.MatchKind)
                .ThenByDescending(g => g.PotentialReclaimableBytes)
                .ToList()
        };
    }

    private static IReadOnlyList<DuplicateGroupItem> BuildGroupItems(List<AudioFile> files)
    {
        var items = new List<DuplicateGroupItem>(files.Count);

        foreach (var file in files)
        {
            int score = CalculateQualityScore(file);
            items.Add(new DuplicateGroupItem
            {
                File = file,
                QualityScore = score
            });
        }

        // Ordenar de mayor a menor calidad técnica
        items = items
            .OrderByDescending(i => i.QualityScore)
            .ThenByDescending(i => i.FileSizeBytes)
            .ToList();

        if (items.Count > 0)
        {
            items[0].IsBestQualityCandidate = true;
        }

        return items;
    }

    private static int CalculateQualityScore(AudioFile file)
    {
        int score = 0;

        bool isLossless = file.QualityAnalysis?.IsLossless == true ||
                          IsLosslessExtension(file.Extension);

        if (isLossless)
        {
            score += 10000;
        }

        int bitrate = file.Bitrate;
        if (bitrate > 0)
        {
            score += Math.Min(bitrate, 2000);
        }

        int sampleRate = file.SampleRate;
        if (sampleRate > 0)
        {
            score += sampleRate / 1000;
        }

        if (file.BitsPerSample >= 24)
        {
            score += 50;
        }

        if (file.QualityAnalysis?.QualityScore > 0)
        {
            score += file.QualityAnalysis.QualityScore;
        }

        return score;
    }

    private static bool IsLosslessExtension(string extension)
    {
        string ext = (extension ?? string.Empty).ToLowerInvariant().Trim();
        return ext is ".flac" or ".wav" or ".aiff" or ".alac" or ".ape";
    }

    private static string GetGroupDisplayTitle(AudioFile file)
    {
        string artist = ExtractArtist(file);
        string title = ExtractTitle(file);
        string version = ExtractVersion(file);

        if (!string.IsNullOrWhiteSpace(artist) && !string.IsNullOrWhiteSpace(title))
        {
            string full = $"{artist} - {title}";
            if (!string.IsNullOrWhiteSpace(version))
            {
                full += $" ({version})";
            }
            return full;
        }

        return Path.GetFileNameWithoutExtension(file.FileName);
    }

    private static string GetNormalizedMetadataKey(AudioFile file)
    {
        string artist = NormalizeString(ExtractArtist(file));
        string title = NormalizeString(ExtractTitle(file));
        string version = NormalizeString(ExtractVersion(file));

        if (string.IsNullOrWhiteSpace(artist) || string.IsNullOrWhiteSpace(title))
        {
            // Si falta artista o título en metadatos, intentar con el nombre de archivo normalizado
            string name = NormalizeString(Path.GetFileNameWithoutExtension(file.FileName));
            return name.Length >= 5 ? name : string.Empty;
        }

        return $"{artist}__{title}__{version}";
    }

    private static string ExtractArtist(AudioFile file)
    {
        if (!string.IsNullOrWhiteSpace(file.Artist))
            return file.Artist;

        return file.ParsedName?.Artist ?? string.Empty;
    }

    private static string ExtractTitle(AudioFile file)
    {
        if (!string.IsNullOrWhiteSpace(file.Title))
            return file.Title;

        return file.ParsedName?.Title ?? string.Empty;
    }

    private static string ExtractVersion(AudioFile file)
    {
        if (!string.IsNullOrWhiteSpace(file.Version))
            return file.Version;

        return file.ParsedName?.Version ?? string.Empty;
    }

    private static string NormalizeString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Minúsculas y eliminar caracteres especiales, manteniendo alfanuméricos
        string clean = Regex.Replace(input.ToLowerInvariant(), @"[^\w\d\s]", " ");
        // Reducir múltiples espacios consecutivos
        clean = Regex.Replace(clean, @"\s+", " ").Trim();
        return clean;
    }
}
