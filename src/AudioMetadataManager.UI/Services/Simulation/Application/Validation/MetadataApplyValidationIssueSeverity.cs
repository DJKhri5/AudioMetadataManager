namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Validation;

/// <summary>
/// Describe la gravedad de un problema encontrado durante la
/// validación previa de una solicitud de aplicación.
/// </summary>
public enum MetadataApplyValidationIssueSeverity
{
    /// <summary>
    /// Información adicional que no impide continuar.
    /// </summary>
    Information = 0,

    /// <summary>
    /// Situación que debe registrarse, pero que no bloquea por
    /// sí sola la operación.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Error que impide que la solicitud continúe hacia la
    /// creación del respaldo o la escritura.
    /// </summary>
    Error = 2
}