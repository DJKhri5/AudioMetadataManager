namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Pipeline.Models;

/// <summary>
/// Identifica las etapas que forman una ejecución completa del
/// pipeline de aplicación.
/// </summary>
public enum MetadataApplicationStage
{
    /// <summary>
    /// El pipeline todavía no ha comenzado.
    /// </summary>
    None = 0,

    /// <summary>
    /// Validación estructural, de seguridad y acceso al archivo.
    /// </summary>
    Validation = 1,

    /// <summary>
    /// Creación de la copia de seguridad obligatoria.
    /// </summary>
    Backup = 2,

    /// <summary>
    /// Escritura de los metadatos aprobados.
    /// </summary>
    MetadataWrite = 3,

    /// <summary>
    /// Lectura posterior y verificación de los valores.
    /// </summary>
    PostWriteVerification = 4,

    /// <summary>
    /// Construcción del resultado final y cierre de la operación.
    /// </summary>
    Finalization = 5
}