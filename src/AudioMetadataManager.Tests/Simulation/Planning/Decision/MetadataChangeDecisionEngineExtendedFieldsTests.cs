using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Planning.Decision;
using System.Reflection;
using Xunit;

namespace AudioMetadataManager.Tests.Simulation.Planning.Decision;

public sealed class MetadataChangeDecisionEngineExtendedFieldsTests
{
    [Fact]
    public void ReadCurrentValue_ReturnsStoredLabel()
    {
        MetadataChangeDecisionEngine engine =
            new();

        AudioFile audioFile =
            new()
            {
                Label =
                    "Chrome Red"
            };

        MethodInfo? method =
            typeof(MetadataChangeDecisionEngine)
                .GetMethod(
                    "ReadCurrentValue",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

        Assert.NotNull(
            method);

        object? result =
            method.Invoke(
                engine,
                new object[]
                {
                    audioFile,
                    MetadataField.Label
                });

        Assert.Equal(
            "Chrome Red",
            Assert.IsType<string>(
                result));
    }

    [Fact]
    public void ReadCurrentValue_PrefersStoredVersion()
    {
        MetadataChangeDecisionEngine engine =
            new();

        AudioFile audioFile =
            new()
            {
                Title =
                    "Example Track (Original Mix)",

                Version =
                    "Extended Mix"
            };

        MethodInfo? method =
            typeof(MetadataChangeDecisionEngine)
                .GetMethod(
                    "ReadCurrentValue",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

        Assert.NotNull(
            method);

        object? result =
            method.Invoke(
                engine,
                new object[]
                {
                    audioFile,
                    MetadataField.Version
                });

        Assert.Equal(
            "Extended Mix",
            Assert.IsType<string>(
                result));
    }
}
