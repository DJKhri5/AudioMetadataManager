using AudioMetadataManager.UI.Models;
using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation;
using AudioMetadataManager.UI.Services.Simulation.Planning.Models;
using Xunit;

namespace AudioMetadataManager.Tests.Renaming;

public class ExternalMetadataRenamingIntegrationTests
{
    [Fact]
    public void Synchronizer_UpdatesProposedFileName_FromMetadataPlan()
    {
        var synchronizer = new SimulationPlanToRenamingSynchronizer();

        var audioFile = new AudioFile
        {
            FileName = "track01_unknown.mp3",
            FullPath = "C:\\Music\\track01_unknown.mp3",
            Extension = ".mp3",
            Artist = "Old Artist",
            Title = "Old Title"
        };

        var changePlan = new MetadataChangePlan
        {
            FileName = audioFile.FileName,
            FilePath = audioFile.FullPath,
            Proposals = new List<MetadataChangeProposal>
            {
                new()
                {
                    Field = MetadataField.Artist,
                    CurrentValue = "Old Artist",
                    ProposedValue = "Tiësto",
                    Decision = MetadataChangeDecision.EligibleForAutomaticApply
                },
                new()
                {
                    Field = MetadataField.Title,
                    CurrentValue = "Old Title",
                    ProposedValue = "Adagio for Strings",
                    Decision = MetadataChangeDecision.EligibleForAutomaticApply
                },
                new()
                {
                    Field = MetadataField.Version,
                    CurrentValue = "",
                    ProposedValue = "Original Mix",
                    Decision = MetadataChangeDecision.EligibleForAutomaticApply
                },
                new()
                {
                    Field = MetadataField.Label,
                    CurrentValue = "",
                    ProposedValue = "Magik Muzik",
                    Decision = MetadataChangeDecision.EligibleForAutomaticApply
                }
            }
        };

        synchronizer.Synchronize(audioFile, changePlan);

        Assert.NotNull(audioFile.Simulation);
        Assert.Equal("Tiësto - Adagio for Strings (Original Mix).mp3", audioFile.Simulation.ProposedFileName);
        Assert.True(audioFile.Simulation.HasChanges);
        Assert.Contains(audioFile.Simulation.Changes, c => c.PropertyName == "Nombre de archivo");
    }

    [Fact]
    public void Synchronizer_HandlesVersionlessCanonicalTracks_Cleanly()
    {
        var synchronizer = new SimulationPlanToRenamingSynchronizer();

        var audioFile = new AudioFile
        {
            FileName = "01_artist_song.flac",
            FullPath = "C:\\Music\\01_artist_song.flac",
            Extension = ".flac"
        };

        var changePlan = new MetadataChangePlan
        {
            FileName = audioFile.FileName,
            FilePath = audioFile.FullPath,
            Proposals = new List<MetadataChangeProposal>
            {
                new()
                {
                    Field = MetadataField.Artist,
                    ProposedValue = "Daft Punk",
                    Decision = MetadataChangeDecision.EligibleForAutomaticApply
                },
                new()
                {
                    Field = MetadataField.Title,
                    ProposedValue = "One More Time",
                    Decision = MetadataChangeDecision.EligibleForAutomaticApply
                }
            }
        };

        synchronizer.Synchronize(audioFile, changePlan);

        Assert.NotNull(audioFile.Simulation);
        Assert.Equal("Daft Punk - One More Time.flac", audioFile.Simulation.ProposedFileName);
    }
}
