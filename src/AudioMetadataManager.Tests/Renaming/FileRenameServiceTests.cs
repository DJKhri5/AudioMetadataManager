using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.Renaming;
using AudioMetadataManager.UI.Services.Renaming.Models;
using System.IO;
using System.Security.Cryptography;
using Xunit;

namespace AudioMetadataManager.Tests.Renaming;

public class FileRenameServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly FileRenameService _renameService;
    private readonly FileRenameCollisionDetector _collisionDetector;

    public FileRenameServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "AMM_RenameTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
        _collisionDetector = new FileRenameCollisionDetector();
        _renameService = new FileRenameService(_collisionDetector);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }
        catch
        {
            // Ignorar errores de limpieza de pruebas
        }
    }

    private string CreateTestFile(string fileName, string content = "TEST_AUDIO_CONTENT_HEADER_12345")
    {
        string filePath = Path.Combine(_tempDirectory, fileName);
        File.WriteAllText(filePath, content);
        return filePath;
    }

    [Fact]
    public void Validate_ReturnsReadyToRename_WhenSafeAndNoCollision()
    {
        string filePath = CreateTestFile("original.mp3");
        AudioFile audioFile = new()
        {
            FullPath = filePath,
            FileName = "original.mp3",
            Extension = ".mp3",
            Simulation = new FileSimulationResult
            {
                ProposedFileName = "Artist - Safe Title.mp3"
            }
        };

        var validation = _collisionDetector.Validate(audioFile);

        Assert.Equal(RenameValidationStatus.ReadyToRename, validation.Status);
        Assert.True(validation.CanProceed);
        Assert.Equal("Artist - Safe Title.mp3", validation.SanitizedFileName);
    }

    [Fact]
    public void Validate_DetectsDestinationCollisionDisk_WhenTargetFileExists()
    {
        string filePath = CreateTestFile("original.mp3");
        CreateTestFile("Target File.mp3"); // Ya existe en disco

        AudioFile audioFile = new()
        {
            FullPath = filePath,
            FileName = "original.mp3",
            Extension = ".mp3",
            Simulation = new FileSimulationResult
            {
                ProposedFileName = "Target File.mp3"
            }
        };

        var validation = _collisionDetector.Validate(audioFile);

        Assert.Equal(RenameValidationStatus.DestinationCollisionDisk, validation.Status);
        Assert.False(validation.CanProceed);
        Assert.True(validation.HasCollision);
    }

    [Fact]
    public void Validate_DetectsDestinationCollisionBatch_WhenTwoFilesTargetSameName()
    {
        string file1 = CreateTestFile("file1.mp3");
        string file2 = CreateTestFile("file2.mp3");

        AudioFile audio1 = new()
        {
            FullPath = file1,
            FileName = "file1.mp3",
            Extension = ".mp3",
            Simulation = new FileSimulationResult { ProposedFileName = "Shared Name.mp3" }
        };

        AudioFile audio2 = new()
        {
            FullPath = file2,
            FileName = "file2.mp3",
            Extension = ".mp3",
            Simulation = new FileSimulationResult { ProposedFileName = "Shared Name.mp3" }
        };

        var batch = new List<AudioFile> { audio1, audio2 };

        var validation = _collisionDetector.Validate(audio1, batch);

        Assert.Equal(RenameValidationStatus.DestinationCollisionBatch, validation.Status);
        Assert.False(validation.CanProceed);
    }

    [Fact]
    public void Validate_IdentifiesIdenticalName_AsNoOp()
    {
        string filePath = CreateTestFile("exact_name.mp3");
        AudioFile audioFile = new()
        {
            FullPath = filePath,
            FileName = "exact_name.mp3",
            Extension = ".mp3",
            Simulation = new FileSimulationResult
            {
                ProposedFileName = "exact_name.mp3"
            }
        };

        var validation = _collisionDetector.Validate(audioFile);

        Assert.Equal(RenameValidationStatus.IdenticalNameNoOp, validation.Status);
        Assert.False(validation.CanProceed);
        Assert.True(validation.IsNoOp);
    }

    [Fact]
    public void Rename_ExecutesPhysicalRename_AndPreservesContentHash()
    {
        string filePath = CreateTestFile("old_track.mp3", "ORIGINAL_DATA_FOR_HASH_TEST");
        string initialHash = ComputeHash(filePath);

        AudioFile audioFile = new()
        {
            FullPath = filePath,
            FileName = "old_track.mp3",
            Extension = ".mp3",
            Simulation = new FileSimulationResult
            {
                ProposedFileName = "New Artist - New Track.mp3"
            }
        };

        var result = _renameService.Rename(audioFile);

        Assert.True(result.WasSuccessful);
        Assert.False(File.Exists(filePath));
        Assert.True(File.Exists(result.NewFilePath));
        Assert.Equal("New Artist - New Track.mp3", audioFile.FileName);
        Assert.Equal(result.NewFilePath, audioFile.FullPath);

        string afterHash = ComputeHash(result.NewFilePath);
        Assert.Equal(initialHash, afterHash);
        Assert.Single(_renameService.Journal);
    }

    [Fact]
    public void Rollback_RestoresOriginalFileOnDisk_AndUpdatesModel()
    {
        string filePath = CreateTestFile("file_to_rollback.mp3");
        AudioFile audioFile = new()
        {
            FullPath = filePath,
            FileName = "file_to_rollback.mp3",
            Extension = ".mp3",
            Simulation = new FileSimulationResult
            {
                ProposedFileName = "Renamed For Rollback.mp3"
            }
        };

        var renameResult = _renameService.Rename(audioFile);
        Assert.True(renameResult.WasSuccessful);
        Assert.True(File.Exists(renameResult.NewFilePath));

        // Revertir
        bool rollbackOk = _renameService.Rollback(renameResult.JournalEntry!, out string error, audioFile);

        Assert.True(rollbackOk);
        Assert.Empty(error);
        Assert.True(File.Exists(filePath));
        Assert.False(File.Exists(renameResult.NewFilePath));
        Assert.Equal("file_to_rollback.mp3", audioFile.FileName);
        Assert.Equal(filePath, audioFile.FullPath);
    }

    private static string ComputeHash(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var sha256 = SHA256.Create();
        return Convert.ToHexString(sha256.ComputeHash(stream));
    }
}
