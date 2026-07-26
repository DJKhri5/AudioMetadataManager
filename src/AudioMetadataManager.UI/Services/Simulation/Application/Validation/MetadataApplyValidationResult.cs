namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Validation;

/// <summary>
/// Contiene el resultado completo de la validación previa de
/// una solicitud de aplicación.
/// </summary>
public sealed class MetadataApplyValidationResult
{
    /// <summary>
    /// Momento UTC en que se completó la validación.
    /// </summary>
    public DateTimeOffset ValidatedAtUtc { get; init; } =
        DateTimeOffset.UtcNow;

    /// <summary>
    /// Problemas encontrados.
    /// </summary>
    public IReadOnlyList<MetadataApplyValidationIssue>
        Issues
    { get; init; } =
            Array.Empty<MetadataApplyValidationIssue>();

    /// <summary>
    /// Errores que impiden continuar.
    /// </summary>
    public IReadOnlyList<MetadataApplyValidationIssue>
        Errors =>
            Issues
                .Where(issue => issue.IsBlocking)
                .ToArray();

    /// <summary>
    /// Advertencias no bloqueantes.
    /// </summary>
    public IReadOnlyList<MetadataApplyValidationIssue>
        Warnings =>
            Issues
                .Where(
                    issue =>
                        issue.Severity ==
                        MetadataApplyValidationIssueSeverity.Warning)
                .ToArray();

    /// <summary>
    /// Mensajes informativos.
    /// </summary>
    public IReadOnlyList<MetadataApplyValidationIssue>
        Information =>
            Issues
                .Where(
                    issue =>
                        issue.Severity ==
                        MetadataApplyValidationIssueSeverity.Information)
                .ToArray();

    /// <summary>
    /// Indica si la solicitud superó todas las comprobaciones
    /// bloqueantes.
    /// </summary>
    public bool IsValid =>
        Errors.Count == 0;

    /// <summary>
    /// Cantidad de errores.
    /// </summary>
    public int ErrorCount =>
        Errors.Count;

    /// <summary>
    /// Cantidad de advertencias.
    /// </summary>
    public int WarningCount =>
        Warnings.Count;

    /// <summary>
    /// Resumen legible.
    /// </summary>
    public string Summary =>
        IsValid
            ? $"Validación correcta. Advertencias: " +
              $"{WarningCount}."
            : $"Validación rechazada. Errores: " +
              $"{ErrorCount}. Advertencias: " +
              $"{WarningCount}.";
}