using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation.Planning.Models;
using AudioMetadataManager.UI.Views.Models.Simulation;
using Xunit;

namespace AudioMetadataManager.Tests
    .Simulation.Views.Models;

public sealed class SimulationProposalProductiveSupportTests
{
    [Theory]
    [InlineData(MetadataField.Genre)]
    [InlineData(MetadataField.Version)]
    [InlineData(MetadataField.Label)]
    public void SupportedField_IsProductivelySelectable(
        MetadataField field)
    {
        SimulationProposalViewModel proposal =
            CreateProposal(
                field);

        Assert.True(
            proposal.IsProductiveApplicationSupported);

        Assert.True(
            proposal.CanSelectForProductiveApplication);

        Assert.Equal(
            "Disponible",
            proposal.ProductiveApplicationStatus);

        proposal.IsSelected =
            true;

        Assert.True(
            proposal.IsSelected);

        Assert.True(
            proposal.IsApprovedForSimulation);
    }

    [Fact]
    public void UnknownField_IsNotProductivelySelectable()
    {
        SimulationProposalViewModel proposal =
            CreateProposal(
                MetadataField.Unknown);

        Assert.False(
            proposal.IsProductiveApplicationSupported);

        Assert.False(
            proposal.CanSelectForProductiveApplication);

        Assert.Equal(
            "No disponible",
            proposal.ProductiveApplicationStatus);

        proposal.IsSelected =
            true;

        Assert.False(
            proposal.IsSelected);
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