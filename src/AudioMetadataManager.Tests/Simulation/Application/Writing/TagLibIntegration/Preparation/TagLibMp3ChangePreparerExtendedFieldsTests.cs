using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation.Application.Models;
using AudioMetadataManager.UI.Services.Simulation.Application.Writing.TagLibIntegration.Preparation;
using System.Security.Cryptography;
using Xunit;

namespace AudioMetadataManager.Tests.Simulation.Application.Writing
    .TagLibIntegration.Preparation;

public sealed class TagLibMp3ChangePreparerExtendedFieldsTests
{
    [Theory]
    [InlineData(MetadataField.Version, "Extended Mix")]
    [InlineData(MetadataField.Label, "Afterlife")]
    public void ExtendedField_IsPreparedWithoutPersisting(
        MetadataField field,
        string requestedValue)
    {
        string filePath =
            CreateTestMp3File();

        try
        {
            string hashBefore =
                ComputeSha256(
                    filePath);

            MetadataFieldChange change =
                new()
                {
                    Field =
                        field,

                    OriginalValue =
                        string.Empty,

                    NewValue =
                        requestedValue,

                    WasManuallyApproved =
                        true
                };

            var result =
                new TagLibMp3ChangePreparer()
                    .Prepare(
                        filePath,
                        new[] { change });

            Assert.True(
                result.WasSuccessful,
                result.Summary);

            Assert.Single(
                result.FieldResults);

            Assert.Equal(
                requestedValue,
                result.FieldResults[0].PreparedValue);

            Assert.False(
                result.SaveWasExecuted);

            Assert.True(
                result.PhysicalFileRemainedUnchanged);

            Assert.Equal(
                hashBefore,
                ComputeSha256(
                    filePath));
        }
        finally
        {
            File.Delete(
                filePath);
        }
    }

    private static string CreateTestMp3File()
    {
        string filePath =
            Path.Combine(
                Path.GetTempPath(),
                $"audio-metadata-preparation-{Guid.NewGuid():N}.mp3");

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
