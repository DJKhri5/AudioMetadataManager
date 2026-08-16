using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Planning.Models;

namespace AudioMetadataManager.UI.Views.Models.Simulation
    .Mapping;

/// <summary>
/// Convierte un plan técnico de modificaciones en modelos
/// preparados para la interfaz de simulación.
/// </summary>
public sealed class SimulationPlanViewModelFactory
{
    /// <summary>
    /// Construye el modelo visual completo de un plan.
    /// </summary>
    public SimulationPlanViewModel Create(
        MetadataChangePlan plan)
    {
        ArgumentNullException.ThrowIfNull(
            plan);

        SimulationPlanViewModel viewModel =
            new()
            {
                PlanId =
                    plan.PlanId,

                FileName =
                    plan.FileName,

                FilePath =
                    plan.FilePath,

                Status =
                    plan.Status
            };

        foreach (
            MetadataChangeProposal proposal
            in plan.Proposals)
        {
            SimulationProposalViewModel proposalViewModel =
                CreateProposal(
                    proposal);

            proposalViewModel.PropertyChanged +=
                (_, eventArgs) =>
                {
                    if (eventArgs.PropertyName is
                        nameof(
                            SimulationProposalViewModel
                                .IsSelected) or
                        nameof(
                            SimulationProposalViewModel
                                .HasSelectableChange) or
                        nameof(
                            SimulationProposalViewModel
                                .HasManualOverride))
                    {
                        viewModel.RefreshSummary();
                    }
                };

            viewModel.Proposals.Add(
                proposalViewModel);
        }

        viewModel.RefreshSummary();

        return viewModel;
    }

    private static SimulationProposalViewModel CreateProposal(
        MetadataChangeProposal proposal)
    {
        bool shouldSelectByDefault =
            proposal.HasActualChange &&
            proposal.Decision ==
                MetadataChangeDecision
                    .EligibleForAutomaticApply;

        return new SimulationProposalViewModel
        {
            Field =
                proposal.Field,

            FieldDisplay =
                GetFieldDisplay(
                    proposal.Field),

            CurrentValue =
                NormalizeDisplayValue(
                    proposal.CurrentValue),

            ProposedValue =
                NormalizeDisplayValue(
                    proposal.ProposedValue),

            Decision =
                proposal.Decision,

            DecisionDisplay =
                GetDecisionDisplay(
                    proposal.Decision),

            Explanation =
                proposal.Explanation,

            Confidence =
                proposal.ConsensusConfidence,

            SupportingSources =
                proposal.SupportingSources,

            HasActualChange =
                proposal.HasActualChange,

            RequiresManualReview =
                proposal.RequiresManualReview,

            IsAutomaticApplyEligible =
                proposal.IsAutomaticApplyEligible,

            IsSelected =
                shouldSelectByDefault,

            ReviewState =
                GetInitialReviewState(
                    proposal)
        };
    }

    private static string GetInitialReviewState(
        MetadataChangeProposal proposal)
    {
        if (!proposal.HasActualChange)
        {
            return "Sin cambios";
        }

        return proposal.Decision switch
        {
            MetadataChangeDecision
                .EligibleForAutomaticApply =>
                    "Preseleccionado",

            MetadataChangeDecision
                .ManualReviewRequired =>
                    "Pendiente de revisión",

            MetadataChangeDecision
                .Conflict =>
                    "Conflicto",

            MetadataChangeDecision
                .InsufficientEvidence =>
                    "Evidencia insuficiente",

            MetadataChangeDecision
                .Rejected =>
                    "Rechazado",

            _ =>
                "Pendiente"
        };
    }

    private static string GetFieldDisplay(
        MetadataField field)
    {
        return field switch
        {
            MetadataField.Artist =>
                "Artista",

            MetadataField.Title =>
                "Título",

            MetadataField.Version =>
                "Versión",

            MetadataField.Album =>
                "Álbum / lanzamiento",

            MetadataField.Genre =>
                "Género",

            MetadataField.Label =>
                "Sello",

            _ =>
                field.ToString()
        };
    }

    private static string GetDecisionDisplay(
        MetadataChangeDecision decision)
    {
        return decision switch
        {
            MetadataChangeDecision.NoChangeRequired =>
                "No requiere cambios",

            MetadataChangeDecision
                .EligibleForAutomaticApply =>
                    "Aplicación automática posible",

            MetadataChangeDecision
                .ManualReviewRequired =>
                    "Revisión manual requerida",

            MetadataChangeDecision
                .InsufficientEvidence =>
                    "Evidencia insuficiente",

            MetadataChangeDecision.Conflict =>
                "Conflicto sin resolver",

            MetadataChangeDecision.Rejected =>
                "Propuesta rechazada",

            _ =>
                "Pendiente"
        };
    }

    private static string NormalizeDisplayValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(
                value)
            ? "(sin información)"
            : value.Trim();
    }
}
