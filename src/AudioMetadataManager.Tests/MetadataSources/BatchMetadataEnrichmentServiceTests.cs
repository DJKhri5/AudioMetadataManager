using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.MetadataSources.Batch;
using AudioMetadataManager.UI.Services.MetadataSources.Batch.Models;
using Xunit;

namespace AudioMetadataManager.Tests.MetadataSources;

public class BatchMetadataEnrichmentServiceTests
{
    [Fact]
    public async Task EnrichBatchAsync_EmptyList_ReturnsEmptyResult()
    {
        var service = new BatchMetadataEnrichmentService();
        var result = await service.EnrichBatchAsync(Array.Empty<AudioFile>());

        Assert.NotNull(result);
        Assert.Equal(0, result.TotalRequested);
        Assert.Equal(0, result.TotalProcessed);
        Assert.False(result.WasCancelled);
    }

    [Fact]
    public async Task EnrichBatchAsync_ProcessesFilesAndReportsProgress()
    {
        var service = new BatchMetadataEnrichmentService();

        var files = new List<AudioFile>
        {
            new()
            {
                FileName = "Armin van Buuren - Communication (Extended Mix).mp3",
                FullPath = "C:\\Music\\Armin van Buuren - Communication (Extended Mix).mp3",
                Extension = ".mp3",
                Artist = "Armin van Buuren",
                Title = "Communication"
            },
            new()
            {
                FileName = "Daft Punk - One More Time.flac",
                FullPath = "C:\\Music\\Daft Punk - One More Time.flac",
                Extension = ".flac",
                Artist = "Daft Punk",
                Title = "One More Time"
            }
        };

        var progressReports = new List<BatchMetadataEnrichmentProgress>();
        var progress = new Progress<BatchMetadataEnrichmentProgress>(p => progressReports.Add(p));

        var result = await service.EnrichBatchAsync(files, progress);

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalRequested);
        Assert.Equal(2, result.TotalProcessed);
        Assert.False(result.WasCancelled);
        Assert.NotEmpty(result.ItemResults);
    }

    [Fact]
    public async Task EnrichBatchAsync_Cancellation_StopsEarlySafely()
    {
        var service = new BatchMetadataEnrichmentService();

        var files = new List<AudioFile>
        {
            new()
            {
                FileName = "Track 01.mp3",
                FullPath = "C:\\Music\\Track 01.mp3",
                Extension = ".mp3"
            },
            new()
            {
                FileName = "Track 02.mp3",
                FullPath = "C:\\Music\\Track 02.mp3",
                Extension = ".mp3"
            }
        };

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        var result = await service.EnrichBatchAsync(files, null, cts.Token);

        Assert.NotNull(result);
        Assert.True(result.WasCancelled);
        Assert.Equal(0, result.TotalProcessed);
    }
}
