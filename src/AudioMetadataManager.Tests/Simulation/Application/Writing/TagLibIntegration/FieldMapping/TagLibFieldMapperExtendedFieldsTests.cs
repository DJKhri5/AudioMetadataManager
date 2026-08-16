using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Writing.TagLibIntegration.FieldMapping;
using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services;
using System.Security.Cryptography;
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
    public void Label_IsPersistedAndChangesPhysicalFile()
    {
        string filePath =
            CreateTestMp3File();

        try
        {
            string hashBefore =
                ComputeSha256(
                    filePath);

            using (TagLib.File tagFile =
                TagLib.File.Create(
                    filePath))
            {
                MetadataFieldChange change =
                    new()
                    {
                        Field =
                            MetadataField.Label,

                        NewValue =
                            "Chrome Red"
                    };

                TagLibFieldMapper mapper =
                    new();

                var result =
                    mapper.PrepareChange(
                        tagFile,
                        change);

                Assert.True(
                    result.WasSuccessful);

                tagFile.Save();
            }

            string hashAfter =
                ComputeSha256(
                    filePath);

            Assert.NotEqual(
                hashBefore,
                hashAfter);

            using TagLib.File reopenedFile =
                TagLib.File.Create(
                    filePath);

            TagLib.Id3v2.Tag? id3v2Tag =
                reopenedFile.GetTag(
                    TagLib.TagTypes.Id3v2)
                as TagLib.Id3v2.Tag;

            Assert.NotNull(
                id3v2Tag);

            TagLib.Id3v2.TextInformationFrame? frame =
                TagLib.Id3v2.TextInformationFrame.Get(
                    id3v2Tag,
                    "TPUB",
                    false);

            Assert.NotNull(
                frame);

            Assert.Contains(
                "Chrome Red",
                frame.Text);

            AudioFile audioFile =
                new()
                {
                    FullPath =
                        filePath,

                    Extension =
                        ".mp3"
                };

            MetadataReaderService reader =
                new();

            reader.ReadMetadata(
                audioFile);

            Assert.Equal(
                "Chrome Red",
                audioFile.Label);
        }
        finally
        {
            File.Delete(
                filePath);
        }
    }

    [Fact]
    public void Version_IsPersistedAsId3v2Tit3AndChangesPhysicalFile()
    {
        string filePath =
            CreateTestMp3File();

        try
        {
            string hashBefore =
                ComputeSha256(
                    filePath);

            using (TagLib.File tagFile =
                TagLib.File.Create(
                    filePath))
            {
                MetadataFieldChange change =
                    new()
                    {
                        Field =
                            MetadataField.Version,

                        NewValue =
                            "Extended Mix"
                    };

                TagLibFieldMapper mapper =
                    new();

                var result =
                    mapper.PrepareChange(
                        tagFile,
                        change);

                Assert.True(
                    result.WasSuccessful);

                tagFile.Save();
            }

            string hashAfter =
                ComputeSha256(
                    filePath);

            Assert.NotEqual(
                hashBefore,
                hashAfter);

            using TagLib.File reopenedFile =
                TagLib.File.Create(
                    filePath);

            Assert.Equal(
                "Extended Mix",
                reopenedFile.Tag.Subtitle);

            TagLib.Id3v2.Tag? id3v2Tag =
                reopenedFile.GetTag(
                    TagLib.TagTypes.Id3v2)
                as TagLib.Id3v2.Tag;

            Assert.NotNull(
                id3v2Tag);

            TagLib.Id3v2.TextInformationFrame? frame =
                TagLib.Id3v2.TextInformationFrame.Get(
                    id3v2Tag,
                    "TIT3",
                    false);

            Assert.NotNull(
                frame);

            Assert.Contains(
                "Extended Mix",
                frame.Text);

            AudioFile audioFile =
                new()
                {
                    FullPath =
                        filePath,

                    Extension =
                        ".mp3"
                };

            MetadataReaderService reader =
                new();

            reader.ReadMetadata(
                audioFile);

            Assert.Equal(
                "Extended Mix",
                audioFile.Version);
        }
        finally
        {
            File.Delete(
                filePath);
        }
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

    private static string CreateTestMp3File()
    {
        string filePath =
            Path.Combine(
                Path.GetTempPath(),
                $"audio-metadata-{Guid.NewGuid():N}.mp3");

        byte[] mpegFrame =
            new byte[417];

        mpegFrame[0] =
            0xFF;

        mpegFrame[1] =
            0xFB;

        mpegFrame[2] =
            0x90;

        mpegFrame[3] =
            0x64;

        File.WriteAllBytes(
            filePath,
            mpegFrame);

        return filePath;
    }

    private static string ComputeSha256(
        string filePath)
    {
        using FileStream stream =
            File.OpenRead(
                filePath);

        return Convert.ToHexString(
            SHA256.HashData(
                stream));
    }
}
