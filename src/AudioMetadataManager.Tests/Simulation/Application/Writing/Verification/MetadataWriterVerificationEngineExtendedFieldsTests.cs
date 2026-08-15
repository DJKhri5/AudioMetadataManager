using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.Verification.Engine;
using Xunit;

namespace AudioMetadataManager.Tests.Simulation.Application
    .Writing.Verification;

public sealed class
    MetadataWriterVerificationEngineExtendedFieldsTests
{
    [Fact]
    public void VerificationEngine_SupportsVersion()
    {
        Assert.True(
            IsFieldSupported(
                MetadataField.Version));
    }

    [Fact]
    public void VerificationEngine_SupportsLabel()
    {
        Assert.True(
            IsFieldSupported(
                MetadataField.Label));
    }

    private static bool IsFieldSupported(
        MetadataField field)
    {
        MetadataWriterVerificationEngine engine =
            new();

        System.Reflection.MethodInfo? method =
            typeof(MetadataWriterVerificationEngine)
                .GetMethod(
                    "IsSupportedField",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Static);

        Assert.NotNull(
            method);

        object? result =
            method.Invoke(
                null,
                new object[]
                {
                    field
                });

        return Assert.IsType<bool>(
            result);
    }
}