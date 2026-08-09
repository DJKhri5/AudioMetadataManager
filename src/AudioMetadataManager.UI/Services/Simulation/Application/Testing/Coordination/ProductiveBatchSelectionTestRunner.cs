using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Mapping;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Services.Simulation.Application.Testing.Infrastructure;
using AudioMetadataManager.UI.Services.Simulation
    .Planning.Models;
using AudioMetadataManager.UI.Views.Models.Simulation;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Ejecuta comprobaciones controladas sobre la selección
/// productiva multiarchivo.
/// </summary>
public sealed class ProductiveBatchSelectionTestRunner
{
    public ProductiveBatchSelectionTestResult Run()
    {
        List<string> messages =
            new();

        ProductiveBatchSelection selection =
            new();

        bool emptySelectionWasCreated =
            selection.FileCount == 0 &&
            selection.ApprovedChangeCount == 0 &&
            !selection.HasItems;

        messages.Add(
            emptySelectionWasCreated
                ? "La selección productiva vacía fue creada correctamente."
                : "La selección productiva vacía presentó un estado inesperado.");

        SimulationPlanViewModel firstPlan =
            CreateApprovedPlan(
                Guid.NewGuid(),
                @"C:\AudioMetadataManager\Tests\batch-selection-1.mp3",
                "batch-selection-1.mp3",
                DiagnosticMetadataTestValues.CreateGenre());

        bool firstAddResult =
            selection.AddOrReplace(
                firstPlan);

        bool approvedPlanWasAdded =
            firstAddResult &&
            selection.FileCount == 1 &&
            selection.Contains(
                firstPlan.FilePath);

        messages.Add(
            approvedPlanWasAdded
                ? "El primer plan aprobado fue agregado correctamente."
                : "El primer plan aprobado no fue agregado correctamente.");

        SimulationPlanViewModel replacementPlan =
            CreateApprovedPlan(
                Guid.NewGuid(),
                firstPlan.FilePath,
                firstPlan.FileName,
                "Techno");

        bool replacementResult =
            selection.AddOrReplace(
                replacementPlan);

        bool duplicatePathWasReplaced =
            replacementResult &&
            selection.FileCount == 1 &&
            selection.Items[0].PlanId ==
                replacementPlan.PlanId &&
            selection.Items[0].ApprovedChangeCount == 1;

        messages.Add(
            duplicatePathWasReplaced
                ? "Una ruta existente fue actualizada sin duplicar el archivo."
                : "La actualización de una ruta existente produjo un estado incorrecto.");

        SimulationPlanViewModel secondPlan =
            CreateApprovedPlan(
                Guid.NewGuid(),
                @"C:\AudioMetadataManager\Tests\batch-selection-2.flac",
                "batch-selection-2.flac",
                "House");

        bool secondAddResult =
            selection.AddOrReplace(
                secondPlan);

        bool secondPlanWasAdded =
            secondAddResult &&
            selection.FileCount == 2 &&
            selection.Contains(
                secondPlan.FilePath);

        messages.Add(
            secondPlanWasAdded
                ? "El segundo plan aprobado fue agregado correctamente."
                : "El segundo plan aprobado no fue agregado correctamente.");

        bool countsWereUpdated =
            selection.FileCount == 2 &&
            selection.ApprovedChangeCount == 2;

        messages.Add(
            countsWereUpdated
                ? "Los conteos de archivos y cambios fueron actualizados."
                : "Los conteos de la selección no son correctos.");

        bool removeResult =
            selection.Remove(
                secondPlan.FilePath);

        bool itemWasRemoved =
            removeResult &&
            selection.FileCount == 1 &&
            !selection.Contains(
                secondPlan.FilePath);

        messages.Add(
            itemWasRemoved
                ? "El archivo solicitado fue eliminado de la selección."
                : "El archivo solicitado no fue eliminado correctamente.");

        SimulationPlanViewModel noApprovalPlan =
            CreatePlanWithoutApprovedChanges(
                firstPlan.FilePath,
                firstPlan.FileName);

        bool noApprovalAddResult =
            selection.AddOrReplace(
                noApprovalPlan);

        bool planWithoutApprovalRemovedExistingItem =
            !noApprovalAddResult &&
            selection.FileCount == 0 &&
            !selection.Contains(
                firstPlan.FilePath);

        messages.Add(
            planWithoutApprovalRemovedExistingItem
                ? "Un plan sin cambios aprobados eliminó la selección anterior."
                : "El plan sin cambios aprobados no actualizó correctamente la selección.");

        selection.AddOrReplace(
            firstPlan);

        selection.AddOrReplace(
            secondPlan);

        MetadataApplyBatchRequestFactory
            batchRequestFactory =
                new();

        MetadataApplyBatchRequest
            batchRequest =
                batchRequestFactory.Create(
                    selection);

        bool batchRequestWasCreated =
            batchRequest is not null;

        messages.Add(
            batchRequestWasCreated
                ? "La solicitud técnica por lote fue creada."
                : "No fue posible crear la solicitud técnica por lote.");

        bool batchRequestWasStructurallyValid =
            batchRequest is not null &&
            batchRequest.IsStructurallyValid;

        messages.Add(
            batchRequestWasStructurallyValid
                ? "La solicitud técnica por lote es estructuralmente válida."
                : "La solicitud técnica por lote no es estructuralmente válida.");

        bool batchCountsWerePreserved =
            batchRequest is not null &&
            batchRequest.ValidRequestCount == 2 &&
            batchRequest.ValidChangeCount == 2 &&
            !batchRequest.HasDuplicateFilePaths;

        messages.Add(
            batchCountsWerePreserved
                ? "La solicitud batch conservó archivos y cambios seleccionados."
                : "La solicitud batch no conservó correctamente sus conteos.");

        selection.Clear();

        bool selectionWasCleared =
            selection.FileCount == 0 &&
            selection.ApprovedChangeCount == 0 &&
            !selection.HasItems;

        messages.Add(
            selectionWasCleared
                ? "La selección productiva fue limpiada correctamente."
                : "La selección productiva no quedó vacía después de Clear().");

        return
            new ProductiveBatchSelectionTestResult
            {
                EmptySelectionWasCreated =
                    emptySelectionWasCreated,

                ApprovedPlanWasAdded =
                    approvedPlanWasAdded,

                DuplicatePathWasReplaced =
                    duplicatePathWasReplaced,

                SecondPlanWasAdded =
                    secondPlanWasAdded,

                CountsWereUpdated =
                    countsWereUpdated,

                ItemWasRemoved =
                    itemWasRemoved,

                PlanWithoutApprovalRemovedExistingItem =
                    planWithoutApprovalRemovedExistingItem,

                BatchRequestWasCreated =
                    batchRequestWasCreated,

                BatchRequestWasStructurallyValid =
                    batchRequestWasStructurallyValid,

                BatchCountsWerePreserved =
                    batchCountsWerePreserved,

                SelectionWasCleared =
                    selectionWasCleared,

                Messages =
                    messages
            };
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
                    DiagnosticMetadataTestValues.CreateGenre(),

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