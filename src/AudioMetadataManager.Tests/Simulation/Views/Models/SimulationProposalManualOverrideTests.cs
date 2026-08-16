using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation.Application.Mapping;
using AudioMetadataManager.UI.Services.Simulation.Planning.Models;
using AudioMetadataManager.UI.Views.Models.Simulation;
using Xunit;

namespace AudioMetadataManager.Tests
    .Simulation.Views.Models;

public sealed class SimulationProposalManualOverrideTests
{
    [Fact]
    public void InsufficientEvidence_ManualValueBecomesSelectable()
    {
        SimulationProposalViewModel proposal =
            CreateInsufficientEvidenceProposal(
                MetadataField.Version,
                string.Empty);

        Assert.False(
            proposal.CanSelect);

        bool applied =
            proposal.TryApplyManualValue(
                "Extended Mix",
                out string validationError);

        Assert.True(
            applied,
            validationError);

        Assert.True(
            proposal.HasManualOverride);

        Assert.Equal(
            "Extended Mix",
            proposal.EffectiveProposedValue);

        Assert.True(
            proposal.HasSelectableChange);

        Assert.True(
            proposal.CanSelectForProductiveApplication);

        proposal.IsSelected =
            true;

        Assert.True(
            proposal.IsManuallyApproved);

        Assert.Equal(
            "Aprobado por el usuario",
            proposal.ReviewState);
    }

    [Fact]
    public void ManualValue_IsMappedAsAuditableUserChange()
    {
        SimulationProposalViewModel proposal =
            CreateInsufficientEvidenceProposal(
                MetadataField.Label,
                string.Empty);

        Assert.True(
            proposal.TryApplyManualValue(
                "Afterlife",
                out _));

        proposal.IsSelected =
            true;

        SimulationPlanViewModel plan =
            new()
            {
                PlanId =
                    Guid.NewGuid(),

                FilePath =
                    @"C:\Tests\manual-label.mp3",

                FileName =
                    "manual-label.mp3",

                Status =
                    MetadataChangePlanStatus
                        .InsufficientEvidence
            };

        plan.Proposals.Add(
            proposal);

        var request =
            new MetadataApplyRequestFactory()
                .Create(
                    plan);

        Assert.Single(
            request.Changes);

        Assert.Equal(
            "Afterlife",
            request.Changes[0].NewValue);

        Assert.True(
            request.Changes[0].WasManuallyApproved);

        Assert.Equal(
            new[] { "Usuario" },
            request.Changes[0].SupportingSources);
    }

    [Fact]
    public void ManualValueEqualToCurrentValue_IsNotSelectable()
    {
        SimulationProposalViewModel proposal =
            CreateInsufficientEvidenceProposal(
                MetadataField.Genre,
                "Trance");

        Assert.True(
            proposal.TryApplyManualValue(
                " trance ",
                out _));

        Assert.False(
            proposal.HasSelectableChange);

        proposal.IsSelected =
            true;

        Assert.False(
            proposal.IsSelected);
    }

    [Fact]
    public void EmptyManualValue_IsRejected()
    {
        SimulationProposalViewModel proposal =
            CreateInsufficientEvidenceProposal(
                MetadataField.Version,
                string.Empty);

        bool applied =
            proposal.TryApplyManualValue(
                "   ",
                out string validationError);

        Assert.False(
            applied);

        Assert.False(
            proposal.HasManualOverride);

        Assert.NotEmpty(
            validationError);
    }

    private static SimulationProposalViewModel
        CreateInsufficientEvidenceProposal(
            MetadataField field,
            string currentValue)
    {
        return
            new SimulationProposalViewModel
            {
                Field =
                    field,

                FieldDisplay =
                    field.ToString(),

                CurrentValue =
                    currentValue,

                ProposedValue =
                    "(sin información)",

                Decision =
                    MetadataChangeDecision
                        .InsufficientEvidence,

                DecisionDisplay =
                    "Evidencia insuficiente",

                HasActualChange =
                    false,

                RequiresManualReview =
                    false,

                IsAutomaticApplyEligible =
                    false,

                Confidence =
                    0
            };
    }
}
