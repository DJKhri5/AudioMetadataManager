using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Validation;

/// <summary>
/// Representa un problema individual detectado durante la
/// validación previa.
/// </summary>
public sealed class MetadataApplyValidationIssue
{
    /// <summary>
    /// Gravedad del problema.
    /// </summary>
    public MetadataApplyValidationIssueSeverity Severity
    { get; init; } =
            MetadataApplyValidationIssueSeverity.Information;

    /// <summary>
    /// Código estable del problema para diagnóstico y pruebas.
    /// </summary>
    public string Code { get; init; } =
        string.Empty;

    /// <summary>
    /// Mensaje legible.
    /// </summary>
    public string Message { get; init; } =
        string.Empty;

    /// <summary>
    /// Campo relacionado, cuando el problema pertenece a una
    /// modificación concreta.
    /// </summary>
    public MetadataField? Field { get; init; }

    /// <summary>
    /// Indica si el problema bloquea la solicitud.
    /// </summary>
    public bool IsBlocking =>
        Severity ==
        MetadataApplyValidationIssueSeverity.Error;

    /// <summary>
    /// Resumen compacto.
    /// </summary>
    public string Summary
    {
        get
        {
            string fieldPart =
                Field.HasValue
                    ? $" [{Field.Value}]"
                    : string.Empty;

            return
                $"{Severity}{fieldPart}: {Message}";
        }
    }
}