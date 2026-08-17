using AudioMetadataManager.UI.Models;
using System.IO;
using System.Reflection.Metadata;
using AudioMetadataManager.UI.Services.Quality;
using AudioMetadataManager.UI.Services.Simulation;
using AudioMetadataManager.UI.Services.Scanning;

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
    private readonly FileScannerExclusionPolicy _exclusionPolicy;

    public FileScannerService()
        : this(new FileScannerExclusionPolicy())
    {
    }

    public FileScannerService(FileScannerExclusionPolicy exclusionPolicy)
    {
        _exclusionPolicy = exclusionPolicy ?? new FileScannerExclusionPolicy();
    }

    /// <summary>
    /// Lee un archivo compatible y construye su modelo completo
    /// utilizando los mismos servicios empleados por el escaneo de
    /// una biblioteca.
    /// </summary>
    public AudioFile? ScanFile(
        string filePath)
    {
        if (string.IsNullOrWhiteSpace(
                filePath))
        {
            return null;
        }

        string normalizedFilePath;

        try
        {
            normalizedFilePath =
                Path.GetFullPath(
                    filePath.Trim());
        }
        catch
        {
            return null;
        }

        if (!File.Exists(
                normalizedFilePath))
        {
            return null;
        }

        string extension =
            Path.GetExtension(
                    normalizedFilePath)
                .ToLowerInvariant();

        if (!SupportedExtensions.Contains(
                extension))
        {
            return null;
        }

        FileInfo info =
            new(normalizedFilePath);

        AudioFile audioFile =
            new()
            {
                FileName =
                    info.Name,

                FullPath =
                    info.FullName,

                Extension =
                    extension,

                FileSizeBytes =
                    info.Length,

                Status =
                    "Encontrado"
            };

        _metadataReader.ReadMetadata(
            audioFile);

        audioFile.QualityAnalysis =
            _qualityAnalyzer.Analyze(
                audioFile);

        audioFile.ParsedName =
            _fileNameParser.Parse(
                audioFile);

        audioFile.Comparison =
            _metadataComparer.Compare(
                audioFile);

        audioFile.Analysis =
            _analysisEngine.Analyze(
                audioFile);

        audioFile.Simulation =
            _simulationService.Build(
                audioFile);

        return audioFile;
    }
    public List<AudioFile> ScanFolder(string folderPath)
    {
        List<AudioFile> result = new();

        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            return result;

        Queue<string> pendingDirectories = new();
        pendingDirectories.Enqueue(folderPath);

        while (pendingDirectories.Count > 0)
        {
            string currentDirectory = pendingDirectories.Dequeue();

            try
            {
                // Encolar subdirectorios no excluidos
                foreach (string subDirectory in Directory.EnumerateDirectories(currentDirectory))
                {
                    if (!_exclusionPolicy.ShouldExcludeDirectory(subDirectory))
                    {
                        pendingDirectories.Enqueue(subDirectory);
                    }
                }

                // Enumerar archivos compatibles del directorio actual
                foreach (string file in Directory.EnumerateFiles(currentDirectory))
                {
                    string extension = Path.GetExtension(file).ToLowerInvariant();

                    if (!SupportedExtensions.Contains(extension))
                    {
                        continue;
                    }

                    AudioFile? audioFile = ScanFile(file);
                    if (audioFile is not null)
                    {
                        result.Add(audioFile);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Ignorar carpetas protegidas sin permisos
            }
            catch (DirectoryNotFoundException)
            {
                // Ignorar carpetas movidas o eliminadas durante el escaneo
            }
        }

        return result;
    }
}