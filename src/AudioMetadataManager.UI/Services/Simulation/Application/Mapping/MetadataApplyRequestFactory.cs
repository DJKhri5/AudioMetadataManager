using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Views.Models.Simulation;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Mapping;

/// <summary>
/// Convierte un plan visual revisado por el usuario en una
/// solicitud técnica de aplicación.
///
/// Sólo incluye propuestas aprobadas, seleccionables y que
/// representan cambios reales.
/// </summary>
public sealed class MetadataApplyRequestFactory
{
    /// <summary>
    /// Construye una solicitud a partir del plan visual.
    /// </summary>
    public MetadataApplyRequest Create(
        SimulationPlanViewModel simulationPlan)
    {
        ArgumentNullException.ThrowIfNull(
            simulationPlan);

        IReadOnlyList<MetadataFieldChange> changes =
            simulationPlan.Proposals
                .Where(
                    IsApprovedProposal)
                .Select(
                    CreateFieldChange)
                .Where(
                    change =>
                        change.IsValidChange)
                .ToArray();

        string? approvedArtworkUrl =
            simulationPlan.HasArtworkCandidate &&
            simulationPlan.IsArtworkApproved
                ? simulationPlan.ArtworkUrl
                : null;

        return new MetadataApplyRequest
        {
            PlanId =
                simulationPlan.PlanId,

            FilePath =
                simulationPlan.FilePath,

            FileName =
                simulationPlan.FileName,

            Changes =
                changes,

            ArtworkUrl =
                approvedArtworkUrl,

            RequireBackup =
                true,

            RequirePostWriteVerification =
                true
        };
    }

    private static bool IsApprovedProposal(
        SimulationProposalViewModel proposal)
    {
        return
            proposal is not null &&
            proposal.IsApprovedForSimulation &&
            proposal.HasActualChange &&
            proposal.CanSelect;
    }

    private static MetadataFieldChange CreateFieldChange(
        SimulationProposalViewModel proposal)
    {
        return new MetadataFieldChange
        {
            Field =
                proposal.Field,

            OriginalValue =
                NormalizeStoredValue(
                    proposal.CurrentValue),

            NewValue =
                NormalizeStoredValue(
                    proposal.ProposedValue),

            WasManuallyApproved =
                proposal.IsManuallyApproved,

            Confidence =
                proposal.Confidence,

            SupportingSources =
                proposal.SupportingSources
        };
    }

    /// <summary>
    /// Convierte los textos de presentación en valores
    /// almacenables.
    ///
    /// La interfaz usa "(sin información)" para mostrar valores
    /// vacíos; ese texto nunca debe convertirse en una etiqueta.
    /// </summary>
    private static string NormalizeStoredValue(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(
                value))
        {
            return string.Empty;
        }

        string normalized =
            value.Trim();

        return string.Equals(
                normalized,
                "(sin información)",
                StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : normalized;
    }
}