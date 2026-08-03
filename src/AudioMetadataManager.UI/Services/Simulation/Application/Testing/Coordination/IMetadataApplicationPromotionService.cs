namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Coordination;

/// <summary>
/// Define una promoción controlada de una copia verificada hacia
/// un archivo de destino, con respaldo productivo, verificación
/// posterior y posibilidad de reversión.
/// </summary>
public interface IMetadataApplicationPromotionService
{
    /// <summary>
    /// Promueve una copia verificada utilizando la configuración
    /// segura predeterminada.
    /// </summary>
    Task<MetadataApplicationPromotionResult> PromoteAsync(
        string verifiedWorkingCopyPath,
        string destinationFilePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Promueve una copia verificada utilizando las opciones
    /// indicadas.
    ///
    /// Las opciones de simulación deben utilizarse solamente en
    /// pruebas controladas sobre archivos temporales.
    /// </summary>
    Task<MetadataApplicationPromotionResult> PromoteAsync(
        string verifiedWorkingCopyPath,
        string destinationFilePath,
        MetadataApplicationPromotionOptions options,
        CancellationToken cancellationToken = default);
}