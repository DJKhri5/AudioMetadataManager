namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Chromaprint.Configuration;

/// <summary>
/// Configuración necesaria para invocar la herramienta externa
/// fpcalc (Chromaprint) y generar huellas acústicas.
///
/// Esta versión solo genera la huella localmente. No incluye
/// todavía la consulta a la API pública de AcoustID.
/// </summary>
public sealed class ChromaprintOptions
{
    /// <summary>
    /// Ruta o nombre del ejecutable fpcalc.
    ///
    /// El valor predeterminado asume que fpcalc está disponible
    /// en el PATH del sistema. Puede reemplazarse por una ruta
    /// absoluta cuando el ejecutable se distribuya junto con la
    /// aplicación.
    /// </summary>
    public string ExecutablePath { get; set; } =
        "fpcalc";

    /// <summary>
    /// Tiempo máximo de espera para que fpcalc termine de
    /// analizar un archivo.
    /// </summary>
    public TimeSpan Timeout { get; set; } =
        TimeSpan.FromSeconds(30);

    /// <summary>
    /// Indica si la configuración es válida para invocar fpcalc.
    /// </summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(
            ExecutablePath) &&
        Timeout > TimeSpan.Zero;
}
