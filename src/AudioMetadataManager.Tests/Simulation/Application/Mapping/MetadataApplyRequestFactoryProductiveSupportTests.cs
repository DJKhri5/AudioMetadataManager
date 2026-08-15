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
    [Fact]
    public void SupportedGenre_IsIncluded()
    {
        SimulationPlanViewModel plan =
            CreatePlan(
                CreateApprovedProposal(
                    MetadataField.Genre,
                    "House",
                    "Electronic"));

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
            MetadataField.Genre,
            request.Changes[0].Field);
    }

    [Fact]
    public void UnsupportedLabel_IsExcluded()
    {
        SimulationPlanViewModel plan =
            CreatePlan(
                CreateApprovedProposal(
                    MetadataField.Label,
                    string.Empty,
                    "Afterlife"));

        MetadataApplyRequestFactory factory =
            new();

        var request =
            factory.Create(
                plan);

        Assert.Empty(
            request.Changes);

        Assert.False(
            request.IsStructurallyValid);
    }

    [Fact]
    public void UnsupportedVersion_IsExcluded()
    {
        SimulationPlanViewModel plan =
            CreatePlan(
                CreateApprovedProposal(
                    MetadataField.Version,
                    string.Empty,
                    "Extended Mix"));

        MetadataApplyRequestFactory factory =
            new();

        var request =
            factory.Create(
                plan);

        Assert.Empty(
            request.Changes);

        Assert.False(
            request.IsStructurallyValid);
    }

    [Fact]
    public void MixedSupportedAndUnsupportedChanges_KeepOnlySupportedChanges()
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
                    "Afterlife"));

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
            MetadataField.Genre,
            request.Changes[0].Field);
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