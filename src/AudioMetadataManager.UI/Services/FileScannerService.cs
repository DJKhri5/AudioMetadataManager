using AudioMetadataManager.UI.Models;
using System.IO;
using System.Reflection.Metadata;
using AudioMetadataManager.UI.Services.Quality;
using AudioMetadataManager.UI.Services.Simulation;

namespace AudioMetadataManager.UI.Services;

public class FileScannerService
{
    private readonly string[] SupportedExtensions =
    {
        ".mp3",
        ".flac",
        ".wav",
        ".aac",
        ".m4a",
        ".ogg",
        ".wma",
        ".opus",
        ".ape",
        ".aiff"
    };

    private readonly MetadataReaderService _metadataReader = new();
    private readonly FileNameParserService _fileNameParser = new();
    private readonly MetadataComparerService _metadataComparer = new();
    private readonly MusicAnalysisEngine _analysisEngine = new();
    private readonly AudioQualityAnalyzerService _qualityAnalyzer = new();
    private readonly FileSimulationService _simulationService = new();
    public List<AudioFile> ScanFolder(string folderPath)
    {
        List<AudioFile> result = new();

        if (!Directory.Exists(folderPath))
            return result;

        foreach (string file in Directory.EnumerateFiles(
                     folderPath,
                     "*.*",
                     SearchOption.AllDirectories))
        {
            string extension = Path.GetExtension(file).ToLower();

            if (!SupportedExtensions.Contains(extension))
                continue;

            FileInfo info = new(file);

            AudioFile audioFile = new()
            {
                FileName = info.Name,
                FullPath = info.FullName,
                Extension = extension,
                FileSizeBytes = info.Length,
                Status = "Encontrado"
            };

            _metadataReader.ReadMetadata(audioFile);

            audioFile.QualityAnalysis =
                _qualityAnalyzer.Analyze(audioFile);

            audioFile.ParsedName =
                _fileNameParser.Parse(audioFile);

            audioFile.Comparison =
                _metadataComparer.Compare(audioFile);

            audioFile.Analysis =
                _analysisEngine.Analyze(audioFile);

            audioFile.Simulation =
                _simulationService.Build(audioFile);

            result.Add(audioFile);
        }

        return result;
    }
}