namespace AudioMetadataManager.UI.Services.MetadataSources
    .Configuration;

/// <summary>
/// Describe el estado actual de configuración de una fuente
/// externa de metadatos.
/// </summary>
public enum MetadataSourceConfigurationState
{
    /// <summary>
    /// El estado todavía no ha sido comprobado.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// La fuente no contiene las credenciales necesarias.
    /// </summary>
    NotConfigured = 1,

    /// <summary>
    /// La fuente contiene una configuración local utilizable.
    ///
    /// Esto no confirma todavía que las credenciales hayan sido
    /// aceptadas por el servicio externo.
    /// </summary>
    Configured = 2,

    /// <summary>
    /// La configuración fue validada correctamente mediante
    /// una conexión con el servicio externo.
    /// </summary>
    ConnectionVerified = 3,

    /// <summary>
    /// La configuración existe, pero fue rechazada por el
    /// servicio externo.
    /// </summary>
    AuthenticationFailed = 4,

    /// <summary>
    /// No fue posible comprobar el estado por un problema
    /// técnico o de almacenamiento local.
    /// </summary>
    Error = 5
}