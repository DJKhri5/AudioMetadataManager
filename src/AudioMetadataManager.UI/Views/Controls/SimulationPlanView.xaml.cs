using AudioMetadataManager.UI.Views.Models.Simulation;
using System.Windows;
using System.Windows.Controls;

namespace AudioMetadataManager.UI.Views.Controls;

/// <summary>
/// Vista visual del plan de simulación.
/// </summary>
public partial class SimulationPlanView : UserControl
{
    public SimulationPlanView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Selecciona únicamente las propuestas consideradas
    /// elegibles para aplicación automática.
    /// </summary>
    private void SelectAutomaticButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (DataContext is not
            SimulationPlanViewModel viewModel)
        {
            return;
        }

        foreach (
            SimulationProposalViewModel proposal
            in viewModel.Proposals)
        {
            proposal.IsSelected =
                proposal.CanSelect &&
                proposal.IsAutomaticApplyEligible;
        }

        viewModel.RefreshSummary();
    }

    /// <summary>
    /// Elimina toda la selección del plan.
    /// </summary>
    private void ClearSelectionButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (DataContext is not
            SimulationPlanViewModel viewModel)
        {
            return;
        }

        foreach (
            SimulationProposalViewModel proposal
            in viewModel.Proposals)
        {
            proposal.IsSelected =
                false;
        }

        viewModel.RefreshSummary();
    }
}