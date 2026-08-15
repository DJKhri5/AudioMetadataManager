using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Mapping;
using AudioMetadataManager.UI.Services.Simulation
    .Planning.Decision;
using AudioMetadataManager.UI.Services.Simulation
    .Planning.Models;
using AudioMetadataManager.UI.Views.Models.Simulation;
using Xunit;

namespace AudioMetadataManager.Tests
    .Simulation.Application.Mapping;

public sealed class MetadataApplyRequestFactoryProductiveSupportTests
{
    [Theory]
    [InlineData(MetadataField.Genre, "House", "Electronic")]
    [InlineData(MetadataField.Version, "Original Mix", "Extended Mix")]
    [InlineData(MetadataField.Label, "", "Afterlife")]
    public void SupportedField_IsIncluded(
        MetadataField field,
        string currentValue,
        string proposedValue)
    {
        SimulationPlanViewModel plan =
            CreatePlan(
                CreateApprovedProposal(
                    field,
                    currentValue,
                    proposedValue));

        MetadataApplyRequestFactory factory =
            new();

        var request =
            factory.Create(
                plan);

        Assert.True(
            request.IsStructurallyValid);

        Assert.Single(
            request.Changes);

        Assert.Equal(
            field,
            request.Changes[0].Field);
    }

    [Fact]
    public void MultipleSupportedChanges_ArePreserved()
    {
        SimulationPlanViewModel plan =
            CreatePlan(
                CreateApprovedProposal(
                    MetadataField.Genre,
                    "House",
                    "Electronic"),
                CreateApprovedProposal(
                    MetadataField.Label,
                    string.Empty,
                    "Afterlife"),
                CreateApprovedProposal(
                    MetadataField.Version,
                    "Original Mix",
                    "Extended Mix"));

        MetadataApplyRequestFactory factory =
            new();

        var request =
            factory.Create(
                plan);

        Assert.True(
            request.IsStructurallyValid);

        Assert.Equal(
            3,
            request.Changes.Count);

        Assert.Contains(
            request.Changes,
            change =>
                change.Field == MetadataField.Genre);

        Assert.Contains(
            request.Changes,
            change =>
                change.Field == MetadataField.Label);

        Assert.Contains(
            request.Changes,
            change =>
                change.Field == MetadataField.Version);
    }

    private static SimulationPlanViewModel CreatePlan(
        params SimulationProposalViewModel[] proposals)
    {
        SimulationPlanViewModel plan =
            new()
            {
                PlanId =
                    Guid.NewGuid(),

                FilePath =
                    @"C:\Tests\productive-support.mp3",

                FileName =
                    "productive-support.mp3",

                Status =
                    MetadataChangePlanStatus
                        .ManualReviewRequired
            };

        foreach (SimulationProposalViewModel proposal
            in proposals)
        {
            plan.Proposals.Add(
                proposal);
        }

        plan.RefreshSummary();

        return plan;
    }

    private static SimulationProposalViewModel
        CreateApprovedProposal(
            MetadataField field,
            string currentValue,
            string proposedValue)
    {
        return
            new SimulationProposalViewModel
            {
                Field =
                    field,

                CurrentValue =
                    currentValue,

                ProposedValue =
                    proposedValue,

                Decision =
                    MetadataChangeDecision
                        .ManualReviewRequired,

                HasActualChange =
                    true,

                RequiresManualReview =
                    true,

                IsAutomaticApplyEligible =
                    false,

                IsSelected =
                    true,

                Confidence =
                    0.85,

                SupportingSources =
                    new[]
                    {
                        "Discogs"
                    }
            };
    }
}