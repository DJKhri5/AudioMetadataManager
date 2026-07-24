using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Planning.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AudioMetadataManager.UI.Views.Models.Simulation;

/// <summary>
/// Representa una propuesta individual dentro de la vista
/// de simulación.
/// </summary>
public sealed class SimulationProposalViewModel
    : INotifyPropertyChanged
{
    private bool _isSelected;

    private string _reviewState =
        "Pendiente";

    public event PropertyChangedEventHandler?
        PropertyChanged;

    /// <summary>
    /// Indica si una propuesta manual ya fue aprobada mediante la
    /// selección del usuario.
    /// </summary>
    public bool IsManuallyApproved =>
        HasActualChange &&
        RequiresManualReview &&
        IsSelected;

    /// <summary>
    /// Indica si la propuesta está lista para formar parte del
    /// futuro lote de aplicación.
    /// </summary>
    public bool IsApprovedForSimulation =>
        HasActualChange &&
        IsSelected &&
        CanSelect;

    /// <summary>
    /// Campo de metadatos evaluado.
    /// </summary>
    public MetadataField Field { get; init; } =
        MetadataField.Unknown;

    /// <summary>
    /// Nombre legible del campo.
    /// </summary>
    public string FieldDisplay { get; init; } =
        string.Empty;

    /// <summary>
    /// Valor almacenado actualmente.
    /// </summary>
    public string CurrentValue { get; init; } =
        string.Empty;

    /// <summary>
    /// Valor propuesto por el motor.
    /// </summary>
    public string ProposedValue { get; init; } =
        string.Empty;

    /// <summary>
    /// Decisión técnica original.
    /// </summary>
    public MetadataChangeDecision Decision { get; init; } =
        MetadataChangeDecision.Pending;

    /// <summary>
    /// Texto legible de la decisión.
    /// </summary>
    public string DecisionDisplay { get; init; } =
        string.Empty;

    /// <summary>
    /// Explicación técnica de la propuesta.
    /// </summary>
    public string Explanation { get; init; } =
        string.Empty;

    /// <summary>
    /// Confianza del consenso.
    /// </summary>
    public double Confidence { get; init; }

    /// <summary>
    /// Confianza preparada para la interfaz.
    /// </summary>
    public string ConfidenceDisplay =>
        $"{Math.Clamp(Confidence, 0, 1) * 100:0.00}%";

    /// <summary>
    /// Fuentes que respaldan la propuesta.
    /// </summary>
    public IReadOnlyList<string> SupportingSources
    { get; init; } =
            Array.Empty<string>();

    /// <summary>
    /// Fuentes preparadas para mostrarse.
    /// </summary>
    public string SourcesDisplay =>
        SupportingSources.Count == 0
            ? "(sin fuentes)"
            : string.Join(
                ", ",
                SupportingSources);

    /// <summary>
    /// Indica si existe una modificación real.
    /// </summary>
    public bool HasActualChange { get; init; }

    /// <summary>
    /// Indica si requiere revisión manual.
    /// </summary>
    public bool RequiresManualReview { get; init; }

    /// <summary>
    /// Indica si puede aplicarse automáticamente.
    /// </summary>
    public bool IsAutomaticApplyEligible { get; init; }

    /// <summary>
    /// Indica si la propuesta fue seleccionada por el usuario.
    ///
    /// Al cambiar la selección también se actualiza el estado
    /// visible de revisión.
    /// </summary>
    public bool IsSelected
    {
        get =>
            _isSelected;

        set
        {
            if (_isSelected == value)
            {
                return;
            }

            _isSelected =
                value;

            UpdateReviewStateFromSelection();

            OnPropertyChanged();

            OnPropertyChanged(
                nameof(IsManuallyApproved));

            OnPropertyChanged(
                nameof(IsApprovedForSimulation));
        }
    }

    /// <summary>
    /// Estado de revisión elegido por el usuario.
    /// </summary>
    public string ReviewState
    {
        get =>
            _reviewState;

        set
        {
            string normalizedValue =
                string.IsNullOrWhiteSpace(value)
                    ? "Pendiente"
                    : value.Trim();

            if (string.Equals(
                    _reviewState,
                    normalizedValue,
                    StringComparison.Ordinal))
            {
                return;
            }

            _reviewState =
                normalizedValue;

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Indica si la fila debe mostrarse como modificable.
    /// </summary>
    public bool CanSelect =>
        HasActualChange &&
        Decision is not
            MetadataChangeDecision.Conflict and not
            MetadataChangeDecision.InsufficientEvidence and not
            MetadataChangeDecision.Rejected;

    /// <summary>
    /// Sincroniza el estado de revisión con la selección actual.
    /// </summary>
    private void UpdateReviewStateFromSelection()
    {
        if (!HasActualChange)
        {
            ReviewState =
                "Sin cambios";

            return;
        }

        if (!CanSelect)
        {
            return;
        }

        if (_isSelected)
        {
            ReviewState =
                RequiresManualReview
                    ? "Aprobado por el usuario"
                    : "Preseleccionado automáticamente";

            return;
        }

        ReviewState =
            RequiresManualReview
                ? "Pendiente de revisión"
                : "No seleccionado";
    }

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