using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Validation;

/// <summary>
/// Define el contrato para validar una solicitud de aplicación
/// antes de ejecutar operaciones sobre el archivo.
/// </summary>
public interface IMetadataApplyRequestValidator
{
    /// <summary>
    /// Valida completamente una solicitud de aplicación.
    /// </summary>
    MetadataApplyValidationResult Validate(
        MetadataApplyRequest request);
}