namespace AudioMetadataManager.UI.Services.MetadataSources
    .Configuration;

/// <summary>
/// Define las operaciones comunes de configuración para una
/// fuente externa de metadatos.
///
/// La interfaz no conoce cómo se almacenan las credenciales.
/// </summary>
public interface IMetadataSourceConfigurationService
{
    /// <summary>
    /// Nombre legible de la fuente.
    /// </summary>
    string SourceName { get; }

    /// <summary>
    /// Consulta el estado local actual.
    /// </summary>
    MetadataSourceConfigurationResult GetStatus();

    /// <summary>
    /// Guarda o reemplaza la credencial principal.
    /// </summary>
    MetadataSourceConfigurationResult SaveCredential(
        string credential);

    /// <summary>
    /// Elimina las credenciales locales de la fuente.
    /// </summary>
    MetadataSourceConfigurationResult DeleteCredential();

    /// <summary>
    /// Comprueba mediante una operación externa si las
    /// credenciales configuradas funcionan correctamente.
    /// </summary>
    Task<MetadataSourceConfigurationResult> TestConnectionAsync(
        CancellationToken cancellationToken = default);
}