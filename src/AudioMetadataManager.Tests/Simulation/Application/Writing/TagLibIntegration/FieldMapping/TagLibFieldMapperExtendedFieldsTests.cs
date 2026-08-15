using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.FieldMapping;
using Xunit;

namespace AudioMetadataManager.Tests
    .Simulation.Application.Writing.TagLibIntegration.FieldMapping;

public sealed class TagLibFieldMapperExtendedFieldsTests
{
    [Theory]
    [InlineData(MetadataField.Version)]
    [InlineData(MetadataField.Label)]
    public void ExtendedField_IsSupported(
        MetadataField field)
    {
        TagLibFieldMapper mapper =
            new();

        Assert.True(
            mapper.IsSupported(
                field));
    }

    [Fact]
    public void Version_UsesSubtitleAndRoundTrips()
    {
        TagLib.Id3v2.Tag tag =
            new();

        TagLibFieldMapper mapper =
            new();

        MetadataFieldChange change =
            new()
            {
                Field =
                    MetadataField.Version,

                OriginalValue =
                    string.Empty,

                NewValue =
                    "Extended Mix",

                WasManuallyApproved =
                    true
            };

        var result =
            mapper.PrepareChange(
                tag,
                change);

        Assert.True(
            result.IsSupported);

        Assert.True(
            result.ValuePrepared);

        Assert.Equal(
            "Extended Mix",
            tag.Subtitle);

        Assert.Equal(
            "Extended Mix",
            mapper.ReadValue(
                tag,
                MetadataField.Version));
    }

    [Fact]
    public void Label_UsesPublisherAndRoundTrips()
    {
        TagLib.Id3v2.Tag tag =
            new();

        TagLibFieldMapper mapper =
            new();

        MetadataFieldChange change =
            new()
            {
                Field =
                    MetadataField.Label,

                OriginalValue =
                    string.Empty,

                NewValue =
                    "Afterlife",

                WasManuallyApproved =
                    true
            };

        var result =
            mapper.PrepareChange(
                tag,
                change);

        Assert.True(
            result.IsSupported);

        Assert.True(
            result.ValuePrepared);

        Assert.Equal(
            "Afterlife",
            tag.Publisher);

        Assert.Equal(
            "Afterlife",
            mapper.ReadValue(
                tag,
                MetadataField.Label));
    }

    [Fact]
    public void ExtendedFields_PreserveIndependentValues()
    {
        TagLib.Id3v2.Tag tag =
            new();

        TagLibFieldMapper mapper =
            new();

        mapper.PrepareChange(
            tag,
            new MetadataFieldChange
            {
                Field =
                    MetadataField.Version,

                NewValue =
                    "Original Mix",

                WasManuallyApproved =
                    true
            });

        mapper.PrepareChange(
            tag,
            new MetadataFieldChange
            {
                Field =
                    MetadataField.Label,

                NewValue =
                    "Anjunabeats",

                WasManuallyApproved =
                    true
            });

        Assert.Equal(
            "Original Mix",
            tag.Subtitle);

        Assert.Equal(
            "Anjunabeats",
            tag.Publisher);
    }
}