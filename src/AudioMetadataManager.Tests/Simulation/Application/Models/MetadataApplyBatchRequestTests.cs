using Xunit;
using AudioMetadataManager.UI.Services
    .MetadataSources.Models;
using AudioMetadataManager.UI.Services
    .Simulation.Application.Models;

namespace AudioMetadataManager.Tests
    .Simulation.Application.Models;

public sealed class MetadataApplyBatchRequestTests
{
    [Fact]
    public void EmptyBatch_IsNotStructurallyValid()
    {
        MetadataApplyBatchRequest request =
            new();

        Assert.False(
            request.IsStructurallyValid);

        Assert.Equal(
            0,
            request.ValidRequestCount);
    }

    [Fact]
    public void ValidRequest_ProducesValidBatch()
    {
        MetadataApplyRequest request =
            CreateValidRequest(
                @"C:\Tests\track-1.flac",
                "track-1.flac");

        MetadataApplyBatchRequest batch =
            new()
            {
                Requests =
                    new[]
                    {
                        request
                    }
            };

        Assert.True(
            batch.IsStructurallyValid);

        Assert.Equal(
            1,
            batch.ValidRequestCount);
    }

    [Fact]
    public void DuplicateFilePaths_InvalidateBatch()
    {
        MetadataApplyRequest first =
            CreateValidRequest(
                @"C:\Tests\duplicate.flac",
                "duplicate.flac");

        MetadataApplyRequest second =
            CreateValidRequest(
                @"C:\Tests\duplicate.flac",
                "duplicate.flac");

        MetadataApplyBatchRequest batch =
            new()
            {
                Requests =
                    new[]
                    {
                        first,
                        second
                    }
            };

        Assert.False(
            batch.IsStructurallyValid);
    }

    [Fact]
    public void ValidChangeCount_AggregatesRequests()
    {
        MetadataApplyRequest first =
            CreateValidRequest(
                @"C:\Tests\track-1.flac",
                "track-1.flac");

        MetadataApplyRequest second =
            CreateValidRequest(
                @"C:\Tests\track-2.flac",
                "track-2.flac");

        MetadataApplyBatchRequest batch =
            new()
            {
                Requests =
                    new[]
                    {
                        first,
                        second
                    }
            };

        Assert.Equal(
            2,
            batch.ValidChangeCount);
    }

    private static MetadataApplyRequest
        CreateValidRequest(
            string filePath,
            string fileName)
    {
        return
            new MetadataApplyRequest
            {
                RequestId =
                    Guid.NewGuid(),

                PlanId =
                    Guid.NewGuid(),

                FilePath =
                    filePath,

                FileName =
                    fileName,

                RequireBackup =
                    true,

                RequirePostWriteVerification =
                    true,

                Changes =
                    new[]
                    {
                        new MetadataFieldChange
                        {
                            Field =
                                MetadataField.Genre,

                            OriginalValue =
                                "House",

                            NewValue =
                                "Techno",

                            WasManuallyApproved =
                                true,

                            Confidence =
                                1.0,

                            SupportingSources =
                                new[]
                                {
                                    "Automated test"
                                }
                        }
                    }
            };
    }
}