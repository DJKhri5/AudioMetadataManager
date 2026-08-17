using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.Renaming;
using AudioMetadataManager.UI.Services.Renaming.Models;
using Xunit;

namespace AudioMetadataManager.Tests.Renaming;

public class FileRenameBatchServiceTests
{
    [Fact]
    public void PrepareBatch_NullOrEmpty_ReturnsEmptyResult()
    {
        var service = new FileRenameBatchService();

        var nullResult = service.PrepareBatch(null!);
        Assert.Empty(nullResult.Items);
        Assert.Equal(0, nullResult.ReadyToRenameCount);

        var emptyResult = service.PrepareBatch(Array.Empty<AudioFile>());
        Assert.Empty(emptyResult.Items);
        Assert.Equal(0, emptyResult.ReadyToRenameCount);
    }

    [Fact]
    public void PrepareBatch_ClassifiesIntraBatchCollisionAndUnchangedCorrectly()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "AMM_BatchPrepTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            // Archivo 1 y 2 van a proponer el mismo nombre
            string file1Path = Path.Combine(tempDir, "file1.mp3");
            string file2Path = Path.Combine(tempDir, "file2.mp3");
            string file3Path = Path.Combine(tempDir, "Artist - 3.mp3");

            File.WriteAllText(file1Path, "dummy1");
            File.WriteAllText(file2Path, "dummy2");
            File.WriteAllText(file3Path, "dummy3");

            var file1 = new AudioFile
            {
                FileName = "file1.mp3",
                FullPath = file1Path,
                Extension = ".mp3",
                Artist = "Artist",
                Title = "SameTitle"
            };

            var file2 = new AudioFile
            {
                FileName = "file2.mp3",
                FullPath = file2Path,
                Extension = ".mp3",
                Artist = "Artist",
                Title = "SameTitle"
            };

            var file3 = new AudioFile
            {
                FileName = "Artist - 3.mp3",
                FullPath = file3Path,
                Extension = ".mp3",
                Artist = "Artist",
                Title = "3"
            };

            var service = new FileRenameBatchService();
            var prep = service.PrepareBatch(new[] { file1, file2, file3 });

            Assert.Equal(3, prep.TotalCandidatesCount);
            // file1 y file2 deben tener conflicto en lote
            Assert.Contains(prep.Items, i => i.Status == RenameValidationStatus.DestinationCollisionBatch);
            // file3 ya coincide (o sin cambios)
            Assert.Contains(prep.Items, i => i.Status == RenameValidationStatus.IdenticalNameNoOp);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }
    }

    [Fact]
    public void ExecuteBatch_PhysicallyRenamesAndRollbacksAllFiles()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "AMM_BatchExecTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            string origA = Path.Combine(tempDir, "track_01.mp3");
            string origB = Path.Combine(tempDir, "track_02.flac");

            File.WriteAllText(origA, "mp3 content test");
            File.WriteAllText(origB, "flac content test");

            var fileA = new AudioFile
            {
                FileName = "track_01.mp3",
                FullPath = origA,
                Extension = ".mp3",
                Artist = "Tiësto",
                Title = "Elements of Life"
            };

            var fileB = new AudioFile
            {
                FileName = "track_02.flac",
                FullPath = origB,
                Extension = ".flac",
                Artist = "Armin van Buuren",
                Title = "Mirage"
            };

            var service = new FileRenameBatchService();
            var prep = service.PrepareBatch(new[] { fileA, fileB });

            Assert.Equal(2, prep.ReadyToRenameCount);

            // Ejecución
            var execResult = service.ExecuteBatch(prep);

            Assert.True(execResult.WasFullySuccessful);
            Assert.Equal(2, execResult.SucceededCount);
            Assert.Equal(0, execResult.FailedCount);

            // Verificar que los archivos físicos existen con sus nuevos nombres
            string expectedA = Path.Combine(tempDir, "Tiësto - Elements of Life.mp3");
            string expectedB = Path.Combine(tempDir, "Armin van Buuren - Mirage.flac");

            Assert.True(File.Exists(expectedA));
            Assert.True(File.Exists(expectedB));
            Assert.False(File.Exists(origA));
            Assert.False(File.Exists(origB));

            // Reversión (Rollback en lote)
            int rolledBack = service.RollbackBatch(execResult, out var rollbackErrors);

            Assert.Equal(2, rolledBack);
            Assert.Empty(rollbackErrors);

            // Verificar que volvieron a su nombre original
            Assert.True(File.Exists(origA));
            Assert.True(File.Exists(origB));
            Assert.False(File.Exists(expectedA));
            Assert.False(File.Exists(expectedB));
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }
    }

    [Fact]
    public void ExecuteBatch_SkipsUnselectedFiles()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "AMM_BatchSkipTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            string origA = Path.Combine(tempDir, "track_01.mp3");
            string origB = Path.Combine(tempDir, "track_02.mp3");

            File.WriteAllText(origA, "file 1");
            File.WriteAllText(origB, "file 2");

            var fileA = new AudioFile
            {
                FileName = "track_01.mp3",
                FullPath = origA,
                Extension = ".mp3",
                Artist = "ArtistA",
                Title = "TrackA"
            };

            var fileB = new AudioFile
            {
                FileName = "track_02.mp3",
                FullPath = origB,
                Extension = ".mp3",
                Artist = "ArtistB",
                Title = "TrackB"
            };

            var service = new FileRenameBatchService();
            var prep = service.PrepareBatch(new[] { fileA, fileB });

            // Deseleccionar fileB
            prep.Items[1].IsSelected = false;

            var execResult = service.ExecuteBatch(prep, onlySelected: true);

            Assert.Equal(1, execResult.SucceededCount);
            Assert.Equal(1, execResult.SkippedCount);

            string expectedA = Path.Combine(tempDir, "ArtistA - TrackA.mp3");
            Assert.True(File.Exists(expectedA));
            Assert.True(File.Exists(origB)); // track_02.mp3 no debe haberse modificado
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }
    }
}
