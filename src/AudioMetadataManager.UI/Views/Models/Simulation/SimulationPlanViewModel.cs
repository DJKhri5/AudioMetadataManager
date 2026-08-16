using AudioMetadataManager.UI.Services.Simulation
    .Planning.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AudioMetadataManager.UI.Views.Models.Simulation;

/// <summary>
/// Representa un plan completo dentro de la interfaz de
/// simulación.
/// </summary>
public sealed class SimulationPlanViewModel
    : INotifyPropertyChanged
{
    private string _fileName =
        string.Empty;

    private string _filePath =
        string.Empty;

    private MetadataChangePlanStatus _status =
        MetadataChangePlanStatus.Pending;

    public event PropertyChangedEventHandler?
        PropertyChanged;

    /// <summary>
    /// Identificador del plan técnico original.
    /// </summary>
    public Guid PlanId { get; init; }

    /// <summary>
    /// Nombre del archivo.
    /// </summary>
    public string FileName
    {
        get =>
            _fileName;

        set
        {
            if (string.Equals(
                    _fileName,
                    value,
                    StringComparison.Ordinal))
            {
                return;
            }

            _fileName =
                value ?? string.Empty;

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Ruta completa del archivo.
    /// </summary>
    public string FilePath
    {
        get =>
            _filePath;

        set
        {
            if (string.Equals(
                    _filePath,
                    value,
                    StringComparison.Ordinal))
            {
                return;
            }

            _filePath =
                value ?? string.Empty;

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Estado global del plan.
    /// </summary>
    public MetadataChangePlanStatus Status
    {
        get =>
            _status;

        set
        {
            if (_status == value)
            {
                return;
            }

            _status =
                value;

            OnPropertyChanged();
            OnPropertyChanged(
                nameof(StatusDisplay));
        }
    }

    /// <summary>
    /// Estado preparado para mostrarse.
    /// </summary>
    public string StatusDisplay =>
        Status switch
        {
            MetadataChangePlanStatus.NoChangesRequired =>
                "No se requieren cambios",

            MetadataChangePlanStatus.ManualReviewRequired =>
                "Revisión manual requerida",

            MetadataChangePlanStatus.ReadyForSimulation =>
                "Preparado para simulación",

            MetadataChangePlanStatus.PartiallyReady =>
                "Parcialmente preparado",

            MetadataChangePlanStatus.BlockedByConflicts =>
                "Bloqueado por conflictos",

            MetadataChangePlanStatus.InsufficientEvidence =>
                "Evidencia insuficiente",

            _ =>
                "Pendiente"
        };

    /// <summary>
    /// Propuestas visibles.
    /// </summary>
    public ObservableCollection<
        SimulationProposalViewModel>
        Proposals
    { get; } =
            new();

    /// <summary>
    /// Cantidad total de propuestas.
    /// </summary>
    public int ProposalCount =>
        Proposals.Count;

    /// <summary>
    /// Cambios reales.
    /// </summary>
    public int ActualChangeCount =>
        Proposals.Count(
            proposal =>
                proposal.HasSelectableChange);

    /// <summary>
    /// Propuestas seleccionadas.
    /// </summary>
    public int SelectedChangeCount =>
        Proposals.Count(
            proposal =>
                proposal.IsSelected);

    /// <summary>
    /// Cambios con revisión manual.
    /// </summary>
    public int ManualReviewCount =>
        Proposals.Count(
            proposal =>
                proposal.HasSelectableChange &&
                (
                    proposal.RequiresManualReview ||
                    proposal.HasManualOverride
                ));

    /// <summary>
    /// Conflictos.
    /// </summary>
    public int ConflictCount =>
        Proposals.Count(
            proposal =>
                !proposal.HasManualOverride &&
                proposal.Decision ==
                MetadataChangeDecision.Conflict);

    /// <summary>
    /// Resumen preparado para la interfaz.
    /// </summary>
    public string Summary =>
        $"Cambios reales: {ActualChangeCount}. " +
        $"Aprobados: {ApprovedChangeCount}. " +
        $"Aprobados manualmente: {ManuallyApprovedCount}. " +
        $"Pendientes de revisión: " +
        $"{Math.Max(
            0,
            ManualReviewCount -
            ManuallyApprovedCount)}. " +
        $"Conflictos: {ConflictCount}.";

    /// <summary>
    /// Actualiza las propiedades calculadas después de una
    /// selección.
    /// </summary>
    public void RefreshSummary()
    {
        OnPropertyChanged(
            nameof(ProposalCount));

        OnPropertyChanged(
            nameof(ActualChangeCount));

        OnPropertyChanged(
            nameof(SelectedChangeCount));

        OnPropertyChanged(
            nameof(ManualReviewCount));

        OnPropertyChanged(
            nameof(ConflictCount));

        OnPropertyChanged(
            nameof(ApprovedProposals));

        OnPropertyChanged(
            nameof(ManuallyApprovedCount));

        OnPropertyChanged(
            nameof(ApprovedChangeCount));

        OnPropertyChanged(
            nameof(HasApprovedChanges));

        OnPropertyChanged(
            nameof(Summary));
    }

    /// <summary>
    /// Propuestas aprobadas para continuar hacia la simulación.
    /// </summary>
    public IReadOnlyList<SimulationProposalViewModel>
        ApprovedProposals =>
            Proposals
                .Where(
                    proposal =>
                        proposal.IsApprovedForSimulation)
                .ToArray();

    /// <summary>
    /// Cantidad de propuestas aprobadas manualmente.
    /// </summary>
    public int ManuallyApprovedCount =>
        Proposals.Count(
            proposal =>
                proposal.IsManuallyApproved);

    /// <summary>
    /// Cantidad total de propuestas aprobadas para simulación.
    /// </summary>
    public int ApprovedChangeCount =>
        ApprovedProposals.Count;

    /// <summary>
    /// Indica si existe al menos una propuesta aprobada.
    /// </summary>
    public bool HasApprovedChanges =>
        ApprovedChangeCount > 0;

    private void OnPropertyChanged(
        [CallerMemberName]
        string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(
                propertyName));
    }
}
