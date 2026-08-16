using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.Simulation
    .Application.Validation;
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

    private string? _manualProposedValue;

    private string _reviewState =
        "Pendiente";

    public event PropertyChangedEventHandler?
        PropertyChanged;

    /// <summary>
    /// Indica si una propuesta manual ya fue aprobada mediante la
    /// selección del usuario.
    /// </summary>
    public bool IsManuallyApproved =>
        HasSelectableChange &&
        (RequiresManualReview || HasManualOverride) &&
        IsSelected;

    /// <summary>
    /// Indica si la propuesta está lista para formar parte del
    /// futuro lote de aplicación.
    /// </summary>
    public bool IsApprovedForSimulation =>
        HasSelectableChange &&
        IsSelected &&
        CanSelectForProductiveApplication;

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
    /// Indica si el usuario sustituyó explícitamente la propuesta
    /// técnica por un valor manual.
    /// </summary>
    public bool HasManualOverride =>
        _manualProposedValue is not null;

    /// <summary>
    /// Valor que se mostrará y, tras aprobación, se enviará al
    /// pipeline productivo.
    /// </summary>
    public string EffectiveProposedValue =>
        HasManualOverride
            ? _manualProposedValue!
            : ProposedValue;

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
    /// Decisión visible después de considerar una intervención
    /// manual explícita del usuario.
    /// </summary>
    public string EffectiveDecisionDisplay =>
        HasManualOverride
            ? "Valor manual del usuario"
            : DecisionDisplay;

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
        HasManualOverride
            ? "Manual"
            : $"{Math.Clamp(Confidence, 0, 1) * 100:0.00}%";

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
        HasManualOverride
            ? "Usuario"
            : SupportingSources.Count == 0
            ? "(sin fuentes)"
            : string.Join(
                ", ",
                SupportingSources);

    /// <summary>
    /// Fuentes auditables que acompañarán el cambio efectivo.
    /// </summary>
    public IReadOnlyList<string> EffectiveSupportingSources =>
        HasManualOverride
            ? new[] { "Usuario" }
            : SupportingSources;

    /// <summary>
    /// Indica si existe una modificación real.
    /// </summary>
    public bool HasActualChange { get; init; }

    /// <summary>
    /// Indica si el valor efectivo representa una modificación
    /// real. Para propuestas técnicas conserva el cálculo original;
    /// para valores manuales vuelve a comparar ambos textos.
    /// </summary>
    public bool HasSelectableChange =>
        HasManualOverride
            ? HasValueChange(
                CurrentValue,
                EffectiveProposedValue)
            : HasActualChange;

    /// <summary>
    /// Indica si requiere revisión manual.
    /// </summary>
    public bool RequiresManualReview { get; init; }

    /// <summary>
    /// Indica si puede aplicarse automáticamente.
    /// </summary>
    public bool IsAutomaticApplyEligible { get; init; }

    /// <summary>
    /// Indica si el pipeline productivo puede escribir actualmente
    /// este campo de metadatos.
    /// </summary>
    public bool IsProductiveApplicationSupported =>
        MetadataProductiveFieldSupport.IsSupported(
            Field);

    /// <summary>
    /// Indica si la propuesta puede seleccionarse para una futura
    /// aplicación productiva.
    /// </summary>
    public bool CanSelectForProductiveApplication =>
        CanSelect &&
        IsProductiveApplicationSupported;

    /// <summary>
    /// Indica si la interfaz puede solicitar un valor manual para
    /// este campo. Una regla de rechazo técnico sigue siendo final.
    /// </summary>
    public bool CanProvideManualValue =>
        IsProductiveApplicationSupported &&
        Decision != MetadataChangeDecision.Rejected;

    public string ManualValueActionDisplay =>
        HasManualOverride
            ? "Editar manual"
            : "Ingresar valor";

    /// <summary>
    /// Estado legible del soporte productivo del campo.
    /// </summary>
    public string ProductiveApplicationStatus =>
        IsProductiveApplicationSupported
            ? "Disponible"
            : "No disponible";

    /// <summary>
    /// Explicación mostrada por la interfaz para la capacidad
    /// productiva del campo.
    /// </summary>
    public string ProductiveApplicationToolTip =>
        IsProductiveApplicationSupported
            ? "Este campo puede incluirse en una aplicación productiva."
            : "Este campo todavía no tiene soporte de escritura productiva segura.";

    /// <summary>
    /// Indica si la propuesta fue seleccionada por el usuario.
    ///
    /// Al cambiar la selección también se actualiza el estado
    /// visible de revisión. Los campos sin soporte productivo no
    /// pueden quedar seleccionados.
    /// </summary>
    public bool IsSelected
    {
        get =>
            _isSelected;

        set
        {
            bool normalizedValue =
                value &&
                CanSelectForProductiveApplication;

            if (_isSelected == normalizedValue)
            {
                return;
            }

            _isSelected =
                normalizedValue;

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
    /// Indica si la fila debe mostrarse como modificable según
    /// la decisión técnica de simulación.
    /// </summary>
    public bool CanSelect =>
        HasSelectableChange &&
        Decision != MetadataChangeDecision.Rejected &&
        (
            HasManualOverride ||
            Decision is not
                MetadataChangeDecision.Conflict and not
                MetadataChangeDecision.InsufficientEvidence
        );

    /// <summary>
    /// Sustituye la propuesta técnica por un valor introducido de
    /// forma explícita por el usuario. El cambio permanece pendiente
    /// hasta que también se seleccione la casilla Aplicar.
    /// </summary>
    public bool TryApplyManualValue(
        string? value,
        out string validationError)
    {
        if (!CanProvideManualValue)
        {
            validationError =
                "Este campo no admite una modificación manual segura.";

            return false;
        }

        string normalizedValue =
            NormalizeStoredValue(
                value);

        if (string.IsNullOrWhiteSpace(
                normalizedValue))
        {
            validationError =
                "El valor manual no puede estar vacío.";

            return false;
        }

        validationError =
            string.Empty;

        if (string.Equals(
                _manualProposedValue,
                normalizedValue,
                StringComparison.Ordinal))
        {
            return true;
        }

        _manualProposedValue =
            normalizedValue;

        IsSelected =
            false;

        ReviewState =
            HasSelectableChange
                ? "Valor manual pendiente"
                : "Sin cambios";

        NotifyManualValueProperties();

        return true;
    }

    /// <summary>
    /// Sincroniza el estado de revisión con la selección actual.
    /// </summary>
    private void UpdateReviewStateFromSelection()
    {
        if (!HasSelectableChange)
        {
            ReviewState =
                "Sin cambios";

            return;
        }

        if (!CanSelect)
        {
            return;
        }

        if (!IsProductiveApplicationSupported)
        {
            ReviewState =
                "Sin soporte productivo";

            return;
        }

        if (_isSelected)
        {
            ReviewState =
                RequiresManualReview ||
                HasManualOverride
                    ? "Aprobado por el usuario"
                    : "Preseleccionado automáticamente";

            return;
        }

        ReviewState =
            RequiresManualReview
                ? "Pendiente de revisión"
                : "No seleccionado";
    }

    private void NotifyManualValueProperties()
    {
        OnPropertyChanged(
            nameof(HasManualOverride));

        OnPropertyChanged(
            nameof(EffectiveProposedValue));

        OnPropertyChanged(
            nameof(EffectiveDecisionDisplay));

        OnPropertyChanged(
            nameof(ConfidenceDisplay));

        OnPropertyChanged(
            nameof(SourcesDisplay));

        OnPropertyChanged(
            nameof(EffectiveSupportingSources));

        OnPropertyChanged(
            nameof(HasSelectableChange));

        OnPropertyChanged(
            nameof(CanSelect));

        OnPropertyChanged(
            nameof(CanSelectForProductiveApplication));

        OnPropertyChanged(
            nameof(IsManuallyApproved));

        OnPropertyChanged(
            nameof(IsApprovedForSimulation));

        OnPropertyChanged(
            nameof(ManualValueActionDisplay));
    }

    private static bool HasValueChange(
        string? currentValue,
        string? proposedValue)
    {
        string current =
            NormalizeStoredValue(
                currentValue);

        string proposed =
            NormalizeStoredValue(
                proposedValue);

        return
            !string.IsNullOrWhiteSpace(
                proposed) &&
            !string.Equals(
                current,
                proposed,
                StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeStoredValue(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(
                value))
        {
            return string.Empty;
        }

        string normalizedValue =
            value.Trim();

        return string.Equals(
                normalizedValue,
                "(sin información)",
                StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : normalizedValue;
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
