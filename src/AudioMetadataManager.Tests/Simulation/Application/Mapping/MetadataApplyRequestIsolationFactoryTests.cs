using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation.Application.Models;
using AudioMetadataManager.UI.Services.Simulation.Application.Testing.Coordination;
using Xunit;

namespace AudioMetadataManager.Tests
    .Simulation.Application.Mapping;

public sealed class MetadataApplyRequestIsolationFactoryTests
{
    [Fact]
    public void NullRequest_IsRejected()
    {
        MetadataApplyRequestIsolationFactory factory =
            new();

        Assert.Throws<ArgumentNullException>(
            () =>
                factory.Create(
                    null!,
                    @"C:\Tests\working.flac"));
    }

    [Fact]
    public void EmptyWorkingPath_IsRejected()
    {
        MetadataApplyRequestIsolationFactory factory =
            new();

        MetadataApplyRequest request =
            CreateRequest();

        Assert.Throws<ArgumentException>(
            () =>
                factory.Create(
                    request,
                    string.Empty));
    }

    [Fact]
    public void IdentifiersAndChanges_ArePreserved()
    {
        MetadataApplyRequestIsolationFactory factory =
            new();

        MetadataApplyRequest request =
            CreateRequest();

        MetadataApplyRequest isolated =
            factory.Create(
                request,
                @"C:\Tests\working.flac");

        Assert.Equal(
            request.RequestId,
            isolated.RequestId);

        Assert.Equal(
            request.PlanId,
            isolated.PlanId);

        Assert.Equal(
            request.CreatedAtUtc,
            isolated.CreatedAtUtc);

        Assert.Same(
            request.Changes,
            isolated.Changes);

        Assert.Equal(
            request.RequireBackup,
            isolated.RequireBackup);

        Assert.Equal(
            request.RequirePostWriteVerification,
            isolated.RequirePostWriteVerification);
    }

    [Fact]
    public void WorkingCopyPath_IsApplied()
    {
        MetadataApplyRequestIsolationFactory factory =
            new();

        MetadataApplyRequest request =
            CreateRequest();

        string workingPath =
            @"C:\Tests\isolated\working-track.flac";

        MetadataApplyRequest isolated =
            factory.Create(
                request,
                workingPath);

        Assert.Equal(
            Path.GetFullPath(
                workingPath),
            isolated.FilePath);

        Assert.Equal(
            "working-track.flac",
            isolated.FileName);
    }

    private static MetadataApplyRequest
        CreateRequest()
    {
        return
            new MetadataApplyRequest
            {
                RequestId =
                    Guid.NewGuid(),

                PlanId =
                    Guid.NewGuid(),

                CreatedAtUtc =
                    DateTimeOffset.UtcNow,

                FilePath =
                    @"C:\Tests\original.flac",

                FileName =
                    "original.flac",

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
                                1.0
                        }
                    }
            };
    }
}