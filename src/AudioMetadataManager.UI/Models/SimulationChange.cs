namespace AudioMetadataManager.UI.Models;

public class SimulationChange
{
    public string PropertyName { get; set; } = string.Empty;

    public string CurrentValue { get; set; } = string.Empty;

    public string ProposedValue { get; set; } = string.Empty;

    public bool HasChange =>
        !string.Equals(
            CurrentValue?.Trim(),
            ProposedValue?.Trim(),
            StringComparison.Ordinal);

    public bool IsSelected { get; set; } = true;

    public string Description { get; set; } = string.Empty;
}