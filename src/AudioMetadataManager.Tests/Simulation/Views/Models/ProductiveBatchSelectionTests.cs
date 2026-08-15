using Xunit;
using AudioMetadataManager.UI.Services
    .MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Planning.Decision;
using AudioMetadataManager.UI.Services.Simulation
    .Planning.Models;
using AudioMetadataManager.UI.Views.Models.Simulation;

namespace AudioMetadataManager.Tests
    .Simulation.Views.Models;

public sealed class ProductiveBatchSelectionTests
{
    [Fact]
    public void NewSelection_IsEmpty()
    {
        ProductiveBatchSelection selection =
            new();

        Assert.False(
            selection.HasItems);

        Assert.Equal(
            0,
            selection.FileCount);

        Assert.Equal(
            0,
            selection.ApprovedChangeCount);
    }

    [Fact]
    public void ApprovedPlan_IsAdded()
    {
        ProductiveBatchSelection selection =
            new();

        SimulationPlanViewModel plan =
            CreateApprovedPlan(
                Guid.NewGuid(),
                @"C:\Tests\selection-1.flac",
                "selection-1.flac",
                "House");

        bool result =
            selection.AddOrReplace(
                plan);

        Assert.True(
            result);

        Assert.True(
            selection.HasItems);

        Assert.Equal(
            1,
            selection.FileCount);

        Assert.Equal(
            1,
            selection.ApprovedChangeCount);

        Assert.True(
            selection.Contains(
                plan.FilePath));
    }

    [Fact]
    public void SamePath_ReplacesExistingPlan()
    {
        ProductiveBatchSelection selection =
            new();

        string filePath =
            @"C:\Tests\replace.flac";

        SimulationPlanViewModel firstPlan =
            CreateApprovedPlan(
                Guid.NewGuid(),
                filePath,
                "replace.flac",
                "House");

        SimulationPlanViewModel replacementPlan =
            CreateApprovedPlan(
                Guid.NewGuid(),
                filePath,
                "replace.flac",
                "Techno");

        selection.AddOrReplace(
            firstPlan);

        bool replacementResult =
            selection.AddOrReplace(
                replacementPlan);

        Assert.True(
            replacementResult);

        Assert.Equal(
            1,
            selection.FileCount);

        Assert.Equal(
            replacementPlan.PlanId,
            selection.Items[0].PlanId);

        Assert.Equal(
            1,
            selection.Items[0].ApprovedChangeCount);
    }

    [Fact]
    public void PlanWithoutApprovedChanges_RemovesExistingSelection()
    {
        ProductiveBatchSelection selection =
            new();

        string filePath =
            @"C:\Tests\remove-approval.flac";

        SimulationPlanViewModel approvedPlan =
            CreateApprovedPlan(
                Guid.NewGuid(),
                filePath,
                "remove-approval.flac",
                "House");

        selection.AddOrReplace(
            approvedPlan);

        SimulationPlanViewModel
            planWithoutApproval =
                CreatePlanWithoutApprovedChanges(
                    filePath,
                    "remove-approval.flac");

        bool result =
            selection.AddOrReplace(
                planWithoutApproval);

        Assert.False(
            result);

        Assert.False(
            selection.HasItems);

        Assert.Equal(
            0,
            selection.FileCount);

        Assert.False(
            selection.Contains(
                filePath));
    }

    [Fact]
    public void Clear_RemovesAllSelectedPlans()
    {
        ProductiveBatchSelection selection =
            new();

        selection.AddOrReplace(
            CreateApprovedPlan(
                Guid.NewGuid(),
                @"C:\Tests\clear-1.flac",
                "clear-1.flac",
                "House"));

        selection.AddOrReplace(
            CreateApprovedPlan(
                Guid.NewGuid(),
                @"C:\Tests\clear-2.flac",
                "clear-2.flac",
                "Techno"));

        Assert.Equal(
            2,
            selection.FileCount);

        Assert.Equal(
            2,
            selection.ApprovedChangeCount);

        selection.Clear();

        Assert.False(
            selection.HasItems);

        Assert.Equal(
            0,
            selection.FileCount);

        Assert.Equal(
            0,
            selection.ApprovedChangeCount);
    }

    [Fact]
    public void EmptySelection_IsNotReadyForExecution()
    {
        ProductiveBatchSelection selection =
            new();

        Assert.False(
            selection.IsReadyForExecution);
    }

    [Fact]
    public void ValidSelectedPlan_IsReadyForExecution()
    {
        SimulationPlanViewModel plan =
            CreateApprovedPlan(
                Guid.NewGuid(),
                @"C:\Tests\ready-for-execution.flac",
                "ready-for-execution.flac",
                "House");

        ProductiveBatchSelection selection =
            new();

        bool wasAdded =
            selection.AddOrReplace(
                plan);

        Assert.True(
            wasAdded);

        Assert.True(
            selection.IsReadyForExecution);
    }

    [Fact]
    public void EmptySelection_IsExecutionUnavailable()
    {
        ProductiveBatchSelection selection =
            new();

        Assert.True(
            selection.IsEmpty);

        Assert.True(
            selection.IsExecutionUnavailable);
    }

    [Fact]
    public void ValidSelectedPlan_IsExecutionAvailable()
    {
        SimulationPlanViewModel plan =
            CreateApprovedPlan(
                Guid.NewGuid(),
                @"C:\Tests\execution-available.flac",
                "execution-available.flac",
                "House");

        ProductiveBatchSelection selection =
            new();

        Assert.True(
            selection.AddOrReplace(
                plan));

        Assert.False(
            selection.IsEmpty);

        Assert.False(
            selection.IsExecutionUnavailable);

        Assert.True(
            selection.IsReadyForExecution);
    }

    private static SimulationPlanViewModel
        CreateApprovedPlan(
            Guid planId,
            string filePath,
            string fileName,
            string genre)
    {
        SimulationPlanViewModel plan =
            new()
            {
                PlanId =
                    planId,

                FilePath =
                    filePath,

                FileName =
                    fileName,

                Status =
                    MetadataChangePlanStatus
                        .ManualReviewRequired
            };

        SimulationProposalViewModel proposal =
            new()
            {
                Field =
                    MetadataField.Genre,

                CurrentValue =
                    string.Empty,

                ProposedValue =
                    genre,

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
                    true
            };

        plan.Proposals.Add(
            proposal);

        plan.RefreshSummary();

        return plan;
    }

    private static SimulationPlanViewModel
        CreatePlanWithoutApprovedChanges(
            string filePath,
            string fileName)
    {
        SimulationPlanViewModel plan =
            new()
            {
                PlanId =
                    Guid.NewGuid(),

                FilePath =
                    filePath,

                FileName =
                    fileName,

                Status =
                    MetadataChangePlanStatus
                        .ManualReviewRequired
            };

        SimulationProposalViewModel proposal =
            new()
            {
                Field =
                    MetadataField.Genre,

                CurrentValue =
                    string.Empty,

                ProposedValue =
                    "House",

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
                    false
            };

        plan.Proposals.Add(
            proposal);

        plan.RefreshSummary();

        return plan;
    }
}