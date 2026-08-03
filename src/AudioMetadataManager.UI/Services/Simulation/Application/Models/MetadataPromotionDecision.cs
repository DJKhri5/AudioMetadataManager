namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Models;

/// <summary>
/// Representa la decisión adoptada durante la segunda
/// confirmación de una aplicación productiva de metadatos.
/// </summary>
public enum MetadataPromotionDecision
{
    /// <summary>
    /// Todavía no se ha solicitado una decisión de promoción.
    /// </summary>
    NotRequested = 0,

    /// <summary>
    /// La promoción fue presentada al usuario y quedó pendiente
    /// de una respuesta válida.
    /// </summary>
    Pending = 100,

    /// <summary>
    /// El usuario aprobó explícitamente la promoción de la copia
    /// verificada hacia el archivo original.
    /// </summary>
    Approved = 200,

    /// <summary>
    /// El usuario rechazó la promoción después de revisar el
    /// resultado de la ejecución aislada.
    /// </summary>
    Declined = 300,

    /// <summary>
    /// La segunda confirmación no pudo completarse porque la
    /// ejecución aislada no produjo una copia promovible.
    /// </summary>
    Unavailable = 400
}