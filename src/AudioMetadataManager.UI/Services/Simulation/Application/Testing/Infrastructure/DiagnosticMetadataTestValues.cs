namespace AudioMetadataManager.UI.Services.Simulation
    .Application.Testing.Infrastructure;

/// <summary>
/// Proporciona valores variables para pruebas diagnósticas que
/// necesitan provocar una modificación física real.
///
/// Los valores no deben ser constantes porque un archivo utilizado
/// anteriormente por el diagnóstico podría contener ya ese mismo
/// valor y producir un falso fallo por ausencia de cambios.
/// </summary>
internal static class DiagnosticMetadataTestValues
{
    /// <summary>
    /// Crea un género diagnóstico diferente en cada ejecución.
    /// </summary>
    public static string CreateGenre()
    {
        return
            $"AudioMetadataManager Diagnostic " +
            $"{Guid.NewGuid():N}";
    }
}