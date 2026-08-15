using AudioMetadataManager.UI.Services.MetadataSources.Models;

namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Validation;

/// <summary>
/// Define los campos de metadatos que el pipeline productivo
/// puede escribir y verificar actualmente.
///
/// Esta política se comparte entre la construcción de solicitudes
/// productivas y el mapper TagLibSharp para evitar que la interfaz
/// prepare cambios que todavía no pueden ejecutarse de forma segura.
/// </summary>
public static class MetadataProductiveFieldSupport
{
    /// <summary>
    /// Indica si el campo puede formar parte de una solicitud
    /// productiva con la infraestructura de escritura actual.
    /// </summary>
    public static bool IsSupported(
        MetadataField field)
    {
        return field is
            MetadataField.Artist or
            MetadataField.Title or
            MetadataField.Version or
            MetadataField.Album or
            MetadataField.Label or
            MetadataField.Genre;
    }
}