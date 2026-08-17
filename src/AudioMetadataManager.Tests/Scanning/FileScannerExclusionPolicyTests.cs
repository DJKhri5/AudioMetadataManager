using AudioMetadataManager.UI.Services;
using AudioMetadataManager.UI.Services.Scanning;
using Xunit;

namespace AudioMetadataManager.Tests.Scanning;

public class FileScannerExclusionPolicyTests
{
    [Theory]
    [InlineData("AudioMetadataManager_Backup", true)]
    [InlineData("AudioMetadataManager_Backup_2026-08-16", true)]
    [InlineData("AMM_Backups", true)]
    [InlineData("AMM_Staging", true)]
    [InlineData("AMM_Backup_01", true)]
    [InlineData("Backups", true)]
    [InlineData("Backup", true)]
    [InlineData("_backups", true)]
    [InlineData(".backup", true)]
    [InlineData(".git", true)]
    [InlineData(".vs", true)]
    [InlineData(".idea", true)]
    [InlineData("$RECYCLE.BIN", true)]
    [InlineData("Musica", false)]
    [InlineData("Trance 2023", false)]
    [InlineData("Lossless", false)]
    [InlineData("Electronic", false)]
    public void ShouldExcludeDirectory_EvaluatesCorrectly(string folderName, bool expectedExcluded)
    {
        var policy = new FileScannerExclusionPolicy();
        string dummyPath = Path.Combine(@"C:\Music", folderName);

        bool result = policy.ShouldExcludeDirectory(dummyPath);

        Assert.Equal(expectedExcluded, result);
    }

    [Fact]
    public void FileScannerService_IgnoresBackupDirectoryDuringScan()
    {
        string testRoot = Path.Combine(Path.GetTempPath(), "AMM_ScannerTest_" + Guid.NewGuid().ToString("N"));
        string backupDir = Path.Combine(testRoot, "AudioMetadataManager_Backup");
        string validSubDir = Path.Combine(testRoot, "Albums");

        try
        {
            Directory.CreateDirectory(testRoot);
            Directory.CreateDirectory(backupDir);
            Directory.CreateDirectory(validSubDir);

            // Crear archivo en raíz
            string rootFile = Path.Combine(testRoot, "track1.mp3");
            File.WriteAllText(rootFile, "dummy mp3");

            // Crear archivo en subcarpeta válida
            string albumFile = Path.Combine(validSubDir, "track2.mp3");
            File.WriteAllText(albumFile, "dummy mp3");

            // Crear archivo en subcarpeta de respaldo (debe ser ignorado)
            string backupFile = Path.Combine(backupDir, "track1_backup.mp3");
            File.WriteAllText(backupFile, "dummy mp3");

            var scanner = new FileScannerService();
            var scannedFiles = scanner.ScanFolder(testRoot);

            Assert.Equal(2, scannedFiles.Count);
            Assert.Contains(scannedFiles, f => f.FullPath.Equals(rootFile, StringComparison.OrdinalIgnoreCase));
            Assert.Contains(scannedFiles, f => f.FullPath.Equals(albumFile, StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(scannedFiles, f => f.FullPath.Equals(backupFile, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            if (Directory.Exists(testRoot))
            {
                try { Directory.Delete(testRoot, true); } catch { }
            }
        }
    }
}
