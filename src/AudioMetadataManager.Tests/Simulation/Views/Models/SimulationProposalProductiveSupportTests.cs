using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation.Planning.Models;
using AudioMetadataManager.UI.Views.Models.Simulation;
using Xunit;

namespace AudioMetadataManager.Tests
    .Simulation.Views.Models;

public sealed class SimulationProposalProductiveSupportTests
{
    [Fact]
    public void SupportedGenre_IsProductivelySelectable()
    {
        SimulationProposalViewModel proposal =
            CreateProposal(
                MetadataField.Genre);

        Assert.True(
            proposal.IsProductiveApplicationSupported);

        Assert.True(
            proposal.CanSelectForProductiveApplication);

        Assert.Equal(
            "Disponible",
            proposal.ProductiveApplicationStatus);
    }

    [Fact]
    public void UnsupportedLabel_IsNotProductivelySelectable()
    {
        SimulationProposalViewModel proposal =
            CreateProposal(
                MetadataField.Label);

        Assert.False(
            proposal.IsProductiveApplicationSupported);

        Assert.False(
            proposal.CanSelectForProductiveApplication);

        Assert.Equal(
            "No disponible",
            proposal.ProductiveApplicationStatus);
    }

    [Fact]
    public void UnsupportedVersion_IsNotProductivelySelectable()
    {
        SimulationProposalViewModel proposal =
            CreateProposal(
                MetadataField.Version);

        Assert.False(
            proposal.IsProductiveApplicationSupported);

        Assert.False(
            proposal.CanSelectForProductiveApplication);
    }

    [Fact]
    public void UnsupportedField_CannotRemainSelected()
    {
        SimulationProposalViewModel proposal =
            CreateProposal(
                MetadataField.Label);

        proposal.IsSelected =
            true;

        Assert.False(
            proposal.IsSelected);

        Assert.False(
            proposal.IsApprovedForSimulation);
    }

    private static SimulationProposalViewModel
        CreateProposal(
            MetadataField field)
    {
        return
            new SimulationProposalViewModel
            {
                Field =
                    field,

                FieldDisplay =
                    field.ToString(),

                CurrentValue =
                    string.Empty,

                ProposedValue =
                    "Nuevo valor",

                Decision =
                    MetadataChangeDecision
                        .ManualReviewRequired,

                DecisionDisplay =
                    "Revisión manual requerida",

                HasActualChange =
                    true,

                RequiresManualReview =
                    true,

                IsAutomaticApplyEligible =
                    false
            };
    }
}